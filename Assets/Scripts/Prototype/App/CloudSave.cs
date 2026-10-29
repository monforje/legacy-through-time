using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The reader's progress in their Google Play saved games, so that a new phone or a reinstall goes on where the
    /// story was left. Under the loading screen Play Games signs the reader in by itself and the saved game is merged
    /// with what is on the phone (Progress.Merge): what the cloud adds is put in place and the menu shows it. After that
    /// the phone's copy goes up when there is something new: back in the menu, the app leaving the screen, a purchase.
    /// Without Play Games (no project id, no Google Play, no network, the reader said no) nothing changes: the progress
    /// stays on the phone, as before.
    static class CloudSave
    {
        public enum State { Off, Connecting, SignedOut, Failed, Synced }

        const string Slot = "progress";

        public static State Status { get; private set; } = State.Off;
        /// The status changed.
        public static event Action Changed;
        /// The cloud copy was merged in; true when it brought the phone something the reader would see.
        public static event Action<bool> Merged;
        /// True while a chapter is open: a cloud copy that comes then waits for the menu (Sync), so that it never
        /// fights the saves the chapter writes.
        public static Func<bool> Reading = () => false;

        static Progress cloud;                 // the copy in the cloud, as last read or written
        static string sent;                    // its text: nothing goes up while the phone has nothing new
        static bool arrived;                   // read, not merged yet
        static bool busy, again;

        /// At launch: the sign-in of Play Games and the merge. Waits up to `wait` seconds (the loading screen stays);
        /// an answer that comes later is still merged.
        public static IEnumerator Start(float wait)
        {
            if (Status != State.Off || !PlayGames.Available) yield break;
            Store.Finished += (id, ok, line) => { if (ok) Sync(); };
            SignIn(false, null);
            for (var t = 0f; t < wait && Status == State.Connecting; t += Time.unscaledDeltaTime) yield return null;
        }

        /// The reader asked (settings): the Play Games sign-in screen, then the merge. `failed` when it did not work.
        public static void SignIn(Action failed) => SignIn(true, failed);

        static void SignIn(bool interactive, Action failed)
        {
            if (!PlayGames.Available || Status == State.Connecting) return;
            SetStatus(State.Connecting);
            PlayGames.SignIn(interactive, ok =>
            {
                if (!ok) { SetStatus(State.SignedOut); failed?.Invoke(); return; }
                PlayGames.Load(Slot, text =>
                {
                    if (text == null) { SetStatus(State.Failed); failed?.Invoke(); return; }
                    cloud = Parse(text);
                    sent = cloud != null ? text : null;
                    arrived = true;
                    if (!Reading()) Merge();
                });
            });
        }

        /// Something worth keeping happened (the menu again, the app leaving the screen, a purchase): a cloud copy that
        /// waited is merged in, and the phone's progress goes up if it has anything new.
        public static void Sync()
        {
            if (arrived && !Reading()) Merge();
            else Push();
        }

        static void Merge()
        {
            arrived = false;
            var here = Collect();
            var restored = Apply(here, Progress.Merge(here, cloud));
            SetStatus(State.Synced);
            Merged?.Invoke(restored);
            Push();
        }

        static void Push()
        {
            if (Status != State.Synced) return;          // never over a cloud copy that was not merged in
            if (busy) { again = true; return; }
            var merged = Progress.Merge(Collect(), cloud);
            var text = JsonUtility.ToJson(merged);
            if (text == sent) return;
            busy = true;
            PlayGames.Save(Slot, text, Describe(merged), ok =>
            {
                busy = false;
                if (ok) { cloud = merged; sent = text; }
                if (again) { again = false; Push(); }
            });
        }

        static void SetStatus(State status)
        {
            if (status == Status) return;
            Debug.Log("[cloud] " + status);
            Status = status;
            Changed?.Invoke();
        }

        static Progress Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            try { return JsonUtility.FromJson<Progress>(text); }
            catch (Exception e) { Debug.LogWarning("[cloud] the saved game is unreadable: " + e.Message); return null; }
        }

        static string Describe(Progress p) => $"Монет: {p.coins} · глав пройдено: {p.finished.Count} · в процессе: {p.chapters.Count}";

        // ------------------------------------------------------------ the phone's copy
        /// Everything the phone keeps, gathered from the stores that own each part.
        static Progress Collect()
        {
            var here = new Progress
            {
                coins = Wallet.Coins, coinsAt = Wallet.ChangedAt, giftDay = Wallet.GiftDay,
                words = new List<string>(Library.Open()), chapters = SaveStore.All(), stats = SaveStore.AllStats(),
                credited = Store.Ledger(),
            };
            // finish marks are PlayerPrefs keys, which cannot be listed: the chapters of this build and of the cloud
            var ids = new HashSet<string>();
            foreach (var s in StoryCatalog.All) foreach (var c in s.Chapters) ids.Add(c.SaveId);
            if (cloud != null) foreach (var f in cloud.finished) ids.Add(f.id);
            foreach (var id in ids)
                if (SaveStore.Finished(id)) here.finished.Add(new FinishMark { id = id, at = SaveStore.FinishedAt(id) });
            return here;
        }

        /// Puts the merged progress in place where it differs from the phone's; true when the reader would see it.
        static bool Apply(Progress here, Progress merged)
        {
            var seen = false;
            if (merged.coins != here.coins || merged.coinsAt != here.coinsAt)
            {
                Wallet.Restore(merged.coins, merged.coinsAt);
                seen |= merged.coins != here.coins;
            }
            if (merged.giftDay != here.giftDay) Wallet.GiftDay = merged.giftDay;
            if (!Same(merged.words, here.words)) { Library.Restore(merged.words); seen = true; }
            if (!Same(merged.credited, here.credited)) Store.RestoreLedger(merged.credited);

            foreach (var f in merged.finished)
            {
                if (SaveStore.Finished(f.id) && SaveStore.FinishedAt(f.id) == f.at) continue;
                seen |= !SaveStore.Finished(f.id);
                SaveStore.RestoreFinished(f.id, f.at);
            }

            var ended = new Dictionary<string, long>();
            foreach (var s in here.stats) ended[s.chapter] = s.at;
            foreach (var s in merged.stats)
                if (!ended.TryGetValue(s.chapter, out var at) || at != s.at) SaveStore.WriteStats(s);

            var mine = new Dictionary<string, StorySave>();
            foreach (var c in here.chapters) mine[c.story] = c;
            var kept = new HashSet<string>();
            foreach (var c in merged.chapters)
            {
                kept.Add(c.story);
                if (mine.TryGetValue(c.story, out var old) && old.at == c.at) continue;
                SaveStore.Write(c);
                seen = true;
            }
            foreach (var c in here.chapters)
                if (!kept.Contains(c.story)) { SaveStore.Delete(c.story); seen = true; }
            return seen;
        }

        static bool Same(List<string> a, List<string> b)
        {
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
            return true;
        }
    }
}

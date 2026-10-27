using System;
using System.Collections.Generic;

namespace LegacyThroughTime.Prototype
{
    /// Where the reader stopped in a chapter: the machine (ink state, `StoryMachine.Save`) at the last line shown, the
    /// background that was on screen and when that was (unix ms; 0 in saves made before the cloud).
    [Serializable]
    sealed class StorySave
    {
        public string story;
        public string machine;
        public string background;
        public long at;
    }

    /// A chapter read to its end, and when (unix ms; 0 for marks made before the cloud).
    [Serializable]
    sealed class FinishMark
    {
        public string id;
        public long at;
    }

    /// Everything of the reader's that should outlive a phone: the purse, the day of the last gift, the words of the
    /// library, the finished chapters, the chapters in progress and the purchases already credited. One copy lives on
    /// the phone (spread over the stores that own each part), one in the reader's Google Play saved games.
    [Serializable]
    sealed class Progress
    {
        /// How many credited transactions are remembered (the ledger of Store).
        public const int CreditedKept = 64;

        public int coins;
        public long coinsAt;                        // when the purse last changed; 0 = not since the cloud came
        public string giftDay = "";
        public List<string> words = new();
        public List<FinishMark> finished = new();
        public List<StorySave> chapters = new();
        public List<string> credited = new();

        public static long Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// One copy out of two (either may be null; neither is changed). The purse goes with its later change, a tie
        /// with the phone; the gift day is the later one; words, finished chapters and credited purchases are pooled.
        /// A chapter keeps its later save, unless it was finished after that save: finishing deletes the save on one
        /// phone while the other copy may still hold it.
        public static Progress Merge(Progress here, Progress cloud)
        {
            here ??= new Progress(); cloud ??= new Progress();
            var purse = cloud.coinsAt > here.coinsAt ? cloud : here;
            var merged = new Progress
            {
                coins = purse.coins, coinsAt = purse.coinsAt,
                giftDay = Later(here.giftDay ?? "", cloud.giftDay ?? ""),
                words = Pool(here.words, cloud.words, int.MaxValue),
                credited = Pool(here.credited, cloud.credited, CreditedKept),
            };

            var done = new Dictionary<string, long>();
            foreach (var f in Both(here.finished, cloud.finished))
                done[f.id] = done.TryGetValue(f.id, out var at) ? Math.Max(at, f.at) : f.at;
            foreach (var pair in done) merged.finished.Add(new FinishMark { id = pair.Key, at = pair.Value });

            var latest = new Dictionary<string, StorySave>();
            foreach (var s in Both(here.chapters, cloud.chapters))
                if (!latest.TryGetValue(s.story, out var other) || s.at > other.at) latest[s.story] = s;
            foreach (var s in latest.Values)
                if (!done.TryGetValue(s.story, out var finishedAt) || finishedAt <= s.at)
                    merged.chapters.Add(new StorySave { story = s.story, machine = s.machine, background = s.background, at = s.at });
            return merged;
        }

        static string Later(string a, string b) => string.CompareOrdinal(a, b) >= 0 ? a : b;

        /// Both lists in order without repeats; the oldest go when there are more than `max`.
        static List<string> Pool(List<string> a, List<string> b, int max)
        {
            var all = new List<string>();
            var seen = new HashSet<string>();
            foreach (var s in Both(a, b)) if (s != null && seen.Add(s)) all.Add(s);
            if (all.Count > max) all.RemoveRange(0, all.Count - max);
            return all;
        }

        static IEnumerable<T> Both<T>(List<T> a, List<T> b) where T : class
        {
            if (a != null) foreach (var x in a) if (x != null) yield return x;
            if (b != null) foreach (var x in b) if (x != null) yield return x;
        }
    }
}

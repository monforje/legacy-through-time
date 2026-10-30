using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The "Награды" popup (templates/shell/rewards.html): four cards with a picture on the right, a timer, a lock and an
    /// action. A timer runs down by itself; at zero the free energy is paid out by itself, the others wait for a tap:
    /// the button pulses, a tap loads, then "Получено", a toast and sparks.
    sealed class RewardsDialog
    {
        const float CardHeight = 164;

        readonly ShellDialog dialog;
        readonly IAnimator anim;
        readonly List<Row> rows = new();

        sealed class Row
        {
            public Rewards.Kind Kind;
            public Text Timer;
            public ShellButton Action;
            public ShellButton Lock;
            public Image Art;
            public bool Claimed;
        }

        public bool IsOpen => dialog.IsOpen;
        public void Close() => dialog.Close();

        public RewardsDialog(Transform parent, IAnimator anim, Action closed)
        {
            this.anim = anim;
            dialog = new ShellDialog(parent, "Награды", anim, closed);
            var list = MenuUi.List(dialog.Content, 0, 0, 0);
            var band = Kit.Round(list, "Band", Kit.Surface2, Kit.RadiusM);
            Ui.TopRow(band.rectTransform, 0, 0, 0, 56);
            var bt = Ui.Txt(band.rectTransform, "Text", "Ежедневные награды", Kit.F800, Kit.FsHead, Kit.Text, TextAnchor.MiddleCenter);
            Ui.Stretch(bt.rectTransform);

            var y = 56f + Kit.Space3;
            foreach (var kind in Rewards.All)
            {
                if (kind.Id == "energy" && ShellPurse.TakenAt("energy") == 0) ShellPurse.Start(kind);     // the first timer starts now
                Card(list, kind, y);
                y += CardHeight + Kit.Space3;
            }
            MenuUi.SetHeight(list, y);
            Tick();
            if (anim.Enabled) anim.Play(Loop());
        }

        void Card(RectTransform list, Rewards.Kind kind, float y)
        {
            var row = new Row { Kind = kind };
            var card = Kit.Round(list, "Reward " + kind.Id, Kit.Surface2, Kit.RadiusM);
            Ui.TopRow(card.rectTransform, 0, 0, y, CardHeight);
            var title = Ui.Txt(card.rectTransform, "Title", kind.Title, Kit.F800, Kit.FsHead, Kit.Text, TextAnchor.UpperLeft);
            title.verticalOverflow = VerticalWrapMode.Truncate;
            Ui.TopRow(title.rectTransform, 14, 14 + 108 + Kit.Space2, 12, 48);
            var text = Ui.Txt(card.rectTransform, "Text", kind.Text, Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.UpperLeft);
            text.verticalOverflow = VerticalWrapMode.Truncate;
            Ui.TopRow(text.rectTransform, 14, 14 + 108 + Kit.Space2, 60, 56);

            // the picture stands over the action: the same width, so their centres meet
            row.Art = Kit.Icon(card.rectTransform, "Art", Kit.Pic(kind.Pic), 88, A.TopRight, new Vector2(-12 - 54, -12 - 44));
            row.Art.raycastTarget = false;

            var timer = Kit.Round(card.rectTransform, "Timer", Kit.Surface, Kit.RadiusS);
            Ui.Box(timer.rectTransform, A.BottomLeft, A.BottomLeft, new Vector2(12, 12), new Vector2(kind.Seconds > 0 || kind.Id == "daily" ? 96 : 78, 40));
            var clock = Kit.Icon(timer.rectTransform, "Clock", Kit.Glyph(kind.Id == "chapter" ? "doc" : "timer"), 16, A.MidLeft, new Vector2(18, 0));
            clock.raycastTarget = false;
            row.Timer = Ui.Txt(timer.rectTransform, "Time", "", Kit.F800, Kit.FsSmall, Kit.Text, TextAnchor.MiddleCenter);
            Ui.Stretch(row.Timer.rectTransform, 28, 0, 6, 0);

            if (kind.NeedsTap)
            {
                var two = kind.Id == "ad";
                row.Action = Parts.Button(card.rectTransform, kind.Id == "ad" ? "Смотреть рекламу" : kind.Id == "chapter" ? "Не готово" : "Получить",
                                          ButtonStyle.Primary, () => Claim(row), 40, Kit.RadiusM);
                Ui.Box(row.Action.Root, A.BottomRight, A.BottomRight, new Vector2(-12, 12), new Vector2(108, 40));
                row.Action.Label.fontSize = two ? Kit.FsMicro : Kit.FsBody;
                row.Action.Label.resizeTextForBestFit = false;
                row.Lock = Parts.IconButton(card.rectTransform, "lock", null, 28, circle: false);
                Ui.Box(row.Lock.Root, A.BottomRight, A.BottomRight, new Vector2(-12 - 108 - Kit.Space2, 12 + 6), new Vector2(28, 28));
                row.Lock.SetEnabled(false);
            }
            rows.Add(row);
        }

        // ------------------------------------------------------------------ time
        IEnumerator Loop()
        {
            while (dialog.IsOpen)
            {
                yield return new WaitForSecondsRealtime(1f);
                if (dialog.IsOpen) Tick();
            }
        }

        void Tick()
        {
            var now = DateTime.Now;
            foreach (var row in rows)
            {
                if (row.Claimed || !row.Timer) continue;
                var left = row.Kind.Id == "chapter" ? 1 : ShellPurse.Remaining(row.Kind, now);
                row.Timer.text = row.Kind.Id == "chapter" ? "0 из 1" : Rewards.Clock(left);
                if (row.Kind.Id == "energy" && left == 0 && ShellPurse.Take(row.Kind, now))
                    ShellToast.Show(dialog.Root, "Бесплатная энергия: +" + row.Kind.Energy, anim);
                if (row.Action == null) continue;
                var ready = row.Kind.Id != "chapter" && left == 0;
                row.Action.SetEnabled(ready);
                if (row.Lock != null) row.Lock.Root.gameObject.SetActive(!ready);
                if (ready && anim.Enabled && !row.Action.Pulsing) { row.Action.Pulsing = true; anim.Play(Pulse(row)); }
            }
        }

        /// A reward that is ready breathes a little (a scale pulse, no glow) until it is taken.
        IEnumerator Pulse(Row row)
        {
            var t = 0f;
            while (row.Action != null && row.Action.Root && !row.Claimed && row.Action.Pulsing)
            {
                row.Action.Root.localScale = Vector3.one * (1f + .05f * (.5f - .5f * Mathf.Cos(t * 3.9f)));
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (row.Action != null && row.Action.Root) row.Action.Root.localScale = Vector3.one;
        }

        // ------------------------------------------------------------------ taking
        void Claim(Row row)
        {
            if (row.Claimed) return;
            var button = row.Action;
            button.Pulsing = false;
            button.SetLoading(true, anim);
            anim.Play(Tween.After(anim.Enabled ? Kit.Dur.Net * .7f : 0, () =>
            {
                if (!dialog.IsOpen) return;
                button.SetLoading(false, anim);
                if (!ShellPurse.Take(row.Kind, DateTime.Now)) return;
                row.Claimed = row.Kind.Id == "daily" || row.Kind.Id == "chapter";
                if (row.Claimed) { button.SetLabel("Получено"); button.SetEnabled(false); }
                var gives = row.Kind.Gems > 0 ? row.Kind.Gems + " " + ShopCatalog.Plural(row.Kind.Gems, "алмаз", "алмаза", "алмазов") : row.Kind.Energy + " энергии";
                ShellToast.Show(dialog.Root, "Награда получена: " + gives, anim);
                var at = (Vector2)dialog.Root.InverseTransformPoint(row.Art.rectTransform.position) + dialog.Root.rect.size * dialog.Root.pivot;
                ShellSparks.Burst(dialog.Root, at, anim, 6);
            }));
        }
    }
}

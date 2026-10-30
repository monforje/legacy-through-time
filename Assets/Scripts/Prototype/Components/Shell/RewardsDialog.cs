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
                y += Card(list, kind, y) + Kit.Space3;
            }
            if (anim.Enabled)
            {
                var items = new List<RectTransform> { band.rectTransform };
                foreach (var row in rows) items.Add((RectTransform)row.Art.rectTransform.parent);
                Parts.Stagger(anim, items);
                foreach (var row in rows) if (row.Kind.Id == "chapter") anim.Play(Parts.Float(row.Art.rectTransform));      // the star floats
            }
            MenuUi.SetHeight(list, y);
            Tick();
            if (anim.Enabled) anim.Play(Loop());
        }

        /// `.card-reward`: padding 12; the title and the text on the left (134 wide), the picture over the action's column
        /// (108 wide) on the right, then the foot with the timer and the action. Returns its height.
        float Card(RectTransform list, Rewards.Kind kind, float y)
        {
            const float pad = Kit.Space3, info = 134, art = 108, foot = 40;
            var row = new Row { Kind = kind };
            var card = Kit.Round(list, "Reward " + kind.Id, Kit.Surface, Kit.RadiusM);
            var title = Ui.Txt(card.rectTransform, "Title", kind.Title, Kit.F800, Kit.FsHead, Kit.Text, TextAnchor.UpperLeft);
            var text = Ui.Txt(card.rectTransform, "Text", kind.Text, Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.UpperLeft);
            var titleHeight = Mathf.Ceil(Ui.TextHeight(title, info));
            var textHeight = Mathf.Ceil(Ui.TextHeight(text, info));
            var infoHeight = titleHeight + Kit.Space2 + textHeight;
            var rowHeight = Mathf.Max(infoHeight, 88);
            Ui.TopRow(title.rectTransform, pad, 304 - pad - info, pad, titleHeight);
            Ui.TopRow(text.rectTransform, pad, 304 - pad - info, pad + titleHeight + Kit.Space2, textHeight);

            // the picture is centred in its column, so its centre meets the centre of the action under it
            row.Art = Kit.Icon(card.rectTransform, "Art", Kit.Pic(kind.Pic), 88, A.TopLeft, new Vector2(304 - pad - art / 2 - 44, -(pad + (rowHeight - 88) / 2)));
            row.Art.raycastTarget = false;

            var footTop = pad + rowHeight + Kit.Space2;
            var timer = Kit.Round(card.rectTransform, "Timer", Kit.Surface2, Kit.RadiusS);
            row.Timer = Ui.Txt(timer.rectTransform, "Time", "00:00:00", Kit.F800, Kit.FsSmall, Kit.Text, TextAnchor.MiddleLeft);
            var chapter = kind.Id == "chapter";
            var timeWidth = chapter ? Ui.Txt(timer.rectTransform, "Probe", "0 из 1", Kit.F800, Kit.FsSmall, Kit.Text).preferredWidth : row.Timer.preferredWidth;
            if (chapter) UnityEngine.Object.Destroy(timer.rectTransform.GetChild(timer.rectTransform.childCount - 1).gameObject);
            Ui.Box(timer.rectTransform, A.TopLeft, A.TopLeft, new Vector2(pad, -footTop), new Vector2(Kit.Space2 + 16 + Kit.Space2 + Mathf.Ceil(timeWidth) + Kit.Space2, foot));
            var clock = chapter ? Kit.Icon(timer.rectTransform, "Clock", Kit.Pic("book"), 16, A.MidLeft, new Vector2(Kit.Space2, 0))
                                : Kit.Icon(timer.rectTransform, "Clock", Kit.Glyph("timer"), 16, A.MidLeft, new Vector2(Kit.Space2, 0));
            clock.raycastTarget = false;
            Ui.Box(row.Timer.rectTransform, A.MidLeft, A.MidLeft, new Vector2(Kit.Space2 + 16 + Kit.Space2, 0), new Vector2(Mathf.Ceil(timeWidth) + 2, 18));

            if (kind.NeedsTap)
            {
                var two = kind.Id == "ad";
                row.Action = Parts.Button(card.rectTransform, two ? "Смотреть рекламу" : chapter ? "Не готово" : "Получить",
                                          ButtonStyle.Primary, () => Claim(row), foot, Kit.RadiusM);
                Ui.Box(row.Action.Root, A.TopLeft, A.TopLeft, new Vector2(304 - pad - art, -footTop), new Vector2(art, foot));
                row.Action.Label.fontSize = two ? Kit.FsMicro : Kit.FsBody;
                row.Action.Label.resizeTextForBestFit = false;
                if (two) { row.Action.Label.horizontalOverflow = HorizontalWrapMode.Wrap; row.Action.Label.lineSpacing = .9f; }
                if (!chapter)
                {
                    row.Lock = Parts.IconButton(card.rectTransform, "lock", null, 28, circle: false);
                    Ui.Box(row.Lock.Root, A.TopLeft, A.TopLeft, new Vector2(304 - pad - art - Kit.Space2 - 28, -(footTop + 6)), new Vector2(28, 28));
                    row.Lock.Glyph.rectTransform.sizeDelta = new Vector2(16, 16);
                    row.Lock.SetEnabled(false);
                }
            }
            var height = footTop + foot + pad;
            Ui.Box(card.rectTransform, A.TopLeft, A.TopLeft, new Vector2(0, -y), new Vector2(304, height));
            rows.Add(row);
            return height;
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

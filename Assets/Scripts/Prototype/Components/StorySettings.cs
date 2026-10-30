using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The settings of a story (every story has them): a modal card of the same grey glass as the tools over the story (StoryHud)
    /// on the dimmed screen, with the switches (sound, music, the sounds of the story, notifications) and two buttons, "Продолжить"
    /// and "В меню". The switches are those of the kit of the story that is read (`switch-on`, `switch-off`).
    sealed class StorySettings
    {
        const float RowHeight = 46, Pad = 30, ButtonHeight = 46, TitleHeight = 34;

        readonly RectTransform layer;
        readonly IAnimator anim;
        RectTransform page, card;
        CanvasGroup group;

        public bool IsOpen => page;

        public StorySettings(RectTransform overlay, IAnimator anim)
        {
            layer = overlay; this.anim = anim;
        }

        /// `toMenu`: the "В меню" button.
        public void Open(Action toMenu)
        {
            if (IsOpen) return;
            page = Ui.Rect(layer, "StorySettings");
            Ui.Stretch(page);
            group = page.gameObject.AddComponent<CanvasGroup>();
            var dim = Ui.Img(page, "Dim", ProceduralArt.White);
            dim.color = new Color(.04f, .07f, .12f, .74f); dim.raycastTarget = true;
            Ui.Stretch(dim.rectTransform);
            PressButton.On(dim.gameObject, Close, 1f);
            var column = Kit.Column(page);

            var rows = new (string label, Func<bool> get, Action<bool> set)[]
            {
                ("Звук", () => Settings.Sound, v => Settings.Sound = v),
                ("Музыка", () => Settings.Music, v => Settings.Music = v),
                ("Звуки истории", () => Settings.Effects, v => Settings.Effects = v),
                ("Уведомления", () => Settings.Notifications, v => Settings.Notifications = v),
            };
            var width = Kit.Width - 2 * Kit.Side;
            var inner = width - 2 * Pad;
            var height = Pad + TitleHeight + Kit.Space2 + rows.Length * RowHeight + Kit.Space4 + 2 * ButtonHeight + Kit.Space2 + Pad;

            card = Ui.Rect(column, "Card");
            Ui.Box(card, A.Center, A.Center, new Vector2(0, 8), new Vector2(width, height));
            var sheet = Kit.Round(card, "Sheet", StoryHud.GlassPanel, Kit.RadiusL);
            sheet.raycastTarget = true;
            Ui.Stretch(sheet.rectTransform);
            Kit.Shadow(card, Kit.RadiusL, 28, .5f, 10);

            var title = Ui.Txt(card, "Title", "Настройки", Kit.F800, Kit.FsTitle, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, Pad + 28, Pad + 28, Pad, TitleHeight);

            var y = Pad + TitleHeight + Kit.Space2;
            for (var i = 0; i < rows.Length; i++)
            {
                Row(card, rows[i].label, rows[i].get, rows[i].set, y, inner);
                if (i > 0) Line(card, y, inner);
                y += RowHeight;
            }
            y += Kit.Space4;
            var go = Parts.Button(card, "Продолжить", ButtonStyle.Primary, Close, ButtonHeight);
            Ui.TopRow(go.Root, Pad, Pad, y, ButtonHeight);
            var menu = Parts.Button(card, "В меню", ButtonStyle.Neutral, () => { Close(); toMenu?.Invoke(); }, ButtonHeight);
            Ui.TopRow(menu.Root, Pad, Pad, y + ButtonHeight + Kit.Space2, ButtonHeight);

            var close = Ui.Img(card, "Close", ProceduralArt.Disc);
            close.color = StoryHud.Glass; close.raycastTarget = true;
            Ui.Box(close.rectTransform, A.TopRight, A.TopRight, new Vector2(-Kit.Space3, -Kit.Space3), new Vector2(Kit.Touch - 4, Kit.Touch - 4));
            Kit.Icon(close.transform, "Glyph", Kit.Glyph("close"), 20).raycastTarget = false;
            PressButton.On(close.gameObject, Close, .94f);

            EdgeBack.Attach(page, () => { Close(); return true; }, 0);
            if (!anim.Enabled) return;
            group.alpha = 0; card.localScale = Vector3.one * .94f;
            anim.Play(Tween.To(Kit.Dur.Normal, e =>
            {
                if (!group) return;
                group.alpha = e; card.localScale = Vector3.one * (.94f + .06f * e);
            }, Easing.InOut));
        }

        public void Close()
        {
            if (!IsOpen) return;
            var closing = page; var g = group;
            page = null;
            g.blocksRaycasts = false;
            if (!anim.Enabled) { UnityEngine.Object.Destroy(closing.gameObject); return; }
            anim.Play(Tween.To(Kit.Dur.Fast, e => { if (g) g.alpha = 1 - e; }, Easing.Linear));
            anim.Play(Tween.After(Kit.Dur.Fast + .02f, () => { if (closing) UnityEngine.Object.Destroy(closing.gameObject); }));
        }

        /// A label on the left, the switch of the kit on the right; a tap anywhere on the row turns it.
        static void Row(RectTransform card, string label, Func<bool> get, Action<bool> set, float top, float inner)
        {
            var row = Ui.Img(card, "Row " + label, ProceduralArt.White);
            row.color = Color.clear; row.raycastTarget = true;
            Ui.TopRow(row.rectTransform, Pad, Pad, top, RowHeight);
            var text = Ui.Txt(row.rectTransform, "Label", label, Kit.F700, Kit.FsBody + 1, Kit.Text, TextAnchor.MiddleLeft);
            Ui.Stretch(text.rectTransform, 0, 0, 70, 0);
            var on = get();
            var knob = Ui.Img(row.rectTransform, "Switch", Art.Sprite(on ? "switch-on" : "switch-off"));
            knob.raycastTarget = false;
            Ui.Box(knob.rectTransform, A.MidRight, A.MidRight, Vector2.zero, new Vector2(54, 30.6f));
            PressButton.On(row.gameObject, () =>
            {
                on = !on; set(on);
                knob.sprite = Art.Sprite(on ? "switch-on" : "switch-off");
            }, .98f);
        }

        /// A thin ink line between two rows.
        static void Line(RectTransform card, float top, float inner)
        {
            var line = Ui.Img(card, "Line", ProceduralArt.White);
            line.color = new Color(Kit.Line.r, Kit.Line.g, Kit.Line.b, .3f); line.raycastTarget = false;
            Ui.TopRow(line.rectTransform, Pad, Pad, top, 1);
        }
    }
}

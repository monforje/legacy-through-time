using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The gear in the top-right corner and the toast it drops under it: the sound switch, (in a story) "В меню" and
    /// "Выйти из игры". A tap outside the card closes it; nothing under the shade gets the tap.
    sealed class SettingsToast
    {
        const float Width = 300, Pad = 20, RowHeight = 48, Gap = 10, PadTop = 22, PadBottom = 26, TitleHeight = 28;

        readonly RectTransform layer;
        readonly IAnimator anim;
        RectTransform root;
        CanvasGroup group;
        Image soundIcon, soundSwitch;

        public bool IsOpen => root;

        public SettingsToast(RectTransform overlay, IAnimator anim)
        {
            layer = overlay; this.anim = anim;
        }

        static float GearTop => Metrics.InsetTop + 16 + Viewport.ExtraTop;

        /// The round gear seal in the top-right corner (the balance pill of a choice stands to its left).
        public static Image Gear(Transform column, Action click)
        {
            var gear = Ui.Icon(column, "Gear", "gear", A.TopRight, A.TopRight, new Vector2(-(Metrics.Side - 4), -GearTop), Metrics.GearSize);
            gear.raycastTarget = true;
            Ui.Lift(gear, 4, .3f);
            PressButton.On(gear.gameObject, click, .9f);
            return gear;
        }

        /// `toMenu`: the "В меню" row; null in the menu itself.
        public void Open(Action toMenu, Action quit)
        {
            if (IsOpen) return;
            root = Ui.Rect(layer, "Settings");
            Ui.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            var shade = Ui.Img(root, "Shade", ProceduralArt.White);
            shade.color = new Color(0, 0, 0, .4f); shade.raycastTarget = true;
            Ui.Stretch(shade.rectTransform);
            shade.gameObject.AddComponent<Tap>().Click = Close;

            // the card hangs under the gear, in the 360-wide column (the shade covers the whole canvas)
            var column = Ui.Rect(root, "Column");
            column.anchorMin = A.BottomCenter; column.anchorMax = A.TopCenter; column.pivot = A.Center;
            column.offsetMin = new Vector2(-Metrics.RefWidth / 2, 0); column.offsetMax = new Vector2(Metrics.RefWidth / 2, 0);

            var rows = toMenu != null ? 3 : 2;
            var height = PadTop + TitleHeight + rows * (Gap + RowHeight) + PadBottom;
            var card = Ui.Sliced(column, "Card", "card");
            card.raycastTarget = true;                         // a tap on the card itself does not close it
            Ui.Box(card.rectTransform, A.TopRight, A.TopRight, new Vector2(-(Metrics.Side - 8), -(GearTop + Metrics.GearSize + 2)), new Vector2(Width, height));
            var panel = card.rectTransform;

            var title = Ui.Txt(panel, "Title", "Настройки", Art.Head, 20, Theme.Ink, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, Pad, Pad, PadTop, TitleHeight);

            var y = PadTop + TitleHeight + Gap;
            BuildSoundRow(panel, y); y += RowHeight + Gap;
            if (toMenu != null) { BuildButtonRow(panel, y, "Menu", "home", "В меню", "btn", toMenu); y += RowHeight + Gap; }
            BuildButtonRow(panel, y, "Quit", "exit", "Выйти из игры", "btn-chosen", quit);

            if (!anim.Enabled) return;
            group.alpha = 0;
            var y0 = panel.anchoredPosition.y;
            anim.Play(Tween.To(.32f, e =>
            {
                if (!panel) return;
                group.alpha = Mathf.Clamp01(e * 1.6f);
                panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, y0 + 24 * (1 - e));
            }, Easing.OutBack));
        }

        public void Close()
        {
            if (!IsOpen) return;
            var closing = root; var g = group;
            root = null;
            g.blocksRaycasts = false;
            if (!anim.Enabled) { UnityEngine.Object.Destroy(closing.gameObject); return; }
            anim.Play(Tween.To(.18f, e => { if (g) g.alpha = 1 - e; }, Easing.Linear));
            anim.Play(Tween.After(.2f, () => { if (closing) UnityEngine.Object.Destroy(closing.gameObject); }));
        }

        void BuildSoundRow(RectTransform panel, float top)
        {
            var row = Ui.Img(panel, "Sound", ProceduralArt.White);
            row.color = Color.clear; row.raycastTarget = true;
            Ui.TopRow(row.rectTransform, Pad, Pad, top, RowHeight);

            soundIcon = Ui.Icon(row.rectTransform, "Icon", "sound", A.MidLeft, A.MidLeft, new Vector2(4, 0), 28, Theme.Ink);
            var label = Ui.Txt(row.rectTransform, "Text", "Звук", Art.Action, Metrics.FsAction, Theme.Ink);
            Ui.Box(label.rectTransform, A.MidLeft, A.MidLeft, new Vector2(42, 0), new Vector2(120, 30));
            soundSwitch = Ui.Icon(row.rectTransform, "Switch", "switch-on", A.MidRight, A.MidRight, new Vector2(-2, -1), 60);
            Sync();
            PressButton.On(row.gameObject, () => { Settings.Sound = !Settings.Sound; Sync(); }, .98f);
        }

        void Sync()
        {
            var on = Settings.Sound;
            soundIcon.sprite = Art.Sprite(on ? "sound" : "sound-off");
            soundSwitch.sprite = Art.Sprite(on ? "switch-on" : "switch-off");
        }

        /// A full-width button: emerald ("btn") or paper with ink text ("btn-chosen", the quieter one).
        void BuildButtonRow(RectTransform panel, float top, string name, string icon, string text, string sprite, Action click)
        {
            var light = sprite == "btn-chosen";
            var colour = light ? Theme.Ink : Theme.Parchment;
            var button = Ui.Sliced(panel, name, sprite);
            Ui.TopRow(button.rectTransform, Pad - 6, Pad - 6, top, RowHeight);
            Ui.Icon(button.rectTransform, "Icon", icon, A.MidLeft, A.MidLeft, new Vector2(18, 1), 24, colour);
            var label = Ui.Txt(button.rectTransform, "Text", text, Art.Action, Metrics.FsAction, colour);
            if (!light) Ui.Emboss(label);
            Ui.Box(label.rectTransform, A.MidLeft, A.MidLeft, new Vector2(52, 1), new Vector2(180, 30));
            Ui.Icon(button.rectTransform, "Chevron", "chevron", A.MidRight, A.MidRight, new Vector2(-18, 0), 12, colour);
            PressButton.On(button.gameObject, () => { Close(); click(); }, .97f, button, "btn-pressed");
        }
    }
}

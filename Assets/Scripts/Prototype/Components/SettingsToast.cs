using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The gear in the top-left corner and the toast it drops: the sound switch and (in a story) "В меню".
    /// A tap outside the card closes it; nothing under the shade gets the tap.
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

        /// The round gear seal, at the height of the balance pill on the other side.
        public static Image Gear(Transform column, Action click)
        {
            var gear = Ui.Icon(column, "Gear", "gear", A.TopLeft, A.TopLeft,
                               new Vector2(Metrics.Side - 4, -(Metrics.InsetTop + 16 + Viewport.ExtraTop)), 44);
            gear.raycastTarget = true;
            Ui.Lift(gear, 4, .3f);
            PressButton.On(gear.gameObject, click, .9f);
            return gear;
        }

        /// `toMenu`: the "В меню" row; null in the menu itself.
        public void Open(Action toMenu)
        {
            if (IsOpen) return;
            root = Ui.Rect(layer, "Settings");
            Ui.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            var shade = Ui.Img(root, "Shade", ProceduralArt.White);
            shade.color = new Color(0, 0, 0, .4f); shade.raycastTarget = true;
            Ui.Stretch(shade.rectTransform);
            shade.gameObject.AddComponent<Tap>().Click = Close;

            var height = PadTop + TitleHeight + Gap + RowHeight + (toMenu != null ? Gap + RowHeight : 0) + PadBottom;
            var card = Ui.Sliced(root, "Card", "card");
            card.raycastTarget = true;                         // a tap on the card itself does not close it
            Ui.Box(card.rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -(Metrics.InsetTop + 8 + Viewport.ExtraTop)), new Vector2(Width, height));
            var panel = card.rectTransform;

            var title = Ui.Txt(panel, "Title", "Настройки", Art.Head, 20, Theme.Ink, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, Pad, Pad, PadTop, TitleHeight);

            var y = PadTop + TitleHeight + Gap;
            BuildSoundRow(panel, y);
            if (toMenu != null) BuildMenuRow(panel, y + RowHeight + Gap, toMenu);

            if (!anim.Enabled) return;
            group.alpha = 0;
            var y0 = panel.anchoredPosition.y;
            anim.Play(Tween.To(.32f, e =>
            {
                if (!panel) return;
                group.alpha = Mathf.Clamp01(e * 1.6f);
                panel.anchoredPosition = new Vector2(0, y0 + 24 * (1 - e));
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

        void BuildMenuRow(RectTransform panel, float top, Action toMenu)
        {
            var button = Ui.Sliced(panel, "Menu", "btn");
            Ui.TopRow(button.rectTransform, Pad - 6, Pad - 6, top, RowHeight);
            Ui.Icon(button.rectTransform, "Icon", "home", A.MidLeft, A.MidLeft, new Vector2(18, 1), 24, Theme.Parchment);
            var label = Ui.Txt(button.rectTransform, "Text", "В меню", Art.Action, Metrics.FsAction, Theme.Parchment);
            Ui.Emboss(label);
            Ui.Box(label.rectTransform, A.MidLeft, A.MidLeft, new Vector2(52, 1), new Vector2(160, 30));
            Ui.Icon(button.rectTransform, "Chevron", "chevron", A.MidRight, A.MidRight, new Vector2(-18, 0), 12, Theme.Parchment);
            PressButton.On(button.gameObject, () => { Close(); toMenu(); }, .97f, button, "btn-pressed");
        }
    }
}

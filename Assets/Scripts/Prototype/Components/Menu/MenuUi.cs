using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Small builders shared by the screens of the menu.
    static class MenuUi
    {
        public static readonly Color Gold = Theme.Hex("E8C88A");
        public static readonly Color Dim = new(.22f, .2f, .2f, 1);         // a locked card: its colours under a dark veil

        /// The header of a section (title, line, ornament) takes this much; its list starts under it.
        public const float HeaderHeight = 100;

        /// The title of a section ("Библиотека", "Магазин", the name of a story) and the line under it; `side` keeps
        /// room for a back arrow.
        public static RectTransform Title(RectTransform parent, string text, string sub, float top, float side = 24, int size = 30)
        {
            var title = Ui.Txt(parent, "Title", text, Art.Accent, size, Theme.Parchment, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, side, side, top, 40);
            Shadow(title);
            if (sub != null)
            {
                var line = Ui.Txt(parent, "Sub", sub, Art.Sub, 13, Gold, TextAnchor.MiddleCenter);
                Ui.TopRow(line.rectTransform, 24, 24, top + 40, 20);
                Shadow(line);
            }
            Ui.Icon(parent, "Orn", "title-orn", A.TopCenter, A.TopCenter, new Vector2(0, -(top + (sub != null ? 62 : 42))), 56);
            return title.rectTransform;
        }

        /// A one-line label that shrinks (down to `min`) instead of running out of its box: names, buttons, prices.
        public static Text Fit(Text t, int min)
        {
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = min;
            t.resizeTextMaxSize = t.fontSize;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }

        /// A page of its own (the shop, the library): the painting of the fire is covered by a calm ground, so that
        /// nothing in it competes with what is on the page. Returns the page; its content starts `PageInset` in.
        public static RectTransform Page(RectTransform parent)
        {
            var ground = Ui.Img(parent, "Ground", ProceduralArt.White);
            ground.color = Theme.Hex("0A1611");
            ground.raycastTarget = true;                          // nothing under the page takes a tap
            Ui.Bleed(ground.rectTransform);
            var page = Ui.Sliced(parent, "Page", "page");
            Ui.Stretch(page.rectTransform, 10, 6, 10, Metrics.HudBottom + Viewport.ExtraTop - 6);
            Ui.Lift(page, 8, .5f);
            return page.rectTransform;
        }

        public const float PageInset = 18;

        public static void Shadow(Text t, float alpha = .6f)
        {
            var s = t.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0, 0, 0, alpha); s.effectDistance = new Vector2(0, -2);
        }

        /// A vertical scroll list from `top` to `bottom` (distances from the edges of the parent); returns the content
        /// that grows down from its top (set its height with SetHeight).
        public static RectTransform List(RectTransform parent, float top, float bottom, float inset = -1)
        {
            var viewport = Ui.Img(parent, "List", ProceduralArt.White);
            viewport.color = Color.clear; viewport.raycastTarget = true;
            Ui.Stretch(viewport.rectTransform, 0, bottom, 0, top);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Ui.Rect(viewport.rectTransform, "Content");
            content.anchorMin = A.TopLeft; content.anchorMax = A.TopRight; content.pivot = A.TopCenter;
            if (inset < 0) inset = Metrics.Side;
            content.offsetMin = new Vector2(inset, 0); content.offsetMax = new Vector2(-inset, 0);
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform; scroll.content = content;
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.decelerationRate = .12f;
            return content;
        }

        public static void SetHeight(RectTransform content, float height) =>
            content.sizeDelta = new Vector2(content.sizeDelta.x, height);

        /// A veil over the whole screen (bleeds past the 360-wide column).
        public static Image Veil(RectTransform parent, float alpha, Action tap = null)
        {
            var veil = Ui.Img(parent, "Veil", ProceduralArt.White);
            veil.color = new Color(.02f, .04f, .035f, alpha);
            Ui.Bleed(veil.rectTransform);
            if (tap != null) { veil.raycastTarget = true; veil.gameObject.AddComponent<Tap>().Click = tap; }
            return veil;
        }

        /// A coin and a number on the dark leather pill: the purse, top left of the menu; a tap opens the shop.
        public static Text Purse(RectTransform parent, Action open)
        {
            var pill = Ui.Img(parent, "Purse", Art.Sprite("pill"));
            pill.type = Art.Tiled("pill") ? Image.Type.Tiled : Image.Type.Sliced;
            pill.raycastTarget = true;
            Ui.Box(pill.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Metrics.Side - 4, -(Metrics.InsetTop + 16 + Viewport.ExtraTop)), new Vector2(112, 44));
            Ui.Lift(pill, 4, .3f);
            Ui.Icon(pill.rectTransform, "Coin", "coin", A.MidLeft, A.MidLeft, new Vector2(7, 0), 30);
            var number = Ui.Txt(pill.rectTransform, "Coins", Wallet.Coins.ToString(), Art.Sub, 18, Theme.Parchment, TextAnchor.MiddleLeft);
            Ui.Emboss(number);
            Ui.Box(number.rectTransform, A.MidLeft, A.MidLeft, new Vector2(42, 1), new Vector2(44, 30));
            Ui.Icon(pill.rectTransform, "Plus", "plus", A.MidRight, A.MidRight, new Vector2(-6, 0), 26);
            PressButton.On(pill.gameObject, open, .95f);
            return number;
        }

        /// A main button of the menu: leather with a label (and a chevron), returns its image.
        public static Image Button(RectTransform parent, string name, string text, Action click, string sprite = "btn", string icon = null)
        {
            var button = Ui.Sliced(parent, name, sprite);
            var light = sprite == "btn-chosen";
            var colour = light ? Theme.Ink : Theme.Parchment;
            var label = Ui.Txt(button.rectTransform, "Text", text, Art.Action, Metrics.FsAction, colour, TextAnchor.MiddleCenter);
            if (!light) Ui.Emboss(label);
            Ui.Stretch(label.rectTransform, icon != null ? 34 : 12, 4, 12, 2);
            Fit(label, 11);                                       // a long price ("1 290,00 ₽") shrinks, never runs out
            if (icon != null) Ui.Icon(button.rectTransform, "Icon", icon, A.MidLeft, A.MidLeft, new Vector2(16, 1), 22, colour);
            PressButton.On(button.gameObject, click, .97f, button, light ? null : "btn-pressed");
            return button;
        }
    }
}

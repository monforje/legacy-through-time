using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The bar at the bottom of the menu: stories, chats, library, shop. The tab that is open is gold, with a ruby under it.
    sealed class TabBar
    {
        public const float Height = 66;
        static readonly (string icon, string label)[] Tabs = { ("tab-stories", "Истории"), ("bubble", "Чат"), ("tab-library", "Библиотека"), ("tab-shop", "Магазин") };

        public readonly RectTransform Root;
        readonly Image[] icons = new Image[Tabs.Length];
        readonly Text[] labels = new Text[Tabs.Length];
        readonly Image[] pips = new Image[Tabs.Length];

        public TabBar(RectTransform parent, Action<MenuScreen.Tab> select)
        {
            Root = Ui.Rect(parent, "Tabs");
            var bottom = Metrics.InsetBottom + Viewport.ExtraBottom;
            var side = (Viewport.Width - Metrics.RefWidth) / 2;
            Root.anchorMin = A.BottomLeft; Root.anchorMax = A.BottomRight; Root.pivot = A.BottomCenter;
            Root.offsetMin = new Vector2(-side, 0); Root.offsetMax = new Vector2(side, Height + bottom);
            var bar = Ui.Sliced(Root, "Bar", "tabbar");
            bar.raycastTarget = true;                                // the bar takes the taps under it
            Ui.Stretch(bar.rectTransform);
            Ui.Lift(bar, -4, .35f);

            // the icons and labels sit in the middle of the bar as it is seen: below its gilt rim, down to the edge
            const float rim = 14, block = 52;
            var row = Ui.Rect(Root, "Row");
            row.anchorMin = A.BottomCenter; row.anchorMax = A.BottomCenter; row.pivot = A.BottomCenter;
            row.anchoredPosition = new Vector2(0, (Height + bottom - rim - block) / 2); row.sizeDelta = new Vector2(Metrics.RefWidth, block);
            for (var i = 0; i < Tabs.Length; i++)
            {
                var k = i;
                var cell = Ui.Img(row, Tabs[i].label, ProceduralArt.White);
                cell.color = Color.clear; cell.raycastTarget = true;
                Ui.Box(cell.rectTransform, A.BottomLeft, A.BottomLeft, new Vector2(Metrics.RefWidth / Tabs.Length * i, 0), new Vector2(Metrics.RefWidth / Tabs.Length, block));
                icons[i] = Ui.Icon(cell.rectTransform, "Icon", Tabs[i].icon, A.TopCenter, A.TopCenter, Vector2.zero, 30);
                labels[i] = MenuUi.Fit(Ui.Txt(cell.rectTransform, "Label", Tabs[i].label, Art.Sub, 12, Theme.Parchment, TextAnchor.MiddleCenter), 9);
                Ui.Box(labels[i].rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -32), new Vector2(Metrics.RefWidth / Tabs.Length - 8, 16));
                pips[i] = Ui.Icon(cell.rectTransform, "Pip", "dot-on", A.BottomCenter, A.Center, new Vector2(0, 1), 9);
                PressButton.On(cell.gameObject, () => select((MenuScreen.Tab)k), .94f);
            }
        }

        public void Show(MenuScreen.Tab tab)
        {
            for (var i = 0; i < icons.Length; i++)
            {
                var on = i == (int)tab;
                icons[i].color = on ? MenuUi.Gold : Theme.Parchment.WithAlpha(.7f);
                labels[i].color = on ? MenuUi.Gold : Theme.Parchment.WithAlpha(.7f);
                pips[i].enabled = on;
            }
        }
    }
}

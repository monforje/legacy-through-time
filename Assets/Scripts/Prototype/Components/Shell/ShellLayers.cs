using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The bottom bar of the shell: five sections with 3D icons (kit.html, "Нижняя панель"). 56 px high, full width, flush
    /// to the bottom edge; the gesture zone is part of the bar. The current section is lit, the others are muted.
    sealed class ShellNav
    {
        public readonly RectTransform Root;
        readonly Image[] cells, icons;
        readonly Text[] labels;
        readonly Image[] badges;
        readonly Text[] badgeText;
        int shown = -1;

        public ShellNav(Transform parent, (string pic, string label)[] items, int selected, Action<int> select)
        {
            var bg = Ui.Img(parent, "Nav", ProceduralArt.White);
            bg.color = Kit.Surface; bg.raycastTarget = true;
            Root = bg.rectTransform;
            Ui.BottomRow(Root, 0, 0, 0, Kit.NavHeight + Viewport.ExtraBottom);
            var row = Ui.Rect(Root, "Row");
            Ui.Stretch(row, Kit.Space2, Metrics.InsetBottom - Kit.Space4 + Viewport.ExtraBottom, Kit.Space2, Kit.Space1);

            cells = new Image[items.Length]; icons = new Image[items.Length]; labels = new Text[items.Length];
            badges = new Image[items.Length]; badgeText = new Text[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var k = i;
                var cell = Kit.Round(row, "Tab " + items[i].label, Color.clear, Kit.RadiusS);
                cell.raycastTarget = true;
                cell.rectTransform.anchorMin = new Vector2((float)i / items.Length, 0);
                cell.rectTransform.anchorMax = new Vector2((float)(i + 1) / items.Length, 1);
                cell.rectTransform.offsetMin = new Vector2(2, 0); cell.rectTransform.offsetMax = new Vector2(-2, 0);
                cells[i] = cell;
                icons[i] = Kit.Icon(cell.rectTransform, "Icon", Kit.Pic(items[i].pic), 28, A.TopCenter, new Vector2(0, -2));
                icons[i].raycastTarget = false;
                labels[i] = Ui.Txt(cell.rectTransform, "Label", items[i].label, Art.Sub, Kit.FsMicro, Kit.Muted, TextAnchor.LowerCenter);
                Ui.BottomRow(labels[i].rectTransform, 0, 0, 2, 14);
                badges[i] = Kit.Round(cell.rectTransform, "Badge", Kit.Danger, 9);
                badges[i].raycastTarget = false;
                Ui.Box(badges[i].rectTransform, A.TopCenter, A.TopCenter, new Vector2(16, 0), new Vector2(26, 18));
                badgeText[i] = Ui.Txt(badges[i].rectTransform, "Text", "", Art.Sub, Kit.FsMicro, Kit.AccentInk, TextAnchor.MiddleCenter);
                Ui.Stretch(badgeText[i].rectTransform);
                badges[i].gameObject.SetActive(false);
                PressButton.On(cell.gameObject, () => select(k), .94f);
            }
            Show(selected);
        }

        public void Show(int index)
        {
            shown = index;
            for (var i = 0; i < cells.Length; i++)
            {
                var on = i == index;
                cells[i].color = on ? Kit.Surface2 : Color.clear;
                labels[i].color = on ? Kit.Accent : Kit.Muted;
                // the other sections are desaturated and dimmer (kit.css: saturate(.35) brightness(.8))
                icons[i].color = on ? Color.white : new Color(.62f, .62f, .7f, 1f);
            }
        }

        public int Shown => shown;

        /// A small counter over the icon ("+3" on the rewards).
        public void SetBadge(int index, string text)
        {
            var has = !string.IsNullOrEmpty(text);
            badges[index].gameObject.SetActive(has);
            badgeText[index].text = text ?? "";
        }
    }

    /// A popup over the dimmed screen (the wheel, the rewards, the settings): a 30% scrim, a panel 16 px from the sides, 44 px from
    /// the top, 36 px from the bottom, a title and a close button; it rises in Kit.Dur.Slow and leaves in Kit.Dur.Fast.
    sealed class ShellDialog
    {
        public readonly RectTransform Root, Panel, Content;
        readonly IAnimator anim;
        readonly Action closed;
        readonly Image dim;
        bool closing;

        public bool IsOpen => Root && !closing;

        public ShellDialog(Transform parent, string title, IAnimator anim, Action closed)
        {
            this.anim = anim; this.closed = closed;
            Root = Ui.Rect(parent, "Dialog");
            Ui.Bleed(Root);
            dim = Ui.Img(Root, "Dim", ProceduralArt.White);
            dim.color = Kit.Scrim.WithAlpha(.3f); dim.raycastTarget = true;     // the dim also catches the taps meant for the screen below
            Ui.Bleed(dim.rectTransform);

            var panel = Kit.Round(Root, "Panel", Kit.Bg, Kit.RadiusL);
            panel.raycastTarget = true;
            Panel = panel.rectTransform;
            Ui.Stretch(Panel, Kit.Side, 36 + Viewport.ExtraBottom, Kit.Side, 44 + Viewport.ExtraTop);
            Ui.Group(Panel);

            var head = Ui.Txt(Panel, "Title", title, Art.Sub, Kit.FsTitle, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(head.rectTransform, 56, 56, Kit.Space3, Kit.Touch);
            var close = Parts.IconButton(Panel, "close", Close, 40, circle: true);
            close.Face.color = Color.clear;
            Ui.Box(close.Root, A.TopRight, A.TopRight, new Vector2(-Kit.Space3, -Kit.Space3 - 2), new Vector2(40, 40));
            Content = Ui.Rect(Panel, "Content");
            Ui.Stretch(Content, Kit.Space3, Kit.Space3, Kit.Space3, 64);

            if (anim != null && anim.Enabled)
            {
                var group = Ui.Group(Panel); group.alpha = 0;
                dim.color = Kit.Scrim.WithAlpha(0);
                anim.Play(Tween.To(Kit.Dur.Slow, e =>
                {
                    if (!Panel) return;
                    group.alpha = e;
                    Panel.localScale = Vector3.one * (.97f + .03f * e);
                    Panel.anchoredPosition = new Vector2(0, -16 * (1 - e));
                    dim.color = Kit.Scrim.WithAlpha(.3f * e);
                }));
            }
        }

        public void Close()
        {
            if (closing || !Root) return;
            closing = true;
            if (anim == null || !anim.Enabled) { Finish(); return; }
            var group = Ui.Group(Panel);
            anim.Play(Tween.To(Kit.Dur.Fast, e =>
            {
                if (!Root) return;
                group.alpha = 1 - e; dim.color = Kit.Scrim.WithAlpha(.3f * (1 - e));
                if (e >= 1) Finish();
            }, Easing.Linear));
        }

        void Finish() { if (Root) UnityEngine.Object.Destroy(Root.gameObject); closed?.Invoke(); }
    }

    /// A line of text that rises over the bottom bar and leaves by itself: "Сохранено", "Награда получена: 3 алмаза".
    static class ShellToast
    {
        public static void Show(Transform parent, string text, IAnimator anim, float seconds = 2.5f, float bottom = 72)
        {
            var box = Kit.Round(parent, "Toast", Kit.Surface2, Kit.RadiusM);
            box.raycastTarget = false;
            Ui.BottomRow(box.rectTransform, Kit.Side, Kit.Side, bottom + Viewport.ExtraBottom, 48);
            var label = Ui.Txt(box.rectTransform, "Text", text, Art.Sub, Kit.FsSmall, Kit.Text, TextAnchor.MiddleCenter);
            Ui.Stretch(label.rectTransform, Kit.Space4, 0, Kit.Space4, 0);
            var group = Ui.Group(box.rectTransform);
            if (anim == null || !anim.Enabled)
            {
                UnityEngine.Object.Destroy(box.gameObject, seconds);
                return;
            }
            group.alpha = 0;
            var rt = box.rectTransform; var y0 = rt.anchoredPosition.y;
            anim.Play(Tween.To(Kit.Dur.Normal, e => { if (rt) { group.alpha = e; rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0 - 12 * (1 - e)); } }));
            anim.Play(Tween.To(Kit.Dur.Fast, e =>
            {
                if (!rt) return;
                group.alpha = 1 - e; rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0 - 8 * e);
                if (e >= 1) UnityEngine.Object.Destroy(rt.gameObject);
            }, Easing.Linear, Kit.Dur.Normal + seconds));
        }
    }

    /// Four-point sparks that fly out of a point and fade: a reward received, a prize of the wheel, a purchase.
    static class ShellSparks
    {
        public static void Burst(RectTransform parent, Vector2 at, IAnimator anim, int count = 5, float radius = 44)
        {
            if (anim == null || !anim.Enabled) return;
            for (var i = 0; i < count; i++)
            {
                var spark = Ui.Img(parent, "Spark", Kit.Pic("sparkle"));
                spark.raycastTarget = false; spark.preserveAspect = true;
                var rt = spark.rectTransform;
                Ui.Box(rt, A.BottomLeft, A.Center, at, new Vector2(24, 24));
                var angle = Mathf.PI * 2f * i / count - Mathf.PI / 2f;
                var to = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                anim.Play(Tween.To(Kit.Dur.Slow * 2.2f, e =>
                {
                    if (!rt) return;
                    rt.anchoredPosition = at + to * e;
                    rt.localScale = Vector3.one * (.2f + .8f * e);
                    rt.localRotation = Quaternion.Euler(0, 0, 60 * e);
                    spark.color = new Color(1, 1, 1, e < .25f ? e / .25f : 1 - (e - .25f) / .75f);
                    if (e >= 1) UnityEngine.Object.Destroy(rt.gameObject);
                }, Easing.OutExpo));
            }
        }
    }
}

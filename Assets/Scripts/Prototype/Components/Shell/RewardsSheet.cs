using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The sheet over the bottom bar that the "Награды" tab opens (templates/shell/home.html, `.sheet`): "Смотреть рекламу" with
    /// its timer and the "+3" flag, "Награды" (the popup with all the rewards) and "Распродажа" (nothing on sale yet). A tap
    /// outside the sheet closes it. The ad reward waits for its timer, then a tap takes it.
    sealed class RewardsSheet
    {
        const float RowHeight = 52, Pad = Kit.Space2;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly Rewards.Kind ad = Rewards.Find("ad");
        readonly Text adLine;
        readonly RectTransform adRow;
        readonly CanvasGroup adGroup;
        readonly PressButton adPress;
        bool closing;

        public bool IsOpen => Root && !closing;

        public RewardsSheet(Transform parent, IAnimator anim, Action openRewards, Action closed)
        {
            this.anim = anim;
            Root = Ui.Rect(parent, "Rewards sheet");
            Ui.Stretch(Root);
            var catcher = Ui.Img(Root, "Catcher", ProceduralArt.White);
            catcher.color = Color.clear; catcher.raycastTarget = true;
            Ui.Bleed(catcher.rectTransform, Kit.Width);
            PressButton.On(catcher.gameObject, () => Close(closed), 1f);

            var height = 3 * RowHeight + 2 * Pad;
            var card = Kit.Round(Root, "Card", Kit.Surface2, Kit.RadiusM);
            card.raycastTarget = true;
            Ui.BottomRow(card.rectTransform, Kit.Side, Kit.Side, 72 + Viewport.ExtraBottom, height);
            Kit.Shadow(card.rectTransform, Kit.RadiusM, 24, .5f, 8);
            var panel = card.rectTransform;

            // 1. the ad
            adRow = Row(panel, 0, "ads", "Смотреть рекламу", "", null, out adLine, out adPress);
            adGroup = Ui.Group(adRow);
            var flag = Kit.Round(adRow, "Flag", Kit.Accent, Kit.RadiusS);
            var amount = Ui.Txt(flag.rectTransform, "Text", "+" + ad.Gems, Kit.F800, Kit.FsSmall, Kit.AccentInk, TextAnchor.MiddleLeft);
            Ui.Box(flag.rectTransform, A.MidRight, A.MidRight, new Vector2(-Kit.Space3, 0), new Vector2(59, 26));
            Ui.Stretch(amount.rectTransform, Kit.Space3, 0, 0, 0);
            Kit.Icon(flag.rectTransform, "Gem", Kit.Pic("gem"), 16, A.MidRight, new Vector2(-Kit.Space3 + 0, 0)).raycastTarget = false;
            adPress.Click = TakeAd;

            // 2. the rewards
            var rewards = Row(panel, 1, "gift", "Награды", "Получите бесплатные ресурсы", () => { Close(closed); openRewards(); }, out _, out _, picture: true);
            Kit.Icon(rewards, "Chevron", Kit.Glyph("chevron-r"), 22, A.MidRight, new Vector2(-Kit.Space3, 0)).raycastTarget = false;

            // 3. the sale
            var sale = Row(panel, 2, "cart", "Распродажа", "Сейчас распродаж нет", null, out _, out _, picture: true);
            Ui.Group(sale).alpha = .5f;

            Tick();
            if (anim.Enabled)
            {
                var group = Ui.Group(panel); group.alpha = 0;
                var y0 = panel.anchoredPosition.y;
                anim.Play(Tween.To(Kit.Dur.Normal, e => { if (panel) { group.alpha = e; panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, y0 - 12 * (1 - e)); } }));
                anim.Play(Loop());
            }
        }

        /// A row: the glyph (or the 3D picture) at 12, the title and the line from 46, an optional tap.
        RectTransform Row(RectTransform panel, int index, string icon, string title, string line, Action tap, out Text lineText, out PressButton press, bool picture = false)
        {
            var row = Kit.Round(panel, "Row " + title, Color.clear, Kit.RadiusS);
            row.raycastTarget = true;
            Ui.Box(row.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Pad, -(Pad + index * RowHeight)), new Vector2(Kit.Width - 2 * Kit.Side - 2 * Pad, RowHeight));
            var rt = row.rectTransform;
            Kit.Icon(rt, "Icon", picture ? Kit.Pic(icon) : Kit.Glyph(icon), 22, A.MidLeft, new Vector2(Kit.Space3, 0)).raycastTarget = false;
            var head = Ui.Txt(rt, "Title", title, Kit.F700, Kit.FsBody, Kit.Text);
            Ui.TopRow(head.rectTransform, 46, 12, 6, 22);
            lineText = Ui.Txt(rt, "Line", line, Kit.F700, Kit.FsSmall, Kit.Muted);
            Ui.TopRow(lineText.rectTransform, 46, 12, 28, 18);
            press = PressButton.On(row.gameObject, tap ?? (() => { }), .98f, row);
            return rt;
        }

        // ------------------------------------------------------------------ the ad timer
        IEnumerator Loop()
        {
            while (IsOpen)
            {
                yield return new WaitForSecondsRealtime(1f);
                if (IsOpen) Tick();
            }
        }

        void Tick()
        {
            var left = ShellPurse.Remaining(ad, DateTime.Now);
            adLine.text = left > 0 ? Rewards.Clock(left) : "Можно забрать";
            adGroup.alpha = left > 0 ? .5f : 1f;
            adPress.Interactable = left == 0;
        }

        void TakeAd()
        {
            if (!ShellPurse.Take(ad, DateTime.Now)) return;
            ShellToast.Show(Root, "Награда получена: " + ad.Gems + " " + ShopCatalog.Plural(ad.Gems, "алмаз", "алмаза", "алмазов"), anim);
            Tick();
        }

        public void Close(Action closed = null)
        {
            if (closing || !Root) return;
            closing = true;
            if (anim == null || !anim.Enabled) { UnityEngine.Object.Destroy(Root.gameObject); closed?.Invoke(); return; }
            var group = Ui.Group(Root);
            group.blocksRaycasts = false;
            anim.Play(Tween.To(Kit.Dur.Fast, e =>
            {
                if (!Root) return;
                group.alpha = 1 - e;
                if (e >= 1) { UnityEngine.Object.Destroy(Root.gameObject); closed?.Invoke(); }
            }, Easing.Linear));
        }
    }
}

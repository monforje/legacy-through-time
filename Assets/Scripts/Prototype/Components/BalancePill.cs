using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Coins: a coin, the number, and (interactive) a plus that tops the purse up by ten.
    sealed class BalancePill : IDisposable
    {
        public readonly RectTransform Root;
        public readonly CanvasGroup Group;

        readonly Bank bank;
        readonly IAnimator anim;
        readonly Text number;

        BalancePill(Transform parent, string name, float width, int coins, Bank bank, IAnimator anim)
        {
            this.bank = bank; this.anim = anim;
            Root = Ui.Rect(parent, name);
            var img = Root.gameObject.AddComponent<Image>();
            img.sprite = Art.Sprite("pill"); img.type = Image.Type.Sliced; img.raycastTarget = false;
            Ui.Lift(img, 5, .34f);
            Group = Root.gameObject.AddComponent<CanvasGroup>();

            Ui.Icon(Root, "Coin", "coin", A.MidLeft, A.MidLeft, new Vector2(width > 110 ? 12 : 14, 1), 28);
            number = Ui.Txt(Root, "Balance", coins.ToString(), Art.Sub, Metrics.FsValue, Theme.Parchment);
            Ui.Emboss(number);
            Ui.Box(number.rectTransform, A.MidLeft, A.MidLeft, new Vector2(width > 110 ? 48 : 52, 1), new Vector2(width > 110 ? 40 : 46, 30));
        }

        /// A balance with a plus above a panel (the picker). Follows the purse.
        public static BalancePill Interactive(RectTransform panel, Bank bank, Func<Action, Action> guard, IAnimator anim)
        {
            var pill = new BalancePill(panel, "Balance", 130, bank.Coins, bank, anim);
            Ui.Box(pill.Root, A.TopRight, A.BottomRight, new Vector2(-2, 80), new Vector2(130, 48));
            var plus = Ui.Icon(pill.Root, "Plus", "plus", A.MidRight, A.MidRight, new Vector2(-8, 0), 32);
            plus.raycastTarget = true;
            PressButton.On(plus.gameObject, guard(() => bank.Add(10)), .9f);
            bank.Changed += pill.Sync;
            return pill;
        }

        /// A small balance in the corner of the screen (the story shows the coins it keeps itself).
        public static BalancePill Corner(Transform page, int coins)
        {
            var pill = new BalancePill(page, "GemChip", 104, coins, null, NoAnimator.Instance);
            // left of the settings gear in the corner
            Ui.Box(pill.Root, A.TopRight, A.TopRight, new Vector2(-(Metrics.Side + Metrics.GearSize + 6), -(Metrics.InsetTop + 16 + Viewport.ExtraTop)), new Vector2(104, 48));
            return pill;
        }

        void Sync()
        {
            if (!number) return;
            number.text = bank.Coins.ToString();
            anim.Play(Effects.Bump(number.rectTransform));
        }

        public void Dispose() { if (bank != null) bank.Changed -= Sync; }
    }
}

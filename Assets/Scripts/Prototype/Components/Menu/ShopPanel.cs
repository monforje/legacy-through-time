using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The shop, a page of its own over a calm ground (MenuUi.Page): the purse, a free chest every day, packs of coins
    /// for money (Store, Google Play). Each pack is a card of two lines - its name across the card, then the coins
    /// it gives and the price button - so that nothing runs into anything. The prices are the store's own, in the
    /// reader's currency; without the store the packs say so instead of a price.
    sealed class ShopPanel : IDisposable
    {
        const float GiftHeight = 112, PackHeight = 100, Gap = 12;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly List<(ShopCatalog.Pack pack, Text price, Image button)> packs = new();
        Text balance, giftLine, message;
        Image giftButton;

        public ShopPanel(RectTransform parent, IAnimator anim)
        {
            this.anim = anim;
            Root = Ui.Rect(parent, "Shop");
            Ui.Stretch(Root);
            var page = MenuUi.Page(Root);

            var title = Ui.Txt(page, "Title", "Магазин", Art.Accent, 28, Theme.Parchment, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, 30, 30, 20, 36);
            MenuUi.Shadow(title);
            // the purse on a plaque under the title
            var purse = Ui.Sliced(page, "Purse", "label");
            Ui.Box(purse.rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -60), new Vector2(170, 38));
            Ui.Icon(purse.rectTransform, "Coin", "coin", A.MidLeft, A.MidLeft, new Vector2(14, 0), 26);
            balance = MenuUi.Fit(Ui.Txt(purse.rectTransform, "Coins", "", Art.Sub, 16, Theme.Parchment, TextAnchor.MiddleCenter), 11);
            Ui.Stretch(balance.rectTransform, 44, 2, 14, 2);
            Ui.Emboss(balance);

            var list = MenuUi.List(page, 110, MenuUi.PageInset);
            list.offsetMin = new Vector2(MenuUi.PageInset, 0); list.offsetMax = new Vector2(-MenuUi.PageInset, 0);
            var y = 4f;
            Gift(list, y); y += GiftHeight + Gap;
            foreach (var pack in ShopCatalog.Packs) { Pack(list, pack, y); y += PackHeight + Gap; }
            message = MenuUi.Fit(Ui.Txt(list, "Message", "", Art.Sub, 14, MenuUi.Gold, TextAnchor.MiddleCenter), 11);
            Ui.TopRow(message.rectTransform, 0, 0, y, 22);
            var note = Ui.Txt(list, "Note", "Покупки — через Google Play", Art.Body, 13, Theme.Parchment.WithAlpha(.6f), TextAnchor.MiddleCenter);
            Ui.TopRow(note.rectTransform, 0, 0, y + 24, 20);
            MenuUi.SetHeight(list, y + 56);

            Wallet.Changed += OnCoins;
            Store.Changed += Sync;
            Store.Finished += OnFinished;
            Sync();
        }

        public void Dispose()
        {
            Wallet.Changed -= OnCoins;
            Store.Changed -= Sync;
            Store.Finished -= OnFinished;
        }

        RectTransform Card(RectTransform list, string name, float top, float height)
        {
            var card = Ui.Sliced(list, name, "card");
            Ui.TopRow(card.rectTransform, 0, 0, top, height);
            Ui.Lift(card, 6, .5f);
            return card.rectTransform;
        }

        void Gift(RectTransform list, float top)
        {
            var rt = Card(list, "Gift", top, GiftHeight);
            Ui.Icon(rt, "Chest", "chest", A.MidLeft, A.MidLeft, new Vector2(14, 0), 66);
            var title = MenuUi.Fit(Ui.Txt(rt, "Title", "Подарок дня", Art.Accent, 20, Theme.Ink), 14);
            Ui.TopRow(title.rectTransform, 92, 16, 18, 28);
            giftLine = MenuUi.Fit(Ui.Txt(rt, "Line", "", Art.Body, 14, Theme.Ink.WithAlpha(.75f)), 10);
            Ui.TopRow(giftLine.rectTransform, 92, 16, 46, 20);
            giftButton = MenuUi.Button(rt, "Take", "Забрать", TakeGift);
            Ui.Box(giftButton.rectTransform, A.BottomLeft, A.BottomLeft, new Vector2(88, 12), new Vector2(140, 38));
        }

        void Pack(RectTransform list, ShopCatalog.Pack pack, float top)
        {
            var rt = Card(list, "Pack " + pack.Id, top, PackHeight);
            Ui.Icon(rt, "Coins", pack.Icon, A.MidLeft, A.MidLeft, new Vector2(10, 0), 68);
            var title = MenuUi.Fit(Ui.Txt(rt, "Title", pack.Title, Art.Accent, 20, Theme.Ink), 14);
            Ui.TopRow(title.rectTransform, 90, 16, 16, 28);
            var amount = MenuUi.Fit(Ui.Txt(rt, "Amount", $"+{pack.Coins} монет", Art.Sub, 15, Theme.Scarlet), 11);
            Ui.Box(amount.rectTransform, A.BottomLeft, A.BottomLeft, new Vector2(90, 20), new Vector2(110, 24));
            var button = MenuUi.Button(rt, "Buy", "…", () => Buy(pack));
            Ui.Box(button.rectTransform, A.BottomRight, A.BottomRight, new Vector2(-14, 12), new Vector2(104, 40));
            packs.Add((pack, button.GetComponentInChildren<Text>(), button));
        }

        void TakeGift()
        {
            if (!Wallet.TakeGift()) return;
            message.text = $"+{ShopCatalog.DailyCoins} монет из сундука";
            if (anim.Enabled) anim.Play(Effects.Shake(giftButton.rectTransform));
            Sync();
        }

        void Buy(ShopCatalog.Pack pack)
        {
            if (Store.Price(pack.Id) == null) { message.text = "Покупки сейчас недоступны"; return; }
            message.text = "Открываем Google Play…";
            Store.Buy(pack.Id);
        }

        void OnFinished(string id, bool ok, string text)
        {
            if (!Root) return;
            message.text = text;
            Sync();
        }

        void OnCoins(int coins) => Sync();

        void Sync()
        {
            if (!Root) return;
            balance.text = $"{Wallet.Coins} монет";
            var ready = Wallet.GiftReady;
            giftButton.gameObject.SetActive(ready);
            giftLine.text = ready ? $"+{ShopCatalog.DailyCoins} монет каждый день" : "Новый подарок " + Until();
            foreach (var (pack, price, button) in packs)
            {
                var p = Store.Price(pack.Id);
                price.text = p ?? (Store.Status == Store.State.Connecting ? "…" : "Скоро");
                button.color = p != null ? Color.white : new Color(.7f, .7f, .7f, 1);
            }
        }

        static string Until()
        {
            var t = ShopCatalog.UntilNextGift(DateTime.Now);
            return t.Hours > 0 ? $"через {t.Hours} ч {t.Minutes} мин" : $"через {Math.Max(1, t.Minutes)} мин";
        }
    }
}

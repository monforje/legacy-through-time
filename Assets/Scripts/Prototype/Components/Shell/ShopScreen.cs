using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The shop of the shell (templates/shell/shop.html): the pills of energy and gems on top, the starter set, the
    /// "Энергия / Алмазы" switch with the best offer and two small ones under it, and the bar "Назад / Магазин / Покупки".
    /// A price comes from the store; without it the placeholder shows and, in the editor only, a tap pays the offer out
    /// as a stand-in. A purchase: the button loads, the numbers run up, the gift opens, sparks fly.
    sealed class ShopScreen : IDisposable
    {
        const float CardGap = Kit.Space3;
        static float Width => Kit.Width - 2 * Kit.Side;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly RectTransform energyPanel, gemPanel;
        readonly ShellPill energyPill, gemPill;
        readonly RectTransform content;
        readonly Image starterGift;
        readonly List<(ShopCatalog.Offer offer, ShellButton button)> buttons = new();
        bool open = true;

        public ShopScreen(RectTransform parent, IAnimator anim, Action back)
        {
            this.anim = anim;
            Root = Ui.Rect(parent, "Shop");
            Ui.Stretch(Root);
            Kit.Ground(Root);

            var top = Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop;
            var bar = Ui.Rect(Root, "Pills");
            Ui.TopRow(bar, Kit.Side, Kit.Side + 52, top, Kit.Touch);
            energyPill = Parts.Pill(bar, "energy", ShellPurse.Energy, () => Switch(0));
            gemPill = Parts.Pill(bar, "gem", ShellPurse.Gems, () => Switch(1));
            Parts.TwoPills(energyPill, gemPill);

            var list = MenuUi.List(Root, top + Kit.Touch + Kit.Space3, Kit.NavHeight + Viewport.ExtraBottom, 0);
            content = list;
            var y = 0f;

            var starter = ShopCatalog.Offers[0];
            starterGift = WideCard(list, starter, y, 204, out _);
            y += 204 + CardGap;

            var segmented = Parts.Segmented(list, new[] { "Энергия", "Алмазы" }, 0, Switch, anim);
            Ui.TopRow(segmented.Root, Kit.Side, Kit.Side, y, 48);
            this.segmented = segmented;
            y += 48 + CardGap;

            energyPanel = Ui.Rect(list, "Energy offers");
            Ui.TopRow(energyPanel, 0, 0, y, 10);
            gemPanel = Ui.Rect(list, "Gem offers");
            Ui.TopRow(gemPanel, 0, 0, y, 10);
            var h = Panel(energyPanel, ShopCatalog.SectionEnergy);
            Panel(gemPanel, ShopCatalog.SectionGems);
            panelHeight = h;
            note = Ui.Txt(list, "Note", "Покупки проходят через Google Play", Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleCenter);
            noteY = y;
            Switch(0);

            nav = new ShellNav(Root, new[] { ("g/back", "Назад"), ("cart", "Магазин"), ("g/receipt", "Покупки") }, 1, i =>
            {
                if (i == 0) back();
                else if (i == 2) ShellToast.Show(Root, "История покупок появится позже", anim);
            });

            ShellPurse.Changed += Sync;
            Store.Changed += SyncPrices;
            Store.Finished += OnFinished;
            SyncPrices();

            if (anim.Enabled) Rise(list);
        }

        readonly ShellSegmented segmented;
        readonly ShellNav nav;
        readonly Text note;
        readonly float panelHeight, noteY;

        // ------------------------------------------------------------------ the switch of the two sections
        void Switch(int index)
        {
            energyPanel.gameObject.SetActive(index == 0);
            gemPanel.gameObject.SetActive(index == 1);
            segmented.Select(index, true);
            var y = noteY;
            Ui.TopRow(note.rectTransform, Kit.Side, Kit.Side, y + panelHeight + Kit.Space3, 20);
            MenuUi.SetHeight(content, y + panelHeight + Kit.Space3 + 20 + Kit.Space4);
        }

        /// Fills a section: the best offer (wide) and the two small ones; returns its height.
        float Panel(RectTransform panel, string section)
        {
            var y = 0f;
            foreach (var o in ShopCatalog.Offers)
                if (o.Section == section && o.Best) { WideCard(panel, o, y, 164, out _); y += 164 + CardGap; }
            var x = 0;
            foreach (var o in ShopCatalog.Offers)
            {
                if (o.Section != section || o.Best) continue;
                SmallCard(panel, o, Kit.Side + x * ((Width - CardGap) / 2 + CardGap), y);
                x++;
            }
            y += 208;
            MenuUi.SetHeight(panel, y);
            return y;
        }

        // ------------------------------------------------------------------ cards
        /// A wide card: the title across the top, the picture on the left, the amounts and the price on the right.
        Image WideCard(RectTransform parent, ShopCatalog.Offer offer, float y, float height, out ShellButton buy)
        {
            var card = Kit.Round(parent, "Offer " + offer.Id, Kit.Surface2, Kit.RadiusM);
            Ui.Box(card.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -y), new Vector2(Width, height));
            var title = Ui.Txt(card.rectTransform, "Title", offer.Title, Kit.F800, Kit.FsHead, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, 12, 12, 12, 26);
            var art = Kit.Icon(card.rectTransform, "Art", Kit.Pic(offer.Pic), 80, A.MidLeft, new Vector2(Width / 4 - 40 + 6, -14));
            art.raycastTarget = false;
            var rows = (offer.Gems > 0 ? 1 : 0) + (offer.Energy > 0 ? 1 : 0);
            var right = new Vector2(Width - 16 - 126, 0);
            var col = Ui.Rect(card.rectTransform, "Buy");
            Ui.Box(col, A.TopLeft, A.TopLeft, new Vector2(right.x, -48), new Vector2(126, rows * 44 + 8 + 44));
            Amounts(col, offer);
            buy = Price(col, offer, rows * 44 + 8);
            return art;
        }

        void SmallCard(RectTransform parent, ShopCatalog.Offer offer, float x, float y)
        {
            var w = (Width - CardGap) / 2;
            var card = Kit.Round(parent, "Offer " + offer.Id, Kit.Surface2, Kit.RadiusM);
            Ui.Box(card.rectTransform, A.TopLeft, A.TopLeft, new Vector2(x, -y), new Vector2(w, 200));
            var art = Kit.Icon(card.rectTransform, "Art", Kit.Pic(offer.Pic), 80, A.TopCenter, new Vector2(0, -8));
            art.raycastTarget = false;
            var col = Ui.Rect(card.rectTransform, "Buy");
            Ui.Box(col, A.TopCenter, A.TopCenter, new Vector2(0, -96), new Vector2(126, 44 + 8 + 44));
            Amounts(col, offer);
            Price(col, offer, 52);
        }

        /// The block of amounts: one shape, a row for gems and a row for energy, the same on every card.
        static void Amounts(RectTransform col, ShopCatalog.Offer offer)
        {
            var rows = (offer.Gems > 0 ? 1 : 0) + (offer.Energy > 0 ? 1 : 0);
            var block = Kit.Round(col, "Amounts", Kit.Surface, Kit.RadiusS);
            Ui.TopRow(block.rectTransform, 0, 0, 0, rows * 44);
            var i = 0;
            void Row(string pic, int n)
            {
                var row = Ui.Rect(block.rectTransform, "Row " + pic);
                Ui.TopRow(row, 0, 0, i * 44, 44);
                var icon = Kit.Icon(row, "Icon", Kit.Pic(pic), 28, A.Center, new Vector2(-26, 0));
                icon.raycastTarget = false;
                var t = Ui.Txt(row, "Number", n.ToString(), Kit.F800, Kit.FsNum, Kit.Text, TextAnchor.MiddleLeft);
                Ui.Box(t.rectTransform, A.Center, A.MidLeft, new Vector2(-8, 0), new Vector2(70, 30));
                if (i > 0)
                {
                    var line = Ui.Img(row, "Line", ProceduralArt.White);
                    line.color = Kit.Surface2; line.raycastTarget = false;
                    Ui.TopRow(line.rectTransform, 0, 0, 0, 1);
                }
                i++;
            }
            if (offer.Gems > 0) Row("gem", offer.Gems);
            if (offer.Energy > 0) Row("energy", offer.Energy);
        }

        ShellButton Price(RectTransform col, ShopCatalog.Offer offer, float top)
        {
            var button = Parts.Button(col, offer.Placeholder, ButtonStyle.Primary, () => Buy(offer), 44);
            Ui.TopRow(button.Root, 0, 0, top, 44);
            button.Label.fontSize = Kit.FsBody;
            buttons.Add((offer, button));
            return button;
        }

        // ------------------------------------------------------------------ buying
        void Buy(ShopCatalog.Offer offer)
        {
            var button = buttons.Find(b => b.offer == offer).button;
            if (Store.Price(offer.Id) != null)
            {
                button.SetLoading(true, anim);
                Store.Buy(offer.Id);                       // the answer comes to OnFinished
                return;
            }
            if (!Application.isEditor) { ShellToast.Show(Root, "Покупки сейчас недоступны", anim); return; }
            // the editor has no store: the stand-in waits as long as the store would and pays the offer out
            button.SetLoading(true, anim);
            anim.Play(Tween.After(anim.Enabled ? Kit.Dur.Net : 0, () =>
            {
                if (!Root) return;
                ShellPurse.Grant(offer);
                Paid(offer, button);
            }));
        }

        void OnFinished(string id, bool ok, string text)
        {
            if (!Root || id == null) return;
            var found = buttons.Find(b => b.offer.Id == id);
            if (found.button == null) return;
            found.button.SetLoading(false, anim);
            if (ok) Paid(found.offer, found.button);
            else ShellToast.Show(Root, text, anim);
        }

        void Paid(ShopCatalog.Offer offer, ShellButton button)
        {
            button.SetLoading(false, anim);
            ShellToast.Show(Root, "Зачислено: " + ShopCatalog.Describe(offer), anim);
            var at = button.Root.position;
            ShellSparks.Burst(Root, (Vector2)Root.InverseTransformPoint(at) + Root.rect.size * Root.pivot - Root.rect.size * 0, anim);
            if (offer.Id == "starter" && starterGift) starterGift.sprite = Kit.Pic("gift-open");
        }

        // ------------------------------------------------------------------ numbers and prices
        void Sync()
        {
            if (!Root || !open) return;
            energyPill.Count(ShellPurse.Energy, anim);
            gemPill.Count(ShellPurse.Gems, anim);
        }

        void SyncPrices()
        {
            if (!Root) return;
            foreach (var (offer, button) in buttons)
                button.SetLabel(Store.Price(offer.Id) ?? offer.Placeholder);
        }

        void Rise(RectTransform list)
        {
            var group = Ui.Group(list); group.alpha = 0;
            var y0 = list.anchoredPosition.y;
            anim.Play(Tween.To(Kit.Dur.Slow, e => { if (list) { group.alpha = e; list.anchoredPosition = new Vector2(list.anchoredPosition.x, y0 - 10 * (1 - e)); } }));
        }

        public void Dispose()
        {
            open = false;
            ShellPurse.Changed -= Sync;
            Store.Changed -= SyncPrices;
            Store.Finished -= OnFinished;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The library, a page of its own over a calm ground (MenuUi.Page): the words the stories explain (Glossary), one
    /// to a card. A word the reader has met is open - its name and its whole explanation, the card as tall as the text;
    /// the others are short dark cards with a lock, so that it is seen how much is still to find.
    sealed class LibraryPanel
    {
        const float Gap = 10, LockedHeight = 56, PadX = 18;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        RectTransform detail;

        public bool DetailOpen => detail;

        public LibraryPanel(RectTransform parent, IReadOnlyList<Glossary.Term> terms, IAnimator anim)
        {
            this.anim = anim;
            Root = Ui.Rect(parent, "Library");
            Ui.Stretch(Root);
            var page = MenuUi.Page(Root);
            var open = 0;
            foreach (var t in terms) if (Library.IsOpen(t.Name)) open++;

            var title = Ui.Txt(page, "Title", "Библиотека", Art.Accent, 28, Theme.Parchment, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, 30, 30, 20, 36);
            MenuUi.Shadow(title);
            var line = MenuUi.Fit(Ui.Txt(page, "Sub", $"Открыто понятий: {open} из {terms.Count}", Art.Sub, 14, MenuUi.Gold, TextAnchor.MiddleCenter), 11);
            Ui.TopRow(line.rectTransform, 30, 30, 58, 20);
            Ui.Icon(page, "Orn", "title-orn", A.TopCenter, A.TopCenter, new Vector2(0, -80), 56);

            var list = MenuUi.List(page, 110, MenuUi.PageInset);
            list.offsetMin = new Vector2(MenuUi.PageInset, 0); list.offsetMax = new Vector2(-MenuUi.PageInset, 0);
            var width = Metrics.RefWidth - 20 - 2 * MenuUi.PageInset;
            var y = 4f;
            foreach (var term in terms)
                y += (Library.IsOpen(term.Name) ? Open(list, term, y, width) : Locked(list, term, y)) + Gap;
            if (terms.Count == 0)
            {
                var empty = Ui.Txt(list, "Empty", "Слова, которые объясняют истории, будут собираться здесь.", Art.Body, 16, Theme.Parchment, TextAnchor.MiddleCenter);
                Ui.TopRow(empty.rectTransform, 10, 10, 20, 60);
                y = 90;
            }
            MenuUi.SetHeight(list, y + 16);
        }

        /// A word met in a story: the name, a thin ornament, the whole explanation. Returns its height.
        float Open(RectTransform list, Glossary.Term term, float top, float width)
        {
            var card = Ui.Sliced(list, "Term " + term.Name, "card");
            Ui.Lift(card, 5, .5f);
            var rt = card.rectTransform;
            var name = MenuUi.Fit(Ui.Txt(rt, "Name", term.Name, Art.Accent, 21, Theme.Ink), 14);
            Ui.TopRow(name.rectTransform, PadX + 26, PadX, 16, 28);
            Ui.Icon(rt, "Orn", "orn", A.TopLeft, A.Center, new Vector2(PadX + 10, -30), 22);
            var text = Ui.Txt(rt, "Text", TextRules.NoBreaks(term.Text), Art.Body, 15, Theme.Ink.WithAlpha(.82f), TextAnchor.UpperLeft);
            text.lineSpacing = .92f;
            var textHeight = Ui.TextHeight(text, width - 2 * PadX);
            Ui.TopRow(text.rectTransform, PadX, PadX, 48, textHeight);
            var height = 48 + textHeight + 22;
            Ui.TopRow(rt, 0, 0, top, height);
            return height;
        }

        /// A word still to be found: a short dark card with a lock.
        float Locked(RectTransform list, Glossary.Term term, float top)
        {
            var card = Ui.Sliced(list, "Term " + term.Name, "card");
            card.color = new Color(.46f, .44f, .42f, 1);
            card.raycastTarget = true;
            var rt = card.rectTransform;
            Ui.TopRow(rt, 0, 0, top, LockedHeight);
            Ui.Icon(rt, "Lock", "lock", A.MidLeft, A.MidLeft, new Vector2(PadX, 1), 22, Theme.Ink.WithAlpha(.7f));
            var hint = MenuUi.Fit(Ui.Txt(rt, "Name", "Откроется в одной из историй", Art.Sub, 14, Theme.Ink.WithAlpha(.75f)), 10);
            Ui.Stretch(hint.rectTransform, PadX + 32, 4, PadX, 2);
            PressButton.On(card.gameObject, () => { if (anim.Enabled) anim.Play(Effects.Shake(rt)); }, .98f);
            return LockedHeight;
        }

        /// The word on a card over the page (kept for the tools; the page shows every open word whole).
        public void Show(Glossary.Term term)
        {
            Close();
            detail = Ui.Rect(Root, "Detail");
            Ui.Stretch(detail);
            MenuUi.Veil(detail, .55f, Close);
            var card = Ui.Sliced(detail, "Card", "card");
            card.raycastTarget = true;
            card.gameObject.AddComponent<Tap>().Click = Close;
            var text = Ui.Txt(card.rectTransform, "Text", TextRules.NoBreaks(term.Text), Art.Body, Metrics.FsBody, Theme.Ink, TextAnchor.UpperLeft);
            var height = Ui.TextHeight(text, Metrics.PlateWidth - 56) + 110;
            Ui.Box(card.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(Metrics.PlateWidth, height));
            Ui.Icon(card.rectTransform, "Orn", "title-orn", A.TopCenter, A.Center, new Vector2(0, -22), 56);
            var name = MenuUi.Fit(Ui.Txt(card.rectTransform, "Name", term.Name, Art.Accent, 28, Theme.Ink, TextAnchor.MiddleCenter), 16);
            Ui.TopRow(name.rectTransform, 24, 24, 38, 36);
            Ui.TopRow(text.rectTransform, 28, 28, 84, height - 110);
            if (anim.Enabled)
            {
                var group = Ui.Group(detail); group.alpha = 0;
                anim.Play(Tween.To(.22f, e => group.alpha = e, Easing.Linear));
            }
        }

        public void Close()
        {
            if (!detail) return;
            Object.Destroy(detail.gameObject);
            detail = null;
        }
    }
}

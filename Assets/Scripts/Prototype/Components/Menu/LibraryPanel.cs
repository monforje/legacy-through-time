using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The "Слова" section (templates/shell/library.html): the words the stories explain (Glossary), one to a card on the
    /// ground of the kit. A word the reader has met is open - its name and its whole explanation, the card as tall as the
    /// text; the others are short cards with a lock, so that it is seen how much is still to find. A word can also be
    /// shown alone in a popup (kept for the tools).
    sealed class LibraryPanel
    {
        const float Gap = Kit.Space2, LockedHeight = 56, PadX = 16;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        ShellDialog detail;

        public bool DetailOpen => detail != null && detail.IsOpen;

        public LibraryPanel(RectTransform parent, IReadOnlyList<Glossary.Term> terms, IAnimator anim)
        {
            this.anim = anim;
            Root = Ui.Rect(parent, "Library");
            Ui.Stretch(Root);
            Kit.Ground(Root);
            var open = 0;
            foreach (var t in terms) if (Library.IsOpen(t.Name)) open++;

            var top = Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop;
            var title = Ui.Txt(Root, "Title", "Слова", Kit.F800, Kit.FsTitle, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, Kit.Side, Kit.Side, top, 34);
            var line = MenuUi.Fit(Ui.Txt(Root, "Sub", $"Открыто понятий: {open} из {terms.Count}", Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleCenter), 11);
            Ui.TopRow(line.rectTransform, Kit.Side, Kit.Side, top + 34, 20);

            var list = MenuUi.List(Root, top + 66, 0, 0);
            var width = Metrics.RefWidth - 2 * Kit.Side;
            var y = 4f;
            foreach (var term in terms)
                y += (Library.IsOpen(term.Name) ? Open(list, term, y, width) : Locked(list, term, y)) + Gap;
            if (terms.Count == 0)
            {
                var empty = Ui.Txt(list, "Empty", "Слова, которые объясняют истории, будут собираться здесь.", Kit.F600, Kit.FsBody, Kit.Muted, TextAnchor.MiddleCenter);
                Ui.TopRow(empty.rectTransform, Kit.Side, Kit.Side, 20, 60);
                y = 90;
            }
            MenuUi.SetHeight(list, y + 16);
        }

        /// A word met in a story: the name and the whole explanation. Returns its height.
        float Open(RectTransform list, Glossary.Term term, float top, float width)
        {
            var card = Kit.Round(list, "Term " + term.Name, Kit.Surface, Kit.RadiusM);
            var rt = card.rectTransform;
            Kit.Icon(rt, "Mark", Kit.Pic("book"), 26, A.TopLeft, new Vector2(PadX + 13, -28)).raycastTarget = false;
            var name = MenuUi.Fit(Ui.Txt(rt, "Name", term.Name, Kit.F800, Kit.FsHead, Kit.Text), 13);
            Ui.TopRow(name.rectTransform, PadX + 36, PadX, 14, 28);
            var text = Ui.Txt(rt, "Text", TextRules.NoBreaks(term.Text), Kit.F600, Kit.FsBody, Kit.Muted, TextAnchor.UpperLeft);
            var textHeight = Ui.TextHeight(text, width - 2 * PadX);
            Ui.TopRow(text.rectTransform, PadX, PadX, 48, textHeight);
            var height = 48 + textHeight + 18;
            Ui.Box(rt, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(width, height));
            return height;
        }

        /// A word still to be found: a short card with a lock.
        float Locked(RectTransform list, Glossary.Term term, float top)
        {
            var card = Kit.Round(list, "Term " + term.Name, Kit.Surface, Kit.RadiusM);
            card.raycastTarget = true;
            var rt = card.rectTransform;
            Ui.Box(rt, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(Metrics.RefWidth - 2 * Kit.Side, LockedHeight));
            Kit.Icon(rt, "Lock", Kit.Glyph("lock"), 22, A.MidLeft, new Vector2(PadX + 11, 0)).color = new Color(1, 1, 1, .6f);
            var hint = MenuUi.Fit(Ui.Txt(rt, "Name", "Откроется в одной из историй", Kit.F800, Kit.FsSmall, Kit.Muted), 10);
            Ui.Stretch(hint.rectTransform, PadX + 36, 4, PadX, 2);
            PressButton.On(card.gameObject, () => { if (anim.Enabled) anim.Play(Effects.Shake(rt)); }, .98f);
            return LockedHeight;
        }

        /// The word on its own over the page (kept for the tools; the page shows every open word whole).
        public void Show(Glossary.Term term)
        {
            Close();
            detail = new ShellDialog(Root, term.Name, anim, () => detail = null);
            var text = Ui.Txt(detail.Content, "Text", TextRules.NoBreaks(term.Text), Kit.F600, Kit.FsBody, Kit.Text, TextAnchor.UpperLeft);
            Ui.Stretch(text.rectTransform, Kit.Space2, 0, Kit.Space2, 0);
        }

        public void Close()
        {
            detail?.Close();
            detail = null;
        }
    }
}

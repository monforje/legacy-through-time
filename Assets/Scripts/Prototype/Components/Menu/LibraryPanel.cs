using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The "Слова" section in two levels (templates/shell/words.html, library.html). The first is the list of stories with
    /// "Открыто N из M" under each; a tap opens the second, the words of that story: a search, a card of rows that open
    /// like an accordion (a word the reader has met shows its whole explanation, the others are locked) and "Что это?".
    /// The words come from the episodes (Glossary); they belong to the first playable story.
    sealed class LibraryPanel
    {
        const float ListRow = 72, WordRow = 52, GroupPad = 4;

        public readonly RectTransform Root;
        readonly IReadOnlyList<StoryEntry> stories;
        readonly IReadOnlyList<Glossary.Term> terms;
        readonly IAnimator anim;
        readonly Action toStories;
        RectTransform page;                       // the words of one story, over the list
        Glossary.Term only;

        public bool DetailOpen => page;

        public LibraryPanel(RectTransform parent, IReadOnlyList<StoryEntry> stories, IReadOnlyList<Glossary.Term> terms, IAnimator anim, Action toStories)
        {
            this.stories = stories; this.terms = terms; this.anim = anim; this.toStories = toStories;
            Root = Ui.Rect(parent, "Library");
            Ui.Stretch(Root);
            var list = ShellList.Page(Root, "Слова", "Слова и понятия по историям");

            var y = 0f;
            var items = new List<RectTransform>();
            for (var i = 0; i < stories.Count; i++)
            {
                items.Add(StoryRow(list, stories[i], y, i == stories.Count - 1));
                y += ListRow;
            }
            MenuUi.SetHeight(list, y + Kit.Space3);
            Parts.Stagger(anim, items);
        }

        IReadOnlyList<Glossary.Term> TermsOf(StoryEntry story) => story == stories[0] ? terms : Array.Empty<Glossary.Term>();

        int Opened(StoryEntry story)
        {
            var n = 0;
            foreach (var t in TermsOf(story)) if (Library.IsOpen(t.Name)) n++;
            return n;
        }

        // ------------------------------------------------------------------ the first level: the stories
        RectTransform StoryRow(RectTransform list, StoryEntry story, float y, bool last)
        {
            var rt = ShellList.Row(list, "Words " + story.Id, y, ListRow, () => Open(story), last);
            var thumb = Kit.Round(rt, "Thumb", Kit.ArtColor(story.Id), Kit.RadiusS);
            thumb.raycastTarget = false;
            Ui.Box(thumb.rectTransform, A.MidLeft, A.MidLeft, new Vector2(Kit.Space2, 0), new Vector2(48, 48));
            thumb.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var picture = Ui.Img(thumb.rectTransform, "Picture", null);
            picture.enabled = false; picture.raycastTarget = false;
            Art.Background(story.Cover, sprite =>
            {
                if (!picture || !sprite) return;
                picture.sprite = sprite; picture.enabled = true;
                Ui.Cover(picture, new Vector2(48, 48), .4f);
            });
            var head = Ui.Txt(rt, "Head", story.Title, Kit.F800, Kit.FsBody, Kit.Text);
            Ui.TopRow(head.rectTransform, 68, 44, 16, 20);
            MenuUi.Fit(head, 11);
            var count = TermsOf(story).Count;
            var line = Ui.Txt(rt, "Line", story.Soon ? "Скоро" : $"Открыто {Opened(story)} из {count}", Kit.F600, Kit.FsSmall, Kit.Muted);
            Ui.TopRow(line.rectTransform, 68, 44, 38, 18);
            Kit.Icon(rt, "Chevron", Kit.Glyph("chevron-r"), 22, A.MidRight, new Vector2(-Kit.Space2, 0)).raycastTarget = false;
            return rt;
        }

        void Open(StoryEntry story)
        {
            if (story.Soon) { ShellToast.Show(Root, "Слова появятся вместе с историей", anim); return; }
            Show(story, null);
        }

        // ------------------------------------------------------------------ the second level: the words of a story
        void Show(StoryEntry story, Glossary.Term first)
        {
            Close();
            page = Ui.Rect(Root, "Words of " + story.Id);
            Ui.Stretch(page);
            Kit.Ground(page);
            var top = Metrics.InsetTop + Viewport.ExtraTop;

            var back = Parts.IconButton(page, "chevron-l", Close, Kit.Touch);
            Ui.Box(back.Root, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(Kit.Touch, Kit.Touch));
            var title = MenuUi.Fit(Ui.Txt(page, "Title", story.Title, Kit.F800, Kit.FsHead, Kit.Text, TextAnchor.MiddleCenter), 12);
            Ui.TopRow(title.rectTransform, Kit.Side + 52, Kit.Side + 52, top, Kit.Touch);

            var all = TermsOf(story);
            var opened = new List<Glossary.Term>(); var closedCount = 0;
            foreach (var t in all) { if (Library.IsOpen(t.Name)) opened.Add(t); else closedCount++; }
            var expanded = new HashSet<string>();
            if (first != null) expanded.Add(first.Name); else if (opened.Count > 0) expanded.Add(opened[0].Name);

            // the group fills the room between the search and the link
            var group = Kit.Round(page, "Group", Kit.Surface, Kit.RadiusM);
            Ui.Stretch(group.rectTransform, Kit.Side, 54 + Viewport.ExtraBottom, Kit.Side, top + Kit.Touch + Kit.Space2 + 36 + Kit.Space3);
            var content = MenuUi.List(group.rectTransform, GroupPad, GroupPad, 0);
            var empty = Ui.Txt(group.rectTransform, "Empty", "Ничего не найдено", Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleCenter);
            Ui.TopRow(empty.rectTransform, 0, 0, 24, 24);
            empty.gameObject.SetActive(false);

            void Rebuild(string query)
            {
                for (var i = content.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(content.GetChild(i).gameObject);
                var q = (query ?? "").Trim().ToLowerInvariant();
                var y = 0f; var shown = 0;
                var width = Kit.Width - 2 * Kit.Side - 2 * GroupPad;
                foreach (var term in opened)
                {
                    if (q.Length > 0 && !term.Name.ToLowerInvariant().Contains(q)) continue;
                    shown++;
                    var name = term.Name;
                    var row = WordRow_(content, term, y, width, expanded.Contains(name), out var height);
                    var rowRt = row;
                    PressButton.On(row.gameObject, () => { if (!expanded.Remove(name)) expanded.Add(name); Rebuild(query); }, .985f, row.GetComponent<Image>());
                    y += height;
                }
                if (q.Length == 0)
                    for (var i = 0; i < closedCount; i++) { Locked(content, y, width); shown++; y += WordRow; }
                empty.gameObject.SetActive(shown == 0);
                MenuUi.SetHeight(content, y);
            }
            Rebuild("");

            var search = Parts.Search(page, "Найти слово…", Rebuild);
            Ui.TopRow(search.Root, Kit.Side, Kit.Side, top + Kit.Touch + Kit.Space2, 36);

            // "Что это?": a line over the link
            var link = Ui.Txt(page, "What", "Что это?", Kit.F700, Kit.FsBody, Kit.Accent, TextAnchor.MiddleCenter);
            link.raycastTarget = true;
            var w = link.preferredWidth + 2 * Kit.Space4;
            Ui.Box(link.rectTransform, A.BottomCenter, A.BottomCenter, new Vector2(0, 10 + Viewport.ExtraBottom), new Vector2(w, Kit.Touch));
            var underline = Ui.Img(link.rectTransform, "Underline", ProceduralArt.White);
            underline.color = Kit.Accent; underline.raycastTarget = false;
            Ui.Box(underline.rectTransform, A.Center, A.Center, new Vector2(0, -8), new Vector2(link.preferredWidth, 1));
            PressButton.On(link.gameObject, () => ShellToast.Show(page,
                $"Слова и понятия из историй. Открываются по мере чтения: сейчас {opened.Count} из {all.Count}.", anim, 3.5f, 124), .96f);

            if (anim.Enabled)
            {
                var fade = Ui.Group(page); fade.alpha = 0;
                anim.Play(Tween.To(Kit.Dur.Normal, e => { if (fade) fade.alpha = e; }, Easing.Linear));
            }
        }

        /// A word the reader has met: the book, the name, the arrow (down when open) and, open, the whole explanation.
        RectTransform WordRow_(RectTransform content, Glossary.Term term, float y, float width, bool open, out float height)
        {
            var face = Kit.Round(content, "Word " + term.Name, Color.clear, Kit.RadiusS);
            face.raycastTarget = true;
            var rt = face.rectTransform;
            var text = Ui.Txt(rt, "Text", TextRules.NoBreaks(term.Text), Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.UpperLeft);
            var textWidth = width - 46 - Kit.Space3;
            var textHeight = open ? Mathf.Ceil(Ui.TextHeight(text, textWidth)) : 0;
            text.gameObject.SetActive(open);
            Ui.TopRow(text.rectTransform, 46, Kit.Space3, WordRow, textHeight);
            height = WordRow + (open ? textHeight + Kit.Space3 : 0);
            Ui.Box(rt, A.TopLeft, A.TopLeft, new Vector2(GroupPad, -y), new Vector2(width, height));

            Kit.Icon(rt, "Mark", Kit.Pic("book"), 22, A.TopLeft, new Vector2(12, -15)).raycastTarget = false;
            var name = Ui.Txt(rt, "Name", term.Name, Kit.F700, Kit.FsBody, Kit.Text, TextAnchor.MiddleLeft);
            Ui.TopRow(name.rectTransform, 46, 46, 0, WordRow);
            MenuUi.Fit(name, 11);
            var chevron = Kit.Icon(rt, "Chevron", Kit.Glyph("chevron-r"), 22, A.TopRight, new Vector2(-12, -15));
            chevron.raycastTarget = false;
            if (open)
            {
                chevron.rectTransform.pivot = new Vector2(.5f, .5f);                                    // it turns about its centre
                chevron.rectTransform.anchoredPosition = new Vector2(-12 - 11, -15 - 11);
                chevron.rectTransform.localRotation = Quaternion.Euler(0, 0, -90);                      // the open row points down
            }
            return rt;
        }

        void Locked(RectTransform content, float y, float width)
        {
            var face = Kit.Round(content, "Locked word", Color.clear, Kit.RadiusS);
            face.raycastTarget = true;
            var rt = face.rectTransform;
            Ui.Box(rt, A.TopLeft, A.TopLeft, new Vector2(GroupPad, -y), new Vector2(width, WordRow));
            var icon = Kit.Icon(rt, "Lock", Kit.Glyph("lock"), 22, A.MidLeft, new Vector2(12, 0));
            icon.raycastTarget = false; icon.color = new Color(1, 1, 1, .6f);
            var head = Ui.Txt(rt, "Head", "Закрытое слово", Kit.F700, Kit.FsBody, Kit.Muted);
            Ui.TopRow(head.rectTransform, 46, 12, 6, 22);
            var line = Ui.Txt(rt, "Line", "Откроется позже в истории", Kit.F700, Kit.FsSmall, Kit.Muted);
            Ui.TopRow(line.rectTransform, 46, 12, 28, 18);
            PressButton.On(face.gameObject, () => { if (anim.Enabled) anim.Play(Effects.Shake(rt)); }, .99f);
        }

        /// The word on its own (kept for the tools): its story opens with it expanded.
        public void Show(Glossary.Term term)
        {
            if (stories.Count == 0) return;
            Show(stories[0], term);
        }

        public void Close()
        {
            if (!page) return;
            UnityEngine.Object.Destroy(page.gameObject);
            page = null;
        }
    }
}

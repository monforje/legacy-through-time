using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The home of the shell (templates/shell/home.html), one column that scrolls as a whole: the story cards in a deck
    /// with the centred one larger and the sides clipped by the deck's box, the "random story" button on its edge, the
    /// dots, the search, the lenses and the grid of covers. A tap on a card opens the chapters of its story.
    sealed class HomeTab
    {
        public const float CardWidth = 276, CardHeight = 300, Gap = 12, SideScale = .88f;
        const float GridGap = Kit.Space3, TagHeight = 26;

        public readonly RectTransform Root;
        readonly IReadOnlyList<StoryEntry> stories;
        readonly Action<StoryEntry> openChapters;
        readonly IAnimator anim;
        readonly List<RectTransform> cards = new();
        readonly List<CanvasGroup> groups = new();
        readonly List<Image> dots = new();
        readonly RectTransform deck, content;
        readonly RectTransform grid;
        readonly float gridTop;
        float pos;
        int current;
        static Sprite scrim;

        public StoryEntry Current => stories[current];

        public HomeTab(RectTransform parent, IReadOnlyList<StoryEntry> stories, Action<StoryEntry> openChapters, IAnimator anim, float top)
        {
            this.stories = stories; this.openChapters = openChapters; this.anim = anim;
            Root = Ui.Rect(parent, "Home");
            Ui.Stretch(Root);
            content = MenuUi.List(Root, top, 0, 0);

            deck = Ui.Rect(content, "Deck");
            Ui.TopRow(deck, Kit.Side, Kit.Side, 0, CardHeight);
            deck.gameObject.AddComponent<RectMask2D>();                  // the sides are cut by the box of the deck, as in the mockup
            var swipe = Ui.Img(deck, "Swipe", ProceduralArt.White);
            swipe.color = Color.clear; swipe.raycastTarget = true;
            Ui.Stretch(swipe.rectTransform);
            swipe.gameObject.AddComponent<Swipe>().Done = dx => Turn(dx < 0 ? 1 : -1);
            for (var i = 0; i < stories.Count; i++) cards.Add(Card(deck, stories[i]));
            current = 0;
            for (var i = 0; i < stories.Count; i++) if (stories[i].Recommended) current = i;
            pos = current;
            Layout(pos);

            // on the left edge of the middle card, in the gap
            var shuffle = Parts.IconButton(content, "shuffle", Shuffle, Kit.Touch, circle: true, accent: true);
            Ui.Box(shuffle.Root, A.TopLeft, A.TopLeft, new Vector2(16.2f, -96), new Vector2(Kit.Touch, Kit.Touch));

            var y = CardHeight + Kit.Space3;
            var dotsRow = Ui.Rect(content, "Dots");
            Ui.TopRow(dotsRow, 0, 0, y, 8);
            for (var i = 0; i < stories.Count; i++)
            {
                var d = Ui.Img(dotsRow, "Dot", ProceduralArt.Disc);
                d.raycastTarget = false;
                Ui.Box(d.rectTransform, A.TopCenter, A.TopCenter, new Vector2((i - (stories.Count - 1) / 2f) * 16, 0), new Vector2(8, 8));
                dots.Add(d);
            }
            Dots();
            y += 8 + Kit.Space3;

            var search = Parts.Search(content, "Истории, авторы…", Filter);
            Ui.TopRow(search.Root, Kit.Side, Kit.Side, y, 36);
            y += 36 + Kit.Space3;

            var chips = Ui.Rect(content, "Chips");
            Ui.TopRow(chips, Kit.Side, Kit.Side, y, Kit.Touch);
            chips.gameObject.AddComponent<RectMask2D>();                  // the row scrolls under its own box
            Chips(chips);
            y += Kit.Touch + Kit.Space3;

            grid = Ui.Rect(content, "Grid");
            Ui.TopRow(grid, 0, 0, y, 0);
            gridTop = y;
            Grid("");
            Place(false);
        }

        // ------------------------------------------------------------------ the deck
        static Sprite Scrim()
        {
            if (scrim) return scrim;
            // linear-gradient(to top, scrim 92% at 15%, transparent at 80%)
            var tex = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (var i = 0; i < 64; i++)
            {
                var h = i / 63f;
                var a = h <= .15f ? .92f : h >= .8f ? 0f : .92f * (1 - (h - .15f) / .65f);
                tex.SetPixel(0, i, new Color(Kit.Scrim.r, Kit.Scrim.g, Kit.Scrim.b, a));
            }
            tex.Apply();
            return scrim = Sprite.Create(tex, new Rect(0, 0, 1, 64), new Vector2(.5f, .5f), 100);
        }

        RectTransform Card(RectTransform parent, StoryEntry story)
        {
            var card = Ui.Rect(parent, "Card " + story.Id);
            Ui.Box(card, A.TopCenter, A.TopCenter, Vector2.zero, new Vector2(CardWidth, CardHeight));
            var group = Ui.Group(card);
            groups.Add(group);

            var face = Kit.Round(card, "Face", Kit.ArtColor(story.Id), Kit.RadiusL);
            face.raycastTarget = true;
            Ui.Stretch(face.rectTransform);
            face.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var picture = Ui.Img(face.rectTransform, "Cover", null);
            picture.enabled = false; picture.raycastTarget = false;
            Art.Background(story.Cover, sprite =>
            {
                if (!picture || !sprite) return;
                picture.sprite = sprite; picture.enabled = true;
                Ui.Cover(picture, new Vector2(CardWidth, CardHeight), .4f);
            });
            var shade = Ui.Img(face.rectTransform, "Shade", Scrim());
            shade.raycastTarget = false;
            Ui.Stretch(shade.rectTransform);

            const float pad = Kit.Space4;
            var inner = CardWidth - 2 * pad;

            if (story.Recommended)
            {
                var badge = Kit.Round(face.rectTransform, "Badge", Kit.Accent.WithAlpha(.8f), Kit.RadiusS);
                var label = Ui.Txt(badge.rectTransform, "Text", "Рекомендуем", Kit.F800, Kit.FsSmall, Kit.AccentInk, TextAnchor.MiddleCenter);
                Ui.Box(badge.rectTransform, A.TopRight, A.TopRight, new Vector2(-Kit.Space3, -Kit.Space3), new Vector2(label.preferredWidth + 2 * Kit.Space3, TagHeight));
                Ui.Stretch(label.rectTransform);
            }

            // the name: 28 px, centred, from 64 px down, 1.1 line height
            var name = Ui.Txt(face.rectTransform, "Name", story.Title, Kit.F800, 28, Kit.Text, TextAnchor.UpperCenter);
            name.lineSpacing = 1.1f / 1.364f;
            Ui.TopRow(name.rectTransform, pad, pad, 64, 62);

            // the stack from the bottom: the button, the text (up to three lines), the tag
            var button = Parts.Button(face.rectTransform, story.Soon ? "Скоро" : "Открыть", ButtonStyle.Primary, () => Tapped(story), 44);
            button.SetEnabled(!story.Soon);
            Ui.Box(button.Root, A.BottomLeft, A.BottomLeft, new Vector2(pad, pad), new Vector2(button.Label.preferredWidth + 2 * Kit.Space4, 44));

            var blurb = Ui.Txt(face.rectTransform, "Blurb", story.Blurb ?? "", Kit.F600, Kit.FsSmall, Kit.Text, TextAnchor.UpperLeft);
            var lines = Mathf.Min(54f, Ui.TextHeight(blurb, inner));
            var blurbBottom = pad + 44 + Kit.Space2;
            Ui.BottomRow(blurb.rectTransform, pad, pad, blurbBottom, lines);
            blurb.verticalOverflow = VerticalWrapMode.Truncate;

            if (!string.IsNullOrEmpty(story.Genre))
            {
                var tag = Kit.Round(face.rectTransform, "Tag", Kit.Surface2, 13);
                var tagText = Ui.Txt(tag.rectTransform, "Text", story.Genre, Kit.F700, Kit.FsSmall, Kit.Text, TextAnchor.MiddleCenter);
                Ui.Box(tag.rectTransform, A.BottomLeft, A.BottomLeft, new Vector2(pad, blurbBottom + lines + Kit.Space2),
                       new Vector2(tagText.preferredWidth + 2 * Kit.Space3, TagHeight));
                Ui.Stretch(tagText.rectTransform);
            }
            PressButton.On(face.gameObject, () => Tapped(story), .99f);
            return card;
        }

        void Tapped(StoryEntry story)
        {
            var i = 0;
            for (; i < stories.Count; i++) if (stories[i] == story) break;
            if (i != current) { Go(i); return; }       // a side card comes to the middle first, as in the mockup
            if (!story.Soon) openChapters(story);
        }

        /// The cards stand by their distance from the middle one: the middle is full size, the sides are smaller and dimmer.
        void Layout(float at)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var d = i - at; var a = Mathf.Abs(d);
                var scale = Mathf.Lerp(1f, SideScale, Mathf.Clamp01(a));
                cards[i].anchoredPosition = new Vector2(d * (CardWidth * (1f + SideScale) / 2f + Gap), 0);
                cards[i].localScale = Vector3.one * scale;
                groups[i].alpha = Mathf.Lerp(1f, .5f, Mathf.Clamp01(a));
                groups[i].blocksRaycasts = a < 1.5f;
            }
        }

        void Place(bool animated)
        {
            Dots();
            if (!animated || !anim.Enabled) { pos = current; Layout(pos); return; }
            var from = pos; var to = current;
            anim.Play(Tween.To(Kit.Dur.Slow, e => { pos = Mathf.Lerp(from, to, e); if (deck) Layout(pos); }));
        }

        public void Turn(int step) => Go(Mathf.Clamp(current + step, 0, stories.Count - 1));

        void Go(int index)
        {
            if (index == current) return;
            current = index;
            Place(true);
        }

        /// "Random story": any other card comes to the middle.
        void Shuffle()
        {
            if (stories.Count < 2) return;
            int next;
            do { next = UnityEngine.Random.Range(0, stories.Count); } while (next == current);
            Go(next);
        }

        void Dots()
        {
            for (var i = 0; i < dots.Count; i++) dots[i].color = i == current ? Kit.Accent : Kit.Line;
        }

        // ------------------------------------------------------------------ lenses and the grid
        void Chips(RectTransform row)
        {
            var labels = new[] { "Для начала", "Новые", "Популярные", "По жанрам" };
            var x = 0f;
            var faces = new List<Image>(); var texts = new List<Text>();
            for (var i = 0; i < labels.Length; i++)
            {
                var k = i;
                var chip = Kit.Round(row, "Chip " + labels[i], i == 0 ? Kit.Accent : Kit.Surface, 22);
                chip.raycastTarget = true;
                var t = Ui.Txt(chip.rectTransform, "Label", labels[i], Kit.F800, Kit.FsBody, i == 0 ? Kit.AccentInk : Kit.Muted, TextAnchor.MiddleCenter);
                var w = t.preferredWidth + 2 * Kit.Space4;
                Ui.Box(chip.rectTransform, A.MidLeft, A.MidLeft, new Vector2(x, 0), new Vector2(w, Kit.Touch));
                Ui.Stretch(t.rectTransform);
                faces.Add(chip); texts.Add(t);
                PressButton.On(chip.gameObject, () =>
                {
                    for (var j = 0; j < faces.Count; j++)
                    {
                        faces[j].color = j == k ? Kit.Accent : Kit.Surface;
                        texts[j].color = j == k ? Kit.AccentInk : Kit.Muted;
                    }
                }, .94f);
                x += w + Kit.Space2;
            }
        }

        void Filter(string query) => Grid(query);

        /// Two columns of covers (16:9) with the title and the genre under them; the search keeps the stories whose title
        /// has the text.
        void Grid(string query)
        {
            for (var i = grid.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(grid.GetChild(i).gameObject);
            var q = (query ?? "").Trim().ToLowerInvariant();
            var shown = 0;
            const float w = (Metrics.RefWidth - 2 * Kit.Side - GridGap) / 2, coverH = w * 9 / 16, headH = 24, smallH = 18;
            const float cellH = coverH + Kit.Space2 + headH + Kit.Space2 + smallH;
            foreach (var story in stories)
            {
                if (q.Length > 0 && !story.Title.ToLowerInvariant().Contains(q)) continue;
                var col = shown % 2; var row = shown / 2; shown++;
                var cell = Ui.Rect(grid, "Mini " + story.Id);
                Ui.Box(cell, A.TopLeft, A.TopLeft, new Vector2(Kit.Side + col * (w + GridGap), -(row * (cellH + GridGap))), new Vector2(w, cellH));
                var cover = Kit.Round(cell, "Cover", Kit.ArtColor(story.Id), Kit.RadiusS);
                cover.raycastTarget = true;
                Ui.TopRow(cover.rectTransform, 0, 0, 0, coverH);
                cover.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                var picture = Ui.Img(cover.rectTransform, "Picture", null);
                picture.enabled = false; picture.raycastTarget = false;
                Art.Background(story.Cover, sprite =>
                {
                    if (!picture || !sprite) return;
                    picture.sprite = sprite; picture.enabled = true;
                    Ui.Cover(picture, new Vector2(w, coverH), .4f);
                });
                var title = Ui.Txt(cell, "Title", story.Title, Kit.F800, Kit.FsHead, Kit.Text);
                Ui.TopRow(title.rectTransform, 0, 0, coverH + Kit.Space2, headH);
                MenuUi.Fit(title, 12);
                var line = Ui.Txt(cell, "Line", story.Genre ?? "", Kit.F600, Kit.FsSmall, Kit.Muted);
                Ui.TopRow(line.rectTransform, 0, 0, coverH + Kit.Space2 + headH + Kit.Space2, smallH);
                var s = story;
                PressButton.On(cover.gameObject, () => { if (!s.Soon) openChapters(s); }, .98f);
            }
            float height;
            if (shown == 0)
            {
                var empty = Ui.Txt(grid, "Empty", "Ничего не найдено", Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleCenter);
                Ui.TopRow(empty.rectTransform, Kit.Side, Kit.Side, 0, 40);
                height = 40;
            }
            else height = Mathf.Ceil(shown / 2f) * cellH + (Mathf.Ceil(shown / 2f) - 1) * GridGap;
            grid.sizeDelta = new Vector2(0, height);
            MenuUi.SetHeight(content, gridTop + height + Kit.Space4);
        }

        /// A horizontal swipe turns the deck; a vertical one belongs to the scroll of the column.
        sealed class Swipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            public Action<float> Done;
            ScrollRect scroll;
            bool vertical;
            float dx;

            public void OnBeginDrag(PointerEventData e)
            {
                dx = 0;
                scroll = GetComponentInParent<ScrollRect>();
                vertical = Mathf.Abs(e.delta.y) > Mathf.Abs(e.delta.x);
                if (vertical && scroll) scroll.OnBeginDrag(e);
            }

            public void OnDrag(PointerEventData e)
            {
                if (vertical) { if (scroll) scroll.OnDrag(e); return; }
                dx += e.delta.x;
            }

            public void OnEndDrag(PointerEventData e)
            {
                if (vertical) { if (scroll) scroll.OnEndDrag(e); return; }
                if (Mathf.Abs(dx) > 40) Done?.Invoke(dx);
            }
        }
    }
}

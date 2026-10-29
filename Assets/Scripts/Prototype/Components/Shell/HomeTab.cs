using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The home of the shell (templates/shell/home.html): the story cards in a deck with the centred one larger, the
    /// "random story" button on its edge, the dots, the search, the lenses and the grid of covers. A tap on a card opens
    /// the chapters of its story.
    sealed class HomeTab
    {
        public const float CardWidth = 276, CardHeight = 300, Gap = 12, SideScale = .88f;

        public readonly RectTransform Root;
        readonly IReadOnlyList<StoryEntry> stories;
        readonly Action<StoryEntry> openChapters;
        readonly IAnimator anim;
        readonly List<RectTransform> cards = new();
        readonly List<CanvasGroup> groups = new();
        readonly List<Image> dots = new();
        readonly RectTransform deck;
        RectTransform grid;
        Text empty;
        float pos;
        int current;

        public StoryEntry Current => stories[current];

        public HomeTab(RectTransform parent, IReadOnlyList<StoryEntry> stories, Action<StoryEntry> openChapters, IAnimator anim, float top)
        {
            this.stories = stories; this.openChapters = openChapters; this.anim = anim;
            Root = Ui.Rect(parent, "Home");
            Ui.Stretch(Root);

            deck = Ui.Rect(Root, "Deck");
            Ui.TopRow(deck, 0, 0, top, CardHeight);
            var swipe = Ui.Img(deck, "Swipe", ProceduralArt.White);
            swipe.color = Color.clear; swipe.raycastTarget = true;
            Ui.Stretch(swipe.rectTransform);
            swipe.gameObject.AddComponent<Swipe>().Done = dx => Turn(dx < 0 ? 1 : -1);
            for (var i = 0; i < stories.Count; i++) cards.Add(Card(deck, stories[i], i));
            Layout(0);
            current = Mathf.Min(1, stories.Count - 1);
            pos = current;
            Layout(pos);

            var shuffle = Parts.IconButton(Root, "shuffle", Shuffle, Kit.Touch, circle: true, accent: true);
            Ui.Box(shuffle.Root, A.TopCenter, A.Center, new Vector2(-(CardWidth / 2 + 8 - 0), -(top + 112)), new Vector2(Kit.Touch, Kit.Touch));
            shuffle.Root.SetAsLastSibling();

            var dotsRow = Ui.Rect(Root, "Dots");
            Ui.TopRow(dotsRow, 0, 0, top + CardHeight + 12, 8);
            for (var i = 0; i < stories.Count; i++)
            {
                var d = Ui.Img(dotsRow, "Dot", ProceduralArt.Disc);
                d.raycastTarget = false;
                Ui.Box(d.rectTransform, A.TopCenter, A.TopCenter, new Vector2((i - (stories.Count - 1) / 2f) * 16, 0), new Vector2(8, 8));
                dots.Add(d);
            }
            Dots();

            var searchTop = top + CardHeight + 32;
            var search = Parts.Search(Root, "Истории, авторы…", Filter);
            Ui.TopRow(search.Root, Kit.Side, Kit.Side, searchTop, 36);

            var chips = Ui.Rect(Root, "Chips");
            Ui.TopRow(chips, Kit.Side, 0, searchTop + 48, Kit.Touch);
            Chips(chips);

            var list = MenuUi.List(Root, searchTop + 48 + Kit.Touch + 8, 0);
            grid = list;
            Grid(list, "");
            Place(false);
        }

        // ------------------------------------------------------------------ the deck
        RectTransform Card(RectTransform parent, StoryEntry story, int index)
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
            var shade = Ui.Img(face.rectTransform, "Shade", ProceduralArt.Gradient(new Color(Kit.Scrim.r, Kit.Scrim.g, Kit.Scrim.b, 0), Kit.Scrim.WithAlpha(.92f)));
            shade.raycastTarget = false;
            Ui.Stretch(shade.rectTransform);

            if (index == 1 && !story.Soon)
            {
                var badge = Kit.Round(face.rectTransform, "Badge", Kit.Accent.WithAlpha(.8f), Kit.RadiusS);
                Ui.Box(badge.rectTransform, A.TopRight, A.TopRight, new Vector2(-12, -12), new Vector2(112, 28));
                var bt = Ui.Txt(badge.rectTransform, "Text", "Рекомендуем", Art.Sub, Kit.FsSmall, Kit.AccentInk, TextAnchor.MiddleCenter);
                Ui.Stretch(bt.rectTransform);
            }

            var name = Ui.Txt(face.rectTransform, "Name", story.Title, Art.Sub, 28, Kit.Text, TextAnchor.MiddleCenter);
            MenuUi.Fit(name, 18);
            Ui.TopRow(name.rectTransform, 16, 16, 56, 76);
            var blurb = Ui.Txt(face.rectTransform, "Blurb", story.Blurb ?? "", Art.Body, Kit.FsSmall, Kit.Text, TextAnchor.LowerLeft);
            blurb.verticalOverflow = VerticalWrapMode.Truncate;
            Ui.BottomRow(blurb.rectTransform, 16, 16, 68, 56);
            var button = Parts.Button(face.rectTransform, story.Soon ? "Скоро" : "Открыть", ButtonStyle.Primary, () => Tapped(story), 44);
            button.SetEnabled(!story.Soon);
            Ui.Box(button.Root, A.BottomLeft, A.BottomLeft, new Vector2(16, 16), new Vector2(112, 44));
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
                groups[i].alpha = a > 1.6f ? 0 : Mathf.Lerp(1f, .5f, Mathf.Clamp01(a));
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
            var x = 0f; var chosen = 0;
            var faces = new List<Image>(); var texts = new List<Text>();
            for (var i = 0; i < labels.Length; i++)
            {
                var k = i;
                var w = 20f + labels[i].Length * 9.2f;
                var chip = Kit.Round(row, "Chip " + labels[i], i == 0 ? Kit.Accent : Kit.Surface, 22);
                chip.raycastTarget = true;
                Ui.Box(chip.rectTransform, A.MidLeft, A.MidLeft, new Vector2(x, 0), new Vector2(w, Kit.Touch));
                var t = Ui.Txt(chip.rectTransform, "Label", labels[i], Art.Action, Kit.FsBody, i == 0 ? Kit.AccentInk : Kit.Muted, TextAnchor.MiddleCenter);
                Ui.Stretch(t.rectTransform);
                faces.Add(chip); texts.Add(t);
                PressButton.On(chip.gameObject, () =>
                {
                    chosen = k;
                    for (var j = 0; j < faces.Count; j++)
                    {
                        faces[j].color = j == chosen ? Kit.Accent : Kit.Surface;
                        texts[j].color = j == chosen ? Kit.AccentInk : Kit.Muted;
                    }
                }, .94f);
                x += w + Kit.Space2;
            }
        }

        void Filter(string query) => Grid(grid, query);

        /// Two columns of covers with the title and the line under them; the search keeps the stories whose title has the text.
        void Grid(RectTransform list, string query)
        {
            for (var i = list.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(list.GetChild(i).gameObject);
            var q = (query ?? "").Trim().ToLowerInvariant();
            var shown = 0;
            const float w = (Metrics.RefWidth - 2 * Kit.Side - Kit.Space3) / 2, coverH = 88, cellH = 150;
            foreach (var story in stories)
            {
                if (q.Length > 0 && !story.Title.ToLowerInvariant().Contains(q)) continue;
                var col = shown % 2; var row = shown / 2; shown++;
                var cell = Ui.Rect(list, "Mini " + story.Id);
                Ui.Box(cell, A.TopLeft, A.TopLeft, new Vector2(Kit.Side + col * (w + Kit.Space3), -(row * cellH)), new Vector2(w, cellH - 8));
                var cover = Kit.Round(cell, "Cover", Kit.ArtColor(story.Id), Kit.RadiusM);
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
                var title = Ui.Txt(cell, "Title", story.Title, Art.Sub, 17, Kit.Text);
                Ui.TopRow(title.rectTransform, 0, 0, coverH + 6, 24);
                MenuUi.Fit(title, 12);
                var line = Ui.Txt(cell, "Line", story.Episode ?? "", Art.Body, Kit.FsSmall, Kit.Muted);
                Ui.TopRow(line.rectTransform, 0, 0, coverH + 30, 18);
                var s = story;
                PressButton.On(cover.gameObject, () => { if (!s.Soon) openChapters(s); }, .98f);
            }
            if (shown == 0)
            {
                empty = Ui.Txt(list, "Empty", "Ничего не найдено", Art.Body, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleCenter);
                Ui.TopRow(empty.rectTransform, Kit.Side, Kit.Side, 24, 40);
            }
            MenuUi.SetHeight(list, Mathf.Ceil(shown / 2f) * cellH + 16);
        }

        sealed class Swipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            public Action<float> Done;
            float dx;
            public void OnBeginDrag(PointerEventData e) => dx = 0;
            public void OnDrag(PointerEventData e) => dx += e.delta.x;
            public void OnEndDrag(PointerEventData e) { if (Mathf.Abs(dx) > 40) Done?.Invoke(dx); }
        }
    }
}

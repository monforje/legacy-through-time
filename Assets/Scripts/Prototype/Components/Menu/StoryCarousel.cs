using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The stories round the fire: one card in the middle, the neighbours dark at the sides (a story announced but not
    /// written yet carries a lock). Swipe or the arrows to turn; a tap on the middle card opens its chapters, on a
    /// neighbour turns to it. Under the card: how far the reader is, and the button to the chapters.
    sealed class StoryCarousel
    {
        public const float CardWidth = 232, CardHeight = 352, Gap = 14;
        const float SideScale = .8f, SideOffset = CardWidth * .86f;

        public readonly RectTransform Root;
        readonly IReadOnlyList<StoryEntry> stories;
        readonly Func<ChapterEntry, MenuScreen.Progress> progress;
        readonly Action<StoryEntry> openChapters;
        readonly IAnimator anim;
        readonly List<RectTransform> cards = new();
        readonly RectTransform deck, ghostLeft, ghostRight;    // empty dark cards where a story has no neighbour
        readonly Image left, right;
        readonly Text status;
        readonly Image button;
        int current;

        public StoryEntry Current => stories[current];

        public StoryCarousel(RectTransform parent, IReadOnlyList<StoryEntry> stories, Func<ChapterEntry, MenuScreen.Progress> progress,
                             Action<StoryEntry> openChapters, IAnimator anim)
        {
            this.stories = stories; this.progress = progress; this.openChapters = openChapters; this.anim = anim;
            Root = Ui.Rect(parent, "Carousel");
            Ui.TopRow(Root, 0, 0, 0, CardHeight + 140);

            deck = Ui.Rect(Root, "Deck");
            Ui.TopRow(deck, 0, 0, 0, CardHeight);
            var drag = deck.gameObject.AddComponent<Image>();
            drag.color = Color.clear; drag.raycastTarget = true;
            deck.gameObject.AddComponent<Swipe>().Done = dx => Turn(dx < 0 ? 1 : -1);

            ghostLeft = Ghost(deck, -1);
            ghostRight = Ghost(deck, 1);
            foreach (var story in stories) cards.Add(Card(deck, story));

            left = Arrow("Prev", -1);
            right = Arrow("Next", 1);

            // under the bottom medallion of the card (it hangs 20 below it)
            status = MenuUi.Fit(Ui.Txt(Root, "Status", "", Art.Sub, 14, MenuUi.Gold, TextAnchor.MiddleCenter), 11);
            Ui.TopRow(status.rectTransform, 20, 20, CardHeight + 26, 22);
            MenuUi.Shadow(status, .8f);
            button = MenuUi.Button(Root, "Open", "Главы", () => { if (!Current.Soon) openChapters(Current); else Locked(); });
            Ui.Box(button.rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -(CardHeight + 52)), new Vector2(196, 46));
            Ui.Lift(button, 5, .4f);

            Place(false);
        }

        // ------------------------------------------------------------ a card
        RectTransform Card(RectTransform parent, StoryEntry story)
        {
            var card = Ui.Rect(parent, "Card " + story.Id);
            Ui.Box(card, A.TopCenter, A.TopCenter, Vector2.zero, new Vector2(CardWidth, CardHeight));
            var hit = card.gameObject.AddComponent<Image>();
            hit.color = Color.clear; hit.raycastTarget = true;
            PressButton.On(card.gameObject, () => Tapped(story), .98f);

            // the cover inside the frame (the frame's band is 12 px of its 120: 23 canvas units on this card)
            var window = Ui.Img(card, "Window", ProceduralArt.White);
            window.color = Theme.Hex("0F2A20");
            Ui.Stretch(window.rectTransform, 14, 14, 14, 14);
            window.gameObject.AddComponent<RectMask2D>();
            var picture = Ui.Img(window.rectTransform, "Cover", null);
            picture.enabled = false;
            var box = new Vector2(CardWidth - 28, CardHeight - 28);
            Art.Background(story.Cover, sprite =>
            {
                if (!picture || !sprite) return;
                picture.sprite = sprite; picture.enabled = true;
                picture.color = story.Soon ? MenuUi.Dim : Color.white;
                var tone = CurrentTone(card);                     // it arrives late: the veil of its card on it too
                picture.canvasRenderer.SetColor(new Color(tone, tone, tone, 1));
                Ui.Cover(picture, box, .42f);
            });
            var shade = Ui.Img(window.rectTransform, "Shade", ProceduralArt.Gradient(new Color(0, 0, 0, 0), new Color(.03f, .06f, .05f, .7f)));
            Ui.Stretch(shade.rectTransform);

            var frame = Ui.Sliced(card, "Frame", "story-frame", fillCenter: false);
            Ui.Stretch(frame.rectTransform);
            Ui.Lift(frame, 10, .5f);
            Ui.Icon(card, "Top", "medallion", A.TopCenter, A.Center, new Vector2(0, -1), 40);
            Ui.Icon(card, "Bottom", "medallion", A.BottomCenter, A.Center, new Vector2(0, 1), 40);

            // the name on its plaque, a little below the middle
            var plaque = Ui.Sliced(card, "Plaque", "title-plaque");
            // narrower than the card, its ribbons just over the frame
            Ui.Box(plaque.rectTransform, A.Center, A.Center, new Vector2(0, -44), new Vector2(CardWidth - 12, 56));
            var name = MenuUi.Fit(Ui.Txt(plaque.rectTransform, "Name", story.Title, Art.Accent, 22, Theme.Parchment, TextAnchor.MiddleCenter), 14);
            Ui.Stretch(name.rectTransform, 30, 8, 30, 6);
            Ui.Emboss(name);

            if (story.Soon)
            {
                var lockFrame = Ui.Icon(card, "LockFrame", "lock-frame", A.Center, A.Center, new Vector2(0, 56), 72);
                Ui.Icon(lockFrame.rectTransform, "Lock", "lock", A.Center, A.Center, new Vector2(0, 1), 30, Theme.Parchment);
                var soon = Ui.Txt(plaque.rectTransform, "Soon", "Скоро у костра", Art.Sub, 14, MenuUi.Gold, TextAnchor.MiddleCenter);
                Ui.Box(soon.rectTransform, A.BottomCenter, A.TopCenter, new Vector2(0, -4), new Vector2(CardWidth - 40, 22));
                MenuUi.Shadow(soon, .9f);
            }
            else Pips(card, story);
            return card;
        }

        /// An empty card in the dark: a story still to be told, where the carousel has no neighbour on that side.
        RectTransform Ghost(RectTransform parent, int side)
        {
            var card = Ui.Rect(parent, side < 0 ? "Ghost left" : "Ghost right");
            Ui.Box(card, A.TopCenter, A.TopCenter, new Vector2(side * SideOffset, -(CardHeight * (1 - SideScale) / 2)), new Vector2(CardWidth, CardHeight));
            card.localScale = Vector3.one * SideScale;
            var window = Ui.Img(card, "Window", ProceduralArt.White);
            window.color = Theme.Hex("0B1A15");
            Ui.Stretch(window.rectTransform, 14, 14, 14, 14);
            var frame = Ui.Sliced(card, "Frame", "story-frame", fillCenter: false);
            Ui.Stretch(frame.rectTransform);
            Ui.Icon(card, "Top", "medallion", A.TopCenter, A.Center, new Vector2(0, -1), 40);
            Ui.Icon(card, "Bottom", "medallion", A.BottomCenter, A.Center, new Vector2(0, 1), 40);
            var lockFrame = Ui.Icon(card, "LockFrame", "lock-frame", A.Center, A.Center, new Vector2(0, 10), 72);
            Ui.Icon(lockFrame.rectTransform, "Lock", "lock", A.Center, A.Center, new Vector2(0, 1), 30, Theme.Parchment);
            Tint(card, .42f);
            return card;
        }

        /// The chapters as rhombuses on the bottom of the card: ruby - read (or reading), gold - open, dim - to come.
        void Pips(RectTransform card, StoryEntry story)
        {
            var n = story.Chapters.Count;
            var row = Ui.Rect(card, "Pips");
            Ui.Box(row, A.BottomCenter, A.BottomCenter, new Vector2(0, 30), new Vector2(18 * n + 26, 30));
            for (var i = 0; i < n; i++)
            {
                var chapter = story.Chapters[i];
                var p = progress(chapter);
                var done = p == MenuScreen.Progress.Finished || p == MenuScreen.Progress.InProgress;
                var pip = Ui.Icon(row, "Pip", done ? "dot-on" : "dot", A.MidLeft, A.Center, new Vector2(22 + 18 * i, 0), 14);
                if (chapter.Soon) pip.color = new Color(1, 1, 1, .45f);
            }
        }

        Image Arrow(string name, int step)
        {
            var arrow = Ui.Icon(Root, name, "arrow", step > 0 ? A.TopRight : A.TopLeft, A.Center,
                                new Vector2(step > 0 ? -22 : 22, -CardHeight * .52f), 26);
            arrow.raycastTarget = true;
            if (step < 0) arrow.rectTransform.localScale = new Vector3(-1, 1, 1);
            Ui.Lift(arrow, 3, .5f);
            PressButton.On(arrow.gameObject, () => Turn(step), .9f);
            return arrow;
        }

        // ------------------------------------------------------------ turning
        void Tapped(StoryEntry story)
        {
            var index = IndexOf(story);
            if (index != current) { Turn(index - current); return; }
            if (story.Soon) Locked(); else openChapters(story);
        }

        int IndexOf(StoryEntry story)
        {
            for (var i = 0; i < stories.Count; i++) if (stories[i] == story) return i;
            return current;
        }

        void Locked()
        {
            if (anim.Enabled) anim.Play(Effects.Shake(cards[current]));
        }

        public void Turn(int step)
        {
            var next = Mathf.Clamp(current + step, 0, stories.Count - 1);
            if (next == current) return;
            current = next;
            Place(anim.Enabled);
        }

        /// Every card to its place: the current one in the middle, full size and bright; the others at the sides.
        void Place(bool animated)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                var offset = i - current;
                var x = offset * SideOffset;
                var s = offset == 0 ? 1f : SideScale;
                var group = Ui.Group(card);
                var alpha = Mathf.Abs(offset) > 1 ? 0 : 1;
                group.blocksRaycasts = Mathf.Abs(offset) <= 1;
                var tone = offset == 0 ? 1f : .5f;
                if (offset == 0) card.SetAsLastSibling();
                // the name is read on the middle card only: on a neighbour it would be cut by the middle one
                var plaque = Ui.Group(card.Find("Plaque"));
                var named = offset == 0 ? 1f : 0f;
                if (!animated)
                {
                    card.anchoredPosition = new Vector2(x, -(CardHeight * (1 - s) / 2));
                    card.localScale = Vector3.one * s;
                    group.alpha = alpha;
                    plaque.alpha = named;
                    Tint(card, tone);
                    continue;
                }
                var from = card.anchoredPosition; var s0 = card.localScale.x; var a0 = group.alpha; var t0 = CurrentTone(card);
                var p0 = plaque.alpha;
                var to = new Vector2(x, -(CardHeight * (1 - s) / 2));
                anim.Play(Tween.To(.38f, e =>
                {
                    if (!card) return;
                    card.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
                    card.localScale = Vector3.one * Mathf.LerpUnclamped(s0, s, e);
                    group.alpha = Mathf.Lerp(a0, alpha, e);
                    plaque.alpha = Mathf.Lerp(p0, named, e);
                    Tint(card, Mathf.Lerp(t0, tone, e));
                }, Easing.OutExpo));
            }
            Show(ghostLeft, current == 0, animated);
            Show(ghostRight, current == stories.Count - 1, animated);
            left.enabled = current > 0;
            right.enabled = current < stories.Count - 1;
            Status();
        }

        void Show(RectTransform ghost, bool on, bool animated)
        {
            var group = Ui.Group(ghost);
            group.blocksRaycasts = false;
            if (!animated) { group.alpha = on ? 1 : 0; return; }
            var a0 = group.alpha;
            anim.Play(Tween.To(.38f, e => { if (group) group.alpha = Mathf.Lerp(a0, on ? 1 : 0, e); }, Easing.Linear));
        }

        /// The neighbours sit in the dark: every graphic of the card dimmed (the colours of the art kept).
        static void Tint(RectTransform card, float tone)
        {
            foreach (var g in card.GetComponentsInChildren<Graphic>(true))
                if (g.name != "Shade") g.canvasRenderer.SetColor(new Color(tone, tone, tone, 1));
        }

        static float CurrentTone(RectTransform card)
        {
            var frame = card.Find("Frame");
            return frame ? frame.GetComponent<Graphic>().canvasRenderer.GetColor().r : 1;
        }

        void Status()
        {
            var story = Current;
            button.gameObject.SetActive(!story.Soon);
            if (story.Soon) { status.text = "Эта легенда ещё рассказывается"; return; }
            int finished = 0, open = 0; ChapterEntry reading = null;
            foreach (var c in story.Chapters)
            {
                if (c.Soon) continue;
                open++;
                var p = progress(c);
                if (p == MenuScreen.Progress.Finished) finished++;
                if (p == MenuScreen.Progress.InProgress && reading == null) reading = c;
            }
            status.text = reading != null ? $"Вы остановились: глава {reading.Number}"
                : finished > 0 ? $"Прочитано глав: {finished} из {story.Chapters.Count}"
                : $"Глав: {story.Chapters.Count} · открыто {open}";
        }

        /// A horizontal swipe over the deck: reports its direction once it is long enough.
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The main menu: a board of story cards over the dimmed title picture. A card shows the preview (the cover
    /// picture), the title, the episode and its progress, a blurb, and "Читать" / "Продолжить" / "Читать снова";
    /// a story in progress also gets "Начать сначала" (two taps: the first one asks). The header scrolls away with
    /// the cards; "Выйти из игры" closes the list; a new version of the game shows a card above the stories.
    sealed class StoryBoard
    {
        public enum Progress { New, InProgress, Finished }

        const float Pad = 12, PreviewHeight = 188, CardGap = 18, ButtonHeight = 48, RestartHeight = 36, TextSide = 18;
        const float HeaderHeight = 92, PreviewFocus = .5f, UpdateHeight = 158;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly Func<StoryEntry, Progress> progress;
        readonly Action<StoryEntry, bool> open;          // (story, from the beginning)
        readonly RectTransform content, body;           // the scrolling list; the cards and the footer in it
        readonly float bodyTop;
        RectTransform update;
        bool leaving;

        public StoryBoard(RectTransform column, IReadOnlyList<StoryEntry> stories, Func<StoryEntry, Progress> progress,
                          Action<StoryEntry, bool> open, Action quit, bool animated)
        {
            this.progress = progress; this.open = open;
            Root = Ui.Rect(column, "Menu");
            Ui.Stretch(Root);
            Root.gameObject.AddComponent<CanvasGroup>();
            anim = animated ? Root.gameObject.AddComponent<CoroutineAnimator>() : NoAnimator.Instance;

            var dim = Ui.Img(Root, "Dim", ProceduralArt.Gradient(Theme.TitleDimTop, Theme.TitleDimBottom));
            Ui.Bleed(dim.rectTransform);

            content = BuildList();
            var top = Metrics.InsetTop + 8 + Viewport.ExtraTop;
            var header = BuildHeader(content, top);

            bodyTop = top + HeaderHeight;
            body = Ui.Rect(content, "Body");
            var y = 0f;
            var cards = new List<RectTransform>();
            foreach (var story in stories)
            {
                var card = BuildCard(body, story, y);
                cards.Add(card);
                y += card.sizeDelta.y + CardGap;
            }
            var footer = BuildQuit(body, y, quit);
            cards.Add(footer);
            y += ButtonHeight + 28;
            Ui.TopRow(body, 0, 0, bodyTop, y);
            Resize();

            if (anim.Enabled) Animate(header, cards);
        }

        void Resize() => content.sizeDelta = new Vector2(0, body.offsetMax.y * -1 + body.rect.height);

        /// A card above the stories: a newer build of the game can be downloaded. "Позже" removes it.
        public void ShowUpdate(string version, Action download)
        {
            if (update || !Root) return;
            var card = Ui.Sliced(content, "Update", "card");
            update = card.rectTransform;
            Ui.TopRow(update, 0, 0, bodyTop, UpdateHeight - CardGap);
            var title = Ui.Txt(update, "Title", "Вышла новая версия", Art.Head, 20, Theme.Ink);
            Ui.TopRow(title.rectTransform, TextSide, TextSide, 18, 28);
            var line = Ui.Txt(update, "Version", "Сборка " + version + ": скачайте и установите поверх", Art.Sub, 13, Theme.Scarlet);
            Ui.TopRow(line.rectTransform, TextSide, TextSide, 46, 20);

            var button = Ui.Sliced(update, "Download", "btn");
            Ui.TopRow(button.rectTransform, Pad - 2, 112, 74, ButtonHeight);
            Ui.Icon(button.rectTransform, "Icon", "download", A.MidLeft, A.MidLeft, new Vector2(16, 1), 22, Theme.Parchment);
            var label = Ui.Txt(button.rectTransform, "Text", "Обновить", Art.Action, Metrics.FsAction, Theme.Parchment);
            Ui.Emboss(label);
            Ui.Stretch(label.rectTransform, 46, 2, 10, 0);
            PressButton.On(button.gameObject, () => { if (!leaving) download(); }, .97f, button, "btn-pressed");

            var later = Ui.Img(update, "Later", ProceduralArt.White);
            later.color = Color.clear; later.raycastTarget = true;
            Ui.Box(later.rectTransform, A.TopRight, A.TopRight, new Vector2(-Pad, -74), new Vector2(96, ButtonHeight));
            var laterText = Ui.Txt(later.rectTransform, "Text", "Позже", Art.Action, Metrics.FsAction, Theme.Ink.WithAlpha(.7f), TextAnchor.MiddleCenter);
            Ui.Stretch(laterText.rectTransform);
            PressButton.On(later.gameObject, HideUpdate, .97f);

            Shift(UpdateHeight);
            if (anim.Enabled)
            {
                var group = Ui.Group(card); group.alpha = 0;
                anim.Play(Tween.To(.4f, e => group.alpha = e, Easing.Linear));
            }
        }

        void HideUpdate()
        {
            if (!update) return;
            UnityEngine.Object.Destroy(update.gameObject);
            update = null;
            Shift(-UpdateHeight);
        }

        /// Moves the cards down (or back up) under the update card.
        void Shift(float by)
        {
            body.anchoredPosition -= new Vector2(0, by);
            Resize();
        }

        /// Fades away and destroys itself; run by the owner (the board's own animator dies with it).
        public IEnumerator Leave()
        {
            if (leaving || !Root) yield break;
            leaving = true;
            var group = Root.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            for (var t = 0f; t < .25f && Root; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1 - t / .25f;
                yield return null;
            }
            if (Root) UnityEngine.Object.Destroy(Root.gameObject);
        }

        // ------------------------------------------------------------ header and the scrolling list
        RectTransform BuildHeader(RectTransform content, float top)
        {
            var header = Ui.Rect(content, "Header");
            Ui.TopRow(header, 36, 36, top, HeaderHeight);
            var title = Ui.Txt(header, "Title", "ИСТОРИИ", Art.Head, Metrics.FsTitle, Theme.Parchment, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, 0, 0, 10, 40);
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, .45f); shadow.effectDistance = new Vector2(0, -2);
            Ui.Icon(header, "Orn", "title-orn", A.TopCenter, A.TopCenter, new Vector2(0, -52), 64);
            return header;
        }

        /// The whole column scrolls (the header too), down to the gesture bar.
        RectTransform BuildList()
        {
            var viewport = Ui.Img(Root, "List", ProceduralArt.White);
            viewport.color = Color.clear; viewport.raycastTarget = true;     // drags anywhere in the list scroll it
            Ui.Stretch(viewport.rectTransform, 0, Metrics.InsetBottom + Viewport.ExtraBottom, 0, 0);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = Ui.Rect(viewport.rectTransform, "Cards");
            content.anchorMin = A.TopLeft; content.anchorMax = A.TopRight; content.pivot = A.TopCenter;
            content.offsetMin = new Vector2(Metrics.Side, 0); content.offsetMax = new Vector2(-Metrics.Side, 0);

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform; scroll.content = content;
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.decelerationRate = .12f;
            return content;
        }

        // ------------------------------------------------------------ a card
        RectTransform BuildCard(RectTransform content, StoryEntry story, float top)
        {
            var state = story.Soon ? Progress.New : progress(story);
            var card = Ui.Sliced(content, "Card " + story.Id, "card");
            var rt = card.rectTransform;
            Ui.Group(card);

            BuildPreview(rt, story, state);

            var y = Pad + PreviewHeight + 10;
            var title = Ui.Txt(rt, "Title", story.Title, Art.Head, 22, Theme.Ink);
            Ui.TopRow(title.rectTransform, TextSide, TextSide, y, 30); y += 30;

            var line = state switch
            {
                Progress.InProgress => story.Episode + " · вы остановились здесь",
                Progress.Finished => story.Episode + " · пройден",
                _ => story.Episode,
            };
            var episode = Ui.Txt(rt, "Episode", line, Art.Sub, Metrics.FsLabel, Theme.Scarlet);
            Ui.TopRow(episode.rectTransform, TextSide, TextSide, y, 22); y += 26;

            var blurb = Ui.Txt(rt, "Blurb", TextRules.NoBreaks(story.Blurb), Art.Body, 15, Theme.Ink.WithAlpha(.85f), TextAnchor.UpperLeft);
            var blurbHeight = Ui.TextHeight(blurb, Metrics.PlateWidth - 2 * TextSide);
            Ui.TopRow(blurb.rectTransform, TextSide, TextSide, y, blurbHeight); y += blurbHeight + 14;

            if (!story.Soon)
            {
                var label = state switch { Progress.InProgress => "Продолжить", Progress.Finished => "Читать снова", _ => "Читать" };
                BuildButton(rt, y, label, () => open(story, state != Progress.InProgress)); y += ButtonHeight;
                if (state == Progress.InProgress) { y += 4; BuildRestart(rt, y, story); y += RestartHeight; }
            }
            else Ui.Group(card).alpha = .82f;

            y += 22;                                            // the bottom border and the shadow of the card
            Ui.TopRow(rt, 0, 0, top, y);
            return rt;
        }

        void BuildPreview(RectTransform card, StoryEntry story, Progress state)
        {
            var box = new Vector2(Metrics.PlateWidth - 2 * Pad, PreviewHeight);
            var preview = Ui.Img(card, "Preview", ProceduralArt.White);
            preview.color = Theme.Hex("0F4F3A");                 // emerald-dark while the picture loads (and for "soon")
            Ui.TopRow(preview.rectTransform, Pad, Pad, Pad, PreviewHeight);
            preview.gameObject.AddComponent<RectMask2D>();

            if (story.Soon)
            {
                Ui.Icon(preview.rectTransform, "Knot", "knot", A.Center, A.Center, new Vector2(0, 14), 64, Theme.Parchment.WithAlpha(.8f));
                var soon = Ui.Txt(preview.rectTransform, "Soon", "СКОРО", Art.Head, 20, Theme.Parchment, TextAnchor.MiddleCenter);
                Ui.Box(soon.rectTransform, A.Center, A.Center, new Vector2(0, -38), new Vector2(200, 30));
            }
            else
            {
                var picture = Ui.Img(preview.rectTransform, "Picture", null);
                picture.enabled = false;
                Art.Background(story.Cover, sprite =>
                {
                    if (!picture || !sprite) return;
                    picture.sprite = sprite; picture.enabled = true;
                    Ui.Cover(picture, box, PreviewFocus);
                    if (anim.Enabled) anim.Play(Tween.To(.4f, e => { if (picture) picture.color = new Color(1, 1, 1, e); }, Easing.Linear));
                });
            }

            Ui.Stretch(Ui.Sliced(preview.rectTransform, "Frame", "frame", fillCenter: false).rectTransform);
            if (state == Progress.Finished)
                Ui.Icon(preview.rectTransform, "Star", "star", A.TopRight, A.TopRight, new Vector2(-10, -10), 26);
        }

        void BuildButton(RectTransform card, float top, string text, Action click)
        {
            var button = Ui.Sliced(card, "Read", "btn");
            Ui.TopRow(button.rectTransform, Pad - 2, Pad - 2, top, ButtonHeight);
            var label = Ui.Txt(button.rectTransform, "Text", text, Art.Action, Metrics.FsAction, Theme.Parchment);
            Ui.Emboss(label);
            Ui.Stretch(label.rectTransform, 22, 2, 40, 0);
            Ui.Icon(button.rectTransform, "Chevron", "chevron", A.MidRight, A.MidRight, new Vector2(-18, 1), 12, Theme.Parchment);
            PressButton.On(button.gameObject, () => { if (!leaving) click(); }, .97f, button, "btn-pressed");
        }

        void BuildRestart(RectTransform card, float top, StoryEntry story)
        {
            var row = Ui.Img(card, "Restart", ProceduralArt.White);
            row.color = Color.clear; row.raycastTarget = true;
            Ui.TopRow(row.rectTransform, Pad, Pad, top, RestartHeight);
            Ui.Icon(row.rectTransform, "Icon", "restart", A.MidLeft, A.MidLeft, new Vector2(10, 0), 20, Theme.Ink.WithAlpha(.75f));
            var label = Ui.Txt(row.rectTransform, "Text", "Начать сначала", Art.Action, Metrics.FsLabel, Theme.Ink.WithAlpha(.75f));
            Ui.Stretch(label.rectTransform, 38, 0, 0, 0);

            var armed = false;
            PressButton.On(row.gameObject, () =>
            {
                if (leaving) return;
                if (armed) { open(story, true); return; }
                armed = true;                                  // the first tap asks: the progress will be lost
                label.text = "Прогресс сотрётся. Ещё раз — начать";
                label.color = Theme.Scarlet;
                anim.Play(Effects.Shake(label.rectTransform));
                anim.Play(Tween.After(3f, () =>
                {
                    if (!label) return;
                    armed = false; label.text = "Начать сначала"; label.color = Theme.Ink.WithAlpha(.75f);
                }));
            }, .98f);
        }

        /// "Выйти из игры" under the cards: quiet, parchment on the dim.
        RectTransform BuildQuit(RectTransform parent, float top, Action quit)
        {
            var row = Ui.Img(parent, "Quit", ProceduralArt.White);
            row.color = Color.clear; row.raycastTarget = true;
            Ui.TopRow(row.rectTransform, 40, 40, top, ButtonHeight);
            var label = Ui.Txt(row.rectTransform, "Text", "Выйти из игры", Art.Action, Metrics.FsAction, Theme.Parchment.WithAlpha(.85f), TextAnchor.MiddleCenter);
            Ui.Stretch(label.rectTransform, 28, 0, 0, 0);
            var width = Ui.TextWidth(label) + 28;
            Ui.Icon(row.rectTransform, "Icon", "exit", A.Center, A.MidLeft, new Vector2(-width / 2, 1), 22, Theme.Parchment.WithAlpha(.85f));
            PressButton.On(row.gameObject, () => { if (!leaving) quit(); }, .97f);
            return row.rectTransform;
        }

        // ------------------------------------------------------------ entrance
        void Animate(RectTransform header, List<RectTransform> cards)
        {
            var headerGroup = Ui.Group(header);
            headerGroup.alpha = 0;
            anim.Play(Tween.To(.5f, e => headerGroup.alpha = e, Easing.Linear));
            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                var group = Ui.Group(card);
                var alpha = group.alpha; var y0 = card.anchoredPosition.y;
                group.alpha = 0;
                anim.Play(Tween.To(.5f, e =>
                {
                    group.alpha = alpha * e;
                    card.anchoredPosition = new Vector2(card.anchoredPosition.x, y0 - 18 * (1 - e));
                }, Easing.OutExpo, .12f + .09f * i));
            }
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The chapters of a story (templates/shell/chapters.html): the back arrow, the title and the way into the chat with
    /// its hero on top, the cover, the progress, then a card of rows: read ones with a check, the one being read with
    /// "Продолжить" and a two-tap restart, new ones with "Читать", those to come with a lock.
    sealed class ChapterList
    {
        const float RowHeight = 68, CoverHeight = 150;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly Action<ChapterEntry, bool> open;          // (chapter, from the beginning)

        public ChapterList(RectTransform parent, StoryEntry story, Func<ChapterEntry, MenuScreen.Progress> progress,
                           Action<ChapterEntry, bool> open, Action openChat, Action close, IAnimator anim)
        {
            this.open = open; this.anim = anim;
            Root = Ui.Rect(parent, "Chapters");
            Ui.Stretch(Root);
            Kit.Ground(Root);

            var top = Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop;
            var back = Parts.IconButton(Root, "chevron-l", close, Kit.Touch);
            Ui.Box(back.Root, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(Kit.Touch, Kit.Touch));
            var title = MenuUi.Fit(Ui.Txt(Root, "Title", story.Title, Art.Sub, Kit.FsHead, Kit.Text, TextAnchor.MiddleCenter), 12);
            Ui.TopRow(title.rectTransform, Kit.Side + 56, Kit.Side + 56, top, Kit.Touch);
            if (!string.IsNullOrEmpty(story.Hero))
            {
                var chat = Parts.IconButton(Root, "chat", openChat, Kit.Touch);
                Ui.Box(chat.Root, A.TopRight, A.TopRight, new Vector2(-Kit.Side, -top), new Vector2(Kit.Touch, Kit.Touch));
            }

            var list = MenuUi.List(Root, top + Kit.Touch + Kit.Space3, 0);
            var width = Metrics.RefWidth - 2 * Kit.Side;
            var y = 0f;
            Cover(list, story, y, width); y += CoverHeight + Kit.Space3;

            var done = 0;
            foreach (var chapter in story.Chapters) if (progress(chapter) == MenuScreen.Progress.Finished) done++;
            Progress(list, y, width, done, story.Chapters.Count); y += 34 + Kit.Space3;

            var group = Kit.Round(list, "Rows", Kit.Surface, Kit.RadiusM);
            Ui.Box(group.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -y), new Vector2(width, story.Chapters.Count * RowHeight + 8));
            var ry = 4f;
            foreach (var chapter in story.Chapters)
            {
                Row(group.rectTransform, chapter, progress(chapter), ry, width - 8);
                ry += RowHeight;
            }
            y += story.Chapters.Count * RowHeight + 8 + Kit.Space4;
            MenuUi.SetHeight(list, y);

            if (!anim.Enabled) return;
            var fade = Ui.Group(Root); fade.alpha = 0;
            anim.Play(Tween.To(Kit.Dur.Normal, e => fade.alpha = e, Easing.Linear));
        }

        static void Cover(RectTransform list, StoryEntry story, float top, float width)
        {
            var card = Kit.Round(list, "Cover", Kit.ArtColor(story.Id), Kit.RadiusL);
            Ui.Box(card.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(width, CoverHeight));
            card.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var picture = Ui.Img(card.rectTransform, "Picture", null);
            picture.enabled = false; picture.raycastTarget = false;
            Art.Background(story.Cover, sprite =>
            {
                if (!picture || !sprite) return;
                picture.sprite = sprite; picture.enabled = true;
                Ui.Cover(picture, new Vector2(width, CoverHeight), .4f);
            });
        }

        static void Progress(RectTransform list, float top, float width, int done, int all)
        {
            var text = Ui.Txt(list, "Progress", $"Пройдено {done} из {all} глав", Art.Body, Kit.FsSmall, Kit.Muted);
            Ui.Box(text.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(width, 18));
            var track = Kit.Round(list, "Track", Kit.Surface2, 4);
            Ui.Box(track.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -(top + 24)), new Vector2(width, 8));
            if (all == 0 || done == 0) return;
            var fill = Kit.Round(track.rectTransform, "Fill", Kit.Accent, 4);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2((float)done / all, 1);
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
        }

        void Row(RectTransform group, ChapterEntry chapter, MenuScreen.Progress state, float top, float width)
        {
            var current = state == MenuScreen.Progress.InProgress;
            var row = Kit.Round(group, "Chapter " + chapter.Number, current ? Kit.Surface2 : Color.clear, Kit.RadiusS);
            row.raycastTarget = true;
            Ui.Box(row.rectTransform, A.TopLeft, A.TopLeft, new Vector2(4, -top), new Vector2(width, RowHeight - 4));
            var rt = row.rectTransform;
            var locked = state == MenuScreen.Progress.Locked;

            var mark = state switch
            {
                MenuScreen.Progress.Finished => Kit.Glyph("check"),
                MenuScreen.Progress.Locked => Kit.Glyph("lock"),
                MenuScreen.Progress.InProgress => Kit.Pic("book"),
                _ => Kit.Pic("star"),
            };
            var icon = Kit.Icon(rt, "Mark", mark, state == MenuScreen.Progress.Finished || locked ? 22 : 30, A.MidLeft, new Vector2(24, 0));
            icon.raycastTarget = false;
            if (locked) icon.color = new Color(1, 1, 1, .6f);

            var head = MenuUi.Fit(Ui.Txt(rt, "Head", $"Глава {chapter.Number}. {chapter.Title}", Art.Sub, Kit.FsBody, locked ? Kit.Muted : Kit.Text), 11);
            Ui.TopRow(head.rectTransform, 52, current ? 132 : 12, 10, 22);
            var line = MenuUi.Fit(Ui.Txt(rt, "State", Line(state), Art.Body, Kit.FsSmall, Kit.Muted), 10);
            Ui.TopRow(line.rectTransform, 52, current ? 132 : 12, 34, 20);

            if (locked)
            {
                PressButton.On(row.gameObject, () => { if (anim.Enabled) anim.Play(Effects.Shake(rt)); }, .99f);
                return;
            }
            PressButton.On(row.gameObject, () => open(chapter, !current), .98f);
            if (current) Resume(rt, chapter, line);
        }

        static string Line(MenuScreen.Progress state) => state switch
        {
            MenuScreen.Progress.Finished => "Прочитана · читать снова",
            MenuScreen.Progress.InProgress => "Вы остановились здесь",
            MenuScreen.Progress.Locked => "Скоро",
            _ => "Новая",
        };

        /// "Продолжить" and, beside it, "Начать сначала": the first tap asks, the second one starts over.
        void Resume(RectTransform row, ChapterEntry chapter, Text line)
        {
            var go = Parts.Button(row, "Читать", ButtonStyle.Primary, () => open(chapter, false), 40);
            Ui.Box(go.Root, A.MidRight, A.MidRight, new Vector2(-6, 0), new Vector2(76, 40));
            go.Label.fontSize = Kit.FsBody;

            var again = Parts.IconButton(row, "shuffle", null, 40, circle: false);
            Ui.Box(again.Root, A.MidRight, A.MidRight, new Vector2(-6 - 76 - Kit.Space2, 0), new Vector2(40, 40));
            var armed = false;
            again.Press.Click = () =>
            {
                if (armed) { open(chapter, true); return; }
                armed = true;                                  // the first tap asks: the progress will be lost
                line.text = "Прогресс сотрётся. Ещё раз — сначала";
                line.color = Kit.Danger;
                if (anim.Enabled) anim.Play(Effects.Shake(line.rectTransform));
                anim.Play(Tween.After(3f, () =>
                {
                    if (!line) return;
                    armed = false; line.text = Line(MenuScreen.Progress.InProgress); line.color = Kit.Muted;
                }));
            };
        }
    }
}

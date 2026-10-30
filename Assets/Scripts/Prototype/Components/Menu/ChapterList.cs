using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The chapters of a story (templates/shell/chapters.html): the back arrow, the title and the way into the chat with its
    /// hero on top; the cover with the genre, the progress, then a card of rows: read ones with a check, the one being read
    /// with "Читать", those to come with a lock. A tap on a read chapter reads it again.
    sealed class ChapterList
    {
        const float RowHeight = 52, GroupPad = 4;

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
            var title = MenuUi.Fit(Ui.Txt(Root, "Title", story.Title, Kit.F800, Kit.FsHead, Kit.Text, TextAnchor.MiddleCenter), 12);
            Ui.TopRow(title.rectTransform, Kit.Side + 52, Kit.Side + 52, top, Kit.Touch);
            if (!string.IsNullOrEmpty(story.Hero))
            {
                var chat = Parts.IconButton(Root, "chat", openChat, Kit.Touch, picture: true);
                Ui.Box(chat.Root, A.TopRight, A.TopRight, new Vector2(-Kit.Side, -top), new Vector2(Kit.Touch, Kit.Touch));
            }

            var list = MenuUi.List(Root, top + Kit.Touch + Kit.Space3, 0, 0);
            var width = Kit.Width - 2 * Kit.Side;
            var coverHeight = Mathf.Round(width * 9 / 16);
            var y = 0f;
            Cover(list, story, y, width, coverHeight); y += coverHeight + Kit.Space3;

            var done = 0;
            foreach (var chapter in story.Chapters) if (progress(chapter) == MenuScreen.Progress.Finished) done++;
            Progress(list, y, width, done, story.Chapters.Count); y += 34 + Kit.Space3;

            var group = Kit.Round(list, "Rows", Kit.Surface, Kit.RadiusM);
            var groupHeight = story.Chapters.Count * RowHeight + 2 * GroupPad;
            Ui.Box(group.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -y), new Vector2(width, groupHeight));
            var ry = GroupPad;
            foreach (var chapter in story.Chapters)
            {
                Row(group.rectTransform, chapter, progress(chapter), ry, width - 2 * GroupPad);
                ry += RowHeight;
            }
            y += groupHeight + Kit.Space4;
            MenuUi.SetHeight(list, y);

            if (!anim.Enabled) return;
            var fade = Ui.Group(Root); fade.alpha = 0;
            anim.Play(Tween.To(Kit.Dur.Normal, e => fade.alpha = e, Easing.Linear));
        }

        /// The picture of the story with its genre in a flag on the corner.
        static void Cover(RectTransform list, StoryEntry story, float top, float width, float height)
        {
            var card = Kit.Round(list, "Cover", Kit.ArtColor(story.Id), Kit.RadiusS);
            Ui.Box(card.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(width, height));
            card.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var picture = Ui.Img(card.rectTransform, "Picture", null);
            picture.enabled = false; picture.raycastTarget = false;
            Art.Background(story.Cover, sprite =>
            {
                if (!picture || !sprite) return;
                picture.sprite = sprite; picture.enabled = true;
                Ui.Cover(picture, new Vector2(width, height), .4f);
            });
            if (string.IsNullOrEmpty(story.Genre)) return;
            var flag = Kit.Round(card.rectTransform, "Flag", Kit.Accent, Kit.RadiusS);
            var label = Ui.Txt(flag.rectTransform, "Text", story.Genre, Kit.F800, Kit.FsSmall, Kit.AccentInk, TextAnchor.MiddleCenter);
            Ui.Box(flag.rectTransform, A.TopRight, A.TopRight, new Vector2(-Kit.Space3, -Kit.Space3), new Vector2(label.preferredWidth + 2 * Kit.Space3, 26));
            Ui.Stretch(label.rectTransform);
        }

        static void Progress(RectTransform list, float top, float width, int done, int all)
        {
            var text = Ui.Txt(list, "Progress", $"Пройдено {done} из {all} глав", Kit.F600, Kit.FsSmall, Kit.Muted);
            Ui.Box(text.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(width, 18));
            var track = Kit.Round(list, "Track", Kit.Surface2, 4);
            Ui.Box(track.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -(top + 26)), new Vector2(width, 8));
            if (all == 0 || done == 0) return;
            var fill = Kit.Round(track.rectTransform, "Fill", Kit.Accent, 4);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2((float)done / all, 1);
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
        }

        /// A row: the mark of the state at 12, the title and the line at 46; the one being read is lit and has "Читать".
        void Row(RectTransform group, ChapterEntry chapter, MenuScreen.Progress state, float top, float width)
        {
            var current = state == MenuScreen.Progress.InProgress;
            var locked = state == MenuScreen.Progress.Locked;
            var row = Kit.Round(group, "Chapter " + chapter.Number, current ? Kit.Surface2 : Color.clear, Kit.RadiusS);
            row.raycastTarget = true;
            Ui.Box(row.rectTransform, A.TopLeft, A.TopLeft, new Vector2(GroupPad, -top), new Vector2(width, RowHeight));
            var rt = row.rectTransform;

            var mark = state switch
            {
                MenuScreen.Progress.Finished => Kit.Glyph("check"),
                MenuScreen.Progress.Locked => Kit.Glyph("lock"),
                MenuScreen.Progress.InProgress => Kit.Pic("book"),
                _ => Kit.Pic("star"),
            };
            var icon = Kit.Icon(rt, "Mark", mark, 22, A.MidLeft, new Vector2(12, 0));
            icon.raycastTarget = false;
            if (locked) icon.color = new Color(1, 1, 1, .6f);

            var name = chapter.Title == "Скоро" ? $"Глава {chapter.Number}" : $"Глава {chapter.Number}. {chapter.Title}";
            var right = current ? 12 + 84 + 12 : 12;
            var head = MenuUi.Fit(Ui.Txt(rt, "Head", name, Kit.F700, Kit.FsBody, locked ? Kit.Muted : Kit.Text), 11);
            Ui.TopRow(head.rectTransform, 46, right, 6, 22);
            var price = !locked && !ShellPurse.ChapterOpen(chapter) ? $"За {chapter.EnergyCost} энергии" : null;     // a chapter after the first, not paid for yet
            var line = MenuUi.Fit(Ui.Txt(rt, "State", price ?? Line(state), Kit.F700, Kit.FsSmall, Kit.Muted), 10);
            Ui.TopRow(line.rectTransform, 46, right, 28, 18);

            if (locked)
            {
                PressButton.On(row.gameObject, () => { if (anim.Enabled) anim.Play(Effects.Shake(rt)); }, .99f, row, null, Kit.Surface2);
                return;
            }
            PressButton.On(row.gameObject, () => open(chapter, !current), .98f, row, null, current ? Kit.Surface2.WithAlpha(1) : Kit.Surface2);
            if (!current) return;
            var go = Parts.Button(rt, "Читать", ButtonStyle.Primary, () => open(chapter, false), Kit.Touch);
            Ui.Box(go.Root, A.MidRight, A.MidRight, new Vector2(-12, 0), new Vector2(84, Kit.Touch));
        }

        static string Line(MenuScreen.Progress state) => state switch
        {
            MenuScreen.Progress.Finished => "Пройдена",
            MenuScreen.Progress.InProgress => "Вы остановились здесь",
            MenuScreen.Progress.Locked => "Скоро",
            _ => "Новая",
        };
    }
}

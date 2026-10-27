using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The chapters of a story over the dimmed fire: read ones with a gold check, the one being read with "Продолжить"
    /// and a two-tap "Начать сначала", open ones with "Читать", those to come dark with a lock.
    sealed class ChapterList
    {
        const float RowHeight = 104, RowGap = 12;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly Action<ChapterEntry, bool> open;          // (chapter, from the beginning)

        public ChapterList(RectTransform parent, StoryEntry story, Func<ChapterEntry, MenuScreen.Progress> progress,
                           Action<ChapterEntry, bool> open, Action close, IAnimator anim)
        {
            this.open = open; this.anim = anim;
            Root = Ui.Rect(parent, "Chapters");
            Ui.Stretch(Root);
            MenuUi.Veil(Root, .74f);

            var top = Metrics.HudBottom + Viewport.ExtraTop;
            MenuUi.Title(Root, story.Title, $"Глав: {story.Chapters.Count}", top, side: 58, size: 27);
            var back = Ui.Icon(Root, "Back", "arrow", A.TopLeft, A.Center, new Vector2(Metrics.Side + 8, -(top + 20)), 22);
            back.rectTransform.localScale = new Vector3(-1, 1, 1);
            back.raycastTarget = true;
            PressButton.On(back.gameObject, close, .9f);

            var list = MenuUi.List(Root, top + MenuUi.HeaderHeight, 0);
            var y = 6f;
            foreach (var chapter in story.Chapters)
            {
                Row(list, chapter, progress(chapter), y);
                y += RowHeight + RowGap;
            }
            MenuUi.SetHeight(list, y + 20);

            if (!anim.Enabled) return;
            var group = Ui.Group(Root); group.alpha = 0;
            anim.Play(Tween.To(.28f, e => group.alpha = e, Easing.Linear));
        }

        void Row(RectTransform list, ChapterEntry chapter, MenuScreen.Progress state, float top)
        {
            var row = Ui.Sliced(list, "Chapter " + chapter.Number, "card");
            row.raycastTarget = true;
            Ui.TopRow(row.rectTransform, 0, 0, top, RowHeight);
            Ui.Lift(row, 6, .45f);
            var rt = row.rectTransform;
            var locked = state == MenuScreen.Progress.Locked;

            var seal = Ui.Icon(rt, "Seal", "round", A.MidLeft, A.MidLeft, new Vector2(16, 2), 48);
            var number = Ui.Txt(seal.rectTransform, "Number", chapter.Number.ToString(), Art.Accent, 22, Theme.Parchment, TextAnchor.MiddleCenter);
            Ui.Stretch(number.rectTransform, 0, 2, 0, 0);
            Ui.Emboss(number);

            var head = Ui.Txt(rt, "Head", $"Глава {chapter.Number}", Art.Sub, 13, locked ? Theme.Ink.WithAlpha(.6f) : Theme.Scarlet);
            Ui.TopRow(head.rectTransform, 78, 64, 18, 18);
            var title = MenuUi.Fit(Ui.Txt(rt, "Title", chapter.Title, Art.Accent, 21, Theme.Ink), 14);
            Ui.TopRow(title.rectTransform, 78, 64, 36, 28);
            var line = MenuUi.Fit(Ui.Txt(rt, "State", Line(state), Art.Body, 14, Theme.Ink.WithAlpha(.7f)), 10);
            Ui.TopRow(line.rectTransform, 78, 64, 64, 22);

            var icon = state switch
            {
                MenuScreen.Progress.Finished => "check",
                MenuScreen.Progress.Locked => "lock",
                _ => "play",
            };
            // the mark of the state on the right; the chapter being read has "Начать сначала" there instead
            if (state != MenuScreen.Progress.InProgress)
                Ui.Icon(rt, "Mark", icon, A.MidRight, A.MidRight, new Vector2(-22, 2), 28,
                        state == MenuScreen.Progress.Finished ? Theme.Emerald : locked ? Theme.Ink.WithAlpha(.55f) : Theme.Scarlet);

            if (locked)
            {
                row.color = new Color(.62f, .6f, .58f, 1);
                PressButton.On(row.gameObject, () => { if (anim.Enabled) anim.Play(Effects.Shake(rt)); }, .99f);
                return;
            }
            PressButton.On(row.gameObject, () => open(chapter, state != MenuScreen.Progress.InProgress), .98f);
            if (state == MenuScreen.Progress.InProgress) Restart(rt, chapter, line);
        }

        static string Line(MenuScreen.Progress state) => state switch
        {
            MenuScreen.Progress.Finished => "Прочитана · читать снова",
            MenuScreen.Progress.InProgress => "С того же места",
            MenuScreen.Progress.Locked => "Скоро у костра",
            _ => "Читать",
        };

        /// "Начать сначала" at the bottom right of the row: the first tap asks, the second one starts over.
        void Restart(RectTransform row, ChapterEntry chapter, Text line)
        {
            var link = Ui.Img(row, "Restart", ProceduralArt.White);
            link.color = Color.clear; link.raycastTarget = true;
            Ui.Box(link.rectTransform, A.MidRight, A.MidRight, new Vector2(-12, 2), new Vector2(48, 48));
            Ui.Icon(link.rectTransform, "Icon", "restart", A.Center, A.Center, Vector2.zero, 24, Theme.Scarlet);
            var armed = false;
            PressButton.On(link.gameObject, () =>
            {
                if (armed) { open(chapter, true); return; }
                armed = true;                                  // the first tap asks: the progress will be lost
                line.text = "Прогресс сотрётся. Ещё раз — сначала";
                line.color = Theme.Scarlet;
                if (anim.Enabled) anim.Play(Effects.Shake(line.rectTransform));
                anim.Play(Tween.After(3f, () =>
                {
                    if (!line) return;
                    armed = false; line.text = Line(MenuScreen.Progress.InProgress); line.color = Theme.Ink.WithAlpha(.7f);
                }));
            }, .95f);
        }
    }
}

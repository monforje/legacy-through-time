using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The "Чат" tab: the heroes of the stories the reader has begun, as in a messenger (docs/notes/chat-rework.md).
    /// A row is the hero's avatar, name, the last line on one line, the story, the time and the unread count. Nothing
    /// begun yet: a state with a way back to the stories.
    sealed class ChatsPanel
    {
        public const float RowHeight = 72;
        const float Gap = 6, Pad = 8, AvatarSize = 48;

        public readonly RectTransform Root;
        public readonly int Rows;

        public ChatsPanel(RectTransform parent, IReadOnlyList<ChatRow> rows, Action<StoryEntry> open, Action toStories, DateTime now, IAnimator anim)
        {
            Rows = rows.Count;
            Root = Ui.Rect(parent, "Chats");
            Ui.Stretch(Root);
            Kit.Ground(Root);

            var top = Metrics.InsetTop + 16 + Viewport.ExtraTop;
            var title = Ui.Txt(Root, "Title", "Чаты", Art.Sub, 24, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, Metrics.Side, Metrics.Side, top, 34);
            var sub = Ui.Txt(Root, "Sub", "Герои историй, которые вы начали", Art.Body, 13, Kit.Muted, TextAnchor.MiddleCenter);
            Ui.TopRow(sub.rectTransform, Metrics.Side, Metrics.Side, top + 34, 20);

            if (rows.Count == 0) { Empty(toStories); return; }

            var list = MenuUi.List(Root, top + 66, 0);
            var y = 0f;
            foreach (var row in rows)
            {
                Row(list, row, y, open, now);
                y += RowHeight + Gap;
            }
            MenuUi.SetHeight(list, y + 12);

            if (!anim.Enabled) return;
            // the rows come one after another, a little from below
            var k = 0;
            foreach (Transform child in list)
            {
                var group = Ui.Group(child); group.alpha = 0;
                var rt = (RectTransform)child; var y0 = rt.anchoredPosition.y;
                anim.Play(Tween.To(.32f, e => { if (rt) { group.alpha = e; rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0 - 10 * (1 - e)); } }, null, .04f * k++));
            }
        }

        void Row(RectTransform list, ChatRow row, float top, Action<StoryEntry> open, DateTime now)
        {
            var face = Kit.Round(list, "Chat " + row.Story.Id, Color.clear);
            face.raycastTarget = true;
            Ui.TopRow(face.rectTransform, 0, 0, top, RowHeight);
            var rt = face.rectTransform;

            var avatar = Kit.Avatar(rt, row.Hero, row.Story.Id, AvatarSize);
            Ui.Box(avatar.rectTransform, A.MidLeft, A.MidLeft, new Vector2(Pad, 0), new Vector2(AvatarSize, AvatarSize));

            const float left = Pad + AvatarSize + 12, right = 64;
            var width = Metrics.RefWidth - 2 * Metrics.Side - left - right;
            var name = Ui.Txt(rt, "Name", row.Hero, Art.Sub, 15, Kit.Text);
            Ui.TopRow(name.rectTransform, left, right, 12, 20);
            MenuUi.Fit(name, 11);
            var said = row.Preview != null;
            var preview = Ui.Txt(rt, "Preview", "", Art.Body, 13, said ? Kit.Muted : Kit.Muted.WithAlpha(.7f), TextAnchor.MiddleLeft,
                                 said ? FontStyle.Normal : FontStyle.Italic);
            Ui.TopRow(preview.rectTransform, left, right, 32, 18);
            Kit.Clamp(preview, said ? row.Preview : "Начните разговор", width);
            var story = Ui.Txt(rt, "Story", row.Story.Title, Art.Sub, 11, Kit.Accent);
            Ui.TopRow(story.rectTransform, left, right, 50, 16);
            MenuUi.Fit(story, 9);

            var time = Ui.Txt(rt, "Time", ChatList.When(row.At, now), Art.Body, 11, Kit.Muted, TextAnchor.MiddleRight);
            Ui.Box(time.rectTransform, A.TopRight, A.TopRight, new Vector2(-Pad, -14), new Vector2(56, 16));
            if (row.Unread > 0)
            {
                var badge = Kit.Round(rt, "Unread", Kit.Accent, 10);
                Ui.Box(badge.rectTransform, A.TopRight, A.TopRight, new Vector2(-Pad, -36), new Vector2(row.Unread > 9 ? 28 : 20, 20));
                var n = Ui.Txt(badge.rectTransform, "Count", row.Unread > 99 ? "99+" : row.Unread.ToString(), Art.Sub, 11, Kit.AccentInk, TextAnchor.MiddleCenter);
                Ui.Stretch(n.rectTransform);
            }

            var line = Ui.Img(rt, "Line", ProceduralArt.White);
            line.color = Kit.Surface2;
            Ui.BottomRow(line.rectTransform, Pad, Pad, 0, 1);

            var story_ = row.Story;
            PressButton.On(face.gameObject, () => open(story_), .985f, face);
        }

        void Empty(Action toStories)
        {
            var box = Ui.Rect(Root, "Empty");
            Ui.Box(box, A.Center, A.Center, new Vector2(0, 20), new Vector2(Metrics.RefWidth - 2 * Metrics.Side - 24, 200));
            var head = Ui.Txt(box, "Head", "Пока нет чатов", Art.Sub, 18, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(head.rectTransform, 0, 0, 40, 28);
            var text = Ui.Txt(box, "Text", "Начните историю, и её герой появится здесь", Art.Body, 13, Kit.Muted, TextAnchor.UpperCenter);
            Ui.TopRow(text.rectTransform, 0, 0, 74, 40);
            var button = Kit.Round(box, "ToStories", Kit.Accent);
            button.raycastTarget = true;
            Ui.Box(button.rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -126), new Vector2(160, 44));
            var label = Ui.Txt(button.rectTransform, "Text", "К историям", Art.Action, 16, Kit.AccentInk, TextAnchor.MiddleCenter);
            Ui.Stretch(label.rectTransform);
            PressButton.On(button.gameObject, toStories, .96f);
        }
    }
}

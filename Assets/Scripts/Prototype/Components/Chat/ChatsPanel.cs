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
        public const float RowHeight = 73;
        const float AvatarSize = 48;

        public readonly RectTransform Root;
        public readonly int Rows;

        public ChatsPanel(RectTransform parent, IReadOnlyList<ChatRow> rows, Action<StoryEntry> open, Action toStories, DateTime now, IAnimator anim)
        {
            Rows = rows.Count;
            Root = Ui.Rect(parent, "Chats");
            Ui.Stretch(Root);
            var list = ShellList.Page(Root, "Чаты", "Герои историй, которые вы начали");

            if (rows.Count == 0) { ShellList.Empty(Root, "chat", "Пока нет чатов", "Начните историю, и её герой появится здесь", toStories, anim); return; }

            var y = 0f;
            var items = new List<RectTransform>();
            for (var i = 0; i < rows.Count; i++)
            {
                items.Add(Row(list, rows[i], y, open, now, i == rows.Count - 1));
                y += RowHeight;
            }
            MenuUi.SetHeight(list, y + Kit.Space3);
            Parts.Stagger(anim, items);
        }

        /// `.list-row`: the avatar at 8, the name, the last line and the story in a column, the time and the unread count on the right.
        RectTransform Row(RectTransform list, ChatRow row, float top, Action<StoryEntry> open, DateTime now, bool last)
        {
            var story_ = row.Story;
            var rt = ShellList.Row(list, "Chat " + row.Story.Id, top, RowHeight, () => open(story_), last);

            var avatar = Kit.Avatar(rt, row.Hero, row.Story.Id, AvatarSize);
            Ui.Box(avatar.rectTransform, A.MidLeft, A.MidLeft, new Vector2(Kit.Space2, 0), new Vector2(AvatarSize, AvatarSize));
            avatar.raycastTarget = false;

            // the right column is as wide as the time or the badge, the text stops 12 before it
            var time = Ui.Txt(rt, "Time", ChatList.When(row.At, now), Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleRight);
            time.horizontalOverflow = HorizontalWrapMode.Overflow;                 // never wraps: the column is as wide as the time
            var badgeWidth = row.Unread > 0 ? Mathf.Max(20, (row.Unread > 99 ? 3 : row.Unread > 9 ? 2 : 1) * 7 + 12) : 0;
            var meta = Mathf.Ceil(Mathf.Max(time.preferredWidth, badgeWidth)) + 2;
            Ui.Box(time.rectTransform, A.TopRight, A.TopRight, new Vector2(-Kit.Space2, -12), new Vector2(meta, 18));
            if (row.Unread > 0)
            {
                var badge = Kit.Round(rt, "Unread", Kit.Accent, 10);
                badge.raycastTarget = false;
                Ui.Box(badge.rectTransform, A.BottomRight, A.BottomRight, new Vector2(-Kit.Space2, 12), new Vector2(badgeWidth, 20));
                var n = Ui.Txt(badge.rectTransform, "Count", row.Unread > 99 ? "99+" : row.Unread.ToString(), Kit.F800, Kit.FsMicro, Kit.AccentInk, TextAnchor.MiddleCenter);
                Ui.Stretch(n.rectTransform);
            }

            const float left = Kit.Space2 + AvatarSize + Kit.Space3;
            var right = Kit.Space2 + meta + Kit.Space3;
            var width = Kit.Width - 2 * ShellList.RowInset - left - right;
            var name = Ui.Txt(rt, "Name", row.Hero, Kit.F800, Kit.FsBody, Kit.Text);
            Ui.TopRow(name.rectTransform, left, right, 8, 20);
            var said = row.Preview != null;
            var preview = Ui.Txt(rt, "Preview", "", Kit.F600, Kit.FsSmall, said ? Kit.Muted : Kit.Muted.WithAlpha(.7f), TextAnchor.MiddleLeft,
                                 said ? FontStyle.Normal : FontStyle.Italic);
            Ui.TopRow(preview.rectTransform, left, right, 30, 18);
            Kit.Clamp(preview, said ? row.Preview : "Начните разговор", width);
            var story = Ui.Txt(rt, "Story", row.Story.Title, Kit.F700, Kit.FsMicro, Kit.Accent);
            Ui.TopRow(story.rectTransform, left, right, 50, 15);
            return rt;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The list pages of the shell, "Чаты" and "Слова" (templates/shell/chats.html, words.html): the title and a line under it,
    /// then rows of the list (`.list-row`) that stand 8 px inside the screen's edges, with a 1 px line between them.
    static class ShellList
    {
        public const float RowInset = Kit.Space2;

        /// The ground, the title (24 px in a 48 px box at the top inset), the line under it and the scrolling column
        /// that starts at 110. Returns the column.
        public static RectTransform Page(RectTransform root, string title, string sub)
        {
            Kit.Ground(root);
            var top = Metrics.InsetTop + Viewport.ExtraTop;
            var head = Ui.Txt(root, "Title", title, Kit.F800, Kit.FsTitle, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(head.rectTransform, Kit.Side, Kit.Side, top, 48);
            var line = Ui.Txt(root, "Sub", sub, Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleCenter);
            Ui.TopRow(line.rectTransform, Kit.Side, Kit.Side, top + 48, 18);
            return MenuUi.List(root, top + 78, 0, 0);
        }

        /// A row of the given height at `y` in the column: rounded so that its highlight is rounded, a line under it unless last.
        public static RectTransform Row(RectTransform list, string name, float y, float height, Action tap, bool last)
        {
            var face = Kit.Round(list, name, Color.clear, Kit.RadiusM);
            face.raycastTarget = true;
            Ui.Box(face.rectTransform, A.TopLeft, A.TopLeft, new Vector2(RowInset, -y), new Vector2(Kit.Width - 2 * RowInset, height));
            if (!last)
            {
                var rule = Ui.Img(face.rectTransform, "Line", ProceduralArt.White);
                rule.color = Kit.Surface2; rule.raycastTarget = false;
                Ui.BottomRow(rule.rectTransform, RowInset, RowInset, 0, 1);
            }
            if (tap != null) PressButton.On(face.gameObject, tap, .985f, face);
            return face.rectTransform;
        }

        /// "Слов пока нет": a floating picture, a head, a line and a button to the stories.
        public static void Empty(RectTransform root, string pic, string head, string text, Action toStories, IAnimator anim)
        {
            var box = Ui.Rect(root, "Empty");
            Ui.Box(box, A.Center, A.Center, new Vector2(0, 20), new Vector2(Kit.Width - 2 * Kit.Side, 260));
            var picture = Kit.Icon(box, "Picture", Kit.Pic(pic), 96, A.TopCenter, new Vector2(0, -8));
            picture.raycastTarget = false;
            var title = Ui.Txt(box, "Head", head, Kit.F800, Kit.FsHead, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, 0, 0, 116, 24);
            var line = Ui.Txt(box, "Text", text, Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.UpperCenter);
            Ui.TopRow(line.rectTransform, 50, 50, 152, 36);
            var button = Parts.Button(box, "К историям", ButtonStyle.Primary, toStories, Kit.Touch);
            button.Face.gameObject.name = "ToStories";
            Ui.Box(button.Root, A.TopCenter, A.TopCenter, new Vector2(0, -204), new Vector2(button.Label.preferredWidth + 2 * Kit.Space4, Kit.Touch));
            if (anim != null && anim.Enabled) anim.Play(Parts.Float(picture.rectTransform));
        }
    }
}

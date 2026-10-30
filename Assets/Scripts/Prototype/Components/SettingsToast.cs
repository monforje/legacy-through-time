using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The gear in the top-right corner and the settings popup it opens (templates/shell/settings.html): a dialog of the
    /// kit with the sound switch, Google Play (where the progress is kept), (in a story) "В меню" and "Выйти из игры".
    /// Nothing under the dim gets the tap. The gear and the popup are the shell's, they look the same over a story.
    sealed class SettingsToast
    {
        /// The Google Play row: a line under its name (the reader's Play Games name, "Подключение…"), or, with
        /// `SignIn`, a button that signs in.
        public sealed class Cloud
        {
            public string Line;
            public Action SignIn;
        }

        const float RowHeight = 52, Gap = Kit.Space2;

        readonly RectTransform layer;
        readonly IAnimator anim;
        ShellDialog dialog;

        public bool IsOpen => dialog != null && dialog.IsOpen;

        public SettingsToast(RectTransform overlay, IAnimator anim)
        {
            layer = overlay; this.anim = anim;
        }

        static float GearTop => Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop;

        /// The round gear of the kit in the top-right corner, on the line of the pills (the balance pill of a choice stands to its left).
        public static Image Gear(Transform column, Action click)
        {
            var button = Parts.IconButton(column, "gear", click, Kit.Touch, circle: true);
            Ui.Box(button.Root, A.TopRight, A.TopRight, new Vector2(-Kit.Side, -GearTop), new Vector2(Kit.Touch, Kit.Touch));
            button.Face.gameObject.name = "Gear";
            return button.Face;
        }

        /// `toMenu`: the "В меню" row; null in the menu itself. `cloud`: the Google Play row; null without Play Games.
        public void Open(Action toMenu, Action quit, Cloud cloud = null)
        {
            if (IsOpen) return;
            dialog = new ShellDialog(layer, "Настройки", anim, () => dialog = null);
            var panel = dialog.Content;
            var y = 0f;

            Sound(panel, y); y += RowHeight + Gap;
            if (cloud?.SignIn != null) { Row(panel, y, "Войти в Play Игры", cloud.SignIn); y += RowHeight + Gap; }
            else if (cloud != null) { CloudLine(panel, y, cloud.Line); y += RowHeight + Gap; }
            if (toMenu != null) { Row(panel, y, "В меню", toMenu); y += RowHeight + Gap; }
            Row(panel, y, "Выйти из игры", quit);
        }

        public void Close() => dialog?.Close();

        /// "Звук" with the switch of the kit; the settings are saved at once.
        void Sound(RectTransform panel, float top)
        {
            var row = Kit.Round(panel, "Sound", Kit.Surface, Kit.RadiusM);
            Ui.TopRow(row.rectTransform, 0, 0, top, RowHeight);
            Kit.Icon(row.rectTransform, "Icon", Kit.Glyph("sound"), 24, A.MidLeft, new Vector2(24, 0)).raycastTarget = false;
            var label = Ui.Txt(row.rectTransform, "Text", "Звук", Art.Action, Kit.FsBody, Kit.Text, TextAnchor.MiddleLeft);
            Ui.Box(label.rectTransform, A.MidLeft, A.MidLeft, new Vector2(48, 0), new Vector2(160, 30));
            var sw = Parts.Switch(row.rectTransform, Settings.Sound, on => { Settings.Sound = on; ShellToast.Show(layer, "Сохранено", anim, 1.5f, 96); }, anim);
            Ui.Box(sw.Root, A.MidRight, A.MidRight, new Vector2(-8, 0), new Vector2(56, Kit.Touch));
        }

        /// "Google Play Игры" with a line under it: who is signed in, or that it is still connecting.
        static void CloudLine(RectTransform panel, float top, string line)
        {
            var row = Kit.Round(panel, "Cloud", Kit.Surface, Kit.RadiusM);
            Ui.TopRow(row.rectTransform, 0, 0, top, RowHeight);
            Kit.Icon(row.rectTransform, "Icon", Kit.Glyph("check"), 22, A.MidLeft, new Vector2(24, 0)).raycastTarget = false;
            var name = Ui.Txt(row.rectTransform, "Text", "Google Play Игры", Art.Action, Kit.FsBody, Kit.Text, TextAnchor.MiddleLeft);
            Ui.Box(name.rectTransform, A.MidLeft, A.MidLeft, new Vector2(48, 9), new Vector2(240, 22));
            var sub = Ui.Txt(row.rectTransform, "Line", line ?? "", Art.Body, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleLeft);
            Ui.Box(sub.rectTransform, A.MidLeft, A.MidLeft, new Vector2(48, -11), new Vector2(240, 18));
        }

        /// A full-width button of the kit: the popup closes first, then the action runs.
        void Row(RectTransform panel, float top, string text, Action click)
        {
            var button = Parts.Button(panel, text, ButtonStyle.Neutral, () => { Close(); click?.Invoke(); }, RowHeight);
            Ui.TopRow(button.Root, 0, 0, top, RowHeight);
        }
    }
}

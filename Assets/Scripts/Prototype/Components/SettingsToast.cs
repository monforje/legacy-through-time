using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The gear in the top-right corner and the settings page it opens (templates/shell/settings.html): the title with a
    /// close button, then three groups - the switches and the language, the sign-in (Google Play: who is signed in, or the
    /// button that signs in), the links. In a story one more row, "В меню". Nothing under the page gets the tap.
    sealed class SettingsToast
    {
        /// The Google Play row: a line under its name (the reader's Play Games name, "Подключение…"), or, with
        /// `SignIn`, a button that signs in.
        public sealed class Cloud
        {
            public string Line;
            public Action SignIn;
        }

        const float RowHeight = 52, GroupPad = 4, GroupGap = Kit.Space3;

        readonly RectTransform layer;
        readonly IAnimator anim;
        RectTransform page;
        CanvasGroup group;

        public bool IsOpen => page;

        public SettingsToast(RectTransform overlay, IAnimator anim)
        {
            layer = overlay; this.anim = anim;
        }

        static GameObject gearObject;

        /// The gear is hidden where the screen has its own button in that corner (the chapters have the chat).
        public static void ShowGear(bool on) { if (gearObject) gearObject.SetActive(on); }

        static float GearTop => Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop;

        /// The round gear of the kit in the top-right corner, on the line of the pills (the balance pill of a choice stands to its left).
        public static Image Gear(Transform column, Action click)
        {
            var button = Parts.IconButton(column, "gear", click, Kit.Touch, circle: true);
            Ui.Box(button.Root, A.TopRight, A.TopRight, new Vector2(-Kit.Side, -GearTop), new Vector2(Kit.Touch, Kit.Touch));
            button.Face.gameObject.name = "Gear";
            gearObject = button.Root.gameObject;
            return button.Face;
        }

        /// `toMenu`: the "В меню" row; null in the menu itself. `quit` is not shown (Android Back leaves the app). `cloud`: the
        /// Google Play state; null without Play Games.
        public void Open(Action toMenu, Action quit, Cloud cloud = null)
        {
            if (IsOpen) return;
            page = Ui.Rect(layer, "Settings");
            Ui.Bleed(page);
            group = page.gameObject.AddComponent<CanvasGroup>();
            var ground = Kit.Ground(page);
            ground.raycastTarget = true;

            var top = Metrics.InsetTop + Viewport.ExtraTop;
            var title = Ui.Txt(page, "Title", "Настройки", Kit.F800, Kit.FsTitle, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(title.rectTransform, Kit.Side + 56, Kit.Side + 56, top, Kit.Touch);
            var close = Parts.IconButton(page, "close", Close, Kit.Touch, circle: true);
            close.Face.color = Color.clear;
            Ui.Box(close.Root, A.TopRight, A.TopRight, new Vector2(-Kit.Side, -top), new Vector2(Kit.Touch, Kit.Touch));

            var list = MenuUi.List(page, top + Kit.Touch, Viewport.ExtraBottom, 0);
            var y = 0f;
            y += Switches(list, y) + GroupGap;
            y += Account(list, y, cloud) + GroupGap;
            y += Links(list, y, toMenu) + GroupGap;
            MenuUi.SetHeight(list, y);

            if (!anim.Enabled) return;
            group.alpha = 0;
            anim.Play(Tween.To(Kit.Dur.Normal, e => { if (group) group.alpha = e; }, Easing.Linear));
        }

        public void Close()
        {
            if (!IsOpen) return;
            var closing = page; var g = group;
            page = null;
            g.blocksRaycasts = false;
            if (!anim.Enabled) { UnityEngine.Object.Destroy(closing.gameObject); return; }
            anim.Play(Tween.To(Kit.Dur.Fast, e => { if (g) g.alpha = 1 - e; }, Easing.Linear));
            anim.Play(Tween.After(Kit.Dur.Fast + .02f, () => { if (closing) UnityEngine.Object.Destroy(closing.gameObject); }));
        }

        // ------------------------------------------------------------------ the groups
        RectTransform Group(RectTransform list, string name, float top, float height)
        {
            var card = Kit.Round(list, name, Kit.Surface, Kit.RadiusM);
            Ui.Box(card.rectTransform, A.TopLeft, A.TopLeft, new Vector2(Kit.Side, -top), new Vector2(Metrics.RefWidth - 2 * Kit.Side, height));
            return card.rectTransform;
        }

        /// A row of a group: 52 high, the glyph at 12, the label at 46 from the row's left.
        RectTransform Row(RectTransform group, int index, string glyph, string label, float labelRight = 46)
        {
            var width = Metrics.RefWidth - 2 * Kit.Side - 2 * GroupPad;
            var row = Kit.Round(group, "Row " + label, Color.clear, Kit.RadiusS);
            row.raycastTarget = true;
            Ui.Box(row.rectTransform, A.TopLeft, A.TopLeft, new Vector2(GroupPad, -(GroupPad + index * RowHeight)), new Vector2(width, RowHeight));
            Kit.Icon(row.rectTransform, "Glyph", Kit.Glyph(glyph), 22, A.MidLeft, new Vector2(12, 0)).raycastTarget = false;
            var text = Ui.Txt(row.rectTransform, "Label", label, Kit.F700, Kit.FsBody, Kit.Text, TextAnchor.MiddleLeft);
            Ui.Stretch(text.rectTransform, 46, 0, labelRight, 0);          // the label stops before the switch or the chevron, and wraps
            return row.rectTransform;
        }

        float Switches(RectTransform list, float y)
        {
            var card = Group(list, "Switches", y, 3 * RowHeight + 2 * GroupPad);
            var notify = Row(card, 0, "bell", "Уведомления", 68);
            var sw = Parts.Switch(notify, Settings.Notifications, on => { Settings.Notifications = on; Saved(); }, anim);
            Ui.Box(sw.Root, A.MidRight, A.MidRight, new Vector2(-12, 0), new Vector2(56, Kit.Touch));
            var sound = Row(card, 1, "sound", "Звук", 68);
            var sw2 = Parts.Switch(sound, Settings.Sound, on => { Settings.Sound = on; Saved(); }, anim);
            Ui.Box(sw2.Root, A.MidRight, A.MidRight, new Vector2(-12, 0), new Vector2(56, Kit.Touch));

            // the language: "<  Русский  >"
            var lang = Row(card, 2, "globe", "Язык", 188);
            var output = Ui.Txt(lang, "Value", Settings.Languages[Settings.Language], Kit.F800, Kit.FsBody, Kit.Text, TextAnchor.MiddleCenter);
            Ui.Box(output.rectTransform, A.MidRight, A.MidRight, new Vector2(-12 - 44, 0), new Vector2(80, 20));
            void Step(int by) { Settings.Language += by; output.text = Settings.Languages[Settings.Language]; Saved(); }
            var prev = Parts.IconButton(lang, "chevron-l", () => Step(-1), Kit.Touch, circle: true);
            prev.Face.color = Color.clear; prev.Glyph.rectTransform.sizeDelta = new Vector2(22, 22);
            Ui.Box(prev.Root, A.MidRight, A.MidRight, new Vector2(-12 - 44 - 80 - 4, 0), new Vector2(Kit.Touch, Kit.Touch));
            var next = Parts.IconButton(lang, "chevron-r", () => Step(1), Kit.Touch, circle: true);
            next.Face.color = Color.clear; next.Glyph.rectTransform.sizeDelta = new Vector2(22, 22);
            Ui.Box(next.Root, A.MidRight, A.MidRight, new Vector2(-12 + 0, 0), new Vector2(Kit.Touch, Kit.Touch));
            return 3 * RowHeight + 2 * GroupPad;
        }

        /// "Войдите, чтобы прогресс сохранялся в облаке" and the three buttons; with Google Play connected, who is signed in.
        float Account(RectTransform list, float y, Cloud cloud)
        {
            const float pad = Kit.Space3, buttonHeight = 44, gap = Kit.Space2;
            var signedIn = cloud != null && cloud.SignIn == null;
            var inner = Metrics.RefWidth - 2 * Kit.Side - 2 * pad;
            var count = signedIn ? 0 : 3;
            var line = signedIn ? (string.IsNullOrEmpty(cloud.Line) ? "Прогресс сохраняется в облаке" : "Google Play Игры · " + cloud.Line)
                                : "Войдите, чтобы прогресс сохранялся в облаке";
            var probe = Ui.Txt(list, "Probe", line, Kit.F600, Kit.FsSmall, Kit.Muted);
            var noteHeight = Mathf.Max(18, Mathf.Ceil(Ui.TextHeight(probe, inner)));
            UnityEngine.Object.Destroy(probe.gameObject);
            var height = pad + noteHeight + (count > 0 ? gap + count * buttonHeight + (count - 1) * gap : 0) + pad;
            var card = Group(list, "Account", y, height);
            var note = Ui.Txt(card, "Note", line, Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleCenter);
            Ui.TopRow(note.rectTransform, pad, pad, pad, noteHeight);
            if (signedIn) return height;

            var top = pad + noteHeight + gap;
            string[] labels = { "Войти с почтой", "Войти через Google", "Войти через Apple" };
            for (var i = 0; i < 3; i++)
            {
                var k = i;
                var button = Parts.Button(card, labels[i], ButtonStyle.Neutral, () =>
                {
                    if (k == 1 && cloud?.SignIn != null) { Close(); cloud.SignIn(); }
                    else ShellToast.Show(page, k == 1 ? "Вход через Google доступен на телефоне" : "Скоро: " + labels[k].ToLowerInvariant(), anim);
                }, buttonHeight, Kit.RadiusS);
                Ui.TopRow(button.Root, pad, pad, top + i * (buttonHeight + gap), buttonHeight);
            }
            return height;
        }

        float Links(RectTransform list, float y, Action toMenu)
        {
            var rows = new (string glyph, string label, Action tap)[]
            {
                ("support", "Поддержка", () => ShellToast.Show(page, "Скоро: чат с поддержкой", anim)),
                ("shield", "Конфиденциальность", () => ShellToast.Show(page, "Скоро: политика конфиденциальности", anim)),
                ("doc", "Пользовательское соглашение", () => ShellToast.Show(page, "Скоро: пользовательское соглашение", anim)),
                ("tools", "Инструменты", () => ShellToast.Show(page, "Скоро: инструменты", anim)),
            };
            var count = rows.Length + (toMenu != null ? 1 : 0);
            var card = Group(list, "Links", y, count * RowHeight + 2 * GroupPad);
            for (var i = 0; i < rows.Length; i++)
            {
                var tap = rows[i].tap;
                var row = Row(card, i, rows[i].glyph, rows[i].label);
                Kit.Icon(row, "Chevron", Kit.Glyph("chevron-r"), 22, A.MidRight, new Vector2(-12, 0)).raycastTarget = false;
                PressButton.On(row.gameObject, tap, .98f);
            }
            if (toMenu != null)
            {
                var row = Row(card, rows.Length, "back", "В меню");
                Kit.Icon(row, "Chevron", Kit.Glyph("chevron-r"), 22, A.MidRight, new Vector2(-12, 0)).raycastTarget = false;
                PressButton.On(row.gameObject, () => { Close(); toMenu(); }, .98f);
            }
            return count * RowHeight + 2 * GroupPad;
        }

        void Saved() => ShellToast.Show(page, "Сохранено", anim);
    }
}

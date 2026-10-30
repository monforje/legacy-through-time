using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    enum ButtonStyle { Primary, Neutral }

    /// The parts of the shell kit (docs/brainstorm/templates/shell/kit.html): buttons, the currency pill, the switch,
    /// the segmented control, the search field. Each one is built in code from Kit tokens; the motion comes from Kit.Dur.
    static class Parts
    {
        public static string Number(int n) => n.ToString("N0", CultureInfo.GetCultureInfo("ru-RU"));

        // ------------------------------------------------------------------ buttons
        /// A text button: Primary is the accent fill with the ink text, Neutral is the raised surface with the light text.
        public static ShellButton Button(Transform parent, string label, ButtonStyle style, Action click, float height = Kit.Touch, int radius = Kit.RadiusM)
        {
            var primary = style == ButtonStyle.Primary;
            var face = Kit.Round(parent, "Button", primary ? Kit.Accent : Kit.Surface2, radius);
            face.raycastTarget = true;
            var text = Ui.Txt(face.rectTransform, "Label", label, Art.Action, Kit.FsBody, primary ? Kit.AccentInk : Kit.Text, TextAnchor.MiddleCenter);
            Ui.Stretch(text.rectTransform, 8, 0, 8, 0);
            var b = new ShellButton { Root = face.rectTransform, Face = face, Label = text, Ink = primary ? Kit.AccentInk : Kit.Text };
            b.Group = Ui.Group(face);
            b.Press = PressButton.On(face.gameObject, click, .97f);
            b.Root.sizeDelta = new Vector2(b.Root.sizeDelta.x, height);
            return b;
        }

        /// A round or rounded-square button with a glyph: the gear, close, back.
        public static ShellButton IconButton(Transform parent, string glyph, Action click, float size = Kit.Touch, bool circle = true, bool accent = false)
        {
            var face = Kit.Round(parent, "Icon button", accent ? Kit.Accent : Kit.Surface2, circle ? (int)(size / 2) : Kit.RadiusS);
            face.raycastTarget = true;
            face.rectTransform.sizeDelta = new Vector2(size, size);
            var icon = Kit.Icon(face.rectTransform, "Glyph", Kit.Glyph(glyph, ink: accent), Mathf.Round(size * .64f));
            var b = new ShellButton { Root = face.rectTransform, Face = face, Glyph = icon };
            b.Group = Ui.Group(face);
            b.Press = PressButton.On(face.gameObject, click, .94f);
            return b;
        }

        // ------------------------------------------------------------------ currency pill
        /// The pill of the top bar: the icon, the number in the middle and a "+" disc concentric with the rounded end.
        public static ShellPill Pill(Transform parent, string pic, int value, Action plus)
        {
            var bg = Kit.Round(parent, "Pill " + pic, Kit.Surface, 22);
            bg.raycastTarget = true;
            var pill = new ShellPill { Root = bg.rectTransform, Shown = value };
            var icon = Kit.Icon(bg.rectTransform, "Icon", Kit.Pic(pic), 32, A.MidLeft, new Vector2(18, 0));
            icon.raycastTarget = false;
            pill.Icon = icon;
            pill.Value = Ui.Txt(bg.rectTransform, "Value", Number(value), Art.Sub, Kit.FsBody, Kit.Text, TextAnchor.MiddleCenter);
            Ui.Stretch(pill.Value.rectTransform, 40, 0, 48, 0);
            if (plus != null)
            {
                // the disc is the end cap of the pill minus 4 px: concentric, 36 px inside the 44 px tap target
                var hit = Ui.Img(bg.rectTransform, "Plus", ProceduralArt.White);
                hit.color = Color.clear; hit.raycastTarget = true;
                Ui.Box(hit.rectTransform, A.MidRight, A.MidRight, Vector2.zero, new Vector2(Kit.Touch, Kit.Touch));
                var disc = Ui.Img(hit.rectTransform, "Disc", ProceduralArt.Disc);
                disc.color = Kit.Surface2; disc.raycastTarget = false;
                Ui.Box(disc.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(36, 36));
                var glyph = Kit.Icon(disc.rectTransform, "Glyph", Kit.Glyph("plus"), 22);
                glyph.raycastTarget = false;
                PressButton.On(hit.gameObject, plus, .92f);
            }
            return pill;
        }

        // ------------------------------------------------------------------ switch
        /// The switch of a settings row: a 52x30 track in a 56x44 tap target, the thumb slides in Kit.Dur.Normal.
        public static ShellSwitch Switch(Transform parent, bool on, Action<bool> changed, IAnimator anim)
        {
            var hit = Ui.Img(parent, "Switch", ProceduralArt.White);
            hit.color = Color.clear; hit.raycastTarget = true;
            hit.rectTransform.sizeDelta = new Vector2(56, Kit.Touch);
            var track = Kit.Round(hit.rectTransform, "Track", on ? Kit.Accent : Kit.Line, 15);
            track.raycastTarget = false;
            Ui.Box(track.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(52, 30));
            var thumb = Ui.Img(track.rectTransform, "Thumb", ProceduralArt.Disc);
            thumb.color = on ? Kit.AccentInk : Kit.Text; thumb.raycastTarget = false;
            Ui.Box(thumb.rectTransform, A.MidLeft, A.MidLeft, new Vector2(on ? 25 : 3, 0), new Vector2(24, 24));
            var sw = new ShellSwitch { Root = hit.rectTransform, Track = track, Thumb = thumb, On = on, Anim = anim };
            PressButton.On(hit.gameObject, () => { sw.Set(!sw.On, true); changed?.Invoke(sw.On); }, .96f);
            return sw;
        }

        // ------------------------------------------------------------------ segmented control
        /// Two or more options in a pill; the accent indicator slides to the chosen one.
        public static ShellSegmented Segmented(Transform parent, string[] labels, int selected, Action<int> changed, IAnimator anim)
        {
            var bg = Kit.Round(parent, "Segmented", Kit.Surface, 22);
            bg.raycastTarget = true;
            bg.rectTransform.sizeDelta = new Vector2(bg.rectTransform.sizeDelta.x, 48);
            var seg = new ShellSegmented { Root = bg.rectTransform, Count = labels.Length, Anim = anim };
            seg.Indicator = Kit.Round(bg.rectTransform, "Indicator", Kit.Accent, 20);
            seg.Indicator.raycastTarget = false;
            seg.Labels = new Text[labels.Length];
            for (var i = 0; i < labels.Length; i++)
            {
                var k = i;
                var cell = Ui.Img(bg.rectTransform, "Option " + labels[i], ProceduralArt.White);
                cell.color = Color.clear; cell.raycastTarget = true;
                cell.rectTransform.anchorMin = new Vector2((float)i / labels.Length, 0);
                cell.rectTransform.anchorMax = new Vector2((float)(i + 1) / labels.Length, 1);
                cell.rectTransform.offsetMin = cell.rectTransform.offsetMax = Vector2.zero;
                seg.Labels[i] = Ui.Txt(cell.rectTransform, "Label", labels[i], Art.Action, Kit.FsBody, Kit.Muted, TextAnchor.MiddleCenter);
                Ui.Stretch(seg.Labels[i].rectTransform);
                PressButton.On(cell.gameObject, () => { seg.Select(k, true); changed?.Invoke(k); }, .97f);
            }
            seg.Select(selected, false);
            return seg;
        }

        // ------------------------------------------------------------------ search field
        /// The search field: 36 px high on screen, a 44 px tap target; the glyph, the text, a clear button when there is text.
        public static ShellSearch Search(Transform parent, string hint, Action<string> changed)
        {
            var box = Kit.Round(parent, "Search", Kit.Surface, Kit.RadiusS);
            box.raycastTarget = true;
            box.rectTransform.sizeDelta = new Vector2(box.rectTransform.sizeDelta.x, 36);
            var glyph = Kit.Icon(box.rectTransform, "Glyph", Kit.Glyph("search"), 18, A.MidLeft, new Vector2(20, 0));
            glyph.raycastTarget = false; glyph.color = new Color(1, 1, 1, .6f);
            var text = Ui.Txt(box.rectTransform, "Text", "", Art.Body, Kit.FsBody, Kit.Text, TextAnchor.MiddleLeft);
            text.supportRichText = false;
            Ui.Stretch(text.rectTransform, 40, 2, 40, 2);
            var ph = Ui.Txt(box.rectTransform, "Placeholder", hint, Art.Body, Kit.FsBody, Kit.Muted, TextAnchor.MiddleLeft);
            Ui.Stretch(ph.rectTransform, 40, 2, 40, 2);
            var field = box.gameObject.AddComponent<InputField>();
            field.targetGraphic = box; field.textComponent = text; field.placeholder = ph;
            field.lineType = InputField.LineType.SingleLine;
            field.caretColor = Kit.Text; field.customCaretColor = true;
            field.selectionColor = Kit.Accent.WithAlpha(.35f);
            var search = new ShellSearch { Root = box.rectTransform, Field = field };

            var clear = Ui.Img(box.rectTransform, "Clear", ProceduralArt.White);
            clear.color = Color.clear; clear.raycastTarget = true;
            Ui.Box(clear.rectTransform, A.MidRight, A.MidRight, Vector2.zero, new Vector2(40, 36));
            var x = Kit.Icon(clear.rectTransform, "Glyph", Kit.Glyph("close"), 16);
            x.raycastTarget = false; x.color = new Color(1, 1, 1, .7f);
            clear.gameObject.SetActive(false);
            PressButton.On(clear.gameObject, () => { field.text = ""; }, .9f);
            field.onValueChanged.AddListener(v => { clear.gameObject.SetActive(v.Length > 0); changed?.Invoke(v); });
            return search;
        }
    }

    sealed class ShellButton
    {
        public RectTransform Root;
        public Image Face, Glyph;
        public Text Label;
        public Color Ink;
        public CanvasGroup Group;
        public PressButton Press;
        /// A ready button breathes (the owner runs it while this is true).
        public bool Pulsing;
        bool loading;
        Image[] dots;

        /// A button that cannot be pressed now: 45% opaque, as in the kit.
        public void SetEnabled(bool on) { Press.Interactable = on && !loading; Group.alpha = on ? 1 : .45f; }

        public void SetLabel(string text) { if (Label) Label.text = text; }

        /// Three dots instead of the label while the action runs; the taps are ignored meanwhile.
        public void SetLoading(bool on, IAnimator anim)
        {
            if (loading == on) return;
            loading = on; Press.Interactable = !on;
            if (Label) Label.enabled = !on;
            if (on)
            {
                dots = new Image[3];
                for (var i = 0; i < 3; i++)
                {
                    dots[i] = Ui.Img(Root, "Dot", ProceduralArt.Disc);
                    dots[i].raycastTarget = false; dots[i].color = Ink;
                    Ui.Box(dots[i].rectTransform, A.Center, A.Center, new Vector2((i - 1) * 12, 0), new Vector2(7, 7));
                }
                if (anim != null && anim.Enabled) anim.Play(Pulse());
            }
            else if (dots != null) { foreach (var d in dots) if (d) UnityEngine.Object.Destroy(d.gameObject); dots = null; }
        }

        System.Collections.IEnumerator Pulse()
        {
            var t = 0f;
            while (loading && dots != null)
            {
                for (var i = 0; i < dots.Length; i++)
                    if (dots[i]) dots[i].color = Ink.WithAlpha(.3f + .7f * Mathf.Max(0, Mathf.Sin((t * 6f) - i * .9f)));
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    sealed class ShellPill
    {
        public RectTransform Root;
        public Image Icon;
        public Text Value;
        public int Shown;

        public void Set(int value) { Shown = value; Value.text = Parts.Number(value); }

        /// The number runs to the new value and the pill bumps (a purchase, a reward).
        public void Count(int to, IAnimator anim)
        {
            if (anim == null || !anim.Enabled || to == Shown) { Set(to); return; }
            var from = Shown;
            anim.Play(Tween.To(Kit.Dur.Slow * 1.5f, e =>
            {
                if (!Value) return;
                Shown = Mathf.RoundToInt(Mathf.Lerp(from, to, e));
                Value.text = Parts.Number(Shown);
            }));
            anim.Play(Tween.To(Kit.Dur.Slow, e => { if (Root) Root.localScale = Vector3.one * (1f + .12f * Mathf.Sin(e * Mathf.PI)); }, Easing.Linear));
        }
    }

    sealed class ShellSwitch
    {
        public RectTransform Root;
        public Image Track, Thumb;
        public bool On;
        public IAnimator Anim;

        public void Set(bool on, bool animate)
        {
            On = on;
            var from = Thumb.rectTransform.anchoredPosition.x; var to = on ? 25f : 3f;
            Track.color = on ? Kit.Accent : Kit.Line;
            Thumb.color = on ? Kit.AccentInk : Kit.Text;
            if (!animate || Anim == null || !Anim.Enabled) { Thumb.rectTransform.anchoredPosition = new Vector2(to, 0); return; }
            Anim.Play(Tween.To(Kit.Dur.Normal, e => { if (Thumb) Thumb.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(from, to, e), 0); }));
        }
    }

    sealed class ShellSegmented
    {
        public RectTransform Root;
        public Image Indicator;
        public Text[] Labels;
        public int Count, Index;
        public IAnimator Anim;

        public void Select(int index, bool animate)
        {
            Index = index;
            var from = Indicator.rectTransform.anchorMin.x; var to = (float)index / Count;
            for (var i = 0; i < Labels.Length; i++) Labels[i].color = i == index ? Kit.AccentInk : Kit.Muted;
            void Place(float x)
            {
                var r = Indicator.rectTransform;
                r.anchorMin = new Vector2(x, 0); r.anchorMax = new Vector2(x + 1f / Count, 1);
                r.offsetMin = new Vector2(4, 4); r.offsetMax = new Vector2(-4, -4);
            }
            if (!animate || Anim == null || !Anim.Enabled) { Place(to); return; }
            Anim.Play(Tween.To(Kit.Dur.Normal, e => { if (Indicator) Place(Mathf.Lerp(from, to, e)); }));
        }
    }

    sealed class ShellSearch
    {
        public RectTransform Root;
        public InputField Field;
        public string Query => Field ? Field.text.Trim() : "";
    }
}

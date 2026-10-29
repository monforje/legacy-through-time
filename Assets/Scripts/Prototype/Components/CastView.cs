using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The character of the scene between the background and the plate, life-size like in a romance visual novel: ONE
    /// person on the screen, the one who speaks now, in full length, the head in the upper third and the legs under the
    /// plate. A new speaker rides in from the side of the screen while the previous one rides out (the heroine from the
    /// right, the others from the left; the right one is mirrored, so that each looks into the screen). Pictures are
    /// Resources/Art/Characters; a shorter character (a child) stands on the same ground, so her head is lower.
    sealed class CastView
    {
        const float Speed = 5, SlideSpeed = 8, SlideReach = 1f;
        // An adult is as tall as the screen with the head top a fifth below the top edge (the legs are cut by the bottom edge); a shorter
        // one stands on a ground line that rises towards the bottom edge, so that the face stays above the plate.
        const float HeightShare = 1f, WidthShare = 1f, GroundSmall = .96f, GroundAdult = 1.2f;

        sealed class Slot
        {
            public Image Image;
            public float Alpha, GoalAlpha, Slide = 1, GoalSlide = 1;    // slide: 1 = outside the screen on its own side, 0 = in its place
            public float Height = 1;
        }

        readonly Slot left, right;
        readonly RectTransform root;
        static readonly Dictionary<string, Sprite> sprites = new();
        int width, height;

        public CastView(RectTransform canvas)
        {
            root = Ui.Rect(canvas, "Cast");
            Ui.Stretch(root);
            left = Make("CastLeft"); right = Make("CastRight");
        }

        Slot Make(string name)
        {
            var image = Ui.Img(root, name, null);
            image.enabled = false; image.preserveAspect = true;
            var r = image.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(.5f, 0); r.pivot = new Vector2(.5f, 0);
            // every picture is drawn looking a little to the right: the right one is mirrored, it looks to the left, into the screen
            if (name == "CastRight") r.localScale = new Vector3(-1, 1, 1);
            return new Slot { Image = image };
        }

        /// The speaker comes in (the heroine from the right, the others from the left) and the other one goes out.
        /// `fallback` is the picture without a face, when the one asked for was not drawn; `height` is 1 for an adult.
        public void Show(string name, string fallback, bool heroine, float characterHeight)
        {
            var (into, outOf) = heroine ? (right, left) : (left, right);
            var sprite = Load(name) ?? Load(fallback);
            Leave(outOf);
            if (!sprite) { Leave(into); return; }
            if (into.Image.sprite != sprite || into.Height != characterHeight)
            {
                into.Image.sprite = sprite; into.Height = characterHeight; Fit(into);
            }
            if (!into.Image.enabled) { into.Slide = 1; into.Alpha = 0; }      // comes in from the side
            into.Image.enabled = true;
            into.GoalSlide = 0; into.GoalAlpha = 1;
            Place(into);
        }

        public void Clear() { Leave(left); Leave(right); }

        static void Leave(Slot slot) { slot.GoalAlpha = 0; slot.GoalSlide = 1; }

        static Sprite Load(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (sprites.TryGetValue(name, out var s)) return s;
            return sprites[name] = Resources.Load<Sprite>("Art/Characters/" + name);
        }

        public void Tick(float deltaTime)
        {
            if (Screen.width != width || Screen.height != height) { Fit(left); Fit(right); width = Screen.width; height = Screen.height; }
            foreach (var s in new[] { left, right })
            {
                if (!s.Image.enabled) continue;
                s.Alpha = Mathf.MoveTowards(s.Alpha, s.GoalAlpha, Speed * deltaTime);
                s.Slide = Mathf.Lerp(s.Slide, s.GoalSlide, 1 - Mathf.Exp(-SlideSpeed * deltaTime));
                if (Mathf.Abs(s.Slide - s.GoalSlide) < .002f) s.Slide = s.GoalSlide;
                Place(s);
                s.Image.color = new Color(1, 1, 1, s.Alpha);
                if (s.Alpha <= 0 && s.GoalAlpha <= 0) { s.Image.enabled = false; s.Image.sprite = null; }
            }
        }

        /// Size by the screen: a share of its height times the character's height, never wider than the screen.
        void Fit(Slot slot)
        {
            var sprite = slot.Image.sprite;
            if (!sprite) return;
            var aspect = sprite.rect.width / sprite.rect.height;
            var h = Viewport.Height * HeightShare * slot.Height;
            var w = h * aspect;
            if (w > Viewport.Width * WidthShare) { w = Viewport.Width * WidthShare; h = w / aspect; }
            slot.Image.rectTransform.sizeDelta = new Vector2(w, h);
            Place(slot);
        }

        void Place(Slot slot)
        {
            var side = slot == right ? 1 : -1;
            var h = slot.Image.rectTransform.sizeDelta.y;
            // the line of the feet from the top of the screen: below the screen for an adult (head top at HeadTop), higher for a child
            var ground = Mathf.Lerp(GroundSmall, GroundAdult, Mathf.Clamp01((slot.Height - .5f) / .5f));
            var y = Viewport.Height * (1 - ground);
            slot.Image.rectTransform.anchoredPosition = new Vector2(side * Viewport.Width * SlideReach * slot.Slide, y);
        }
    }
}

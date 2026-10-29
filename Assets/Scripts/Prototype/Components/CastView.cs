using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The characters of the scene between the background and the plate: the heroine on one side, the other on the
    /// opposite one, the speaker bright and in front, the other dimmer. Pictures come from Resources/Art/Characters.
    sealed class CastView
    {
        const bool HeroineRight = true;
        const float Speed = 5, Dim = .62f;
        // how big and how high: the plate covers the lower part of the screen, so the figure stands above it
        const float SlideSpeed = 8, SlideReach = .75f;      // the figure rides in from (and out to) the side of the screen
        const float HeightShare = .5f, Lift = .1f, WidthShare = 1f, Side = .2f, ShadeShare = .3f;

        sealed class Slot
        {
            public Image Image;
            public string Name;
            public float Alpha, Bright = 1, GoalAlpha, GoalBright = 1;
            public float Slide = 1, GoalSlide = 1;       // 1: outside the screen on its own side, 0: in its place
        }

        readonly Slot left, right;
        readonly RectTransform root;
        readonly Image shade;           // the cut of the figures at the waist sinks into this dark gradient at the screen's bottom
        static readonly Dictionary<string, Sprite> sprites = new();
        int width, height;

        public CastView(RectTransform canvas)
        {
            root = Ui.Rect(canvas, "Cast");
            Ui.Stretch(root);
            left = Make("CastLeft"); right = Make("CastRight");
            shade = Ui.Img(root, "CastShade", Gradient()); shade.enabled = false;
            var r = shade.rectTransform;
            r.anchorMin = Vector2.zero; r.anchorMax = new Vector2(1, 0); r.pivot = new Vector2(.5f, 0);
        }

        static Sprite gradient;

        /// One column of pixels, clear at the top and dark at the bottom.
        static Sprite Gradient()
        {
            if (gradient) return gradient;
            const int n = 64;
            var texture = new Texture2D(1, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (var y = 0; y < n; y++)
            {
                var t = Mathf.SmoothStep(0, 1, Mathf.Clamp01((float)(n - 1 - y) / (n - 1) * 1.6f));   // y = 0 is the bottom
                texture.SetPixel(0, y, new Color(.07f, .05f, .04f, t));
            }
            texture.Apply();
            return gradient = Sprite.Create(texture, new Rect(0, 0, 1, n), new Vector2(.5f, 0));
        }

        Slot Make(string name)
        {
            var image = Ui.Img(root, name, null);
            image.enabled = false; image.preserveAspect = true;
            var r = image.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(.5f, 0); r.pivot = new Vector2(.5f, 0);
            return new Slot { Image = image };
        }

        /// The heroine (her slot is the right one) and the other; a null name leaves the slot empty. `speaker` is
        /// 1 for the heroine, -1 for the other, 0 for neither (the narration keeps them as they are).
        public void Show(string heroine, string heroineFallback, string other, string otherFallback, int speaker)
        {
            var (heroineSlot, otherSlot) = HeroineRight ? (right, left) : (left, right);
            Put(heroineSlot, heroine, heroineFallback, speaker >= 0 ? 1 : Dim);
            Put(otherSlot, other, otherFallback, speaker <= 0 ? 1 : Dim);
        }

        public void Clear()
        {
            foreach (var s in new[] { left, right }) { s.GoalAlpha = 0; s.GoalSlide = 1; s.Name = null; }
        }

        void Put(Slot slot, string name, string fallback, float bright)
        {
            if (name == null) { slot.GoalAlpha = 0; slot.GoalSlide = 1; slot.Name = null; return; }
            var sprite = Load(name) ?? Load(fallback);
            if (!sprite) { slot.GoalAlpha = 0; slot.GoalSlide = 1; slot.Name = null; return; }
            if (slot.Image.sprite != sprite) { slot.Image.sprite = sprite; Fit(slot); }
            slot.Name = name;
            if (!slot.Image.enabled) { slot.Slide = 1; slot.Alpha = 0; }     // comes in from the side
            slot.Image.enabled = true;
            slot.GoalSlide = 0;
            slot.GoalAlpha = 1; slot.GoalBright = bright;
            slot.Image.transform.SetAsLastSibling();           // the one told last is in front ...
            if (bright < 1) slot.Image.transform.SetAsFirstSibling();   // ... and the silent one behind
            shade.transform.SetAsLastSibling();                         // the dark gradient is over both
        }

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
                s.Bright = Mathf.MoveTowards(s.Bright, s.GoalBright, Speed * deltaTime);
                s.Slide = Mathf.Lerp(s.Slide, s.GoalSlide, 1 - Mathf.Exp(-SlideSpeed * deltaTime));
                if (Mathf.Abs(s.Slide - s.GoalSlide) < .002f) s.Slide = s.GoalSlide;
                Place(s);
                s.Image.color = new Color(s.Bright, s.Bright, s.Bright, s.Alpha);
                if (s.Alpha <= 0 && s.GoalAlpha <= 0) { s.Image.enabled = false; s.Image.sprite = null; }
            }
            var shown = Mathf.Max(left.Alpha, right.Alpha);
            shade.enabled = shown > 0;
            shade.color = new Color(1, 1, 1, shown);
            shade.rectTransform.sizeDelta = new Vector2(0, Viewport.Height * ShadeShare);
        }

        /// Size and place by the screen: the figure's height is a share of the screen, but never wider than the screen.
        void Fit(Slot slot)
        {
            var sprite = slot.Image.sprite;
            if (!sprite) return;
            var aspect = sprite.rect.width / sprite.rect.height;
            var h = Viewport.Height * HeightShare;
            var w = h * aspect;
            if (w > Viewport.Width * WidthShare) { w = Viewport.Width * WidthShare; h = w / aspect; }
            var r = slot.Image.rectTransform;
            r.sizeDelta = new Vector2(w, h);
            Place(slot);
        }

        void Place(Slot slot)
        {
            var side = slot == right ? 1 : -1;
            var x = Viewport.Width * (Side + SlideReach * slot.Slide);
            slot.Image.rectTransform.anchoredPosition = new Vector2(side * x, Viewport.Height * Lift);
        }
    }
}

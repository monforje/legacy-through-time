using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The painted background of the current location (Resources/Art/Backgrounds/<id>.png) fades in over the previous
    /// one. A location without a picture (and `black`) fades the pictures out to the stand-in colour of its tint.
    sealed class BackdropTint
    {
        const float Speed = 6, FadeSeconds = .6f;
        readonly Image colour, back, front;
        Color goal = Theme.Backdrop;
        string id;
        float fade = 1;
        int width, height;

        public BackdropTint(Image colour, Image back, Image front)
        {
            this.colour = colour; this.back = back; this.front = front;
        }

        public void Set(string backgroundId)
        {
            if (backgroundId == id) return;
            id = backgroundId;
            goal = string.IsNullOrEmpty(backgroundId) ? Theme.Backdrop : Theme.FromTone(Backdrops.For(backgroundId));
            // what is on top now goes under, the new picture (if any) starts transparent on top
            back.sprite = front.sprite; back.enabled = front.enabled; back.color = front.color;
            var sprite = Art.Background(backgroundId);
            front.sprite = sprite; front.enabled = sprite != null;
            front.color = new Color(1, 1, 1, 0);
            fade = 0;
            Cover(back); Cover(front);
        }

        public void Tick(float deltaTime)
        {
            colour.color = Color.Lerp(colour.color, goal, 1 - Mathf.Exp(-Speed * deltaTime));
            if (Screen.width != width || Screen.height != height) { Cover(back); Cover(front); }
            if (fade >= 1) return;
            fade = Mathf.Min(1, fade + deltaTime / FadeSeconds);
            front.color = new Color(1, 1, 1, front.sprite != null ? fade : 0);
            if (back.enabled) back.color = new Color(1, 1, 1, front.sprite != null ? 1 : 1 - fade);
            if (fade >= 1) back.enabled = false;
        }

        /// "Cover" fit: the picture fills the screen and the overflow is cropped, whatever the aspect ratio.
        void Cover(Image image)
        {
            width = Screen.width; height = Screen.height;
            if (image.sprite == null) return;
            var size = image.sprite.rect.size;
            var k = Mathf.Max(Metrics.RefWidth / size.x, Viewport.Height / size.y);
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size * k;
        }
    }
}

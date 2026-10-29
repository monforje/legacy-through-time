using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The bottom of a chat rises over the on-screen keyboard, and the feed ends above it.
    sealed class KeyboardLift : MonoBehaviour
    {
        public RectTransform Feed;
        public float FeedBottom;

        void Update()
        {
            var lift = 0f;
            if (TouchScreenKeyboard.isSupported && TouchScreenKeyboard.visible && Screen.height > 0)
                lift = Mathf.Max(0, TouchScreenKeyboard.area.height / Screen.height * Viewport.Height - Metrics.InsetBottom - Viewport.ExtraBottom);
            var rt = (RectTransform)transform;
            var y = Mathf.Lerp(rt.anchoredPosition.y, lift, Mathf.Min(1, Time.unscaledDeltaTime * 14));
            if (Mathf.Abs(y - lift) < .5f) y = lift;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            if (Feed) Feed.offsetMin = new Vector2(Feed.offsetMin.x, FeedBottom + y);
        }
    }

    /// Three dots that breathe one after another: "печатает…".
    sealed class TypingDots : MonoBehaviour
    {
        public UnityEngine.UI.Graphic[] Dots;

        void Update()
        {
            for (var i = 0; i < Dots.Length; i++)
            {
                if (!Dots[i]) continue;
                var c = Dots[i].color;
                c.a = .35f + .65f * Mathf.Max(0, Mathf.Sin((Time.unscaledTime * 5f) - i * .9f));
                Dots[i].color = c;
            }
        }
    }
}

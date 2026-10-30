using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The bottom of a chat rises over the on-screen keyboard, and the feed ends above it. The height of the keyboard is
    /// what Unity reports (`TouchScreenKeyboard.area`) or, where it reports nothing (Android in a full-screen window), what
    /// the window itself says: the part of the screen below the visible frame.
    sealed class KeyboardLift : MonoBehaviour
    {
        public RectTransform Feed;
        public float FeedBottom;
        /// True while the field of the chat is being typed in.
        public System.Func<bool> Typing;

        float androidPx, nextAsk;

        void Update()
        {
            var lift = 0f;
            if (Screen.height > 0)
            {
                var px = 0f;
                if (TouchScreenKeyboard.isSupported && TouchScreenKeyboard.visible) px = TouchScreenKeyboard.area.height;
                if (Typing != null && Typing() && Time.unscaledTime >= nextAsk) { androidPx = Keyboard.WindowPx(); nextAsk = Time.unscaledTime + .1f; }
                if (Typing == null || !Typing()) androidPx = 0;
                px = Mathf.Max(px, androidPx);
                if (px < Screen.height * .12f) px = 0;                                   // the bar of the system is not a keyboard
                if (px > 0) lift = Mathf.Max(0, px / Screen.height * Viewport.Height - Metrics.InsetBottom - Viewport.ExtraBottom);
            }
            var rt = (RectTransform)transform;
            var y = Mathf.Lerp(rt.anchoredPosition.y, lift, Mathf.Min(1, Time.unscaledDeltaTime * 14));
            if (Mathf.Abs(y - lift) < .5f) y = lift;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            if (Feed) Feed.offsetMin = new Vector2(Feed.offsetMin.x, FeedBottom + y);
        }

        static class Keyboard
        {
            /// The part of the window the keyboard covers, in pixels (0 off Android).
            public static float WindowPx()
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    using var window = activity.Call<AndroidJavaObject>("getWindow");
                    using var decor = window.Call<AndroidJavaObject>("getDecorView");
                    using var frame = new AndroidJavaObject("android.graphics.Rect");
                    decor.Call("getWindowVisibleDisplayFrame", frame);
                    using var root = decor.Call<AndroidJavaObject>("getRootView");
                    return Mathf.Max(0, root.Call<int>("getHeight") - frame.Get<int>("bottom"));
                }
                catch (System.Exception) { return 0; }
#else
                return 0;
#endif
            }
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

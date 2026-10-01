using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The canvas is 360 units wide on every aspect ratio (Match = Width); its height follows the screen,
    /// and the notch and the gesture bar add room beyond the 32 / 24 px already in the layout.
    static class Viewport
    {
        public static float Height { get; private set; } = Metrics.RefHeight;
        public static float ExtraTop { get; private set; }
        public static float ExtraBottom { get; private set; }

        public static void Measure()
        {
            Height = Metrics.RefWidth * Screen.height / Screen.width;
            var safe = Screen.safeArea;
            var k = Height / Screen.height;
            ExtraTop = Mathf.Max(0, (Screen.height - safe.yMax) * k - Metrics.InsetTop);
            ExtraBottom = Mathf.Max(0, safe.yMin * k - Metrics.InsetBottom);
        }

        /// Test hook: a fixed canvas without a real screen.
        public static void Set(float height, float extraTop = 0, float extraBottom = 0)
        {
            Height = height; ExtraTop = extraTop; ExtraBottom = extraBottom;
        }
    }
}

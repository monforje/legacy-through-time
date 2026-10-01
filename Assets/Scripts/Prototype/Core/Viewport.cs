using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The canvas is at least 360x780 units (CanvasScaler Expand): a taller phone adds height, a wider screen (tablet)
    /// adds room at the sides, so the layout never grows past the reference scale. The screens live in a 360-wide
    /// column (ScreenHost); the notch and the gesture bar add room beyond the 32 / 24 px already in the layout.
    static class Viewport
    {
        public static float Width { get; private set; } = Metrics.RefWidth;
        public static float Height { get; private set; } = Metrics.RefHeight;
        public static float ExtraTop { get; private set; }
        public static float ExtraBottom { get; private set; }

        public static void Measure()
        {
            var scale = Mathf.Min(Screen.width / Metrics.RefWidth, Screen.height / Metrics.RefHeight);
            Width = Screen.width / scale;
            Height = Screen.height / scale;
            var safe = Screen.safeArea;
            var k = Height / Screen.height;
            ExtraTop = Mathf.Max(0, (Screen.height - safe.yMax) * k - Metrics.InsetTop);
            ExtraBottom = Mathf.Max(0, safe.yMin * k - Metrics.InsetBottom);
        }

        /// Test hook: a fixed canvas without a real screen.
        public static void Set(float height, float extraTop = 0, float extraBottom = 0)
        {
            Width = Metrics.RefWidth; Height = height; ExtraTop = extraTop; ExtraBottom = extraBottom;
        }
    }
}

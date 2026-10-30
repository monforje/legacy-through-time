using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The canvas has the units of a 360-wide phone: at least 360 x 640 (a 16:9 phone such as the Huawei P10 is 360 x 640 dp),
    /// a taller phone adds height, a wider screen adds room at the sides. A canvas unit never grows past
    /// `DpCap` dp, so on a tablet the blocks stay the size of a phone's and the shell's column (`ColumnWidth`) stops
    /// at `MaxColumn`. The story screens live in a 360-wide column (ScreenHost); the shell in a column that can be wider.
    /// The notch and the gesture bar add room beyond the 32 / 24 px already in the layout.
    static class Viewport
    {
        /// The shortest canvas the layouts are made for.
        public const float MinHeight = 640;
        /// The widest column of the shell (templates/shell is drawn at 360).
        public const float MaxColumn = 440;
        /// The most dp a canvas unit may have (1 unit = 1 dp on a phone).
        const float DpCap = 1.35f;

        public static float Width { get; private set; } = Metrics.RefWidth;
        public static float Height { get; private set; } = Metrics.RefHeight;
        public static float ExtraTop { get; private set; }
        public static float ExtraBottom { get; private set; }

        /// Screen pixels in a canvas unit (the canvas scale).
        public static float Scale { get; private set; } = 1;

        /// The width of the shell's column: the canvas, between the width of a phone and `MaxColumn`.
        public static float ColumnWidth => Mathf.Clamp(Width, Metrics.RefWidth, MaxColumn);

        /// The scale for a screen: 360 x 640 units always fit (Expand), a unit is at most `DpCap` dp on a device with a known density.
        public static float ScaleFor(int pixelsWide, int pixelsHigh, float dpi, bool editor)
        {
            var scale = Mathf.Min(pixelsWide / Metrics.RefWidth, pixelsHigh / MinHeight);
            if (!editor && dpi > 0) scale = Mathf.Min(scale, Mathf.Max(1f, dpi / 160f * DpCap));
            return Mathf.Max(.1f, scale);
        }

        public static void Measure()
        {
            Scale = ScaleFor(Screen.width, Screen.height, Screen.dpi, Application.isEditor);
            Width = Screen.width / Scale;
            Height = Screen.height / Scale;
            var safe = Screen.safeArea;
            var k = Height / Screen.height;
            ExtraTop = Mathf.Max(0, (Screen.height - safe.yMax) * k - Metrics.InsetTop);
            ExtraBottom = Mathf.Max(0, safe.yMin * k - Metrics.InsetBottom);
        }

        /// Test and tool hook: a fixed canvas without a real screen.
        public static void Set(float height, float extraTop = 0, float extraBottom = 0, float width = Metrics.RefWidth)
        {
            Width = width; Height = height; ExtraTop = extraTop; ExtraBottom = extraBottom;
        }
    }
}

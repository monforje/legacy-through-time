using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Colours of the "Наследие" kit (DESIGN.md). The lengths live in Metrics.
    static class Theme
    {
        public static readonly Color Parchment = Hex("F7E9D8");
        public static readonly Color Ink = Hex("2A1B1B");
        public static readonly Color Emerald = Hex("157554");
        public static readonly Color Scarlet = Hex("E15654");
        public static readonly Color Backdrop = Hex("2F6B4F");      // stand-in for the painted background

        public static readonly Color LoadingBackground = Hex("0B1F17");
        public static readonly Color TitleDimTop = Hex("1C2A22").WithAlpha(.5f);
        public static readonly Color TitleDimBottom = Hex("143020").WithAlpha(.85f);

        public static Color Hex(string rgb) { ColorUtility.TryParseHtmlString("#" + rgb, out var c); return c; }
        public static Color WithAlpha(this Color c, float a) { c.a = a; return c; }
        public static Color FromTone(Tone t) => t.V <= .03f ? new Color(.015f, .02f, .018f) : Color.HSVToRGB(t.H, t.S, t.V);
    }
}

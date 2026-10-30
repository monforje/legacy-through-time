using System;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The pictures of the wheel, drawn pixel by pixel from the same formulas as the CSS of templates/shell/kit.css
    /// (`.wheel::before`, `.wheel-disc`, `.wheel-active`, `.wheel-hub`): conic sectors in two tones with thin dividers, the
    /// glow of the accent in the middle, the rim, the light that falls on the sector at the top, the hub with its halo.
    /// Each sprite is made once; its box in ref units is the size given below, 3 (2 for the soft glows) pixels per unit.
    static class WheelArt
    {
        public const float DiscSize = 266, LightSize = 252, HubSize = 126, GlowSize = 360;

        static Sprite disc, light, hub, glow;

        public static Sprite Disc => disc ? disc : disc = Build(DiscSize, 3, DiscPixel);
        public static Sprite Light => light ? light : light = Build(LightSize, 3, LightPixel);
        public static Sprite Hub => hub ? hub : hub = Build(HubSize, 3, HubPixel);
        public static Sprite Glow => glow ? glow : glow = Build(GlowSize, 2, GlowPixel);

        static Sprite Build(float size, int scale, Func<float, float, int, Color> pixel)
        {
            var n = Mathf.CeilToInt(size * scale);
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var data = new Color32[n * n];
            for (var y = 0; y < n; y++)
                for (var x = 0; x < n; x++)
                    data[y * n + x] = pixel((x + .5f) / scale - size / 2, (y + .5f) / scale - size / 2, scale);
            tex.SetPixels32(data);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(.5f, .5f), scale * 100);
        }

        // ------------------------------------------------------------------ colour helpers (straight alpha)
        static Color Over(Color top, Color under)
        {
            var a = top.a + under.a * (1 - top.a);
            if (a <= 0) return new Color(top.r, top.g, top.b, 0);
            return new Color((top.r * top.a + under.r * under.a * (1 - top.a)) / a, (top.g * top.a + under.g * under.a * (1 - top.a)) / a,
                             (top.b * top.a + under.b * under.a * (1 - top.a)) / a, a);
        }

        static Color Paint(Color under, Color colour, float coverage) => Over(new Color(colour.r, colour.g, colour.b, colour.a * Mathf.Clamp01(coverage)), under);

        /// The inside of a circle of the given radius, antialiased at the given pixels per unit.
        static float Cov(float edge, float r, int scale) => Mathf.Clamp01((edge - r) * scale + .5f);

        /// The area under the tail of a gaussian: 1 well inside, .5 at the edge, 0 far outside (box-shadow blur, sigma = blur / 2).
        static float Fall(float distance, float blur) => .5f * Erfc(distance / (blur / 2 * 1.41421356f));

        static float Erfc(float x)
        {
            var z = Mathf.Abs(x);
            var t = 1 / (1 + .5f * z);
            var y = t * Mathf.Exp(-z * z - 1.26551223f + t * (1.00002368f + t * (.37409196f + t * (.09678418f + t * (-.18628806f + t * (.27886807f
                  + t * (-1.13520398f + t * (1.48851587f + t * (-.82215223f + t * .17087277f)))))))));
            return x >= 0 ? y : 2 - y;
        }

        static Color32 Out(Color c)
        {
            if (c.a <= 0) c = new Color(Kit.Line.r, Kit.Line.g, Kit.Line.b, 0);       // the colour of the fringe under bilinear filtering
            return c;
        }

        /// Degrees clockwise from the top, shifted so that the first sector starts at 0 (CSS `from -18deg`).
        static float Turn(float dx, float dy) => Mathf.Repeat(Mathf.Atan2(dx, dy) * Mathf.Rad2Deg + 18f, 360f);

        // ------------------------------------------------------------------ the disc: .wheel-disc
        static Color DiscPixel(float dx, float dy, int scale)
        {
            var r = Mathf.Sqrt(dx * dx + dy * dy);
            var t = Turn(dx, dy);
            var c = new Color(0, 0, 0, 0);
            c = Paint(c, Kit.Line, Cov(133, r, scale));                       // 0 0 0 7px line
            c = Paint(c, Kit.Surface2, Cov(132, r, scale));                   // 0 0 0 6px surface-2
            var tone = (int)(t / 36f) % 2 == 0 ? Kit.Surface : Kit.Surface2;
            var inside = tone;
            if (t % 36f < .6f) inside = Kit.Line;                             // the thin dividers
            var glowAlpha = .22f * Mathf.Max(0, 1 - r / 128f);                // radial-gradient(accent 22%, transparent 72%)
            inside = Over(new Color(Kit.Accent.r, Kit.Accent.g, Kit.Accent.b, glowAlpha), inside);
            c = Paint(c, inside, Cov(126, r, scale));
            return Out(c);
        }

        // ------------------------------------------------------------------ the light on the top sector: .wheel-active
        static Color LightPixel(float dx, float dy, int scale)
        {
            var r = Mathf.Sqrt(dx * dx + dy * dy);
            var t = Turn(dx, dy);
            if (t >= 36f) return Out(new Color(0, 0, 0, 0));
            var edge = Color.Lerp(Kit.Accent, Kit.Text, .4f);                  // accent 60% / text
            var c = t < 1f || t >= 35f ? edge : new Color(0, 0, 0, 0);
            c = Over(new Color(Kit.Text.r, Kit.Text.g, Kit.Text.b, .42f), c);
            var mask = Mathf.Clamp01((r / 126f - .12f) / .88f);               // transparent at 12%, opaque at the rim
            c.a *= mask * Cov(124.74f, r, scale);                             // clip-path: circle(49.5%)
            return Out(c);
        }

        // ------------------------------------------------------------------ the hub: .wheel-hub
        static Color HubPixel(float dx, float dy, int scale)
        {
            var r = Mathf.Sqrt(dx * dx + dy * dy);
            var c = new Color(0, 0, 0, 0);
            if (r > 32) c = new Color(Kit.Accent.r, Kit.Accent.g, Kit.Accent.b, .55f * Fall(r - 32, 24));     // 0 0 24px accent 55%
            c = Paint(c, Kit.Surface2, Cov(39, r, scale));                    // 0 0 0 7px surface-2
            c = Paint(c, Kit.Accent, Cov(37, r, scale));                      // 0 0 0 5px accent
            c = Paint(c, Kit.Surface, Cov(32, r, scale));
            return Out(c);
        }

        // ------------------------------------------------------------------ the halo of the rim: .wheel::before
        static Color GlowPixel(float dx, float dy, int scale)
        {
            var r = Mathf.Sqrt(dx * dx + dy * dy);
            var c = new Color(0, 0, 0, 0);
            if (r > 140) c = new Color(Kit.Accent.r, Kit.Accent.g, Kit.Accent.b, .4f * Fall(r - 140, 34));      // 0 0 34px accent 40%
            else c = new Color(Kit.Accent.r, Kit.Accent.g, Kit.Accent.b, .2f * Fall(140 - r, 26));              // inset 0 0 26px accent 20%
            var ring = Color.Lerp(Kit.Line, Kit.Accent, .55f);                // 0 0 0 1px accent 55% / line
            var onRing = Mathf.Clamp01((141 - r) * scale + .5f) * Mathf.Clamp01((r - 140) * scale + .5f);
            c = Paint(c, ring, onRing);
            return Out(c);
        }
    }
}

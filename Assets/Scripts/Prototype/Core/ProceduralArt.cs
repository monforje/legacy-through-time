using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Textures made in code: a flat white, the dimming gradient of the title, the vignette over the background.
    static class ProceduralArt
    {
        static Sprite white;

        public static Sprite White
        {
            get
            {
                if (white) return white;
                var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                t.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                t.Apply();
                return white = Sprite.Create(t, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f), 100);
            }
        }

        static Sprite disc;

        /// A soft-edged white disc (dots of the spinner; the game tints it).
        public static Sprite Disc
        {
            get
            {
                if (disc) return disc;
                const int n = 256;                                       // large: it is drawn from 8 to 100 units, at up to 3 pixels a unit
                var t = NewTexture(n, n);
                for (var y = 0; y < n; y++)
                    for (var x = 0; x < n; x++)
                    {
                        var d = Mathf.Sqrt(Mathf.Pow(x + .5f - n / 2f, 2) + Mathf.Pow(y + .5f - n / 2f, 2));
                        t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(n / 2f - d)));      // one pixel of anti-aliasing
                    }
                t.Apply();
                return disc = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 400);
            }
        }

        static readonly System.Collections.Generic.Dictionary<int, Sprite> rounded = new();

        /// Pixels in a unit of the rounded rectangle: the device draws up to 3 pixels a unit, so the corners are cut at 4.
        const int RoundedScale = 4;

        /// A white rounded rectangle as a 9-slice (the game tints it): bubbles, fields, rows of the neutral kit.
        public static Sprite Rounded(int radius)
        {
            if (rounded.TryGetValue(radius, out var cached) && cached) return cached;
            var r = radius * RoundedScale;
            var n = (radius * 2 + 2) * RoundedScale;
            var t = NewTexture(n, n);
            for (var y = 0; y < n; y++)
                for (var x = 0; x < n; x++)
                {
                    // distance to the nearest corner centre, 0 along the straight edges
                    var dx = Mathf.Max(0, Mathf.Max(r - (x + .5f), x + .5f - (n - r)));
                    var dy = Mathf.Max(0, Mathf.Max(r - (y + .5f), y + .5f - (n - r)));
                    t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + .5f)));
                }
            t.Apply();
            var border = new Vector4(r, r, r, r);
            return rounded[radius] = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100 * RoundedScale, 0, SpriteMeshType.FullRect, border);
        }

        static readonly System.Collections.Generic.Dictionary<(int, int), Sprite> rings = new();

        /// A white ring of the given thickness inside a rounded rectangle, as a 9-slice: the inner outline of a field
        /// (kit.css `box-shadow: inset 0 0 0 Npx`) or, bigger, an outline around it.
        public static Sprite RoundedRing(int radius, int thickness)
        {
            if (rings.TryGetValue((radius, thickness), out var cached) && cached) return cached;
            var r = radius * RoundedScale; var th = thickness * RoundedScale;
            var n = (radius * 2 + 2) * RoundedScale;
            var t = NewTexture(n, n);
            for (var y = 0; y < n; y++)
                for (var x = 0; x < n; x++)
                {
                    var dx = Mathf.Max(0, Mathf.Max(r - (x + .5f), x + .5f - (n - r)));
                    var dy = Mathf.Max(0, Mathf.Max(r - (y + .5f), y + .5f - (n - r)));
                    // signed distance to the edge of the rounded rectangle: negative inside
                    var cx = Mathf.Min(x + .5f, n - x - .5f); var cy = Mathf.Min(y + .5f, n - y - .5f);
                    var d = dx > 0 || dy > 0 ? Mathf.Sqrt(dx * dx + dy * dy) - r : -Mathf.Min(cx, cy);
                    var a = Mathf.Clamp01(.5f - d) * Mathf.Clamp01(d + th + .5f);
                    t.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            t.Apply();
            var border = new Vector4(r, r, r, r);
            return rings[(radius, thickness)] = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100 * RoundedScale, 0, SpriteMeshType.FullRect, border);
        }

        static Sprite spinner;

        /// The loading ring of a button (kit.css `.btn.is-loading::before`): 18 units, a 2 unit line, the right quarter open.
        public static Sprite Spinner
        {
            get
            {
                if (spinner) return spinner;
                const int s = 4, n = 18 * s;
                var t = NewTexture(n, n);
                for (var y = 0; y < n; y++)
                    for (var x = 0; x < n; x++)
                    {
                        float dx = x + .5f - n / 2f, dy = y + .5f - n / 2f;
                        var r = Mathf.Sqrt(dx * dx + dy * dy);
                        var ring = Mathf.Clamp01(n / 2f - r + .5f) * Mathf.Clamp01(r - (n / 2f - 2 * s) + .5f);
                        var open = Mathf.Abs(Mathf.Atan2(dy, dx)) < Mathf.PI / 4;            // the border on the right is transparent
                        t.SetPixel(x, y, new Color(1, 1, 1, open ? 0 : ring));
                    }
                t.Apply();
                return spinner = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100 * s);
            }
        }

        static Sprite shimmer;

        /// The band of a skeleton: surface, a lighter middle, surface again (kit.css `.skeleton`), 2 widths wide.
        public static Sprite Shimmer
        {
            get
            {
                if (shimmer) return shimmer;
                const int n = 128;
                var t = NewTexture(n, 1);
                for (var x = 0; x < n; x++)
                {
                    var u = x / (n - 1f);
                    var k = u < .25f ? 0 : u < .5f ? (u - .25f) / .25f : u < .75f ? 1 - (u - .5f) / .25f : 0;
                    t.SetPixel(x, 0, Color.Lerp(Kit.Surface, Kit.Surface2, k));
                }
                t.Apply();
                return shimmer = Sprite.Create(t, new Rect(0, 0, n, 1), new Vector2(.5f, .5f), 100);
            }
        }

        // 8-bit alpha has only 256 levels, so a smooth dark gradient stretched over a phone screen shows as
        // visible bands. A fixed pseudo-random offset of about one level breaks the bands up (dithering).
        static float Dither(int x, int y) { unchecked { return ((x * 73856093 ^ y * 19349663) & 255) / 255f - .5f; } }

        static Color Dithered(Color c, int x, int y) { c.a = Mathf.Clamp01(c.a + Dither(x, y) / 255f); return c; }

        /// Vertical gradient (top -> bottom).
        public static Sprite Gradient(Color top, Color bottom)
        {
            const int h = 256;
            var t = NewTexture(8, h);
            for (var y = 0; y < h; y++)
                for (var x = 0; x < 8; x++)
                    t.SetPixel(x, y, Dithered(Color.Lerp(bottom, top, y / (h - 1f)), x, y));
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 8, h), new Vector2(.5f, .5f), 100);
        }

        /// Dark edges around a transparent centre.
        public static Sprite Vignette()
        {
            const int n = 256;
            var t = NewTexture(n, n);
            for (var y = 0; y < n; y++)
                for (var x = 0; x < n; x++)
                {
                    var dx = (x - n / 2f) / (n / 2f);
                    var dy = (y - n * .6f) / (n * .6f);
                    var d = Mathf.Clamp01((Mathf.Sqrt(dx * dx + dy * dy) - .55f) / .75f);
                    t.SetPixel(x, y, Dithered(new Color(.02f, .08f, .055f, d * d * .55f), x, y));
                }
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100);
        }

        static Texture2D NewTexture(int w, int h) =>
            new(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
    }
}

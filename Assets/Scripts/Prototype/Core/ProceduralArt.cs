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
                const int n = 64;
                var t = NewTexture(n, n);
                for (var y = 0; y < n; y++)
                    for (var x = 0; x < n; x++)
                    {
                        var d = Mathf.Sqrt(Mathf.Pow(x + .5f - n / 2f, 2) + Mathf.Pow(y + .5f - n / 2f, 2));
                        t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(n / 2f - d)));      // one pixel of anti-aliasing
                    }
                t.Apply();
                return disc = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100);
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

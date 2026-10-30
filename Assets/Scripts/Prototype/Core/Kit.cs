using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The neutral UI kit of the app shell: the tokens of docs/brainstorm/templates/shell/kit.css (colours, radii,
    /// spacing, type, motion) and the loading of its icons (Resources/Shell). The screens of the shell look the same
    /// whatever story they belong to; "Наследие" stays inside the stories. Change a value here and in kit.css together.
    static class Kit
    {
        // surfaces and text
        public static readonly Color Bg = Theme.Hex("22244F"), Surface = Theme.Hex("2E3167"), Surface2 = Theme.Hex("393D7E");
        public static readonly Color Line = Theme.Hex("8087D4"), Scrim = Theme.Hex("0F1030");
        public static readonly Color Text = Theme.Hex("F6F7FF"), Muted = Theme.Hex("C3C8F4");
        // the one accent and the meanings
        public static readonly Color Accent = Theme.Hex("A8B0FF"), AccentInk = Theme.Hex("14163A");
        public static readonly Color Ok = Theme.Hex("58D6A0"), Danger = Theme.Hex("FF7F8A");
        // currencies
        public static readonly Color Coin = Theme.Hex("FFD25F"), Gem = Theme.Hex("66D6FF"), Energy = Theme.Hex("D6AAFF");

        static readonly Color[] Art_ =
        {
            Theme.Hex("5562D9"), Theme.Hex("247696"), Theme.Hex("A64E92"), Theme.Hex("93651F"), Theme.Hex("705ECE"),
            Theme.Hex("3F6EB9"), Theme.Hex("8D53C4"), Theme.Hex("A85A2F"), Theme.Hex("277A75"), Theme.Hex("6C60C5"),
        };

        /// The colour of a hero's avatar or a cover until there is art (from the world of the story).
        public static Color ArtColor(string storyId) => Art_[ChatList.ArtIndex(storyId)];
        public static Color ArtColor(int index) => Art_[((index % Art_.Length) + Art_.Length) % Art_.Length];

        public const int RadiusS = 10, RadiusM = 16, RadiusL = 24;
        public const float Space1 = 4, Space2 = 8, Space3 = 12, Space4 = 16, Space5 = 24, Space6 = 32;
        /// The width of the shell's column and the room on each side of it up to the edges of the canvas (backgrounds and
        /// bars run into it, content does not).
        public static float Width => Viewport.ColumnWidth;
        public static float Wing => (Viewport.Width - Viewport.ColumnWidth) / 2;

        /// A column of the shell's width in the middle of `parent` (a full-canvas layer): what the screen puts on it keeps the
        /// phone's proportions on a tablet.
        public static RectTransform Column(RectTransform parent, string name = "Column")
        {
            var column = Ui.Rect(parent, name);
            column.anchorMin = A.BottomCenter; column.anchorMax = A.TopCenter; column.pivot = A.Center;
            column.offsetMin = new Vector2(-Width / 2, 0); column.offsetMax = new Vector2(Width / 2, 0);
            return column;
        }

        /// The side margin of the shell (the stories' plates use Metrics.Side = 20).
        public const float Side = Space4;
        /// The smallest tap target.
        public const float Touch = 44;
        /// The bottom bar: 4 + 44 + 8 (the gesture zone lives inside it).
        public const float NavHeight = 56;
        public const int FsTitle = 24, FsHead = 18, FsBody = 15, FsSmall = 13, FsNum = 20, FsMicro = 11;

        /// Durations of the motion tokens in seconds (kit.css --dur-*). One place: change here, not at the call.
        public static class Dur
        {
            public const float Fast = .12f, Normal = .2f, Slow = .32f, Stagger = .04f, Spin = 3.6f, Net = .9f;
        }

        // ------------------------------------------------------------------ icons
        static readonly Dictionary<string, Sprite> icons = new();

        /// The weights of Nunito the mockups use: 800 (titles, buttons, values), 700 (tags, tab labels), 600 (text).
        public static Font F800 => Art.Font("Nunito-ExtraBold");
        public static Font F700 => Art.Font("Nunito-Bold");
        public static Font F600 => Art.Font("Nunito-SemiBold");

        /// Number of icons that were not found (tests assert zero).
        public static int Missing { get; private set; }

        /// A picture or a glyph of the shell kit: "gem", "coin", "g/gear", "g/plus-ink".
        public static Sprite Pic(string name)
        {
            if (icons.TryGetValue(name, out var s) && s) return s;
            s = Resources.Load<Sprite>("Shell/" + name);
            if (!s) { Missing++; Debug.LogError("Shell icon missing: Resources/Shell/" + name); s = ProceduralArt.Disc; }
            return icons[name] = s;
        }

        /// A glyph (22 px tube icon): the light one for the dark ground, the "ink" one for the accent buttons.
        public static Sprite Glyph(string name, bool ink = false) => Pic("g/" + name + (ink ? "-ink" : ""));

        /// An icon of the given square size, centred in the given box.
        public static Image Icon(Transform parent, string name, Sprite sprite, float size, Vector2? anchor = null, Vector2? pos = null)
        {
            var img = Ui.Img(parent, name, sprite);
            img.preserveAspect = true;
            var a = anchor ?? A.Center;
            Ui.Box(img.rectTransform, a, a, pos ?? Vector2.zero, new Vector2(size, size));
            return img;
        }

        // ------------------------------------------------------------------ shadow
        static readonly Dictionary<(int, int, int), Sprite> shadows = new();

        /// `box-shadow: 0 dy blur scrim@alpha` under a rounded box: a blurred rounded rectangle (9-sliced) behind `target`, one
        /// sibling below it, with the same anchors. The default is the kit's `--shadow` (0 8px 24px, scrim 50%).
        public static Image Shadow(RectTransform target, int radius = RadiusL, float blur = 24, float alpha = .5f, float dy = 8)
        {
            var key = (radius, Mathf.RoundToInt(blur), Mathf.RoundToInt(alpha * 100));
            var margin = Mathf.CeilToInt(blur * 1.5f);
            if (!shadows.TryGetValue(key, out var sprite) || !sprite)
            {
                const int scale = 2;
                var border = (radius + margin) * scale;
                var n = 2 * border + 4;
                var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                var data = new Color32[n * n];
                float c = n / 2f, half = c - margin * scale, r = radius * scale, sigma = blur * scale / 2f;
                for (var y = 0; y < n; y++)
                    for (var x = 0; x < n; x++)
                    {
                        float qx = Mathf.Abs(x + .5f - c) - (half - r), qy = Mathf.Abs(y + .5f - c) - (half - r);
                        var d = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
                        var a = alpha * .5f * Erfc(d / (sigma * 1.41421356f));
                        data[y * n + x] = new Color(1, 1, 1, Mathf.Clamp01(a));
                    }
                tex.SetPixels32(data); tex.Apply(false, true);
                sprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100 * scale, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
                shadows[key] = sprite;
            }
            var image = Ui.Img(target.parent, "Shadow", sprite, Image.Type.Sliced);
            image.color = Scrim; image.raycastTarget = false;
            var rt = image.rectTransform;
            rt.anchorMin = target.anchorMin; rt.anchorMax = target.anchorMax; rt.pivot = target.pivot;
            rt.sizeDelta = target.sizeDelta + new Vector2(2 * margin, 2 * margin);
            rt.anchoredPosition = target.anchoredPosition + new Vector2(0, -dy);
            rt.SetSiblingIndex(target.GetSiblingIndex());
            return image;
        }

        static float Erfc(float x)
        {
            var z = Mathf.Abs(x);
            var t = 1 / (1 + .5f * z);
            var y = t * Mathf.Exp(-z * z - 1.26551223f + t * (1.00002368f + t * (.37409196f + t * (.09678418f + t * (-.18628806f + t * (.27886807f
                  + t * (-1.13520398f + t * (1.48851587f + t * (-.82215223f + t * .17087277f)))))))));
            return x >= 0 ? y : 2 - y;
        }

        // ------------------------------------------------------------------ shapes and text
        public static Image Round(Transform parent, string name, Color colour, int radius = RadiusM)
        {
            var img = Ui.Img(parent, name, ProceduralArt.Rounded(radius), Image.Type.Sliced);
            img.color = colour;
            return img;
        }

        /// A round avatar with the first letter of the name.
        public static Image Avatar(Transform parent, string hero, string storyId, float size)
        {
            var disc = Ui.Img(parent, "Avatar", ProceduralArt.Disc);
            disc.color = ArtColor(storyId);
            var letter = Ui.Txt(disc.rectTransform, "Letter", string.IsNullOrEmpty(hero) ? "?" : hero.Substring(0, 1).ToUpperInvariant(),
                                Kit.F800, Mathf.RoundToInt(size * .4f), Text, TextAnchor.MiddleCenter);
            Ui.Stretch(letter.rectTransform);
            disc.rectTransform.sizeDelta = new Vector2(size, size);
            return disc;
        }

        /// The text cut with "…" so that it fits one line of the given width.
        public static void Clamp(Text t, string text, float width)
        {
            text = (text ?? "").Replace('\n', ' ');
            t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;               // one line, never wrapped: cut with "…" instead
            width -= 2;
            if (Ui.TextWidth(t) <= width) { t.horizontalOverflow = HorizontalWrapMode.Overflow; return; }
            var lo = 0; var hi = text.Length;
            while (lo < hi)
            {
                var mid = (lo + hi + 1) / 2;
                t.text = text.Substring(0, mid).TrimEnd() + "…";
                if (Ui.TextWidth(t) <= width) lo = mid; else hi = mid - 1;
            }
            t.text = text.Substring(0, lo).TrimEnd() + "…";
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        /// The whole screen in the colour of the kit (covers the painting of the menu behind it).
        public static Image Ground(RectTransform parent)
        {
            var ground = Ui.Img(parent, "Ground", ProceduralArt.White);
            ground.color = Bg; ground.raycastTarget = true;
            Ui.Bleed(ground.rectTransform, Width);
            return ground;
        }
    }
}

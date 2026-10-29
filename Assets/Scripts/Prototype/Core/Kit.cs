using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The neutral UI kit of the app shell (docs/brainstorm/templates/shell/kit.css): the screens of the shell (chats,
    /// later the rest) look the same whatever story they belong to; "Наследие" stays inside the stories.
    static class Kit
    {
        public static readonly Color Bg = Theme.Hex("15172A"), Surface = Theme.Hex("1F2238"), Surface2 = Theme.Hex("2A2E4A");
        public static readonly Color Text = Theme.Hex("ECEEF8"), Muted = Theme.Hex("A9AEC8");
        public static readonly Color Accent = Theme.Hex("8B93FF"), AccentInk = Theme.Hex("10122A");
        public static readonly Color Danger = Theme.Hex("E5646E"), Energy = Theme.Hex("C79BFF");

        static readonly Color[] Art_ =
        {
            Theme.Hex("4A58B8"), Theme.Hex("2A7088"), Theme.Hex("8F4F7C"), Theme.Hex("8A5F22"), Theme.Hex("5A4F9E"),
            Theme.Hex("3A67A6"), Theme.Hex("7C52B0"), Theme.Hex("9A5A3A"), Theme.Hex("2A746F"), Theme.Hex("6A5EBA"),
        };

        /// The colour of a hero's avatar until there is art (from the world of the story).
        public static Color ArtColor(string storyId) => Art_[ChatList.ArtIndex(storyId)];

        public const int RadiusS = 10, RadiusM = 16;

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
                                Art.Sub, Mathf.RoundToInt(size * .4f), Text, TextAnchor.MiddleCenter);
            Ui.Stretch(letter.rectTransform);
            disc.rectTransform.sizeDelta = new Vector2(size, size);
            return disc;
        }

        /// The text cut with "…" so that it fits one line of the given width.
        public static void Clamp(Text t, string text, float width)
        {
            text = (text ?? "").Replace('\n', ' ');
            t.text = text;
            if (Ui.TextWidth(t) <= width) return;
            var lo = 0; var hi = text.Length;
            while (lo < hi)
            {
                var mid = (lo + hi + 1) / 2;
                t.text = text.Substring(0, mid).TrimEnd() + "…";
                if (Ui.TextWidth(t) <= width) lo = mid; else hi = mid - 1;
            }
            t.text = text.Substring(0, lo).TrimEnd() + "…";
        }

        /// The whole screen in the colour of the kit (covers the painting of the menu behind it).
        public static Image Ground(RectTransform parent)
        {
            var ground = Ui.Img(parent, "Ground", ProceduralArt.White);
            ground.color = Bg; ground.raycastTarget = true;
            Ui.Bleed(ground.rectTransform);
            return ground;
        }
    }
}

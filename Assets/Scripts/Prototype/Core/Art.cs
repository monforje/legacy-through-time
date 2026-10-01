using System.Collections.Generic;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Sprites and fonts (Resources/Heritage, Resources/Fonts: `task assets:unity`, `task assets:fonts`).
    /// A missing asset is logged and replaced, so that one broken file does not take the whole screen down.
    static class Art
    {
        /// How many assets could not be loaded (tests assert zero).
        public static int Missing { get; private set; }

        static readonly Dictionary<string, Sprite> sprites = new();
        static readonly Dictionary<string, Font> fonts = new();

        public static Sprite Sprite(string name)
        {
            if (sprites.TryGetValue(name, out var s) && s) return s;
            s = Resources.Load<Sprite>("Heritage/" + name);
            if (!s)
            {
                Missing++;
                Debug.LogError($"Heritage sprite missing: {name} (run `task assets:unity`)");
                s = ProceduralArt.White;
            }
            return sprites[name] = s;
        }

        /// The logo of the loading screen (Resources/Brand/logo.png: `task assets:brand`).
        public static Sprite Logo
        {
            get
            {
                if (logo) return logo;
                logo = Resources.Load<Sprite>("Brand/logo");
                if (!logo)
                {
                    Missing++;
                    Debug.LogError("Logo missing: Resources/Brand/logo.png (run `task assets:brand`)");
                    logo = ProceduralArt.White;
                }
                return logo;
            }
        }

        static Sprite logo;

        public static Font Font(string name)
        {
            if (fonts.TryGetValue(name, out var f) && f) return f;
            f = Resources.Load<Font>("Fonts/" + name);
            if (!f)
            {
                Missing++;
                Debug.LogError($"Font missing: {name} (run `task assets:fonts`)");
                f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return fonts[name] = f;
        }

        public static Font Head => Font("RussoOne-Regular");     // titles
        public static Font Sub => Font("Rubik-Bold");            // labels, values
        public static Font Body => Font("Nunito-Medium");        // plate text
        public static Font Action => Font("Nunito-Bold");        // buttons

        /// Native size of a sprite in canvas units (png @4x at 400 ppu).
        public static Vector2 NativeSize(Sprite s) => s.rect.size / (s.pixelsPerUnit / 100f);
    }
}

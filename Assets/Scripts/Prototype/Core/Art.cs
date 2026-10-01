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

        /// Painted background of a location (`# bg: id`, Resources/Art/Backgrounds/<id>.png), loaded in the background so that
        /// a scene change does not stall a frame. `ready` gets null when there is no picture; it runs at once when it is cached.
        public static void Background(string id, System.Action<Sprite> ready)
        {
            if (string.IsNullOrEmpty(id)) { ready(null); return; }
            if (backgrounds.TryGetValue(id, out var s) && s) { ready(s); return; }
            var request = Resources.LoadAsync<Sprite>("Art/Backgrounds/" + id);
            request.completed += _ => ready(backgrounds[id] = request.asset as Sprite);
        }

        /// Starts loading a background that is needed soon (the title) so that it is cached by then.
        public static void Prefetch(string id) => Background(id, _ => { });

        /// Unloads every cached background but `keep`: a 1080x1936 one holds about 1 MB of GPU memory, there are 47 of them.
        public static void KeepBackgrounds(params string[] keep)
        {
            var drop = new List<string>();
            foreach (var pair in backgrounds)
                if (System.Array.IndexOf(keep, pair.Key) < 0) drop.Add(pair.Key);
            foreach (var id in drop)
            {
                var sprite = backgrounds[id];
                backgrounds.Remove(id);
                if (!sprite) continue;
                var texture = sprite.texture;
                Resources.UnloadAsset(sprite);
                Resources.UnloadAsset(texture);
            }
        }

        static readonly Dictionary<string, Sprite> backgrounds = new();

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

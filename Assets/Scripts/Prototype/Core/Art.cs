using System.Collections.Generic;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Sprites and fonts (Resources/Heritage, Resources/Heritage3D, Resources/Fonts: `task assets:unity`,
    /// `task assets:blender`, `task assets:fonts`).
    /// A missing asset is logged and replaced, so that one broken file does not take the whole screen down.
    static class Art
    {
        /// The UI kit: true - the Blender renders (Resources/Heritage3D: enamel, paper and thread under a warm lamp),
        /// false - the flat SVG kit (Resources/Heritage). Both have the same names, sizes and 9-slice borders; a
        /// sprite the Blender kit lacks comes from the SVG one. Switching drops the loaded sprites; images already
        /// on screen keep theirs until they are built again.
        public static bool BlenderKit
        {
            get => blenderKit;
            set
            {
                if (blenderKit == value) return;
                blenderKit = value;
                sprites.Clear();
                tiled = null;
            }
        }

        static bool blenderKit = true;

        /// How many assets could not be loaded (tests assert zero).
        public static int Missing { get; private set; }

        static readonly Dictionary<string, Sprite> sprites = new();
        static readonly Dictionary<string, Font> fonts = new();
        static HashSet<string> tiled;

        /// The story kit of the chapter being read (`sa-`: the surveyor's notebook of «По хребтам Сихотэ-Алиня»), null
        /// in the menu and in stories without one. A sprite of the kit is `<prefix><name>` in Resources/Heritage3D, with the
        /// size and the borders of `<name>`; one it lacks comes from the main kit. Images already on screen keep theirs.
        public static string StoryKit { get; set; }

        /// The name a sprite has in the story kit, or the name itself when the story has no such sprite.
        static string Themed(string name)
        {
            if (StoryKit == null || !blenderKit) return name;
            var themed = StoryKit + name;
            return Resources.Load<Sprite>("Heritage3D/" + themed) ? themed : name;
        }

        public static Sprite Sprite(string name)
        {
            name = Themed(name);
            if (sprites.TryGetValue(name, out var s) && s) return s;
            // the chosen kit first, then the other one: the menu's components exist only in the Blender kit
            var (first, second) = blenderKit ? ("Heritage3D/", "Heritage/") : ("Heritage/", "Heritage3D/");
            s = Resources.Load<Sprite>(first + name);
            if (!s) s = Resources.Load<Sprite>(second + name);
            if (!s)
            {
                Missing++;
                Debug.LogError($"Heritage sprite missing: {name} (run `task assets:unity`)");
                s = ProceduralArt.White;
            }
            return sprites[name] = s;
        }

        /// The edges and the centre of this 9-slice sprite repeat at their own size instead of stretching: the paper
        /// of the Blender kit, whose grain and twisted threads must not smear when a card or a plate grows.
        /// From Resources/Heritage3D/slices.txt (`name left bottom right top sliced|tiled fill`).
        public static bool Tiled(string name)
        {
            if (!blenderKit) return false;
            name = Themed(name);
            if (tiled == null)
            {
                tiled = new HashSet<string>();
                var list = Resources.Load<TextAsset>("Heritage3D/slices");
                if (list)
                    foreach (var line in list.text.Split('\n'))
                    {
                        var p = line.Trim().Split(' ');
                        if (p.Length >= 6 && p[5] == "tiled") tiled.Add(p[0]);
                    }
            }
            return tiled.Contains(name);
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

        /// A sprite from Resources/Brand (the loader's ring and hourglass).
        public static Sprite Brand(string name)
        {
            if (brand.TryGetValue(name, out var s) && s) return s;
            s = Resources.Load<Sprite>("Brand/" + name);
            if (!s)
            {
                Missing++;
                Debug.LogError($"Brand art missing: Resources/Brand/{name}.png");
                s = ProceduralArt.White;
            }
            return brand[name] = s;
        }

        static readonly Dictionary<string, Sprite> brand = new();

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
        public static Font Accent => Font("YesevaOne-Regular");  // names of stories and chapters in the menu: a display serif
        public static Font Sub => Font("Rubik-Bold");            // labels, values
        public static Font Body => Font("Nunito-Medium");        // plate text
        public static Font Action => Font("Nunito-Bold");        // buttons

        /// Native size of a sprite in canvas units (png @4x at 400 ppu).
        public static Vector2 NativeSize(Sprite s) => s.rect.size / (s.pixelsPerUnit / 100f);
    }
}

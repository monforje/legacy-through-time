using System.IO;
using UnityEditor;
using UnityEngine;

namespace LegacyThroughTime.Editor
{
    /// Sprites of the Heritage UI kit (`task assets:unity` copies them here from
    /// docs/brainstorm/templates/assets/heritage/png). Borders for 9-slice come from slices.txt,
    /// written by the same generator, so nothing is clicked by hand in the Sprite Editor.
    public sealed class HeritageSpriteImporter : AssetPostprocessor
    {
        const string Dir = "Assets/Resources/Heritage/";
        const string LogoDir = "Assets/Resources/Brand/";      // the loading-screen logo: one plain sprite
        const string BgDir = "Assets/Resources/Art/Backgrounds/";   // painted scene backgrounds: plain sprites, compressed
        const string AudioDir = "Assets/Resources/Audio/";         // sounds of the story: see docs/audio
        const string IconDir = "Assets/Art/Brand/";            // launcher icon layers: plain textures for PlayerSettings
        // PNGs are @4x; with the default Canvas reference of 100 ppu this gives 1 canvas unit = 1 px of the 360x780 layout.
        public const float PixelsPerUnit = 400f;

        /// Bump when the settings below change: Unity then reimports every texture and sound with them.
        public override uint GetVersion() => 2;

        void OnPreprocessTexture()
        {
            if (!assetPath.EndsWith(".png")) return;
            if (assetPath.StartsWith(LogoDir)) { ImportBrand(sprite: true); return; }
            if (assetPath.StartsWith(BgDir)) { ImportBackground(); return; }
            if (assetPath.StartsWith(IconDir)) { ImportBrand(sprite: false); return; }
            if (!assetPath.StartsWith(Dir)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            // Mip-maps and trilinear filtering: when the screen (or the Game view) is smaller than the texture, the
            // downscale is smooth instead of dropping pixels ("pixel crawl" on thin outlines).
            importer.mipmapEnabled = true;
            importer.mipMapsPreserveCoverage = false;
            importer.borderMipmap = true;          // lower mips keep the edge pixels instead of bleeding the neighbours' colour
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            AndroidAstc4x4(importer);

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;   // 9-slice needs the full rect
            if (TryReadBorder(Path.GetFileNameWithoutExtension(assetPath), out var border))
                settings.spriteBorder = border;
            importer.SetTextureSettings(settings);
        }

        /// Music and ambience are long loops: streamed, not decoded into memory (a 2-minute stereo clip is 20 MB raw).
        /// Ambience, effects and UI sounds are mono; effects and UI are short: decoded once and ready to play without delay.
        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(AudioDir)) return;
            var importer = (AudioImporter)assetImporter;
            var loop = assetPath.StartsWith(AudioDir + "music/") || assetPath.StartsWith(AudioDir + "ambience/");
            var settings = importer.defaultSampleSettings;
            settings.loadType = loop ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = loop ? .4f : .5f;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = !assetPath.StartsWith(AudioDir + "music/");   // only the music is stereo
        }

        /// Backgrounds are shown full-screen once at a time: no mip-maps, compressed (a 1080x1935 PNG is 8 MB raw).
        void ImportBackground()
        {
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 2048;
        }

        /// Brand art is not 9-sliced: the logo is a sprite at the size of its pixels / 100, the icon layers plain textures.
        void ImportBrand(bool sprite)
        {
            var importer = (TextureImporter)assetImporter;
            importer.textureType = sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            if (sprite) importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 400;                   // 1536 px wide -> 384 canvas units
            importer.mipmapEnabled = sprite;
            importer.filterMode = sprite ? FilterMode.Trilinear : FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            if (sprite) AndroidAstc4x4(importer);
        }

        /// The UI kit and the logo on Android: ASTC 4x4 is 8 bits a pixel instead of 32 and keeps the thin ink outlines
        /// (the PNGs are @4x and shown smaller, through mip-maps). Uncompressed they were ~37 MB of the APK.
        static void AndroidAstc4x4(TextureImporter importer)
        {
            var android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.format = TextureImporterFormat.ASTC_4x4;
            android.compressionQuality = 100;
            android.maxTextureSize = 2048;
            importer.SetPlatformTextureSettings(android);
        }

        // Format of a slices.txt line: `name left bottom right top sliced|tiled fill_center`
        static bool TryReadBorder(string name, out Vector4 border)
        {
            border = default;
            var file = Dir + "slices.txt";
            if (!File.Exists(file)) return false;
            foreach (var line in File.ReadLines(file))
            {
                var p = line.Split(' ');
                if (p.Length < 5 || p[0] != name) continue;
                border = new Vector4(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]), float.Parse(p[4]));
                return true;
            }
            return false;
        }
    }
}

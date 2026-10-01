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
        const string IconDir = "Assets/Art/Brand/";            // launcher icon layers: plain textures for PlayerSettings
        // PNGs are @4x; with the default Canvas reference of 100 ppu this gives 1 canvas unit = 1 px of the 360x780 layout.
        public const float PixelsPerUnit = 400f;

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

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;   // 9-slice needs the full rect
            if (TryReadBorder(Path.GetFileNameWithoutExtension(assetPath), out var border))
                settings.spriteBorder = border;
            importer.SetTextureSettings(settings);
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

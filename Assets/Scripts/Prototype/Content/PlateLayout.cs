using System;
using System.Collections.Generic;

namespace LegacyThroughTime.Prototype
{
    /// Where a plate stands. The rule: the top edge hangs on a fixed line and the text grows DOWN; the lowest
    /// edge allowed is the options zone (the options overlap the plate a little); a text that does not fit
    /// above that limit grows UP from it. Pure arithmetic, tested in Tests/Reader.
    static class PlateLayout
    {
        public const float MinHeight = 110;

        /// The line of the plate top for a canvas of the given height: a fixed share of it, so that on a
        /// shorter screen (MatePad) the plate keeps its place relative to the painted characters.
        public static float TopLine(float canvasHeight) => Metrics.PlateTop * canvasHeight / Metrics.RefHeight;

        public static float Height(PlateSkin skin, float textHeight)
        {
            var h = Math.Max(MinHeight, textHeight + skin.PadTop + skin.PadBottom);
            return skin.Tiled ? RoundUpToTile(h, skin.TileCorner, skin.TileSize) : h;
        }

        /// Tiled edges repeat whole tiles, so a cloud is the corners plus a whole number of tiles.
        public static float RoundUpToTile(float height, float corners, float tile) =>
            corners + tile * (float)Math.Max(0, Math.Ceiling((height - corners) / tile));

        public static float StackHeight(IReadOnlyList<float> heights, float gap)
        {
            if (heights.Count == 0) return 0;
            var sum = gap * (heights.Count - 1);
            foreach (var h in heights) sum += h;
            return sum;
        }

        /// Bottom edge of the plate. `optionsHeight` is the stack of options under it (0 when there are none).
        public static float Bottom(float topLine, float height, float floor, float optionsHeight)
        {
            var limit = optionsHeight > 0 ? floor + optionsHeight - Metrics.OptionsOverlap : floor;
            return Math.Max(topLine - height, limit);
        }
    }
}

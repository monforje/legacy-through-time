using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// An Image whose Tiled mode is CSS `border-image: round`: the corners stay, the edges and the centre repeat a
    /// WHOLE number of tiles, each stretched or squeezed a little to fill the length. Unity's own Tiled cuts the last
    /// tile, which cuts an ornament of the edge in half; Sliced stretches the edge, which smears it. With round tiles
    /// the patterns of the Blender kit stay whole at any size, while a card or a plate grows and shrinks.
    /// Every other type (and a sprite without borders) is drawn as by Image.
    sealed class KitImage : Image
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var s = overrideSprite;
            if (type != Type.Tiled || !s || s.border == Vector4.zero) { base.OnPopulateMesh(vh); return; }
            vh.Clear();

            var r = GetPixelAdjustedRect();
            var ppu = pixelsPerUnit * pixelsPerUnitMultiplier;
            var border = s.border / ppu;                                   // left, bottom, right, top in canvas units
            var tile = new Vector2(s.rect.width - s.border.x - s.border.z, s.rect.height - s.border.y - s.border.w) / ppu;
            // a rect smaller than its two borders squeezes the borders, as Sliced does
            var kx = border.x + border.z > r.width ? r.width / (border.x + border.z) : 1;
            var ky = border.y + border.w > r.height ? r.height / (border.y + border.w) : 1;
            border = new Vector4(border.x * kx, border.y * ky, border.z * kx, border.w * ky);

            var outer = DataUtility.GetOuterUV(s);
            var inner = DataUtility.GetInnerUV(s);
            var xs = Cuts(r.xMin, r.xMax, border.x, border.z, tile.x);
            var ys = Cuts(r.yMin, r.yMax, border.y, border.w, tile.y);
            var c = (Color32)color;
            for (var i = 0; i < xs.Length - 1; i++)
            for (var j = 0; j < ys.Length - 1; j++)
            {
                bool first = i == 0, last = i == xs.Length - 2, bottom = j == 0, top = j == ys.Length - 2;
                if (!first && !last && !bottom && !top && !fillCenter) continue;
                var (u0, u1) = first ? (outer.x, inner.x) : last ? (inner.z, outer.z) : (inner.x, inner.z);
                var (v0, v1) = bottom ? (outer.y, inner.y) : top ? (inner.w, outer.w) : (inner.y, inner.w);
                Quad(vh, new Vector2(xs[i], ys[j]), new Vector2(xs[i + 1], ys[j + 1]), new Vector2(u0, v0), new Vector2(u1, v1), c);
            }
        }

        /// The cut lines along one axis: the start border, n equal tiles, the end border (no tiles when the middle is empty).
        static float[] Cuts(float min, float max, float start, float end, float tile)
        {
            var middle = max - min - start - end;
            var n = middle <= 1e-3f || tile <= 1e-3f ? 0 : Mathf.Max(1, Mathf.RoundToInt(middle / tile));
            if (n == 0) return new[] { min, min + start, max };     // the two borders meet
            var cuts = new float[n + 3];
            cuts[0] = min;
            for (var k = 0; k <= n; k++) cuts[1 + k] = min + start + middle * k / n;
            cuts[n + 2] = max;
            return cuts;
        }

        static void Quad(VertexHelper vh, Vector2 p0, Vector2 p1, Vector2 uv0, Vector2 uv1, Color32 c)
        {
            var k = vh.currentVertCount;
            vh.AddVert(new Vector3(p0.x, p0.y), c, new Vector4(uv0.x, uv0.y));
            vh.AddVert(new Vector3(p0.x, p1.y), c, new Vector4(uv0.x, uv1.y));
            vh.AddVert(new Vector3(p1.x, p1.y), c, new Vector4(uv1.x, uv1.y));
            vh.AddVert(new Vector3(p1.x, p0.y), c, new Vector4(uv1.x, uv0.y));
            vh.AddTriangle(k, k + 1, k + 2);
            vh.AddTriangle(k + 2, k + 3, k);
        }
    }
}

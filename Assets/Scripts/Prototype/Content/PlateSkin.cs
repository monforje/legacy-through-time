using System;

namespace LegacyThroughTime.Prototype
{
    enum TailSide { None, Right, Left }
    enum TagSide { Left, Center, Right }
    enum Ornament { None, Knot, Bubbles }

    /// Everything that makes one style of plate look like itself. A new style is one more entry in PlateSkins.
    sealed class PlateSkin
    {
        public string Sprite;               // 9-slice sprite of the body
        public bool Tiled;                  // the edges repeat whole tiles (clouds) instead of stretching
        public float TileSize, TileCorner;  // tiled only: tile of the edges and the corner part of the sprite border
        public TailSide Tail;               // the horn is part of the sprite; this only picks the sprite and the headroom
        public float PadX, PadTop, PadBottom;
        public TagSide TagSide;
        public string TagSprite = "label";
        public bool TagDark, TagGem;
        public Ornament Ornament;

        /// Extra height of the body image above the plate for the horn.
        public float Headroom => Tail == TailSide.None ? 0 : 32;
    }

    static class PlateSkins
    {
        // The corner ornaments of a plate end 25 px from its edge; the text never touches them (PadX 30).
        static PlateSkin Plain(string sprite, TagSide side = TagSide.Center, Ornament ornament = Ornament.None) =>
            new PlateSkin { Sprite = sprite, PadX = 30, PadTop = 28, PadBottom = 26, TagSide = side, Ornament = ornament };

        static PlateSkin Cloud(string sprite) => new PlateSkin
        {
            Sprite = sprite, Tiled = true, TileSize = 40, TileCorner = 80, PadX = 34, PadTop = 30, PadBottom = 28,
            TagSide = TagSide.Left, Ornament = Ornament.Bubbles,
        };

        public static PlateSkin For(PlateStyle style) => style switch
        {
            PlateStyle.Narr => Plain("plate-mint", ornament: Ornament.Knot),
            PlateStyle.Fact => Plain("plate", ornament: Ornament.None).With(s => s.TagGem = true),
            PlateStyle.Hint => Plain("plate-rose", ornament: Ornament.Knot).With(s => { s.TagSprite = "label-rose"; s.TagDark = true; }),
            PlateStyle.SpeechLeft => Plain("plate-tail-r", TagSide.Left).With(s => s.Tail = TailSide.Right),
            PlateStyle.SpeechRight => Plain("plate-tail-l", TagSide.Right).With(s => s.Tail = TailSide.Left),
            PlateStyle.Thought => Cloud("thought"),
            PlateStyle.ThoughtPuff => Cloud("thought-cloud"),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };

        static PlateSkin With(this PlateSkin skin, Action<PlateSkin> edit) { edit(skin); return skin; }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// A plate of text: body (9-slice or tiled sprite, with the speech horn built into it), the text, the
    /// ornaments (knot, thought bubbles) and the name tag. Built and measured here, placed by the owner.
    sealed class PlateView
    {
        public readonly RectTransform Root;
        public readonly PlateSkin Skin;
        public readonly Text Body;
        public readonly string FullText;
        public readonly float Height;
        public readonly NameTag Tag;
        /// Pieces that move on their own when the plate appears and disappears.
        public readonly RectTransform Knot, BubbleNear, BubbleFar;

        public PlateView(Transform parent, PlateSpec spec)
        {
            Skin = PlateSkins.For(spec.Style);
            Root = Ui.Rect(parent, "Plate");

            // The speech tail is part of the body sprite (one outline, no seam to align): the sprite has headroom
            // above the plate for the horn, so the body image reaches that far over the top edge.
            var body = Ui.Rect(Root, "Body");
            Ui.Stretch(body, 0, 0, 0, -Skin.Headroom);
            var img = body.gameObject.AddComponent<Image>();
            img.sprite = Art.Sprite(Skin.Sprite); img.type = Skin.Tiled ? Image.Type.Tiled : Image.Type.Sliced; img.raycastTarget = false;
            Ui.Lift(img, 8, .34f);

            FullText = TextRules.NoBreaks(spec.Text);
            Body = Ui.Txt(Root, "Text", FullText, Art.Body, Metrics.FsBody, Theme.Ink);
            Height = PlateLayout.Height(Skin, Ui.TextHeight(Body, Metrics.PlateWidth - 2 * Skin.PadX));
            Ui.Stretch(Body.rectTransform, Skin.PadX, Skin.PadBottom, Skin.PadX, Skin.PadTop);

            switch (Skin.Ornament)
            {
                case Ornament.Knot:
                    Knot = Ui.Icon(Root, "Knot", "knot", A.TopRight, A.TopRight, new Vector2(-8, 32)).rectTransform;
                    break;
                case Ornament.Bubbles:
                    BubbleNear = Ui.Icon(Root, "Bubble1", "bubble", A.TopRight, A.TopRight, new Vector2(-72, 24), 18).rectTransform;
                    BubbleFar = Ui.Icon(Root, "Bubble2", "bubble", A.TopRight, A.TopRight, new Vector2(-90, 42), 10).rectTransform;
                    break;
            }

            Tag = new NameTag(Root, new NameTagSpec
            {
                Text = spec.Label, Side = Skin.TagSide, Sprite = Skin.TagSprite, Dark = Skin.TagDark, Gem = Skin.TagGem,
            });
        }

        public void PlaceBottom(float bottom) =>
            Ui.BottomRow(Root, Metrics.Side, Metrics.Side, bottom, Height);
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    sealed class NameTagSpec
    {
        public string Text;
        public TagSide Side = TagSide.Center;
        public string Sprite = "label";
        public bool Dark, Gem, Italic;
        /// > 0: a fixed width (a share of the panel); otherwise it fits the text.
        public float FixedWidth;
        /// How far the tag top stands above the top edge of its parent.
        public float Lift = 17;
    }

    /// The tilted sticker on the top edge of a plate or panel: a name, "Факт", an action.
    sealed class NameTag
    {
        const float MinWidth = 110, MinHeight = 36, PadLeft = 30, Margin = 24;

        public readonly RectTransform Root;

        public NameTag(Transform parent, NameTagSpec spec)
        {
            Root = Ui.Rect(parent, "Label");
            var img = Root.gameObject.AddComponent<Image>();
            img.sprite = Art.Sprite(spec.Sprite); img.type = Image.Type.Sliced; img.raycastTarget = false;
            Ui.Lift(img, 4, .38f);

            var text = Ui.Txt(Root, "Text", spec.Text, Art.Sub, Metrics.FsLabel, spec.Dark ? Theme.Ink : Theme.Parchment,
                              TextAnchor.MiddleCenter, spec.Italic ? FontStyle.Italic : FontStyle.Normal);
            if (!spec.Dark) Ui.Emboss(text);

            var padRight = spec.Gem ? 38f : 30f;
            var width = spec.FixedWidth > 0 ? spec.FixedWidth : Mathf.Max(MinWidth, Ui.TextWidth(text) + 60 + (spec.Gem ? 8 : 0));
            var height = Mathf.Max(MinHeight, Ui.TextHeight(text, width - PadLeft - padRight) + 10);
            Ui.Stretch(text.rectTransform, PadLeft, 6, padRight, 4);

            Place(spec.Side, width, height, spec.Lift);
            Decorate(spec.Gem);
        }

        void Place(TagSide side, float width, float height, float lift)
        {
            var x = side == TagSide.Left ? Margin + width / 2 : side == TagSide.Right ? -(Margin + width / 2) : 0;
            var anchor = new Vector2(side == TagSide.Left ? 0 : side == TagSide.Right ? 1 : .5f, 1);
            Ui.Box(Root, anchor, A.Center, new Vector2(x, lift - height / 2), new Vector2(width, height));
            // A tilted sticker, a different angle for each side. (The web version rotates clockwise, Unity counter-clockwise.)
            Root.localEulerAngles = new Vector3(0, 0, side == TagSide.Left ? 1.6f : side == TagSide.Right ? -1.4f : .6f);
        }

        void Decorate(bool gem)
        {
            Ui.Icon(Root, "OrnL", "label-orn", A.MidLeft, A.MidLeft, new Vector2(12, 0));
            if (gem) Ui.Icon(Root, "Gem", "gem", A.MidRight, A.MidRight, new Vector2(-10, 0), 22);
            else Ui.Icon(Root, "OrnR", "label-orn", A.MidRight, A.MidRight, new Vector2(-12, 0));
        }
    }
}

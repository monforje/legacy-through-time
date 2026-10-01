using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Anchors and pivots by name.
    static class A
    {
        public static readonly Vector2 TopLeft = new(0, 1), TopCenter = new(.5f, 1), TopRight = new(1, 1);
        public static readonly Vector2 MidLeft = new(0, .5f), Center = new(.5f, .5f), MidRight = new(1, .5f);
        public static readonly Vector2 BottomLeft = new(0, 0), BottomCenter = new(.5f, 0), BottomRight = new(1, 0);
    }

    /// Small builders for uGUI in code.
    static class Ui
    {
        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Stretch(RectTransform r, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(left, bottom); r.offsetMax = new Vector2(-right, -top);
            return r;
        }

        /// Point-anchored box: `anchor` is both anchorMin and anchorMax.
        public static RectTransform Box(RectTransform r, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            r.anchorMin = r.anchorMax = anchor; r.pivot = pivot;
            r.anchoredPosition = pos; r.sizeDelta = size;
            return r;
        }

        /// Stretches horizontally between margins, sits on `bottom` and is `height` tall.
        public static RectTransform BottomRow(RectTransform r, float left, float right, float bottom, float height)
        {
            r.anchorMin = A.BottomLeft; r.anchorMax = A.BottomRight; r.pivot = A.BottomCenter;
            r.offsetMin = new Vector2(left, bottom); r.offsetMax = new Vector2(-right, bottom + height);
            return r;
        }

        /// Stretches horizontally between margins, hangs from `top` (distance from the top edge, positive down).
        public static RectTransform TopRow(RectTransform r, float left, float right, float top, float height)
        {
            r.anchorMin = A.TopLeft; r.anchorMax = A.TopRight; r.pivot = A.TopCenter;
            r.offsetMin = new Vector2(left, -top - height); r.offsetMax = new Vector2(-right, -top);
            return r;
        }

        public static Image Img(Transform parent, string name, Sprite sprite, Image.Type type = Image.Type.Simple, bool fillCenter = true)
        {
            var img = Rect(parent, name).gameObject.AddComponent<Image>();
            img.sprite = sprite; img.type = type; img.fillCenter = fillCenter; img.raycastTarget = false;
            return img;
        }

        /// A 9-slice (or tiled) image of a Heritage sprite.
        public static Image Sliced(Transform parent, string name, string sprite, bool tiled = false, bool fillCenter = true) =>
            Img(parent, name, Art.Sprite(sprite), tiled ? Image.Type.Tiled : Image.Type.Sliced, fillCenter);

        /// An image with the sprite's own size (or the given width, keeping the aspect), anchored at a point.
        public static Image Icon(Transform parent, string name, string sprite, Vector2 anchor, Vector2 pivot, Vector2 pos,
                                 float width = 0, Color? tint = null)
        {
            var s = Art.Sprite(sprite);
            var img = Img(parent, name, s);
            var size = Art.NativeSize(s);
            if (width > 0) size *= width / size.x;
            Box(img.rectTransform, anchor, pivot, pos, size);
            img.color = tint ?? Color.white;
            return img;
        }

        public static Text Txt(Transform parent, string name, string text, Font font, int size, Color color,
                                TextAnchor align = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var t = Rect(parent, name).gameObject.AddComponent<Text>();
            t.font = font; t.fontSize = size; t.color = color; t.alignment = align; t.fontStyle = style;
            t.supportRichText = true; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = 1f;
            t.text = text;
            return t;
        }

        /// Height of the text when wrapped to the given width.
        public static float TextHeight(Text t, float width)
        {
            t.rectTransform.sizeDelta = new Vector2(width, 0);
            return t.preferredHeight;
        }

        /// Width of the text on one line.
        public static float TextWidth(Text t)
        {
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var w = t.preferredWidth;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            return w;
        }

        /// Pressed-in emboss of light text on dark parts.
        public static void Emboss(Text t)
        {
            var s = t.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0, 0, 0, .45f); s.effectDistance = new Vector2(0, -1);
        }

        /// Soft cast shadow imitation (the hard +3 px one is baked into the sprites).
        public static void Lift(Graphic g, float dy = 6, float alpha = .3f)
        {
            var s = g.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(.03f, .09f, .065f, alpha); s.effectDistance = new Vector2(0, -dy);
        }

        /// The CanvasGroup of an object, added if there is none. TryGetComponent, not `?? AddComponent`: in the Editor
        /// a missing component is a "fake null" that `??` does not see.
        public static CanvasGroup Group(Component c) =>
            c.TryGetComponent<CanvasGroup>(out var g) ? g : c.gameObject.AddComponent<CanvasGroup>();
    }
}

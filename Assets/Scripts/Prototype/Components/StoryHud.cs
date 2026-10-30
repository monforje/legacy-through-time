using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The tools over the screens of every story: back to the menu on the left, the collapse arrow next to it (it hides the
    /// text so that the picture can be seen, and brings it back), settings on the right. Round, greyish and translucent: the
    /// picture shows through them, they do not shout over it. The settings modal (StorySettings) has the same glass.
    sealed class StoryHud
    {
        /// The glass of the tools: a cool grey that lets the picture through.
        public static readonly Color Glass = new Color(.36f, .38f, .43f, .5f);
        public static readonly Color GlassPanel = new Color(.16f, .18f, .23f, .93f);

        public readonly RectTransform Root;
        readonly RectTransform arrow;
        readonly IAnimator anim;
        readonly Action<bool> collapse;
        bool collapsed;

        public bool Collapsed => collapsed;

        public StoryHud(RectTransform hud, Action back, Action<bool> collapse, Action settings, IAnimator anim)
        {
            this.anim = anim; this.collapse = collapse;
            Root = Ui.Rect(hud, "StoryHud");
            Ui.Stretch(Root);
            var top = Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop;
            Seal("Back", A.TopLeft, new Vector2(Kit.Side, -top), back).Glyph(Kit.Glyph("back"));
            // next to the back button: the balance chip of a choice takes the middle and the right of the top row
            var middle = Seal("Collapse", A.TopLeft, new Vector2(Kit.Side + Kit.Touch + Kit.Space2, -top), () => Collapse(!collapsed));
            arrow = Ui.Icon(middle.Root, "Chevron", "chevron-down", A.Center, A.Center, new Vector2(0, 1), 18, new Color(1, 1, 1, .95f)).rectTransform;
            Seal("Settings", A.TopRight, new Vector2(-Kit.Side, -top), settings).Glyph(Kit.Glyph("gear"));
        }

        /// The text is back (a new screen of the story brings it; the arrow turns down again).
        public void Reset()
        {
            if (!collapsed) return;
            collapsed = false;
            arrow.localEulerAngles = Vector3.zero;
        }

        public void Destroy()
        {
            if (Root) UnityEngine.Object.Destroy(Root.gameObject);
        }

        void Collapse(bool on)
        {
            collapsed = on;
            collapse(on);
            var from = on ? 0f : 180f; var to = on ? 180f : 0f;
            if (!anim.Enabled) { arrow.localEulerAngles = new Vector3(0, 0, to); return; }
            anim.Play(Tween.To(.25f, e => { if (arrow) arrow.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(from, to, e)); }, Easing.InOut));
        }

        sealed class SealButton
        {
            public RectTransform Root;
            public void Glyph(Sprite glyph) => Kit.Icon(Root, "Glyph", glyph, 22).raycastTarget = false;
        }

        SealButton Seal(string name, Vector2 anchor, Vector2 pos, Action click)
        {
            var seal = Ui.Img(Root, name, ProceduralArt.Disc);
            seal.color = Glass; seal.raycastTarget = true;
            Ui.Box(seal.rectTransform, anchor, anchor, pos, new Vector2(Kit.Touch, Kit.Touch));
            PressButton.On(seal.gameObject, click, .94f);
            return new SealButton { Root = seal.rectTransform };
        }
    }
}

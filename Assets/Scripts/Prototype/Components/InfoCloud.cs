using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// An explanation of a term at the top of the screen, with a red "!" seal on its corner. Closes on a tap.
    sealed class InfoCloud
    {
        const float Margin = 40, MinHeight = 120, TileSize = 32, TileCorners = 64, PadX = 30, PadY = 26;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly CanvasGroup group;
        bool dismissed;

        public InfoCloud(Transform parent, string text, System.Func<System.Action, System.Action> guard, IAnimator anim)
        {
            this.anim = anim;
            Root = Ui.Rect(parent, "Info");
            var img = Root.gameObject.AddComponent<Image>();
            img.sprite = Art.Sprite("info"); img.type = Image.Type.Tiled; img.raycastTarget = true;
            Ui.Lift(img, 8, .34f);

            var width = Metrics.RefWidth - 2 * Margin;
            var label = Ui.Txt(Root, "Text", TextRules.NoBreaks(text), Art.Body, Metrics.FsBody, Theme.Ink, TextAnchor.MiddleCenter);
            var height = Mathf.Max(MinHeight, Ui.TextHeight(label, width - 2 * PadX) + 2 * PadY);
            height = PlateLayout.RoundUpToTile(height, TileCorners, TileSize);      // the edges repeat whole tiles
            Ui.Stretch(label.rectTransform, PadX, PadY, PadX, PadY);
            Ui.Box(Root, A.TopCenter, A.TopCenter, new Vector2(0, -(Metrics.InsetTop + 16 + Viewport.ExtraTop)), new Vector2(width, height));
            Ui.Icon(Root, "Badge", "badge", A.TopRight, A.Center, new Vector2(-2, 3));

            group = Ui.Group(Root);
            PressButton.On(Root.gameObject, guard(Dismiss), 1);
            if (anim.Enabled)
                anim.Play(Tween.To(.42f, e => { Root.localScale = Vector3.one * (.9f + .1f * e); group.alpha = e; }));
        }

        public void Dismiss()
        {
            if (dismissed || !Root) return;
            dismissed = true;
            anim.Play(Effects.DismissUp(Root, group));
        }
    }
}

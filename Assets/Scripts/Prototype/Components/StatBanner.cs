using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// "+1 Память степи": a green ribbon that drops in from the top, and leaves by itself after a few seconds or on a tap.
    sealed class StatBanner
    {
        const float ShowTime = .45f, StayTime = 3.5f, Height = 56;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly CanvasGroup group;
        readonly float top;
        bool dismissed;

        public StatBanner(Transform parent, string text, IAnimator anim)
        {
            this.anim = anim;
            Root = Ui.Rect(parent, "Stat");
            var img = Root.gameObject.AddComponent<Image>();
            img.sprite = Art.Sprite("banner"); img.type = Image.Type.Sliced; img.raycastTarget = true;
            Ui.Lift(img, 5, .34f);
            top = -(Metrics.InsetTop + 24 + Viewport.ExtraTop);
            Ui.Box(Root, A.TopCenter, A.TopCenter, new Vector2(0, top), new Vector2(Metrics.PlateWidth, Height));

            var label = Ui.Txt(Root, "Text", TextRules.NoBreaks(text), Art.Action, Metrics.FsAction, Theme.Parchment, TextAnchor.MiddleCenter);
            Ui.Emboss(label); Ui.Stretch(label.rectTransform, 60, 11, 52, 8);
            Ui.Icon(Root, "Icon", "stat-steppe", A.MidLeft, A.MidLeft, new Vector2(18, 1.5f), 32);

            group = Ui.Group(Root);
            PressButton.On(Root.gameObject, Dismiss, 1);
            if (!anim.Enabled) return;
            anim.Play(Tween.To(ShowTime, e => { Root.anchoredPosition = new Vector2(0, top + 24 * (1 - e)); group.alpha = e; }));
            anim.Play(Tween.After(StayTime, Dismiss));
        }

        public void Dismiss()
        {
            if (dismissed || !Root) return;
            dismissed = true;
            anim.Play(Leave());
        }

        IEnumerator Leave()
        {
            yield return Effects.DismissUp(Root, group);
            if (Root) Object.Destroy(Root.gameObject);
        }
    }
}

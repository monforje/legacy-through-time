using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// A block docked to the bottom of the screen (choice list, picker): the frame, a name tag on top and a
    /// round arrow above it that slides the whole block down off the screen and back. It also slides in
    /// from below when it appears. The owner puts the content into `Root`.
    sealed class SlidePanel
    {
        const float SlideTime = .45f, FadeTime = .27f, StartDelay = .12f;

        public readonly RectTransform Root;
        public readonly PressButton CollapseButton;
        public bool Collapsed { get; private set; }

        readonly IAnimator anim;
        readonly float height, baseY;
        readonly RectTransform arrow;
        readonly List<CanvasGroup> dissolving = new();     // parts that fade while the block slides (name tag, balance)

        public SlidePanel(Transform parent, float height, float bottom, string label, float labelShare, IAnimator anim)
        {
            this.anim = anim; this.height = height;
            baseY = bottom + Viewport.ExtraBottom;

            Root = Ui.Rect(parent, "Panel");
            var frame = Ui.Sliced(Root, "Frame", "plate");
            Ui.Stretch(frame.rectTransform);
            frame.raycastTarget = true;
            Ui.Lift(frame, 8, .34f);
            Ui.BottomRow(Root, Metrics.Side, Metrics.Side, baseY, height);

            var tag = new NameTag(Root, new NameTagSpec { Text = label, Italic = true, FixedWidth = Metrics.PlateWidth * labelShare, Lift = 20 });
            dissolving.Add(Ui.Group(tag.Root));

            var holder = Ui.Rect(Root, "Collapse");
            Ui.Box(holder, A.TopCenter, A.Center, new Vector2(0, 62), new Vector2(48, 40));
            var seal = Ui.Img(holder, "Round", Art.Sprite("round"));
            Ui.Box(seal.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(36, 38));
            seal.raycastTarget = true;
            Ui.Lift(seal, 2, .5f);
            arrow = Ui.Icon(holder, "Chevron", "chevron-down", A.Center, A.Center, new Vector2(0, 2), 16, Theme.Parchment).rectTransform;
            var touch = holder.gameObject.AddComponent<Image>(); touch.color = Color.clear; touch.raycastTarget = true;
            CollapseButton = PressButton.On(holder.gameObject, null);

            if (anim.Enabled)
            {
                SetShown(0);
                Collapsed = true;
                anim.Play(Tween.After(StartDelay, () => Collapse(false)));
            }
        }

        public void Dissolve(CanvasGroup group) => dissolving.Add(group);

        public void Toggle() => Collapse(!Collapsed);

        public void Collapse(bool on)
        {
            Collapsed = on;
            var from = (baseY - Root.anchoredPosition.y) / (height + Metrics.CollapseExtra);      // how deep it is now (0..1)
            var to = on ? 1f : 0f;
            anim.Play(Tween.To(SlideTime, e => SetShown(1 - Mathf.Lerp(from, to, e)), Easing.InOut));
            anim.Play(Tween.To(SlideTime, e => arrow.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(on ? 0 : 180, on ? 180 : 0, e)), Easing.InOut));
            // the name tag and the balance dissolve instead of jumping
            anim.Play(Tween.To(FadeTime, e => { var a = on ? 1 - e : e; foreach (var g in dissolving) if (g) g.alpha = a; }, Easing.Linear));
        }

        /// 1 = in place, 0 = sunk below the screen.
        void SetShown(float shown) =>
            Root.anchoredPosition = new Vector2(0, baseY - (height + Metrics.CollapseExtra) * (1 - shown));
    }
}

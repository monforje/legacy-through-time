using System;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Shows screens one after another and owns what is shared between them: the transition (the old plate
    /// leaves, the new one comes a moment later) and the stat banners above the screens.
    sealed class ScreenHost
    {
        public enum Transition
        {
            /// The old screen plays its exit, the new one comes in after a short pause.
            Cross,
            /// The new screen replaces the old at once (the same plate stays, only options are added).
            Replace,
        }

        const float CrossPause = .12f;

        readonly MonoBehaviour owner;
        readonly IAnimator overlayAnimator;      // banners outlive the screens, so they animate on the host's owner
        readonly RectTransform pages, overlay;
        StatBanner banner;

        public PageView Current { get; private set; }

        public ScreenHost(MonoBehaviour owner, RectTransform canvas)
        {
            this.owner = owner;
            pages = Ui.Rect(canvas, "Pages"); Ui.Stretch(pages);
            overlay = Ui.Rect(canvas, "Overlay"); Ui.Stretch(overlay);
            overlayAnimator = owner.gameObject.AddComponent<CoroutineAnimator>();
        }

        public PageView Show(Page page, Bank bank, Action next, Transition transition = Transition.Cross)
        {
            var old = Current;
            var delay = old != null && transition == Transition.Cross ? CrossPause : 0;
            Current = new PageView(pages, page, bank, next, animated: true, introDelay: delay);
            if (old == null) return Current;
            if (transition == Transition.Cross) owner.StartCoroutine(old.Outro());
            else { old.Freeze(); old.Dispose(); }
            return Current;
        }

        /// A stat banner over whatever is on the screen; a new one replaces the old.
        public void Banner(string text)
        {
            banner?.Dismiss();
            banner = new StatBanner(overlay, text, overlayAnimator);
        }
    }
}

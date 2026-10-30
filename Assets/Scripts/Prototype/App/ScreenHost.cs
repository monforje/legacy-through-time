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
        readonly RectTransform pages, shell, overlay;

        /// The 360-wide column of the screens (the menu is built here too), the column of the corner buttons above
        /// it, and the full-canvas layer above everything (loading, banners, the settings toast).
        public RectTransform Pages => pages;
        /// The column of the shell (menu, chats): as wide as the canvas up to Viewport.MaxColumn.
        public RectTransform Shell => shell;
        public RectTransform Hud { get; }
        public RectTransform Overlay => overlay;
        public IAnimator OverlayAnimator => overlayAnimator;
        StatBanner banner;
        readonly CanvasGroup pagesGroup;

        public PageView Current { get; private set; }

        public ScreenHost(MonoBehaviour owner, RectTransform canvas)
        {
            this.owner = owner;
            pages = Column(canvas, "Pages");
            pagesGroup = pages.gameObject.AddComponent<CanvasGroup>();
            shell = Column(canvas, "Shell", Viewport.ColumnWidth);
            Hud = Column(canvas, "Hud", Viewport.ColumnWidth);
            overlay = Ui.Rect(canvas, "Overlay"); Ui.Stretch(overlay);
            overlayAnimator = owner.gameObject.AddComponent<CoroutineAnimator>();
        }

        /// The 360-wide column in the middle of a wider canvas.
        static RectTransform Column(RectTransform canvas, string name, float width = Metrics.RefWidth)
        {
            var column = Ui.Rect(canvas, name);
            column.anchorMin = A.BottomCenter; column.anchorMax = A.TopCenter; column.pivot = A.Center;
            column.offsetMin = new Vector2(-width / 2, 0); column.offsetMax = new Vector2(width / 2, 0);
            return column;
        }

        /// The text and the options hide so that the picture can be seen (the collapse arrow of a story), and come back.
        public void ShowPages(bool shown, bool animate = true)
        {
            pagesGroup.blocksRaycasts = shown;
            var from = pagesGroup.alpha; var to = shown ? 1f : 0f;
            if (!animate || !overlayAnimator.Enabled) { pagesGroup.alpha = to; return; }
            overlayAnimator.Play(Tween.To(.22f, e => { if (pagesGroup) pagesGroup.alpha = Mathf.Lerp(from, to, e); }, Easing.InOut));
        }

        /// The current screen leaves (to the menu): nothing is shown in the column afterwards.
        public void Clear()
        {
            banner?.Dismiss(); banner = null;
            if (Current == null) return;
            owner.StartCoroutine(Current.Outro());
            Current = null;
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

        /// The loading screen above everything; the caller hides it with `Hide(loading)` when the first screen is ready.
        public LoadingScreen ShowLoading() => new LoadingScreen(overlay, overlayAnimator);

        public void Hide(LoadingScreen loading) => owner.StartCoroutine(loading.Hide());

        /// A stat banner over whatever is on the screen; a new one replaces the old.
        public void Banner(string text, string icon = "stat-steppe")
        {
            banner?.Dismiss();
            banner = new StatBanner(overlay, text, overlayAnimator, icon);
        }
    }
}

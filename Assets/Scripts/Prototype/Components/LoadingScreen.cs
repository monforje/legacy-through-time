using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The loading screen: a still logo. It stays at
    /// least `MinSeconds` so that it can be seen. It leaves in two steps: the spinner goes, then the logo and the
    /// background dissolve (the owner starts the next screen in between, see StoryPlayer).
    sealed class LoadingScreen
    {
        public const float MinSeconds = 1.8f;
        /// Time from the start of Hide() after which the next screen should start: the spinner is gone, the dissolve begins.
        public const float SpinnerFade = .25f;
        const float LogoWidth = 320, Dissolve = .6f;
        const int Dots = 12;
        const float SpinnerRadius = 17, DotSize = 7, TurnSeconds = 1f;

        public readonly RectTransform Root;
        readonly CanvasGroup group, spinnerGroup;
        readonly RectTransform spinner;
        bool leaving;

        public LoadingScreen(Transform parent, IAnimator anim)
        {
            Root = Ui.Rect(parent, "Loading");
            Ui.Stretch(Root);
            group = Root.gameObject.AddComponent<CanvasGroup>();

            var back = Ui.Img(Root, "Back", ProceduralArt.White);
            back.color = Theme.LoadingBackground; back.raycastTarget = true;      // taps do not reach what is under it
            Ui.Stretch(back.rectTransform);

            // The logo does not move: no fade, no breathing.
            var art = Art.Logo;
            var native = Art.NativeSize(art);
            var image = Ui.Img(Root, "Logo", art);
            image.preserveAspect = true;
            var logoHeight = LogoWidth * native.y / native.x;
            Ui.Box(image.rectTransform, A.Center, A.Center, new Vector2(0, 24), new Vector2(LogoWidth, logoHeight));

            spinner = Ui.Rect(Root, "Spinner");
            Ui.Box(spinner, A.Center, A.Center, new Vector2(0, 24 - logoHeight / 2 - 40), new Vector2(SpinnerRadius * 2 + DotSize, SpinnerRadius * 2 + DotSize));
            spinnerGroup = spinner.gameObject.AddComponent<CanvasGroup>();
            BuildDots();

            if (anim.Enabled) anim.Play(Spin());
        }

        /// Twelve dots on a circle that fade out one after another: a comet tail that turns with the whole ring.
        void BuildDots()
        {
            for (var i = 0; i < Dots; i++)
            {
                var angle = -i * Mathf.PI * 2 / Dots + Mathf.PI / 2;
                var dot = Ui.Img(spinner, "Dot" + i, ProceduralArt.Disc);
                dot.color = Theme.Parchment.WithAlpha(Mathf.Lerp(1f, .12f, (float)i / (Dots - 1)));
                Ui.Box(dot.rectTransform, A.Center, A.Center,
                       new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SpinnerRadius, Vector2.one * DotSize * Mathf.Lerp(1f, .6f, (float)i / (Dots - 1)));
            }
        }

        IEnumerator Spin()
        {
            var started = Time.unscaledTime;
            while (Root && !leaving)
            {
                spinner.localEulerAngles = new Vector3(0, 0, -360f * ((Time.unscaledTime - started) / TurnSeconds % 1f));
                yield return null;
            }
        }

        /// The spinner goes first, then the logo and the background dissolve. Run it on an animator that outlives the screen.
        public IEnumerator Hide()
        {
            leaving = true;
            yield return Tween.To(SpinnerFade, e => { if (spinnerGroup) spinnerGroup.alpha = 1 - e; }, Easing.Linear);
            yield return Tween.To(Dissolve, e => { if (group) group.alpha = 1 - e; }, Easing.Linear);
            if (Root) Object.Destroy(Root.gameObject);
        }
    }
}

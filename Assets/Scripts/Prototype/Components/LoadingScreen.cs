using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The loading screen: the logo and a plain spinner; after `LogoSeconds` the logo gives way to a slideshow of
    /// full-screen scenes (the spinner stays). It stays at least `MinSeconds` so that it can be seen. It leaves in two steps: the spinner goes, then the logo and the
    /// background dissolve (the owner starts the next screen in between, see StoryPlayer).
    sealed class LoadingScreen
    {
        public const float LogoSeconds = 1.6f, SlideSeconds = 3f, MinSeconds = LogoSeconds + SlideSeconds;
        /// Time from the start of Hide() after which the next screen should start: the spinner is gone, the dissolve begins.
        public const float SpinnerFade = .25f;
        const float LogoWidth = 360, Dissolve = .6f, CrossFade = .7f;
        const int Slides = 3;
        const string Backdrop = "menu_campfire";
        const int Dots = 12;
        const float SpinnerRadius = 17, DotSize = 7, TurnSeconds = 1f;

        public readonly RectTransform Root;
        readonly CanvasGroup group, spinnerGroup;
        readonly RectTransform spinner;
        readonly CanvasGroup logo, slideA, slideB;
        bool leaving;

        public LoadingScreen(Transform parent, IAnimator anim)
        {
            Root = Ui.Rect(parent, "Loading");
            Ui.Stretch(Root);
            group = Root.gameObject.AddComponent<CanvasGroup>();

            var back = Ui.Img(Root, "Back", ProceduralArt.White);
            back.color = Theme.LoadingBackground; back.raycastTarget = true;      // taps do not reach what is under it
            Ui.Stretch(back.rectTransform);

            // The same painted backdrop and dim as the shell's title; the flat colour shows until the picture is in.
            var paint = Ui.Img(Root, "Paint", null);
            Ui.Stretch(paint.rectTransform);
            paint.enabled = false;
            var fit = paint.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            Art.Background(Backdrop, sprite =>
            {
                if (!paint || !sprite) return;
                paint.sprite = sprite; paint.enabled = true;
                fit.aspectRatio = sprite.rect.width / sprite.rect.height;
            });
            var dim = Ui.Img(Root, "Dim", ProceduralArt.Gradient(Theme.TitleDimTop, Theme.TitleDimBottom));
            Ui.Bleed(dim.rectTransform);

            // Scenes under the logo: they come in after it leaves.
            slideA = Slide(0); slideB = Slide(1);

            // The logo does not move: no fade, no breathing (it only leaves for the slideshow).
            var art = Art.Logo;
            var native = Art.NativeSize(art);
            var image = Ui.Img(Root, "Logo", art);
            image.preserveAspect = true;
            logo = image.gameObject.AddComponent<CanvasGroup>();
            var logoHeight = LogoWidth * native.y / native.x;
            Ui.Box(image.rectTransform, A.Center, A.Center, new Vector2(0, 40), new Vector2(LogoWidth, logoHeight));

            spinner = Ui.Rect(Root, "Spinner");
            Ui.Box(spinner, A.BottomCenter, A.Center, new Vector2(0, 64), new Vector2(SpinnerRadius * 2 + DotSize, SpinnerRadius * 2 + DotSize));
            spinnerGroup = spinner.gameObject.AddComponent<CanvasGroup>();
            BuildDots();

            if (anim.Enabled) { anim.Play(Spin()); anim.Play(Show()); }
        }

        CanvasGroup Slide(int i)
        {
            var rect = Ui.Rect(Root, "Slide" + i);
            Ui.Stretch(rect);
            var g = rect.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0; g.blocksRaycasts = false;
            var img = Ui.Img(rect, "Scene", null);
            img.raycastTarget = false;
            img.preserveAspect = false;
            var fit = img.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            return g;
        }

        void SetScene(CanvasGroup g, int n)
        {
            var img = g.GetComponentInChildren<Image>();
            var sprite = Resources.Load<Sprite>("Brand/Loading/slide" + (n % Slides + 1));
            if (!sprite) return;
            img.sprite = sprite;
            img.GetComponent<AspectRatioFitter>().aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        /// The logo for `LogoSeconds`, then it dissolves into the first scene; the scenes cross-fade one into the next.
        IEnumerator Show()
        {
            yield return new WaitForSecondsRealtime(LogoSeconds);
            var n = 0;
            var front = slideA; var back = slideB;
            SetScene(front, n);
            yield return Tween.To(CrossFade, e => { if (front) front.alpha = e; if (logo) logo.alpha = 1 - e; }, Easing.Linear);
            while (Root && !leaving)
            {
                yield return new WaitForSecondsRealtime(SlideSeconds);
                if (!Root || leaving) yield break;
                SetScene(back, ++n);
                var from = front; var to = back;
                yield return Tween.To(CrossFade, e => { if (to) to.alpha = e; if (from) from.alpha = 1 - e; }, Easing.Linear);
                front = to; back = from;
            }
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

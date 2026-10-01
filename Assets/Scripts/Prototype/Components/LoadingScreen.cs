using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The loading screen: the logo in the middle of a deep green screen, breathing slowly, with a thin red thread
    /// running under it while the story loads. It stays at least `MinSeconds` so that it can be seen, then fades out.
    sealed class LoadingScreen
    {
        public const float MinSeconds = 1.8f;
        const float LogoWidth = 320, FadeIn = .9f, FadeOut = .55f;

        public readonly RectTransform Root;
        readonly CanvasGroup group;
        readonly RectTransform logo, runner;
        readonly IAnimator anim;
        bool leaving;

        public LoadingScreen(Transform parent, IAnimator anim)
        {
            this.anim = anim;
            Root = Ui.Rect(parent, "Loading");
            Ui.Stretch(Root);
            group = Root.gameObject.AddComponent<CanvasGroup>();

            var back = Ui.Img(Root, "Back", ProceduralArt.White);
            back.color = Theme.LoadingBackground; back.raycastTarget = true;      // taps do not reach what is under it
            Ui.Stretch(back.rectTransform);

            var art = Art.Logo;
            var native = Art.NativeSize(art);
            var image = Ui.Img(Root, "Logo", art);
            image.preserveAspect = true;
            logo = image.rectTransform;
            Ui.Box(logo, A.Center, A.Center, new Vector2(0, 24), new Vector2(LogoWidth, LogoWidth * native.y / native.x));

            // the thread: a faint track and a red piece that runs along it
            var track = Ui.Img(Root, "Track", ProceduralArt.White);
            track.color = Theme.Parchment.WithAlpha(.14f);
            Ui.Box(track.rectTransform, A.Center, A.Center, new Vector2(0, -logo.sizeDelta.y / 2 - 28), new Vector2(150, 3));
            var piece = Ui.Img(track.transform, "Run", ProceduralArt.White);
            piece.color = Theme.Scarlet;
            runner = piece.rectTransform;
            Ui.Box(runner, A.MidLeft, A.MidLeft, Vector2.zero, new Vector2(46, 3));

            if (!anim.Enabled) return;
            group.alpha = 0;
            anim.Play(Run());
        }

        IEnumerator Run()
        {
            var started = Time.unscaledTime;
            while (Root && !leaving)
            {
                var t = Time.unscaledTime - started;
                var appear = Easing.OutExpo(Mathf.Clamp01(t / FadeIn));
                group.alpha = appear;
                var breathe = 1 + .015f * Mathf.Sin(t * 2.2f);                       // a slow breath
                logo.localScale = Vector3.one * Mathf.Lerp(.9f, 1f, appear) * breathe;
                var along = Mathf.PingPong(t * 1.1f, 1);                              // the thread runs back and forth
                runner.anchoredPosition = new Vector2(Mathf.Lerp(0, 150 - 46, Easing.InOut(along)), 0);
                yield return null;
            }
        }

        /// Fades out and removes itself. Run it on an animator that outlives the screen.
        public IEnumerator Hide()
        {
            leaving = true;
            var from = group.alpha;
            yield return Tween.To(FadeOut, e => { if (group) group.alpha = from * (1 - e); }, Easing.Linear);
            if (Root) Object.Destroy(Root.gameObject);
        }
    }
}

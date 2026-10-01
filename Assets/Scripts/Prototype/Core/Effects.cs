using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Small reusable animations. Each returns a routine for an IAnimator.
    static class Effects
    {
        /// "No": a short horizontal shake.
        public static IEnumerator Shake(RectTransform rt)
        {
            var x0 = rt.anchoredPosition.x;
            yield return Tween.To(.35f, e => rt.anchoredPosition = new Vector2(x0 + Mathf.Sin(e * Mathf.PI * 4) * 5 * (1 - e), rt.anchoredPosition.y), Easing.Linear);
            rt.anchoredPosition = new Vector2(x0, rt.anchoredPosition.y);
        }

        /// A press that was accepted: swell and settle.
        public static IEnumerator Pop(RectTransform rt) =>
            Tween.To(.38f, e => rt.localScale = Vector3.one * (1 + .04f * Mathf.Sin(e * Mathf.PI)));

        /// A number that changed: it jumps up and shrinks back.
        public static IEnumerator Bump(RectTransform rt) =>
            Tween.To(.38f, e => rt.localScale = Vector3.one * Mathf.Lerp(1.5f, 1, e));

        /// Text that rises a few px while it fades in. Starts hidden *before* the delay, so it never blinks.
        public static IEnumerator FadeUp(Text t, float delay, float rise = 8)
        {
            var rt = t.rectTransform; var y0 = rt.anchoredPosition.y; var c = t.color.WithAlpha(1);
            t.color = c.WithAlpha(0);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0 - rise);
            yield return new WaitForSecondsRealtime(delay);
            yield return Tween.To(.6f, e => { t.color = c.WithAlpha(e); rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0 - rise * (1 - e)); });
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0);
        }

        /// Fade and drift of a card at the top of the screen when it is dismissed.
        public static IEnumerator DismissUp(RectTransform rt, CanvasGroup group, float drift = 16)
        {
            var y0 = rt.anchoredPosition.y;
            group.blocksRaycasts = false;
            yield return Tween.To(.3f, e => { group.alpha = 1 - e; rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0 + drift * e); }, Easing.Linear);
        }
    }
}

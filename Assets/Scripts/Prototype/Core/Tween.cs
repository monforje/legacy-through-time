using System;
using System.Collections;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    static class Easing
    {
        /// Fast start, long settle (~ cubic-bezier(.22, 1, .36, 1)): the default for things that move in.
        public static float OutExpo(float t) => 1f - Mathf.Pow(1f - t, 4f);
        /// Overshoots slightly and settles: for things that "pop" into place.
        public static float OutBack(float t) { const float c1 = 1.4f, c3 = c1 + 1f; var u = t - 1f; return 1f + c3 * u * u * u + c1 * u * u; }
        public static float InOut(float t) => t < .5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        public static float InQuad(float t) => t * t;
        public static float Linear(float t) => t;
    }

    /// Time-based interpolation as a coroutine. Uses unscaled time: the UI does not slow down with Time.timeScale.
    static class Tween
    {
        public static IEnumerator To(float seconds, Action<float> apply, Func<float, float> ease = null, float delay = 0)
        {
            ease ??= Easing.OutExpo;
            if (delay > 0) yield return new WaitForSecondsRealtime(delay);
            for (var t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                apply(ease(t / seconds));
                yield return null;
            }
            apply(ease(1f));
        }

        public static IEnumerator After(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action();
        }
    }
}

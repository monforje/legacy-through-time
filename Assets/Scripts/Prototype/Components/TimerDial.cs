using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Countdown of a timed choice: a ring with a pie that drains clockwise; pulses in the last seconds.
    sealed class TimerDial
    {
        const float PulseFrom = 3;      // seconds left when it starts to pulse

        public readonly RectTransform Root;
        readonly Image fill;

        public TimerDial(RectTransform plate)
        {
            // above the right top corner of the plate, so that it does not cover the name tag
            Root = Ui.Rect(plate, "Timer");
            Ui.Box(Root, A.TopRight, A.BottomRight, new Vector2(-10, 24), new Vector2(50, 50));

            var back = Ui.Img(Root, "Back", Art.Sprite("disc")); back.color = Theme.Parchment; Ui.Stretch(back.rectTransform);
            fill = Ui.Img(Root, "Fill", Art.Sprite("disc"), Image.Type.Filled);
            fill.color = Theme.Ink; fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Top; fill.fillClockwise = true; fill.fillAmount = 1;
            Ui.Stretch(fill.rectTransform);

            var ring = Ui.Img(Root, "Ring", Art.Sprite("timer"));
            Ui.Box(ring.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(60, 60));
        }

        /// Runs until `stop()` or until the time is up (then `timeout`). Starts when the options are on screen.
        public IEnumerator Run(float seconds, Func<bool> stop, Action timeout)
        {
            yield return new WaitForSecondsRealtime(.4f);
            var t0 = Time.unscaledTime;
            while (!stop())
            {
                var left = 1 - (Time.unscaledTime - t0) / seconds;
                fill.fillAmount = Mathf.Max(left, 0);
                if (left * seconds < PulseFrom) Root.localScale = Vector3.one * (1 + .07f * (1 + Mathf.Sin(Time.unscaledTime * 9)));
                if (left <= 0) break;
                yield return null;
            }
            Root.localScale = Vector3.one;
            if (!stop()) timeout();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// How a plate appears and disappears. In: the plate rises into place, the name tag pops, the knot and the
    /// bubbles of a thought float up one after another. Out: the same in reverse and faster.
    /// Create it after the plate has been placed (it remembers the rest pose of every piece).
    sealed class PlateAnimator
    {
        enum Kind { Plate, Tag, Knot, BubbleNear, BubbleFar }

        readonly struct Timing
        {
            public readonly float Delay, Duration, From;
            public readonly Func<float, float> Ease;
            public Timing(float delay, float duration, float from, Func<float, float> ease) { Delay = delay; Duration = duration; From = from; Ease = ease; }
        }

        // The only place that says when each piece comes in.
        static readonly Dictionary<Kind, Timing> Timings = new()
        {
            [Kind.Plate] = new Timing(0, .45f, .96f, Easing.OutExpo),
            [Kind.Tag] = new Timing(.12f, .42f, .7f, Easing.OutBack),
            [Kind.Knot] = new Timing(.3f, .42f, .3f, Easing.OutBack),
            [Kind.BubbleNear] = new Timing(.3f, .38f, .2f, Easing.OutBack),
            [Kind.BubbleFar] = new Timing(.42f, .38f, .2f, Easing.OutBack),
        };

        const float SlideIn = 14, SlideOut = 10;

        sealed class Part
        {
            public Kind Kind; public RectTransform Rt; public CanvasGroup Group;
            public Vector2 Pos; public Vector3 Scale;
        }

        readonly List<Part> parts = new();

        public PlateAnimator(PlateView view)
        {
            Add(Kind.Plate, view.Root);
            Add(Kind.Tag, view.Tag.Root);
            Add(Kind.Knot, view.Knot);
            Add(Kind.BubbleNear, view.BubbleNear);
            Add(Kind.BubbleFar, view.BubbleFar);
        }

        void Add(Kind kind, RectTransform rt)
        {
            if (!rt) return;
            parts.Add(new Part { Kind = kind, Rt = rt, Group = Ui.Group(rt), Pos = rt.anchoredPosition, Scale = rt.localScale });
        }

        /// `e`: 0 = hidden, 1 = in place.
        static void Pose(Part x, float e, float slide, float shrink)
        {
            x.Group.alpha = Mathf.Clamp01(e * 1.5f);
            var s = Mathf.LerpUnclamped(1 - shrink, 1, e);
            x.Rt.localScale = new Vector3(x.Scale.x * s, x.Scale.y * s, 1);
            x.Rt.anchoredPosition = new Vector2(x.Pos.x, x.Pos.y - slide * (1 - e));
        }

        static void PoseIn(Part x, float e)
        {
            var timing = Timings[x.Kind];
            Pose(x, e, x.Kind == Kind.Plate ? SlideIn : 0, 1 - timing.From);
        }

        public void PoseHidden() { foreach (var x in parts) PoseIn(x, 0); }

        public IEnumerator Intro(float wait)
        {
            if (wait > 0) yield return new WaitForSecondsRealtime(wait);
            var total = 0f;
            foreach (var x in parts) total = Mathf.Max(total, Timings[x.Kind].Delay + Timings[x.Kind].Duration);
            for (var t = 0f; t < total; t += Time.unscaledDeltaTime)
            {
                foreach (var x in parts)
                {
                    var timing = Timings[x.Kind];
                    PoseIn(x, timing.Ease(Mathf.Clamp01((t - timing.Delay) / timing.Duration)));
                }
                yield return null;
            }
            foreach (var x in parts) Pose(x, 1, 0, 0);
        }

        /// `e`: 0 = in place, 1 = gone.
        public void PoseLeaving(float e)
        {
            foreach (var x in parts)
            {
                var plate = x.Kind == Kind.Plate;
                Pose(x, 1 - e, plate ? SlideOut : 0, plate ? .03f : .25f);
            }
        }
    }
}

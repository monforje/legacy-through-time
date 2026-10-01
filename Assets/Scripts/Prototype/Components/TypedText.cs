using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Text that appears word by word (timing: RevealPlan). A tap shows the rest at once (Finish).
    sealed class TypedText
    {
        static readonly string InkHex = ColorUtility.ToHtmlStringRGB(Theme.Ink);

        readonly Text text;
        readonly string full;
        readonly string[] words;
        readonly IAnimator anim;

        public bool Typing { get; private set; }
        /// Raised once, when the whole text is on screen (by time or by a tap).
        public event Action Finished;

        public TypedText(Text text, string full, IAnimator anim)
        {
            this.text = text; this.full = full; this.anim = anim;
            words = full.Split(' ');
        }

        public void Start(float delay)
        {
            if (!anim.Enabled) return;
            Typing = true;
            text.text = Render(0, RevealPlan.Step(words.Length));
            anim.Play(Run(delay));
        }

        /// The text was read already: show it all and raise Finished.
        public void ShowAtOnce()
        {
            if (!anim.Enabled) return;
            Typing = true;
            Finish();
        }

        public void Finish()
        {
            if (!Typing) return;
            Typing = false;
            text.text = full;
            Finished?.Invoke();
        }

        IEnumerator Run(float delay)
        {
            if (delay > 0) yield return new WaitForSecondsRealtime(delay);
            var step = RevealPlan.Step(words.Length);
            var total = RevealPlan.Total(words.Length);
            for (var t = 0f; t < total && Typing; t += Time.unscaledDeltaTime)
            {
                text.text = Render(t, step);
                yield return null;
            }
            Finish();
        }

        string Render(float t, float step)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < words.Length; i++)
            {
                var alpha = (int)(RevealPlan.Alpha(i, t, step) * 255);
                sb.Append("<color=#").Append(InkHex).Append(alpha.ToString("X2")).Append('>').Append(words[i]).Append("</color>");
                if (i < words.Length - 1) sb.Append(' ');
            }
            return sb.ToString();
        }
    }
}

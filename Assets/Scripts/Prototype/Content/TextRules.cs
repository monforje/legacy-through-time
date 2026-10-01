using System;
using System.Text.RegularExpressions;

namespace LegacyThroughTime.Prototype
{
    /// Typography and wording rules for text shown on the screens.
    static class TextRules
    {
        static readonly Regex ShortWord = new(@"(?<=^|[\s(«„])([А-Яа-яЁё]{1,2})\s", RegexOptions.Compiled);
        static readonly Regex StatLine = new(@"^\s*([А-ЯЁ][А-ЯЁ ]*?)\s+([+\-−–]\d+)\s*\.?\s*$", RegexOptions.Compiled);
        static readonly Regex Caps = new(@"[А-ЯЁ]{2,}(?:-[А-ЯЁ]+)*", RegexOptions.Compiled);
        static readonly string[] ProperNames = { "Сафии", "Юсуфа", "Сююмбике", "Казани", "Джан-Али", "Сафия", "Юсуф" };

        /// Russian typography: prepositions and conjunctions up to two letters and dashes do not detach from the next word.
        public static string NoBreaks(string text) =>
            ShortWord.Replace(text, "$1 ").Replace(" — ", " — ");

        /// "ПАМЯТЬ СТЕПИ +1" -> "+1 Память степи"; null when the text is not a change of a stat.
        public static string StatToast(string systemText)
        {
            var m = StatLine.Match(systemText);
            if (!m.Success) return null;
            return m.Groups[2].Value.Replace('−', '-').Replace('–', '-') + " " + SentenceCase(m.Groups[1].Value);
        }

        /// ALL CAPS shouting of system lines -> normal sentences; names keep their capital.
        public static string SentenceCase(string text) =>
            Caps.Replace(text, m =>
            {
                var before = text.Substring(0, m.Index).TrimEnd();
                var sentenceStart = before.Length == 0 || before.EndsWith(".") || before.EndsWith("«");
                var lower = m.Value.ToLowerInvariant();
                foreach (var name in ProperNames)
                    if (lower == name.ToLowerInvariant()) return name;
                return sentenceStart ? char.ToUpperInvariant(lower[0]) + lower.Substring(1) : lower;
            });
    }

    /// Timing of the word-by-word typing of a plate text (the plate has risen by then). Pure functions of time.
    static class RevealPlan
    {
        public const float Lead = .3f;          // seconds before the first word
        public const float FadeIn = .24f;       // one word goes from invisible to full
        public const float MaxTotal = 1.8f;     // a long text hurries up so that it never takes longer
        public const float DefaultStep = .026f;

        public static float Step(int words) => Math.Min(DefaultStep, (MaxTotal - Lead - FadeIn) / Math.Max(words, 1));
        public static float Total(int words) => words * Step(words) + Lead + FadeIn;

        /// Opacity (0..1) of word `index` at time `t`.
        public static float Alpha(int index, float t, float step)
        {
            var a = (t - (Lead + index * step)) / FadeIn;
            return a < 0 ? 0 : a > 1 ? 1 : a;
        }
    }
}

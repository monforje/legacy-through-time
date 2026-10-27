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
        static readonly Regex Marked = new(@"(\p{L}[\p{L}\-]*)\*", RegexOptions.Compiled);
        static readonly Regex Term = new(@"^([^—\n<]+?)\s+—\s+", RegexOptions.Compiled);

        /// A plate shows at most this many characters; a longer line is said in two (or more) screens.
        public const int PlateMaxChars = 150;

        /// A word marked with an asterisk ("калфак*", explained at the top of the screen) is set in italics instead.
        public static string Emphasis(string text) => Marked.Replace(text, "<i>$1</i>");

        /// "Калфак — женский головной убор…": the explained word of a note in italics.
        public static string TermFirst(string note) => Term.Replace(note, "<i>$1</i> — ", 1);

        /// A text longer than `max` characters -> parts of about equal length, cut after the end of a sentence nearest
        /// to the middle (or, without one, after a comma, or at a space), so that a long speech is said in two taps.
        public static string[] Split(string text, int max = PlateMaxChars)
        {
            text = text.Trim();
            if (text.Length <= max) return new[] { text };
            var cut = Cut(text);
            if (cut <= 0 || cut >= text.Length) return new[] { text };
            var head = Split(text.Substring(0, cut), max);
            var tail = Split(text.Substring(cut), max);
            var parts = new string[head.Length + tail.Length];
            head.CopyTo(parts, 0); tail.CopyTo(parts, head.Length);
            return parts;
        }

        /// Where to cut: the best place nearest to the middle, sentence ends first.
        static int Cut(string text)
        {
            var middle = text.Length / 2;
            foreach (var marks in new[] { ".!?…", ",;:", " " })
            {
                var best = -1;
                for (var i = 1; i < text.Length - 1; i++)
                {
                    if (text[i] != ' ') continue;
                    var before = text[i - 1];
                    if (marks != " " && marks.IndexOf(before) < 0) continue;
                    if (text.LastIndexOf('<', i) > text.LastIndexOf('>', i)) continue;      // never inside a tag
                    if (best < 0 || Math.Abs(i - middle) < Math.Abs(best - middle)) best = i;
                }
                // a sentence end is worth it only if it leaves both parts a fair share
                if (best > 0 && Math.Min(best, text.Length - best) >= text.Length / 4) return best + 1;
            }
            return -1;
        }

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

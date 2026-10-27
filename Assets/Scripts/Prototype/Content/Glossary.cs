using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LegacyThroughTime.Prototype
{
    /// The words a story explains (`# note: Калфак — женский головной убор…`): the entries of the library. Every
    /// note of a compiled story is an entry; the ones the reader has met are open (Library), the rest are shown
    /// as closed. Plain text parsing: tested by `task test:reader`.
    static class Glossary
    {
        public sealed class Term
        {
            public string Name;    // "Калфак"
            public string Text;    // "женский головной убор. Его расшивали узорами…"
        }

        static readonly Regex NoteTag = new(@"""\^note:\s*((?:[^""\\]|\\.)*)""", RegexOptions.Compiled);

        /// "Калфак — женский головной убор." -> the word and its explanation (the text with a capital letter).
        public static Term Parse(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) return null;
            note = note.Trim();
            var dash = note.IndexOf(" — ", System.StringComparison.Ordinal);
            if (dash <= 0) return new Term { Name = note, Text = "" };
            var text = note.Substring(dash + 3).Trim();
            if (text.Length > 0) text = char.ToUpperInvariant(text[0]) + text.Substring(1);
            return new Term { Name = note.Substring(0, dash).Trim(), Text = text };
        }

        /// Every term of a compiled story, in the order of the story, each once.
        public static List<Term> FromStory(string json)
        {
            var terms = new List<Term>();
            var seen = new HashSet<string>();
            foreach (Match m in NoteTag.Matches(json ?? ""))
            {
                var term = Parse(m.Groups[1].Value.Replace("\\\"", "\"").Replace("\\\\", "\\"));
                if (term != null && seen.Add(term.Name)) terms.Add(term);
            }
            return terms;
        }
    }
}

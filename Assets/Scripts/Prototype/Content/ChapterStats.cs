using System;
using System.Collections.Generic;
using System.Globalization;
using LegacyThroughTime.Narrative;

namespace LegacyThroughTime.Prototype
{
    /// One variable of the story as it was at the end of a chapter.
    [Serializable]
    sealed class StatValue
    {
        public string name;
        public string kind;      // "int", "float", "bool", "string"
        public string value;
    }

    /// What the reader became in a chapter: the story's variables (Память степи, the trust of Сафия, the knife ...) at
    /// its end, kept after its save is gone so that the next chapter starts from them instead of from zero. The coins
    /// are not here: they are the reader's purse (Wallet), not the story's.
    [Serializable]
    sealed class ChapterStats
    {
        public string chapter;
        public List<StatValue> vars = new();
        public long at;

        static readonly HashSet<string> NotCarried = new() { "gems" };

        /// The variables of the machine now; lists are left out (no story keeps one between chapters yet).
        public static ChapterStats Of(StoryMachine machine, string chapter, long at)
        {
            var stats = new ChapterStats { chapter = chapter, at = at };
            foreach (var name in machine.VariableNames)
            {
                if (NotCarried.Contains(name)) continue;
                var (kind, value) = machine.GetVariable(name) switch
                {
                    int i => ("int", i.ToString(CultureInfo.InvariantCulture)),
                    float f => ("float", f.ToString("R", CultureInfo.InvariantCulture)),
                    bool b => ("bool", b ? "true" : "false"),
                    string s => ("string", s),
                    _ => (null, null),
                };
                if (kind != null) stats.vars.Add(new StatValue { name = name, kind = kind, value = value });
            }
            return stats;
        }

        /// Puts the variables into a chapter that starts; one it does not declare, or of another type, is skipped.
        public void ApplyTo(StoryMachine machine)
        {
            var declared = new HashSet<string>(machine.VariableNames);
            foreach (var v in vars)
            {
                if (v == null || !declared.Contains(v.name) || NotCarried.Contains(v.name)) continue;
                object value = v.kind switch
                {
                    "int" => int.TryParse(v.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : null,
                    "float" => float.TryParse(v.value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : null,
                    "bool" => v.value == "true",
                    "string" => v.value,
                    _ => null,
                };
                var now = machine.GetVariable(v.name);
                if (value == null || now == null || now.GetType() != value.GetType()) continue;
                machine.SetVariable(v.name, value);
            }
        }

        /// The stats a chapter starts with: those of the end of the chapter before it in its story; null for the first
        /// chapter or when the one before was not finished.
        public static ChapterStats Before(StoryEntry story, ChapterEntry chapter, Func<string, ChapterStats> read)
        {
            ChapterEntry previous = null;
            foreach (var c in story.Chapters)
            {
                if (c == chapter) return previous != null ? read(previous.SaveId) : null;
                previous = c;
            }
            return null;
        }
    }
}

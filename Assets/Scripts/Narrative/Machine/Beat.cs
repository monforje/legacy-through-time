using System;
using System.Collections.Generic;

namespace LegacyThroughTime.Narrative
{
    /// <summary>
    /// Control states Q of the story machine. <see cref="End"/> and <see cref="Fault"/> are
    /// absorbing: no input leaves them.
    /// </summary>
    public enum StoryPhase
    {
        /// <summary>Initial state: nothing presented yet.</summary>
        Ready,
        /// <summary>A line of text is on screen; waits for Advance.</summary>
        Line,
        /// <summary>A tag-only line (stage direction: music, background…); waits for Advance.</summary>
        Directive,
        /// <summary>Options are on screen; waits for Choose(i).</summary>
        Choice,
        End,
        Fault,
    }

    /// <summary>How a line is presented. Derived from its tags by <see cref="StoryStyle"/>.</summary>
    public enum Voice
    {
        Narration,
        Dialogue,
        Thought,
        System,
        Flashback,
        Sound,
    }

    /// <summary>An ink tag split as <c>key: value</c> (<see cref="Value"/> is null for a bare <c># key</c>).</summary>
    public readonly struct StoryTag
    {
        public readonly string Key;
        public readonly string Value;

        public StoryTag(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public static StoryTag Parse(string raw)
        {
            int colon = raw.IndexOf(':');
            return colon < 0
                ? new StoryTag(raw.Trim(), null)
                : new StoryTag(raw.Substring(0, colon).Trim(), raw.Substring(colon + 1).Trim());
        }

        public override string ToString() => Value == null ? Key : Key + ": " + Value;
    }

    /// <summary>A selectable option of a <see cref="StoryPhase.Choice"/> beat.</summary>
    public sealed class StoryOption
    {
        public int Index { get; }
        /// <summary>Display text with style markup (premium mark, price) removed.</summary>
        public string Text { get; }
        /// <summary>Text exactly as written in ink.</summary>
        public string RawText { get; }
        public IReadOnlyList<StoryTag> Tags { get; }
        public bool IsPremium { get; }
        /// <summary>Price in premium currency parsed from the text, 0 if none.</summary>
        public int Cost { get; }

        public StoryOption(int index, string text, string rawText, IReadOnlyList<StoryTag> tags, bool isPremium, int cost)
        {
            Index = index;
            Text = text;
            RawText = rawText;
            Tags = tags;
            IsPremium = isPremium;
            Cost = cost;
        }

        public override string ToString() => $"{Index}: {Text}";
    }

    /// <summary>A global variable that changed during one transition: Δ = (name, before, after).</summary>
    public readonly struct VariableChange
    {
        public readonly string Name;
        public readonly object Before;
        public readonly object After;

        public VariableChange(string name, object before, object after)
        {
            Name = name;
            Before = before;
            After = after;
        }

        public override string ToString() => $"{Name}: {Before} → {After}";
    }

    /// <summary>
    /// The machine's output symbol λ for one transition: what to show, plus the variable
    /// delta that transition caused. Immutable.
    /// </summary>
    public sealed class Beat
    {
        static readonly IReadOnlyList<StoryTag> NoTags = Array.Empty<StoryTag>();
        static readonly IReadOnlyList<StoryOption> NoOptions = Array.Empty<StoryOption>();
        static readonly IReadOnlyList<VariableChange> NoChanges = Array.Empty<VariableChange>();
        static readonly IReadOnlyList<string> NoWarnings = Array.Empty<string>();

        public StoryPhase Phase { get; }
        public Voice Voice { get; }
        /// <summary>Who speaks, or null for narration/thoughts/system lines.</summary>
        public string Speaker { get; }
        /// <summary>The line without the speaker prefix and trailing newline.</summary>
        public string Text { get; }
        public IReadOnlyList<StoryTag> Tags { get; }
        public IReadOnlyList<StoryOption> Options { get; }
        public IReadOnlyList<VariableChange> Changes { get; }
        public IReadOnlyList<string> Warnings { get; }
        public string Error { get; }

        internal Beat(StoryPhase phase, Voice voice = Voice.Narration, string speaker = null, string text = "",
            IReadOnlyList<StoryTag> tags = null, IReadOnlyList<StoryOption> options = null,
            IReadOnlyList<VariableChange> changes = null, IReadOnlyList<string> warnings = null, string error = null)
        {
            Phase = phase;
            Voice = voice;
            Speaker = speaker;
            Text = text ?? "";
            Tags = tags ?? NoTags;
            Options = options ?? NoOptions;
            Changes = changes ?? NoChanges;
            Warnings = warnings ?? NoWarnings;
            Error = error;
        }

        internal static readonly Beat Initial = new Beat(StoryPhase.Ready);

        internal Beat WithDelta(IReadOnlyList<VariableChange> changes, IReadOnlyList<string> warnings) =>
            new Beat(Phase, Voice, Speaker, Text, Tags, Options, changes, warnings, Error);

        /// <summary>True if any tag has this key.</summary>
        public bool HasTag(string key)
        {
            foreach (var t in Tags)
                if (string.Equals(t.Key, key, StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>Value of the first tag with this key, or null.</summary>
        public string TagValue(string key)
        {
            foreach (var t in Tags)
                if (string.Equals(t.Key, key, StringComparison.Ordinal)) return t.Value;
            return null;
        }

        public override string ToString()
        {
            switch (Phase)
            {
                case StoryPhase.Line: return Speaker != null ? $"[{Voice}] {Speaker}: {Text}" : $"[{Voice}] {Text}";
                case StoryPhase.Directive: return "[Directive] #" + string.Join(" #", Tags);
                case StoryPhase.Choice: return "[Choice] " + string.Join(" | ", Options);
                case StoryPhase.Fault: return "[Fault] " + Error;
                default: return "[" + Phase + "]";
            }
        }
    }
}

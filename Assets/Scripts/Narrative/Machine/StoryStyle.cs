using System;
using System.Collections.Generic;
using System.Globalization;

namespace LegacyThroughTime.Narrative
{
    /// <summary>
    /// The house style: how raw ink lines and choices map onto presentation. Pure data plus
    /// two pure functions, so writers' conventions can change without touching the machine.
    ///
    /// Default conventions (see content/stories/*/episode-*.ink):
    /// <list type="bullet">
    /// <item>Dialogue is written <c>Имя: реплика</c>.</item>
    /// <item><c># narr</c>, <c># thought</c>, <c># system</c>, <c># flashback</c>, <c># sfx</c> set the voice.</item>
    /// <item>A paid option starts with 💎 and ends with its price: <c>💎 Взять нож (5 💎)</c>.</item>
    /// </list>
    /// </summary>
    public sealed class StoryStyle
    {
        /// <summary>Tag key → voice. The first matching tag of a line wins.</summary>
        public Dictionary<string, Voice> VoiceTags { get; } = new Dictionary<string, Voice>(StringComparer.Ordinal)
        {
            ["narr"] = Voice.Narration,
            ["thought"] = Voice.Thought,
            ["system"] = Voice.System,
            ["flashback"] = Voice.Flashback,
            ["sfx"] = Voice.Sound,
        };

        /// <summary>Voices in which a <c>Name: text</c> prefix is read as a speaker.</summary>
        public HashSet<Voice> SpeakerVoices { get; } = new HashSet<Voice> { Voice.Dialogue, Voice.Flashback };

        /// <summary>Longest prefix accepted as a speaker name.</summary>
        public int MaxSpeakerLength { get; set; } = 40;

        /// <summary>Marks a premium option; empty disables premium parsing.</summary>
        public string PremiumMark { get; set; } = "💎";

        public static StoryStyle Default => new StoryStyle();

        /// <summary>Splits a line into (voice, speaker, text).</summary>
        public void ParseLine(string line, IReadOnlyList<StoryTag> tags, out Voice voice, out string speaker, out string text)
        {
            text = line.Trim();
            speaker = null;

            bool tagged = false;
            voice = Voice.Narration;
            foreach (var tag in tags)
            {
                if (tag.Value != null || !VoiceTags.TryGetValue(tag.Key, out var v)) continue;
                voice = v;
                tagged = true;
                break;
            }

            // Untagged lines are dialogue when they carry a speaker, narration otherwise.
            var candidate = tagged ? voice : Voice.Dialogue;
            if (SpeakerVoices.Contains(candidate) && TrySplitSpeaker(text, out var name, out var rest))
            {
                speaker = name;
                text = rest;
                voice = candidate;
            }
        }

        bool TrySplitSpeaker(string line, out string name, out string rest)
        {
            name = rest = null;
            int colon = line.IndexOf(':');
            if (colon <= 0 || colon > MaxSpeakerLength) return false;
            if (colon + 1 < line.Length && line[colon + 1] != ' ') return false; // "12:30" is not a speaker
            for (int i = 0; i < colon; i++)
            {
                char c = line[i];
                if (c == '.' || c == '!' || c == '?' || c == '«' || c == '"' || c == '(') return false;
            }
            name = line.Substring(0, colon).Trim();
            rest = line.Substring(colon + 1).Trim();
            return name.Length > 0;
        }

        /// <summary>Strips the premium mark and trailing price from an option.</summary>
        public void ParseOption(string raw, out string text, out bool isPremium, out int cost)
        {
            text = raw.Trim();
            isPremium = false;
            cost = 0;
            if (string.IsNullOrEmpty(PremiumMark) || !text.StartsWith(PremiumMark, StringComparison.Ordinal)) return;

            isPremium = true;
            text = text.Substring(PremiumMark.Length).Trim();

            // Trailing "(N 💎)".
            if (!text.EndsWith(")", StringComparison.Ordinal)) return;
            int open = text.LastIndexOf('(');
            if (open < 0) return;
            var inside = text.Substring(open + 1, text.Length - open - 2).Replace(PremiumMark, "").Trim();
            if (!int.TryParse(inside, NumberStyles.None, CultureInfo.InvariantCulture, out cost)) return;
            text = text.Substring(0, open).Trim();
        }
    }
}

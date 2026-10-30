using System;
using LegacyThroughTime.Narrative;

namespace LegacyThroughTime.Prototype
{
    /// A beat of the story machine -> a screen. The conventions are those of the ink file
    /// (header of content/stories/golden-cage/episode-01.ink): `Имя: текст` is speech (the heroine on the left,
    /// everybody else on the right), `# thought` is a cloud, `# narr` is the narration plate, `# system` is a hint
    /// (or a stat banner for "ПАМЯТЬ СТЕПИ +1"), `# flashback` keeps its speakers, `# sfx` is a sound caption.
    static class BeatMapper
    {
        /// The hero of the story being read: the label of the thoughts.
        public static string Heroine => Cast.HeroLabel;
        const string PlaceMark = "МЕСТО:";

        /// Is the speaker on the hero's side (the plate on the left)? Also "Маленькая Сююмбике".
        public static bool IsHeroine(string speaker) => Cast.IsHeroSpeaker(speaker);

        /// The banner text for a stat change line, or null for any other beat.
        public static string StatBanner(Beat beat) =>
            beat.Voice == Voice.System ? TextRules.StatToast(beat.Text) : null;

        /// The text of a system line that is a hint (not a stat banner), or null for any other beat. Hints do not stop
        /// the reader: they hang at the top of the next screen.
        public static string Hint(Beat beat) =>
            beat.Voice == Voice.System && StatBanner(beat) == null ? TextRules.SentenceCase(beat.Text) : null;

        /// The explanation of a word marked with an asterisk (`# note: Чура — слуга`), the word in italics; or null.
        public static string Note(Beat beat)
        {
            var note = beat.TagValue("note");
            return string.IsNullOrEmpty(note) ? null : TextRules.TermFirst(note.Trim());
        }

        /// The plate of a line; null when the line is a stat banner (those do not stop the reader).
        public static PlateSpec Plate(Beat line)
        {
            if (StatBanner(line) != null) return null;
            var text = TextRules.Emphasis(line.Text);          // the words explained by a note: italics, not asterisks
            switch (line.Voice)
            {
                case Voice.Dialogue:
                case Voice.Flashback when line.Speaker != null:
                    return new PlateSpec
                    {
                        Style = IsHeroine(line.Speaker) ? PlateStyle.SpeechLeft : PlateStyle.SpeechRight,
                        Label = line.Speaker, Text = text,
                    };
                case Voice.Thought: return new PlateSpec { Style = PlateStyle.Thought, Label = Heroine, Text = text };
                case Voice.System: return new PlateSpec { Style = PlateStyle.Hint, Label = "Подсказка", Text = TextRules.SentenceCase(text) };
                case Voice.Flashback: return new PlateSpec { Style = PlateStyle.Narr, Label = "Воспоминание", Text = text };
                case Voice.Sound: return new PlateSpec { Style = PlateStyle.Narr, Label = "Звук", Text = text };
                default:
                    return text.StartsWith(PlaceMark, StringComparison.Ordinal)
                        ? new PlateSpec { Style = PlateStyle.Fact, Label = "Место", Text = text.Substring(PlaceMark.Length).Trim() }
                        : new PlateSpec { Style = PlateStyle.Narr, Label = "…", Text = text };
            }
        }

        /// A choice. The last plate stays on the screen and the options appear next to it (`lastPlate` may be null).
        public static ChoiceSpec Choice(Beat choice, int coins, out bool anyPaid)
        {
            var items = new Option[choice.Options.Count];
            anyPaid = false;
            for (var i = 0; i < items.Length; i++)
            {
                var o = choice.Options[i];
                items[i] = new Option(o.Text, o.Cost, o.Index);
                anyPaid |= o.Cost > 0;
            }
            return new ChoiceSpec { Items = items, Free = true, Coins = anyPaid ? coins : -1 };
        }

        public static Page LinePage(PlateSpec plate) => new Page { Plate = plate };

        public static Page ChoicePage(PlateSpec lastPlate, Beat choice, int coins)
        {
            var spec = Choice(choice, coins, out _);
            if (lastPlate == null)
            {
                spec.PanelLabel = "Что ты сделаешь?";
                return new Page { Choice = spec };
            }
            spec.Mirror = lastPlate.Style == PlateStyle.SpeechRight;
            return new Page
            {
                Plate = new PlateSpec { Style = lastPlate.Style, Label = lastPlate.Label, Text = lastPlate.Text, Instant = true },
                Choice = spec,
            };
        }
    }
}

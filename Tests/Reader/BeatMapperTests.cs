using System;
using System.Collections.Generic;
using System.Linq;
using LegacyThroughTime.Narrative;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    public class BeatMapperTests
    {
        const string Ink = @"
VAR gems = 20
-> start
=== start ===
МЕСТО: Ногайская Орда. Утро. 1533 год. # narr
Рассвет. # narr
Сафия: Ханбике.
Сююмбике: А если я не буду?
Он пахнет сухой травой. # thought
Маленькая Сююмбике: А если порву? # flashback
Женский голос: Не тяни нитку. # flashback
Смех. # flashback
Тихий вокальный мотив. # sfx
ОТКРЫТАЯ ИНФОРМАЦИЯ: «ДИВАН». # system
ПАМЯТЬ СТЕПИ +1 # system
* [Надеть]
    -> end
* {gems >= 8} [💎 Спрятать (8 💎)]
    -> end
=== end ===
Конец. # narr
-> END
";

        static Beat[] Beats()
        {
            // the story compiled by inklecate is needed to run the machine; the episode stands in for it
            var json = Project.EpisodeJson();
            if (json == null) Assert.Ignore("episode-01.json is not compiled (task ink)");
            var machine = StoryMachine.FromJson(json, seed: 1);
            var beats = new List<Beat>();
            machine.Advance();
            while (!machine.IsFinished && beats.Count < 400)
            {
                beats.Add(machine.Current);
                if (machine.Phase == StoryPhase.Choice) machine.Choose(0); else machine.Advance();
            }
            return beats.ToArray();
        }

        static Beat Find(IEnumerable<Beat> beats, Func<Beat, bool> where) => beats.First(where);

        [Test]
        public void TheHeroineIsOnTheLeftAndEverybodyElseOnTheRight()
        {
            Assert.That(BeatMapper.IsHeroine("Сююмбике"), Is.True);
            Assert.That(BeatMapper.IsHeroine("Маленькая Сююмбике"), Is.True);
            Assert.That(BeatMapper.IsHeroine("Сафия"), Is.False);
            Assert.That(BeatMapper.IsHeroine(null), Is.False);
        }

        [Test]
        public void EveryVoiceOfTheStoryBecomesTheRightPlate()
        {
            var beats = Beats();
            var lines = beats.Where(b => b.Phase == StoryPhase.Line).ToList();

            var place = BeatMapper.Plate(Find(lines, b => b.Text.StartsWith("МЕСТО:")));
            Assert.That((place.Style, place.Label), Is.EqualTo((PlateStyle.Fact, "Место")));
            Assert.That(place.Text, Does.StartWith("Ногайская Орда"));

            var heroine = BeatMapper.Plate(Find(lines, b => b.Speaker == "Сююмбике"));
            Assert.That(heroine.Style, Is.EqualTo(PlateStyle.SpeechLeft));

            var other = BeatMapper.Plate(Find(lines, b => b.Speaker == "Сафия"));
            Assert.That((other.Style, other.Label), Is.EqualTo((PlateStyle.SpeechRight, "Сафия")));

            var thought = BeatMapper.Plate(Find(lines, b => b.Voice == Voice.Thought));
            Assert.That((thought.Style, thought.Label), Is.EqualTo((PlateStyle.Thought, "Сююмбике")));

            var child = BeatMapper.Plate(Find(lines, b => b.Speaker == "Маленькая Сююмбике"));
            Assert.That(child.Style, Is.EqualTo(PlateStyle.SpeechLeft), "a flashback speech keeps its speaker");

            var memory = BeatMapper.Plate(Find(lines, b => b.Voice == Voice.Flashback && b.Speaker == null));
            Assert.That(memory.Label, Is.EqualTo("Воспоминание"));

            var sound = BeatMapper.Plate(Find(lines, b => b.Voice == Voice.Sound));
            Assert.That(sound.Label, Is.EqualTo("Звук"));

            var narration = BeatMapper.Plate(Find(lines, b => b.Voice == Voice.Narration && !b.Text.StartsWith("МЕСТО:")));
            Assert.That((narration.Style, narration.Label), Is.EqualTo((PlateStyle.Narr, "…")));
        }

        [Test]
        public void SystemLinesAreHintsOrBanners()
        {
            var lines = Beats().Where(b => b.Phase == StoryPhase.Line && b.Voice == Voice.System).ToList();
            var banner = lines.Where(b => BeatMapper.StatBanner(b) != null).ToList();
            var hints = lines.Except(banner).ToList();
            Assert.That(banner, Is.Not.Empty);
            Assert.That(hints, Is.Not.Empty);
            foreach (var b in banner) Assert.That(BeatMapper.Plate(b), Is.Null, "a banner does not stop the reader");
            foreach (var b in hints)
            {
                var plate = BeatMapper.Plate(b);
                Assert.That((plate.Style, plate.Label), Is.EqualTo((PlateStyle.Hint, "Подсказка")));
                Assert.That(plate.Text, Is.Not.EqualTo(plate.Text.ToUpperInvariant()).Or.Length.LessThan(4), "shouting is turned into a sentence");
            }
        }

        [Test]
        public void ChoicesKeepTheLastPlateAndReportTheirIndex()
        {
            var beats = Beats();
            var choice = Find(beats, b => b.Phase == StoryPhase.Choice);
            var line = new PlateSpec { Style = PlateStyle.SpeechRight, Label = "Сафия", Text = "Ханбике." };

            var page = BeatMapper.ChoicePage(line, choice, coins: 12);
            Assert.That(page.Plate.Instant, Is.True, "the plate was read already");
            Assert.That(page.Plate.Text, Is.EqualTo("Ханбике."));
            Assert.That(page.Choice.Mirror, Is.True, "a plate with its label on the right mirrors the options");
            Assert.That(page.Choice.Free, Is.True, "the story pays for its own options");
            Assert.That(page.Choice.Items.Select(o => o.Index), Is.EqualTo(Enumerable.Range(0, choice.Options.Count)));
        }

        [Test]
        public void TheBalanceIsShownOnlyWhenSomethingIsPaid()
        {
            var beats = Beats();
            var line = new PlateSpec { Style = PlateStyle.Narr, Label = "…", Text = "…" };
            foreach (var choice in beats.Where(b => b.Phase == StoryPhase.Choice))
            {
                var page = BeatMapper.ChoicePage(line, choice, coins: 7);
                var paid = choice.Options.Any(o => o.Cost > 0);
                Assert.That(page.Choice.Coins, Is.EqualTo(paid ? 7 : -1));
            }
        }

        [Test]
        public void AChoiceWithoutAPlateGetsItsOwnPanel()
        {
            var choice = Find(Beats(), b => b.Phase == StoryPhase.Choice);
            var page = BeatMapper.ChoicePage(null, choice, 0);
            Assert.That(page.Plate, Is.Null);
            Assert.That(page.Choice.PanelLabel, Is.Not.Empty);
        }

        [Test]
        public void TheWholeStoryMapsToScreensWithTextAndNoMarkupOnAnyPath()
        {
            var json = Project.EpisodeJson();
            if (json == null) Assert.Ignore("episode-01.json is not compiled (task ink)");
            var screens = new HashSet<string>();
            for (var seed = 0; seed < 40; seed++)
            {
                var rng = new Random(seed);
                var machine = StoryMachine.FromJson(json, seed: seed);
                machine.Advance();
                PlateSpec last = null;
                for (var step = 0; step < 3000 && !machine.IsFinished; step++)
                {
                    var beat = machine.Current;
                    if (beat.Phase == StoryPhase.Line)
                    {
                        var plate = BeatMapper.Plate(beat);
                        if (plate != null)
                        {
                            last = plate;
                            Assert.That(plate.Text, Is.Not.Empty);
                            Assert.That(plate.Text, Does.Not.Contain("<").And.Not.Contain(">"), "rich-text markup would be rendered by the UI");
                            Assert.That(PlateSkins.For(plate.Style), Is.Not.Null);
                            screens.Add(plate.Style + "|" + plate.Label + "|" + plate.Text);
                        }
                        machine.Advance();
                    }
                    else if (beat.Phase == StoryPhase.Choice)
                    {
                        var page = BeatMapper.ChoicePage(last, beat, machine.GetInt("gems"));
                        Assert.That(page.Choice.Items.All(o => !string.IsNullOrWhiteSpace(o.Text)));
                        machine.Choose(rng.Next(beat.Options.Count));
                    }
                    else machine.Advance();
                }
                Assert.That(machine.Phase, Is.EqualTo(StoryPhase.End), $"seed {seed} did not reach the end");
            }
            Assert.That(screens.Count, Is.GreaterThan(300));
        }
    }

    public class BackdropsTests
    {
        [Test]
        public void SameIdSameTone() => Assert.That(Backdrops.For("steppe_dawn"), Is.EqualTo(Backdrops.For("steppe_dawn")));

        [Test]
        public void MoodWordsPickTheirTone()
        {
            Assert.That(Backdrops.For("village_house_night").V, Is.LessThan(.3f));
            Assert.That(Backdrops.For("village_house_dawn").V, Is.GreaterThan(.4f));
            Assert.That(Backdrops.For("dream_tower").H, Is.EqualTo(.72f));
            Assert.That(Backdrops.For("black"), Is.EqualTo(Backdrops.Black));
            Assert.That(Backdrops.For(null), Is.EqualTo(Backdrops.Default));
        }

        [Test]
        public void EveryOtherLocationStaysInTheGreenToTealRange()
        {
            foreach (var id in new[] { "kazan_panorama", "palace_corridor", "khan_chamber", "tent_entrance", "yusuf_hall" })
            {
                var tone = Backdrops.For(id);
                Assert.That(tone.H, Is.InRange(.33f, .47f), id);
            }
        }
    }
}

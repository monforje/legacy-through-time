using System.Collections.Generic;
using System.IO;
using System.Linq;
using LegacyThroughTime.Narrative;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    /// The prologue of «По хребтам Сихотэ-Алиня»: it plays to the end whatever the reader picks, and every picture
    /// it asks for is in Resources/Art (a placeholder or the real one).
    public class SikhoteAlinTests
    {
        static string Json()
        {
            foreach (var candidate in new[]
            {
                Project.Path_("Assets", "StreamingAssets", "stories", "sikhote-alin", "episode-01.json"),
                Project.Path_("buckets", "stories", "sikhote-alin", "episode-01.json"),
            })
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
            return null;
        }

        /// Every beat of a walk that takes the `pick`-th option (the last one when there are fewer) at every choice.
        static List<Beat> Walk(string json, int pick)
        {
            var machine = StoryMachine.FromJson(json, seed: 1);
            machine.Advance();
            var beats = new List<Beat>();
            for (var step = 0; step < 2000 && !machine.IsFinished; step++)
            {
                beats.Add(machine.Current);
                if (machine.Phase == StoryPhase.Choice) machine.Choose(System.Math.Min(pick, machine.Current.Options.Count - 1));
                else machine.Advance();
            }
            Assert.That(machine.Phase, Is.EqualTo(StoryPhase.End), $"walk {pick} did not reach the end");
            return beats;
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void EveryWalkReachesTheEndAndEveryPictureIsThere(int pick)
        {
            var json = Json();
            if (json == null) Assert.Ignore("sikhote-alin/episode-01.json is not compiled (task ink)");
            Cast.Use("sikhote-alin");
            try
            {
                foreach (var beat in Walk(json, pick))
                {
                    var bg = beat.TagValue("bg");
                    if (bg != null && bg != "black")
                        Assert.That(File.Exists(Project.Path_("Assets", "Resources", "Art", "Backgrounds", bg + ".png")), $"background {bg}");

                    var speaker = beat.Voice == Voice.Thought ? BeatMapper.Heroine : beat.Speaker;
                    var id = beat.TagValue("char");
                    if (id == "none") continue;
                    id ??= Cast.Id(speaker);
                    if (beat.Voice is Voice.Dialogue or Voice.Flashback && id == null)
                        Assert.Fail($"the speaker «{beat.Speaker}» has no character");
                    if (id == null) continue;
                    Assert.That(File.Exists(Project.Path_("Assets", "Resources", "Art", "Characters", Cast.Plain(id, false) + ".png")), $"character {id}");
                    var face = Cast.Sprite(id, beat.TagValue("emo"), false);
                    Assert.That(File.Exists(Project.Path_("Assets", "Resources", "Art", "Characters", face + ".png")), $"face {face} (the sprite sheet lists it)");
                }
            }
            finally { Cast.Use("golden-cage"); }
        }

        [Test]
        public void TheHeroOfThePrologueIsOnTheLeftAndTheParentsOnTheRight()
        {
            Cast.Use("sikhote-alin");
            try
            {
                Assert.That(BeatMapper.IsHeroine("Володя"), Is.True);
                Assert.That(BeatMapper.IsHeroine("Арсеньев"), Is.True);
                Assert.That(BeatMapper.IsHeroine("Мать"), Is.False);
                Assert.That(BeatMapper.IsHeroine("Отец"), Is.False);
                Assert.That(BeatMapper.Heroine, Is.EqualTo("Арсеньев"));
                Assert.That(Cast.IsHero("volodya") && Cast.IsHero("arsenyev") && !Cast.IsHero("mother"));
                Assert.That(Cast.Sprite("arsenyev", "smile", true), Is.EqualTo("arsenyev_smile"));
                Assert.That(Cast.Height("volodya"), Is.LessThan(1f));
            }
            finally { Cast.Use("golden-cage"); }
            Assert.That(BeatMapper.IsHeroine("Сююмбике"), Is.True);
            Assert.That(Cast.Sprite("suyumbike", "sad", true), Is.EqualTo("suyumbike_travel_sad"));
        }

        [Test]
        public void TheStoryIsInTheCatalogWithItsHero()
        {
            var story = StoryCatalog.Find("sikhote-alin");
            Assert.That(story.Hero, Is.EqualTo("Арсеньев"));
            Assert.That(story.First.Night, Is.EqualTo("ep01"));
            Assert.That(story.Chapters.Select(c => c.SaveId), Is.Unique);
        }
    }
}

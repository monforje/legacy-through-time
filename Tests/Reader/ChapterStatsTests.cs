using System.Linq;
using LegacyThroughTime.Narrative;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    /// The stats a chapter ends with go on into the next chapter of the story, and survive the cloud.
    public class ChapterStatsTests
    {
        static StoryMachine Episode()
        {
            var json = Project.EpisodeJson();
            if (json == null) Assert.Ignore("episode-01.json is not compiled (task ink)");
            return StoryMachine.FromJson(json, seed: 1);
        }

        [Test]
        public void TheVariablesOfAChapterEndGoIntoTheNextOne()
        {
            var ended = Episode();
            ended.SetVariable("steppe_memory", 3);
            ended.SetVariable("has_knife", true);
            ended.SetVariable("gems", 2);
            var stats = ChapterStats.Of(ended, "golden-cage", 100);
            Assert.That(stats.vars.Select(v => v.name), Does.Not.Contain("gems"), "the coins are the purse's");

            var next = Episode();
            next.SetVariable("gems", 77);
            stats.ApplyTo(next);
            Assert.That(next.GetInt("steppe_memory"), Is.EqualTo(3));
            Assert.That(next.GetBool("has_knife"), Is.True);
            Assert.That(next.GetInt("gems"), Is.EqualTo(77));
        }

        [Test]
        public void UnknownOrMistypedVariablesAreSkipped()
        {
            var stats = new ChapterStats
            {
                chapter = "golden-cage",
                vars =
                {
                    new StatValue { name = "no_such_var", kind = "int", value = "5" },
                    new StatValue { name = "steppe_memory", kind = "bool", value = "true" },
                    new StatValue { name = "wisdom", kind = "int", value = "2" },
                },
            };
            var machine = Episode();
            stats.ApplyTo(machine);
            Assert.That(machine.GetInt("steppe_memory"), Is.EqualTo(0));
            Assert.That(machine.GetInt("wisdom"), Is.EqualTo(2));
        }

        [Test]
        public void AChapterStartsFromTheEndOfTheOneBefore()
        {
            var story = StoryCatalog.Find("golden-cage");
            ChapterStats Read(string id) => new ChapterStats { chapter = id };
            Assert.That(ChapterStats.Before(story, story.Chapters[0], Read), Is.Null);
            Assert.That(ChapterStats.Before(story, story.Chapters[1], Read).chapter, Is.EqualTo(story.Chapters[0].SaveId));
            Assert.That(ChapterStats.Before(story, story.Chapters[2], _ => null), Is.Null);
        }

        [Test]
        public void TheLaterFinishOfAChapterKeepsItsStats()
        {
            ChapterStats At(long at, int memory) => new ChapterStats
            {
                chapter = "golden-cage", at = at,
                vars = { new StatValue { name = "steppe_memory", kind = "int", value = memory.ToString() } },
            };
            var here = new Progress { stats = { At(100, 1) } };
            var cloud = new Progress { stats = { At(200, 4) } };
            Assert.That(Progress.Merge(here, cloud).stats.Single().vars.Single().value, Is.EqualTo("4"));
            Assert.That(Progress.Merge(cloud, here).stats.Single().vars.Single().value, Is.EqualTo("4"));
            Assert.That(Progress.Merge(here, null).stats.Single().at, Is.EqualTo(100));
        }
    }
}

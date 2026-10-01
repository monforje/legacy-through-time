using System.IO;
using System.Linq;
using LegacyThroughTime.Narrative;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    public class StoryCatalogTests
    {
        [Test]
        public void IdsAreUniqueAndEveryCardHasItsTexts()
        {
            var ids = StoryCatalog.All.Select(s => s.Id).ToList();
            Assert.That(ids, Is.Unique);
            foreach (var s in StoryCatalog.All)
                Assert.That(new[] { s.Id, s.Title, s.Episode, s.Blurb }, Has.None.Null.And.None.Empty, s.Id ?? "?");
        }

        [Test]
        public void APlayableStoryHasItsCoverAndItsCompiledStory()
        {
            foreach (var s in StoryCatalog.All.Where(s => !s.Soon))
            {
                Assert.That(File.Exists(Project.Path_("Assets", "Resources", "Art", "Backgrounds", s.Cover + ".png")), $"cover of {s.Id}: {s.Cover}");
                Assert.That(File.Exists(Project.Path_("content", s.Path.Replace(".json", ".ink"))), $"story of {s.Id}: {s.Path}");
            }
        }

        [Test]
        public void FindReturnsTheEntryOrNull()
        {
            Assert.That(StoryCatalog.Find("golden-cage").Title, Is.EqualTo("Золотая клетка"));
            Assert.That(StoryCatalog.Find("nope"), Is.Null);
        }

        /// The player saves the machine at every line on screen and resumes from it: a machine loaded from such a save
        /// must go on exactly like the one that was saved (the same lines, options and stats), whatever the line.
        [Test]
        public void AStoryResumedFromAnySavedLineGoesOnTheSame()
        {
            var json = Project.EpisodeJson();
            if (json == null) Assert.Ignore("episode-01.json is not compiled (task ink)");

            var machine = StoryMachine.FromJson(json, seed: 1);
            machine.Advance();
            var resumed = 0;
            for (var step = 0; step < 3000 && !machine.IsFinished; step++)
            {
                var beat = machine.Current;
                if (beat.Phase == StoryPhase.Line && BeatMapper.Plate(beat) != null && step % 5 == 0)
                {
                    var copy = StoryMachine.FromJson(json, seed: 1);
                    copy.Load(machine.Save());
                    Assert.That(copy.Current.Text, Is.EqualTo(beat.Text), $"step {step}: the saved line");
                    LockStep(Clone(json, machine), copy, step);
                    resumed++;
                }
                Step(machine, step);
            }
            Assert.That(machine.Phase, Is.EqualTo(StoryPhase.End));
            Assert.That(resumed, Is.GreaterThan(20));
        }

        static StoryMachine Clone(string json, StoryMachine m)
        {
            var c = StoryMachine.FromJson(json, seed: 1);
            c.Load(m.Save());
            return c;
        }

        /// Both machines take the same inputs for a while and must show the same.
        static void LockStep(StoryMachine a, StoryMachine b, int from)
        {
            for (var i = 0; i < 40 && !a.IsFinished; i++)
            {
                Assert.That(b.Phase, Is.EqualTo(a.Phase), $"from step {from}, +{i}: phase");
                Assert.That(b.Current.Text, Is.EqualTo(a.Current.Text), $"from step {from}, +{i}: text");
                Assert.That(b.Current.Options.Select(o => o.Text), Is.EqualTo(a.Current.Options.Select(o => o.Text)), $"from step {from}, +{i}: options");
                Assert.That(b.GetInt("gems"), Is.EqualTo(a.GetInt("gems")), $"from step {from}, +{i}: gems");
                Step(a, i); Step(b, i);
            }
        }

        static void Step(StoryMachine m, int i)
        {
            if (m.Phase == StoryPhase.Choice) m.Choose(i % m.Current.Options.Count);
            else m.Advance();
        }
    }
}

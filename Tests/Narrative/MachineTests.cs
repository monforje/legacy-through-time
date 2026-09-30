using System;
using System.Collections.Generic;
using System.Linq;
using LegacyThroughTime.Narrative.Ink;
using NUnit.Framework;

namespace LegacyThroughTime.Narrative.Tests
{
    /// <summary>Algebraic properties of the story machine M = (Q, Σ, Λ, V, δ, λ).</summary>
    [TestFixture]
    public class MachineTests
    {
        public static IEnumerable<TestCaseData> Stories() =>
            Corpus.StoryFiles().Select(f => new TestCaseData(f).SetName("Machine_" + Corpus.Name(f)));

        /// <summary>A random player: Advance when enabled, otherwise a random option.</summary>
        static List<string> Play(StoryMachine m, Random rng, int maxSteps, List<int> inputs = null)
        {
            var log = new List<string>();
            for (int i = 0; i < maxSteps && !m.IsFinished; i++)
            {
                if (m.CanAdvance)
                {
                    Assert.IsTrue(m.Advance());
                    inputs?.Add(-1);
                }
                else
                {
                    int pick = rng.Next(m.Current.Options.Count);
                    Assert.IsTrue(m.Choose(pick));
                    inputs?.Add(pick);
                }
                log.Add(m.Current.ToString());
            }
            return log;
        }

        [TestCaseSource(nameof(Stories))]
        public void Invariants_Hold_On_Every_Beat(string file)
        {
            var story = InkStory.FromJson(Corpus.Json(file));
            for (int walk = 0; walk < 40; walk++)
            {
                var m = new StoryMachine(story, seed: walk);
                var rng = new Random(walk);
                for (int i = 0; i < 400 && !m.IsFinished; i++)
                {
                    if (m.CanAdvance) m.Advance(); else m.Choose(rng.Next(m.Current.Options.Count));
                    var b = m.Current;
                    Assert.AreNotEqual(StoryPhase.Fault, b.Phase, b.Error);
                    Assert.AreEqual(b.Phase == StoryPhase.Choice, b.Options.Count > 0, "Choice ⇔ options exist");
                    if (b.Phase == StoryPhase.Line) Assert.IsNotEmpty(b.Text.Trim(), "a Line always has text");
                    if (b.Phase == StoryPhase.Directive) Assert.IsNotEmpty(b.Tags, "a Directive always has tags");
                    Assert.IsFalse(b.Text.EndsWith("\n"), "line text is trimmed");
                }
            }
        }

        [TestCaseSource(nameof(Stories))]
        public void Transition_Function_Is_Total_Via_Guards(string file)
        {
            var story = InkStory.FromJson(Corpus.Json(file));
            var m = new StoryMachine(story, seed: 1);
            var rng = new Random(1);
            for (int i = 0; i < 200 && !m.IsFinished; i++)
            {
                var before = m.Current;
                var save = m.Save();
                // Disabled inputs are rejected and change nothing.
                if (m.CanAdvance)
                {
                    Assert.IsFalse(m.Choose(0));
                    Assert.IsFalse(m.Choose(-1));
                }
                else
                {
                    Assert.IsFalse(m.Advance());
                    Assert.IsFalse(m.Choose(m.Current.Options.Count));
                    Assert.IsFalse(m.Choose(-1));
                }
                Assert.AreSame(before, m.Current);
                Assert.AreEqual(save, m.Save());

                if (m.CanAdvance) m.Advance(); else m.Choose(rng.Next(m.Current.Options.Count));
            }
            if (m.IsFinished)
            {
                Assert.IsFalse(m.Advance(), "End is absorbing");
                Assert.IsFalse(m.Choose(0), "End is absorbing");
            }
        }

        [TestCaseSource(nameof(Stories))]
        public void Deterministic_Given_Seed_And_Inputs(string file)
        {
            var story = InkStory.FromJson(Corpus.Json(file));
            for (int seed = 0; seed < 10; seed++)
            {
                var a = Play(new StoryMachine(story, seed: seed), new Random(seed), 300);
                var b = Play(new StoryMachine(story, seed: seed), new Random(seed), 300);
                CollectionAssert.AreEqual(a, b);
            }
        }

        [TestCaseSource(nameof(Stories))]
        public void Load_After_Save_Is_Observationally_Identity(string file)
        {
            var story = InkStory.FromJson(Corpus.Json(file));
            for (int seed = 0; seed < 20; seed++)
            {
                var rng = new Random(seed);
                var m = new StoryMachine(story, seed: seed);
                Play(m, rng, rng.Next(1, 60));
                if (m.IsFinished) continue;

                var restored = new StoryMachine(story, seed: 999);
                restored.Load(m.Save());
                Assert.AreEqual(m.Current.ToString(), restored.Current.ToString());
                Assert.AreEqual(m.Save(), restored.Save());

                var tail = Play(m, new Random(seed + 1), 200);
                var tailRestored = Play(restored, new Random(seed + 1), 200);
                CollectionAssert.AreEqual(tail, tailRestored);
            }
        }

        [TestCaseSource(nameof(Stories))]
        public void Deltas_Compose_To_Final_Variables(string file)
        {
            // v_n = v_0 ⊕ Δ_1 ⊕ … ⊕ Δ_n : replaying every beat's delta reconstructs the state.
            var story = InkStory.FromJson(Corpus.Json(file));
            for (int seed = 0; seed < 10; seed++)
            {
                var m = new StoryMachine(story, seed: seed);
                var replayed = m.VariableNames.ToDictionary(n => n, n => m.GetVariable(n));
                var rng = new Random(seed);
                for (int i = 0; i < 300 && !m.IsFinished; i++)
                {
                    if (m.CanAdvance) m.Advance(); else m.Choose(rng.Next(m.Current.Options.Count));
                    foreach (var d in m.Current.Changes)
                    {
                        Assert.AreEqual(Show(replayed[d.Name]), Show(d.Before), $"delta of {d.Name} starts where the last ended");
                        replayed[d.Name] = d.After;
                    }
                }
                foreach (var name in m.VariableNames)
                    Assert.AreEqual(Show(m.GetVariable(name)), Show(replayed[name]), name);
            }
        }

        static string Show(object o) => o is InkList l ? "list:" + l : o?.GetType().Name + ":" + o;
    }
}

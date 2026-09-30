using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LegacyThroughTime.Narrative.Ink;
using NUnit.Framework;
using Ref = Ink.Runtime;

namespace LegacyThroughTime.Narrative.Tests
{
    /// <summary>
    /// Differential testing: our runner and the official ink runtime play the same story with
    /// the same seed and the same randomly chosen choices; every observable — each line's
    /// text and tags, choice lists, every global variable after every line, errors — must be
    /// identical. Save/load round-trips are injected mid-walk on our side, so the walk also
    /// proves Load(Save(s)) ≡ s observationally.
    /// </summary>
    [TestFixture]
    public class DifferentialTests
    {
        const int WalksPerStory = 150;
        const int MaxSteps = 400;

        public static IEnumerable<TestCaseData> Stories() =>
            Corpus.StoryFiles().Select(f => new TestCaseData(f).SetName("Differential_" + Corpus.Name(f)));

        [TestCaseSource(nameof(Stories))]
        public void MatchesReferenceRuntime(string inkFile)
        {
            var json = Corpus.Json(inkFile);
            var story = InkStory.FromJson(json);
            int totalLines = 0;
            for (int walk = 0; walk < WalksPerStory; walk++)
                totalLines += Walk(json, story, walk, saveLoadEvery: walk % 3 == 0 ? 0 : 1 + walk % 7);
            TestContext.Out.WriteLine($"{Corpus.Name(inkFile)}: {WalksPerStory} walks, {totalLines} lines compared");
        }

        static int Walk(string json, InkStory story, int walkSeed, int saveLoadEvery)
        {
            var rng = new Random(walkSeed * 7919 + 17);
            int storySeed = walkSeed % 100;

            var refErrors = new List<string>();
            var reference = new Ref.Story(json) { allowExternalFunctionFallbacks = true };
            reference.onError += (msg, type) => { if (type == global::Ink.ErrorType.Error) refErrors.Add(msg); };
            reference.state.storySeed = storySeed;

            var ourErrors = new List<string>();
            var ours = NewRunner(story, storySeed, ourErrors);

            var trace = new StringBuilder();
            int lines = 0;
            for (int step = 0; step < MaxSteps; step++)
            {
                string where = $"walk {walkSeed}, step {step}";
                Assert.AreEqual(reference.canContinue, ours.CanContinue, $"{where}: canContinue\n{trace}");

                if (reference.canContinue)
                {
                    var expected = reference.Continue();
                    var actual = ours.Continue();
                    trace.Append("> ").Append(expected);
                    Assert.AreEqual(expected, actual, $"{where}: text\n{trace}");
                    Assert.AreEqual(reference.currentTags, ours.CurrentTags.ToList(), $"{where}: tags\n{trace}");
                    Assert.AreEqual(refErrors.Count > 0, ourErrors.Count > 0,
                        $"{where}: errors ref=[{string.Join("; ", refErrors)}] ours=[{string.Join("; ", ourErrors)}]\n{trace}");
                    AssertGlobalsEqual(reference, ours, $"{where}\n{trace}");
                    lines++;
                }
                else
                {
                    var expected = reference.currentChoices.Select(c => c.text + Tags(c.tags)).ToList();
                    var actual = ours.CurrentChoices.Select(c => c.Text + Tags(c.Tags)).ToList();
                    Assert.AreEqual(expected, actual, $"{where}: choices\n{trace}");
                    if (expected.Count == 0) break;
                    int pick = rng.Next(expected.Count);
                    trace.Append("* ").AppendLine(expected[pick]);
                    reference.ChooseChoiceIndex(pick);
                    ours.ChooseChoiceIndex(pick);
                }

                if (saveLoadEvery > 0 && step % saveLoadEvery == 0)
                {
                    var saved = ours.SaveState();
                    ours = NewRunner(story, 0, ourErrors);
                    ours.LoadState(saved);
                    Assert.AreEqual(saved, ours.SaveState(), $"{where}: save is not a fixed point of load∘save");
                }
            }
            return lines;
        }

        static InkRunner NewRunner(InkStory story, int seed, List<string> errors)
        {
            var r = new InkRunner(story, seed) { AllowExternalFunctionFallbacks = true };
            r.OnError = (msg, isWarning) => { if (!isWarning) errors.Add(msg); };
            return r;
        }

        static string Tags(IEnumerable<string> tags) => tags == null || !tags.Any() ? "" : " #" + string.Join(" #", tags);

        static void AssertGlobalsEqual(Ref.Story reference, InkRunner ours, string where)
        {
            foreach (var name in ours.Story.GlobalVariableNames)
            {
                var expected = DescribeRef(reference.variablesState[name]);
                int slot = ours.Story.GlobalSlot(name);
                var actual = DescribeOurs(ours.Store.Globals[slot]);
                Assert.AreEqual(expected, actual, $"{where}: global '{name}'");
            }
        }

        static string DescribeRef(object v) => v switch
        {
            null => "null",
            int i => "i:" + i,
            float f => "f:" + f.ToString(CultureInfo.InvariantCulture),
            bool b => "b:" + b,
            string s => "s:" + s,
            Ref.InkList l => "l:" + string.Join(",", l.OrderBy(kv => kv.Key.fullName, StringComparer.Ordinal).Select(kv => kv.Key.fullName + "=" + kv.Value)),
            Ref.Path p => "d:" + p,
            _ => "?" + v,
        };

        static string DescribeOurs(Value v) => v.Kind switch
        {
            ValueKind.None => "null",
            ValueKind.Int => "i:" + v.I,
            ValueKind.Float => "f:" + v.F.ToString(CultureInfo.InvariantCulture),
            ValueKind.Bool => "b:" + (v.I != 0),
            ValueKind.String => "s:" + v.Str,
            ValueKind.List => "l:" + string.Join(",", v.ListValue.OrderBy(kv => kv.Key.FullName, StringComparer.Ordinal).Select(kv => kv.Key.FullName + "=" + kv.Value)),
            ValueKind.DivertTarget => "d:" + v.Target.Path,
            _ => "?" + v,
        };
    }
}

using System.Linq;
using LegacyThroughTime.Narrative.Ink;
using NUnit.Framework;
using Ref = Ink.Runtime;

namespace LegacyThroughTime.Narrative.Tests
{
    /// <summary>Host-facing API calls agree with the reference runtime.</summary>
    [TestFixture]
    public class ApiDifferentialTests
    {
        static (Ref.Story reference, InkRunner ours) Pair(string corpusName, int seed = 5)
        {
            var file = Corpus.StoryFiles().First(f => Corpus.Name(f) == corpusName);
            var json = Corpus.Json(file);
            var reference = new Ref.Story(json) { allowExternalFunctionFallbacks = true };
            reference.state.storySeed = seed;
            return (reference, new InkRunner(InkStory.FromJson(json), seed));
        }

        [TestCase("add", 2, 3)]
        [TestCase("fact", 7)]
        [TestCase("describe", 10)]
        [TestCase("greet", "Host")]
        [TestCase("noisy")]
        [TestCase("void_fn")]
        public void EvaluateFunction(string name, params object[] args)
        {
            var (reference, ours) = Pair("functions");
            reference.Continue();
            ours.Continue();
            for (int i = 0; i < 3; i++)
            {
                var expected = reference.EvaluateFunction(name, out string expectedText, args);
                var actual = ours.EvaluateFunction(name, out string actualText, args);
                Assert.AreEqual(expected, actual);
                Assert.AreEqual(expectedText, actualText);
            }
            // Evaluating functions must not disturb the story itself.
            Assert.AreEqual(reference.ContinueMaximally(), ours.ContinueMaximally());
            Assert.AreEqual(reference.variablesState["counter"], ours.GetVariable("counter"));
        }

        [TestCase("right")]
        [TestCase("left")]
        [TestCase("hub")]
        public void ChoosePathString(string path)
        {
            var (reference, ours) = Pair("basics");
            reference.ContinueMaximally();
            ours.ContinueMaximally();
            reference.ChoosePathString(path);
            ours.ChoosePathString(path);
            Assert.AreEqual(reference.ContinueMaximally(), ours.ContinueMaximally());
            CollectionAssert.AreEqual(reference.currentChoices.Select(c => c.text), ours.CurrentChoices.Select(c => c.Text));
        }

        [Test]
        public void GlobalTags_And_KnotTags()
        {
            var (reference, ours) = Pair("basics");
            CollectionAssert.AreEqual(reference.globalTags, ours.Story.GlobalTags);
        }

        [Test]
        public void Host_Variable_Write_Is_Seen_By_Story()
        {
            var (reference, ours) = Pair("basics");
            reference.variablesState["gold"] = 50;
            ours.SetVariable("gold", 50);
            Assert.AreEqual(reference.ContinueMaximally(), ours.ContinueMaximally());
        }
    }
}

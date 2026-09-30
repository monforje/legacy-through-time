using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LegacyThroughTime.Narrative.Ink;
using NUnit.Framework;
using Ref = Ink.Runtime;

namespace LegacyThroughTime.Narrative.Tests
{
    /// <summary>Ours vs the reference runtime on the real episode. Run with --filter Category=Benchmark.</summary>
    [TestFixture, Explicit, Category("Benchmark")]
    public class Benchmark
    {
        const int Walks = 300;

        [Test]
        public void Episode()
        {
            var file = Corpus.StoryFiles().First(f => f.Contains("episode-01"));
            var json = Corpus.Json(file);

            Measure("load    ref ", 200, () => new Ref.Story(json));
            Measure("load    ours", 200, () => InkStory.FromJson(json));

            var story = InkStory.FromJson(json);
            var refStory = new Ref.Story(json);
            Measure("play    ref ", Walks, i => PlayRef(refStory, i));
            Measure("play    ours", Walks, i => PlayOurs(story, i));
        }

        static int PlayRef(Ref.Story s, int seed)
        {
            s.ResetState();
            s.state.storySeed = seed;
            var rng = new Random(seed);
            int lines = 0;
            while (true)
            {
                while (s.canContinue) { s.Continue(); _ = s.currentTags; lines++; }
                if (s.currentChoices.Count == 0) return lines;
                s.ChooseChoiceIndex(rng.Next(s.currentChoices.Count));
            }
        }

        static int PlayOurs(InkStory story, int seed)
        {
            var r = new InkRunner(story, seed);
            var rng = new Random(seed);
            int lines = 0;
            while (true)
            {
                while (r.CanContinue) { r.Continue(); _ = r.CurrentTags; lines++; }
                var choices = r.CurrentChoices;
                if (choices.Count == 0) return lines;
                r.ChooseChoiceIndex(rng.Next(choices.Count));
            }
        }

        static void Measure(string name, int n, Action action) => Measure(name, n, _ => { action(); return 0; });

        static void Measure(string name, int n, Func<int, int> action)
        {
            for (int i = 0; i < 20; i++) action(i); // warm-up / JIT
            var times = new double[7];
            long lines = 0, alloc = 0;
            for (int rep = 0; rep < times.Length; rep++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                long alloc0 = GC.GetAllocatedBytesForCurrentThread();
                var sw = Stopwatch.StartNew();
                lines = 0;
                for (int i = 0; i < n; i++) lines += action(i);
                sw.Stop();
                alloc = GC.GetAllocatedBytesForCurrentThread() - alloc0;
                times[rep] = sw.Elapsed.TotalMilliseconds;
            }
            Array.Sort(times);
            double ms = times[times.Length / 2]; // median
            var perLine = lines > 0 ? $"  {ms * 1000 / lines,7:F2} µs/line  {alloc / (double)lines,8:F0} B/line" : "";
            TestContext.Out.WriteLine($"{name}: {ms / n,8:F3} ms/op  {alloc / n / 1024.0,8:F1} KB/op{perLine}");
        }
    }
}

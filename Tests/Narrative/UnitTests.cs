using System;
using System.Collections.Generic;
using LegacyThroughTime.Narrative.Ink;
using LegacyThroughTime.Narrative.Json;
using NUnit.Framework;

namespace LegacyThroughTime.Narrative.Tests
{
    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void InkRandom_Matches_Seeded_System_Random()
        {
            var rng = new Random(123);
            for (int i = 0; i < 2000; i++)
            {
                int seed = i < 10 ? new[] { 0, 1, -1, int.MaxValue, int.MinValue, 42, 99, 100, -100, 7 }[i] : rng.Next(int.MinValue, int.MaxValue);
                var ours = new InkRandom(seed);
                var reference = new Random(seed);
                for (int k = 0; k < 8; k++) Assert.AreEqual(reference.Next(), ours.Next(), $"seed {seed}, draw {k}");
            }
        }

        [Test]
        public void Json_RoundTrips()
        {
            var w = new JsonWriter();
            w.BeginObject()
                .Property("s", "quote \" backslash \\ newline \n tab \t ctrl \u0001 юникод 💎")
                .Property("i", -42)
                .Key("f").Float(3f)
                .Key("g").Float(0.1f)
                .Property("b", true)
                .Key("n").Null()
                .Key("a").BeginArray().Int(1).BeginArray().EndArray().BeginObject().EndObject().EndArray()
                .EndObject();
            var parsed = (Dictionary<string, object>)JsonReader.Parse(w.ToString());
            Assert.AreEqual("quote \" backslash \\ newline \n tab \t ctrl \u0001 юникод 💎", parsed["s"]);
            Assert.AreEqual(-42, parsed["i"]);
            Assert.AreEqual(3f, parsed["f"]);
            Assert.IsInstanceOf<float>(parsed["f"], "floats keep their type through a round trip");
            Assert.AreEqual(0.1f, parsed["g"]);
            Assert.AreEqual(true, parsed["b"]);
            Assert.IsNull(parsed["n"]);
            Assert.AreEqual(3, ((List<object>)parsed["a"]).Count);
        }

        [TestCase("{\"a\":}")]
        [TestCase("[1,2")]
        [TestCase("\"unterminated")]
        [TestCase("{} extra")]
        public void Json_Rejects_Malformed(string json) => Assert.Throws<FormatException>(() => JsonReader.Parse(json));

        [Test]
        public void Loader_Rejects_Non_Ink()
        {
            Assert.Throws<FormatException>(() => InkStory.FromJson("{\"root\":[]}"));
            Assert.Throws<FormatException>(() => InkStory.FromJson("{\"inkVersion\":99,\"root\":[null]}"));
        }

        [Test]
        public void Journal_Rollback_Is_Exact_Inverse()
        {
            var s = new Store(3, 4);
            s.SetGlobal(0, Value.Int(1));
            s.SetGlobal(1, Value.String("a"));
            s.IncrementVisit(2);
            s.SetTurn(3, 5);
            var globals = (Value[])s.Globals.Clone();
            var visits = (int[])s.Visits.Clone();
            var turns = (int[])s.Turns.Clone();

            s.BeginRecording();
            s.SetGlobal(0, Value.Int(2));
            s.SetGlobal(0, Value.Int(3));
            s.SetGlobal(2, Value.Bool(true));
            s.IncrementVisit(2);
            s.IncrementVisit(0);
            s.SetTurn(3, 9);
            s.SetTurn(1, 9);
            s.Rollback();

            for (int i = 0; i < 3; i++) Assert.IsTrue(globals[i].SameAs(s.Globals[i]));
            CollectionAssert.AreEqual(visits, s.Visits);
            CollectionAssert.AreEqual(turns, s.Turns);

            s.BeginRecording();
            s.SetGlobal(0, Value.Int(7));
            s.Commit();
            Assert.AreEqual(7, s.Globals[0].I);
        }
    }
}

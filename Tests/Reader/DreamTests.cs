using System;
using System.Collections.Generic;
using System.Linq;
using LegacyThroughTime.Narrative.Json;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    /// The talk with the heroine after a chapter: the replies of a day, the memory of the dreams before, what goes
    /// to the chat server and how its stream is read.
    public class DreamTests
    {
        static DreamNight Night() => DreamNight.FromJson(
            "{\"id\":\"ep01\",\"title\":\"Первая ночь в Казани\",\"limit\":3,\"opening\":[\"Это снова ты.\"]," +
            "\"opening_again\":[\"Ты опять пришёл.\"],\"chips\":[\"Ты скучаешь по степи?\"],\"wake\":\"Рассвет.\",\"year\":1533}");

        static Dictionary<string, object> Parse(string json) => (Dictionary<string, object>)JsonReader.Parse(json);

        [Test]
        public void TheNightOfTheServerIsRead()
        {
            var n = Night();
            Assert.That((n.Id, n.Title, n.Limit, n.Wake), Is.EqualTo(("ep01", "Первая ночь в Казани", 3, "Рассвет.")));
            Assert.That(n.Chips, Is.EqualTo(new[] { "Ты скучаешь по степи?" }));
        }

        [Test]
        public void SheSpeaksFirstAndRemembersTheVoiceOnceSheHasAMemory()
        {
            var first = Dream.Begin(Night(), "2026-10-29", "", new Random(1));
            Assert.That(first.lines.Single().text, Is.EqualTo("Это снова ты."));
            Assert.That(first.lines[0].role, Is.EqualTo("heroine"));
            var again = Dream.Begin(Night(), "2026-10-30", "Голос говорил о Казани.", new Random(1));
            Assert.That(again.lines.Single().text, Is.EqualTo("Ты опять пришёл."));
            Assert.That(again.memory, Is.EqualTo("Голос говорил о Казани."));
        }

        [Test]
        public void TheRepliesOfADayAreTheLimitAndTheBoughtOnes()
        {
            var d = Dream.Begin(Night(), "2026-10-29", "", new Random(1));
            Assert.That(d.Left(3), Is.EqualTo(3));
            for (var i = 0; i < 3; i++)
            {
                d.lines.Add(new DreamLine { role = "voice", text = "?" });
                d.lines.Add(new DreamLine { role = "heroine", text = "…" });
            }
            Assert.That(d.Left(3), Is.EqualTo(0));
            d.bought += Dream.ExtraReplies;
            Assert.That(d.Left(3), Is.EqualTo(Dream.ExtraReplies));
            Assert.That(d.IsOf("ep01", "2026-10-29"), Is.True);
            Assert.That(d.IsOf("ep01", "2026-10-30"), Is.False, "a new day is a new dream");
        }

        [Test]
        public void OnlyTheLinesNotYetRememberedGoToTheSummary()
        {
            var d = Dream.Begin(Night(), "2026-10-29", "старое", new Random(1));
            Assert.That(d.Unremembered, Is.False, "her own first line alone is nothing to remember");
            d.lines.Add(new DreamLine { role = "voice", text = "Казань сгорит." });
            d.lines.Add(new DreamLine { role = "heroine", text = "Сны лгут." });
            Assert.That(d.Unremembered, Is.True);
            d.remembered = 2;
            var body = Parse(d.SummaryBody());
            var history = (List<object>)body["history"];
            Assert.That(history.Count, Is.EqualTo(1));
            Assert.That(((Dictionary<string, object>)history[0])["content"], Is.EqualTo("Сны лгут."));
            Assert.That(body["memory"], Is.EqualTo("старое"));
            d.remembered = 3;
            Assert.That(d.Unremembered, Is.False);
        }

        [Test]
        public void TheChatBodyCarriesTheLinesTheVariablesOfTheirKindAndTheMemory()
        {
            var d = Dream.Begin(Night(), "2026-10-29", "помню \"сон\"", new Random(1));
            d.lines.Add(new DreamLine { role = "voice", text = "Ты боишься Казани?\nСкажи." });
            var stats = new ChapterStats
            {
                chapter = "golden-cage",
                vars =
                {
                    new StatValue { name = "safiya_trust", kind = "int", value = "2" },
                    new StatValue { name = "has_knife", kind = "bool", value = "true" },
                    new StatValue { name = "mood", kind = "string", value = "тихо" },
                },
            };
            var body = Parse(d.ChatBody(stats));
            var history = (List<object>)body["history"];
            Assert.That(history.Count, Is.EqualTo(2));
            Assert.That(((Dictionary<string, object>)history[1])["role"], Is.EqualTo("voice"));
            Assert.That(((Dictionary<string, object>)history[1])["content"], Is.EqualTo("Ты боишься Казани?\nСкажи."));
            var state = (Dictionary<string, object>)body["state"];
            Assert.That(state["safiya_trust"], Is.EqualTo(2));
            Assert.That(state["has_knife"], Is.EqualTo(true));
            Assert.That(state["mood"], Is.EqualTo("тихо"));
            Assert.That(body["memory"], Is.EqualTo("помню \"сон\""));
            Assert.That(((Dictionary<string, object>)Parse(d.ChatBody(null))["state"]).Count, Is.EqualTo(0));
        }

        [Test]
        public void TheNightOpensAfterTheChapterAndOnlyWithTheDream()
        {
            var dreamt = new ChapterStats { vars = { new StatValue { name = "dream_flag", kind = "bool", value = "true" } } };
            var not = new ChapterStats { vars = { new StatValue { name = "dream_flag", kind = "bool", value = "false" } } };
            Assert.That(Dream.Opens(false, dreamt), Is.False);
            Assert.That(Dream.Opens(true, dreamt), Is.True);
            Assert.That(Dream.Opens(true, not), Is.False);
            Assert.That(Dream.Opens(true, null), Is.True, "read before the stats were kept");
        }

        [Test]
        public void TheStreamIsReadWhateverTheChunksAre()
        {
            const string wire =
                "event: sources\ndata: {\"hits\":[]}\n\n" +
                "event: delta\ndata: {\"text\":\"Степь \"}\n\n" +
                "event: delta\r\ndata: {\"text\":\"шепчет.\"}\r\n\r\n" +
                "event: done\ndata: {\"flags\":[{\"kind\":\"future\",\"detail\":\"1552\"}]}\n\n";
            foreach (var size in new[] { 1, 3, 7, wire.Length })
            {
                var stream = new DreamStream();
                var events = new List<DreamStream.Event>();
                for (var i = 0; i < wire.Length; i += size) events.AddRange(stream.Feed(wire.Substring(i, Math.Min(size, wire.Length - i))));
                Assert.That(events.Select(e => e.Name), Is.EqualTo(new[] { "sources", "delta", "delta", "done" }), $"chunks of {size}");
                Assert.That(string.Concat(events.Select(e => e.Text)), Is.EqualTo("Степь шепчет."));
                Assert.That(events[3].Flags, Is.EqualTo(new[] { "future" }));
            }
            var error = new DreamStream().Feed("event: error\ndata: {\"message\":\"HTTP 502\"}\n\n").Single();
            Assert.That((error.Name, error.Message), Is.EqualTo(("error", "HTTP 502")));
        }
    }
}

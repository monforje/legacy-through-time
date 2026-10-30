using System.Collections.Generic;
using System.Linq;
using LegacyThroughTime.Narrative.Json;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    /// The talk with the hero of a story: its lines, the energy, the list of chats, what goes to the chat server.
    public class ChatTests
    {
        static Dictionary<string, object> Parse(string json) => (Dictionary<string, object>)JsonReader.Parse(json);

        static Dictionary<string, object> Item(object list, int i) => (Dictionary<string, object>)((List<object>)list)[i];

        [Test]
        public void TheHeroSpeaksFirstWhenTheAuthorGaveAnOpening()
        {
            var chat = Chat.Begin("golden-cage", "Это снова ты.", 100);
            Assert.That(chat.lines.Single().role, Is.EqualTo(ChatLine.Hero));
            Assert.That(chat.Started, Is.False, "her own line alone is not a talk yet");
            Assert.That(Chat.Begin("golden-cage", null, 100).lines, Is.Empty);
            Assert.That(Chat.Begin("golden-cage", "  ", 100).lines, Is.Empty);
        }

        [Test]
        public void TheReadersWordsAreTrimmedAndCut()
        {
            Assert.That(Chat.Clean("  привет \n"), Is.EqualTo("привет"));
            Assert.That(Chat.Clean("   "), Is.Null);
            Assert.That(Chat.Clean(null), Is.Null);
            Assert.That(Chat.Clean(new string('я', Chat.MaxLetters + 50)).Length, Is.EqualTo(Chat.MaxLetters));
        }

        [Test]
        public void ARepliesAreUnreadUntilTheChatIsSeen()
        {
            var chat = Chat.Begin("golden-cage", null, 1);
            chat.Say("Ты здесь?", 2);
            chat.Reply("Здесь.", 3, seen: true);
            Assert.That(chat.unread, Is.EqualTo(0));
            chat.Reply("Ещё здесь.", 4, seen: false);
            chat.Reply("И тут.", 5, seen: false);
            Assert.That(chat.unread, Is.EqualTo(2));
            chat.MarkSeen();
            Assert.That(chat.unread, Is.EqualTo(0));
            Assert.That(chat.Started, Is.True);
        }

        [Test]
        public void AFailedMessageIsTakenBack()
        {
            var chat = Chat.Begin("golden-cage", "Привет.", 1);
            var said = chat.Say("Как ты?", 2);
            chat.Take(said);
            Assert.That(chat.lines.Select(l => l.text), Is.EqualTo(new[] { "Привет." }));
            Assert.That(chat.Started, Is.False);
        }

        [Test]
        public void TheFlagReportsTheLastAnswerOnceAndNeverTheAuthorsOpening()
        {
            var chat = Chat.Begin("golden-cage", "Привет.", 1);
            Assert.That(chat.ReportableIndex(), Is.EqualTo(-1), "only the author's line");
            chat.Say("Ну?", 2);
            chat.Reply("Что?", 3, true);
            chat.Say("Ещё", 4);
            chat.Reply("Да.", 5, true);
            Assert.That(chat.ReportableIndex(), Is.EqualTo(4));
            chat.lines[4].reported = true;
            Assert.That(chat.ReportableIndex(), Is.EqualTo(-1), "the last answer is reported already; the older ones are not offered");
            chat.Say("И?", 6);
            Assert.That(chat.ReportableIndex(), Is.EqualTo(-1), "the reader spoke last: still the last answer");
        }

        [Test]
        public void OnlyTheLinesNotYetRememberedGoToTheSummary()
        {
            var chat = Chat.Begin("golden-cage", "Привет.", 1);
            chat.memory = "старое";
            Assert.That(chat.Unremembered, Is.False);
            chat.Say("Казань сгорит.", 2);
            chat.Reply("Сны лгут.", 3, true);
            Assert.That(chat.Unremembered, Is.True);
            chat.remembered = 2;
            var body = Parse(chat.SummaryBody());
            Assert.That(((List<object>)body["history"]).Count, Is.EqualTo(1));
            Assert.That(Item(body["history"], 0)["content"], Is.EqualTo("Сны лгут."));
            Assert.That(body["memory"], Is.EqualTo("старое"));
            chat.remembered = 3;
            Assert.That(chat.Unremembered, Is.False);
        }

        [Test]
        public void TheChatBodyCarriesTheLinesTheVariablesOfTheirKindAndTheMemory()
        {
            var chat = Chat.Begin("golden-cage", "Привет.", 1);
            chat.memory = "помню \"разговор\"";
            chat.Say("Ты боишься Казани?\nСкажи.", 2);
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
            var body = Parse(chat.ChatBody(stats));
            Assert.That(body["story"], Is.EqualTo("golden-cage"));
            Assert.That(Item(body["history"], 1)["role"], Is.EqualTo("reader"), "the reader's line is the last");
            Assert.That(Item(body["history"], 1)["content"], Is.EqualTo("Ты боишься Казани?\nСкажи."));
            var state = (Dictionary<string, object>)body["state"];
            Assert.That(state["safiya_trust"], Is.EqualTo(2));
            Assert.That(state["has_knife"], Is.EqualTo(true));
            Assert.That(state["mood"], Is.EqualTo("тихо"));
            Assert.That(body["memory"], Is.EqualTo("помню \"разговор\""));
            Assert.That(((Dictionary<string, object>)Parse(chat.ChatBody(null))["state"]), Is.Empty);
        }

        [Test]
        public void OnlyTheLastLinesGoWithAMessage()
        {
            var chat = new Chat { story = "golden-cage" };
            for (var i = 0; i < 50; i++) chat.Say("строка " + i, i);
            var history = (List<object>)Parse(chat.ChatBody(null))["history"];
            Assert.That(history.Count, Is.EqualTo(30));
            Assert.That(Item(history, 29)["content"], Is.EqualTo("строка 49"));
        }

        [Test]
        public void AReportCarriesTheLineAndWhatLedToIt()
        {
            var chat = Chat.Begin("golden-cage", "Привет.", 1);
            chat.Say("Что будет?", 2);
            chat.Reply("Всё сгорит.", 3, true);
            chat.Say("Спасибо", 4);
            var body = Parse(chat.ReportBody(2, null));
            Assert.That(body["message"], Is.EqualTo("Всё сгорит."));
            Assert.That(body["reason"], Is.EqualTo("reader"));
            Assert.That(((List<object>)body["history"]).Count, Is.EqualTo(3), "nothing after the reported line");
        }

        // ------------------------------------------------------------ energy
        [Test]
        public void EachMessageCostsEnergyAndNoneLeftBlocksTheField()
        {
            var e = new Energy();
            Assert.That(e.value, Is.EqualTo(Energy.Max));
            for (var i = 0; i < Energy.Max; i++) Assert.That(e.TrySpend(), Is.True, "message " + i);
            Assert.That(e.value, Is.EqualTo(0));
            Assert.That(e.CanSend, Is.False);
            Assert.That(e.TrySpend(), Is.False);
            Assert.That(e.value, Is.EqualTo(0), "never below zero");
        }

        [Test]
        public void AChapterAfterTheFirstCostsEnergyAndTheFirstIsFree()
        {
            Assert.That(new ChapterEntry { Number = 1 }.EnergyCost, Is.EqualTo(0));
            Assert.That(new ChapterEntry { Number = 2 }.EnergyCost, Is.EqualTo(Energy.ChapterCost));
            var e = new Energy();
            Assert.That(e.TrySpend(Energy.ChapterCost), Is.True);
            Assert.That(e.value, Is.EqualTo(Energy.Max - Energy.ChapterCost));
            Assert.That(e.TrySpend(Energy.ChapterCost), Is.True);
            Assert.That(e.TrySpend(Energy.ChapterCost), Is.False, "not enough: nothing is taken");
            Assert.That(e.value, Is.EqualTo(0));
            Assert.That(e.TrySpend(0), Is.True, "a free thing costs nothing");
        }

        [Test]
        public void AnInterstitialIsDueOnceInThirtyMinutesAndNeverForAPayingReader()
        {
            const long minute = 60_000;
            Assert.That(AdRules.InterstitialDue(0, 1_000 * minute, false), Is.True, "the first one");
            Assert.That(AdRules.InterstitialDue(1_000 * minute, 1_029 * minute, false), Is.False, "29 minutes later");
            Assert.That(AdRules.InterstitialDue(1_000 * minute, 1_030 * minute, false), Is.True, "30 minutes later");
            Assert.That(AdRules.InterstitialDue(0, 1_000 * minute, true), Is.False, "a reader who has paid");
        }

        [Test]
        public void ARefundNeverGoesOverTheMaxAndARefillFillsUp()
        {
            var e = new Energy();
            e.Refund();
            Assert.That(e.value, Is.EqualTo(Energy.Max));
            e.TrySpend(); e.TrySpend();
            e.Refund();
            Assert.That(e.value, Is.EqualTo(Energy.Max - 1));
            e.Refill();
            Assert.That(e.value, Is.EqualTo(Energy.Max));
        }

        [Test]
        public void ANewDayFillsTheEnergyUp()
        {
            var e = new Energy();
            Assert.That(e.NewDay("2026-10-30"), Is.False, "full already: nothing changed, the day is kept");
            e.TrySpend(); e.TrySpend();
            Assert.That(e.NewDay("2026-10-30"), Is.False, "the same day");
            Assert.That(e.value, Is.EqualTo(Energy.Max - 2));
            Assert.That(e.NewDay("2026-10-31"), Is.True);
            Assert.That(e.value, Is.EqualTo(Energy.Max));
        }

        [Test]
        public void TheTimeOfARowIsTheHourTheDayOrTheDate()
        {
            var now = new System.DateTime(2026, 10, 30, 15, 0, 0, System.DateTimeKind.Local);
            long Ms(System.DateTime t) => new System.DateTimeOffset(t).ToUnixTimeMilliseconds();
            Assert.That(ChatList.When(Ms(now.Date.AddHours(12).AddMinutes(40)), now), Is.EqualTo("12:40"));
            Assert.That(ChatList.When(Ms(now.AddDays(-1)), now), Is.EqualTo("Вчера"));
            Assert.That(ChatList.When(Ms(now.AddDays(-4)), now), Is.EqualTo("Пн"), "2026-10-26 is a Monday");
            Assert.That(ChatList.When(Ms(now.AddDays(-30)), now), Is.EqualTo("30.09"));
            Assert.That(ChatList.When(0, now), Is.EqualTo(""), "never said anything");
        }

        [Test]
        public void AnAvatarKeepsItsColour()
        {
            Assert.That(ChatList.ArtIndex("golden-cage"), Is.EqualTo(ChatList.ArtIndex("golden-cage")));
            Assert.That(ChatList.ArtIndex("golden-cage"), Is.InRange(0, 9));
            Assert.That(ChatList.ArtIndex(null), Is.InRange(0, 9));
        }

        // ------------------------------------------------------------ the stream of the server
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
                var stream = new ChatStream();
                var events = new List<ChatStream.Event>();
                for (var i = 0; i < wire.Length; i += size) events.AddRange(stream.Feed(wire.Substring(i, System.Math.Min(size, wire.Length - i))));
                Assert.That(events.Select(e => e.Name), Is.EqualTo(new[] { "sources", "delta", "delta", "done" }), $"chunks of {size}");
                Assert.That(string.Concat(events.Select(e => e.Text)), Is.EqualTo("Степь шепчет."));
                Assert.That(events[3].Flags, Is.EqualTo(new[] { "future" }));
            }
            var error = new ChatStream().Feed("event: error\ndata: {\"message\":\"HTTP 502\"}\n\n").Single();
            Assert.That((error.Name, error.Message), Is.EqualTo(("error", "HTTP 502")));
        }

        // ------------------------------------------------------------ the list of chats
        static StoryEntry Story(string id, string hero, bool soon = false) => new StoryEntry { Id = id, Title = id, Hero = hero, Soon = soon };

        [Test]
        public void TheListHoldsHeroesOfStartedStoriesTheLatestTalkFirst()
        {
            var a = Story("a", "Анна"); var b = Story("b", "Борис"); var c = Story("c", "Вера");
            var soon = Story("soon", "Гюль", soon: true); var nobody = Story("nobody", null);
            var chats = new Dictionary<string, Chat>
            {
                ["a"] = new Chat { story = "a", lines = { new ChatLine { role = ChatLine.Reader, text = "раньше", at = 10 } } },
                ["b"] = new Chat { story = "b", unread = 2, lines = { new ChatLine { role = ChatLine.Hero, text = "позже", at = 20 } } },
            };
            var rows = ChatList.Build(new[] { a, b, c, soon, nobody }, id => id != "d", id => chats.GetValueOrDefault(id));
            Assert.That(rows.Select(r => r.Hero), Is.EqualTo(new[] { "Борис", "Анна", "Вера" }),
                "announced stories and stories without a hero have no chat; never-talked ones come last");
            Assert.That((rows[0].Preview, rows[0].Unread), Is.EqualTo(("позже", 2)));
            Assert.That(rows[2].Preview, Is.Null, "the screen shows 'Начните разговор'");
        }

        [Test]
        public void StoriesNotStartedHaveNoRow()
        {
            var rows = ChatList.Build(new[] { Story("a", "Анна"), Story("b", "Борис") }, id => id == "b", _ => null);
            Assert.That(rows.Select(r => r.Story.Id), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void NeverTalkedRowsKeepTheOrderOfTheMenu()
        {
            var rows = ChatList.Build(Enumerable.Range(0, 6).Select(i => Story("s" + i, "H" + i)), _ => true, _ => null);
            Assert.That(rows.Select(r => r.Story.Id), Is.EqualTo(Enumerable.Range(0, 6).Select(i => "s" + i)));
        }

        [Test]
        public void TheHeroFeelsWhatTheLastReadChapterLeft()
        {
            var one = new ChapterStats { chapter = "one", at = 100 };
            var two = new ChapterStats { chapter = "two", at = 200 };
            Assert.That(ChatList.Latest(new[] { two, one, null }), Is.SameAs(two));
            Assert.That(ChatList.Latest(new ChapterStats[0]), Is.Null);
        }

        [Test]
        public void TheCatalogGivesTheGoldenCageItsHero()
        {
            Assert.That(StoryCatalog.Find("golden-cage").Hero, Is.EqualTo("Сююмбике"));
        }
    }
}

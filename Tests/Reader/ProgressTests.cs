using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    /// Two copies of the reader's progress, the phone's and the one in Google Play saved games, become one.
    public class ProgressTests
    {
        static Progress Fresh() => new Progress { coins = ShopCatalog.StartCoins };

        static StorySave Chapter(string id, long at, string machine = "{}") =>
            new StorySave { story = id, machine = machine, background = "yurt", at = at };

        [Test]
        public void ANewPhoneTakesEverythingFromTheCloud()
        {
            var cloud = new Progress
            {
                coins = 340, coinsAt = 1000, giftDay = "2026-10-26",
                words = { "Калфак", "Бий" },
                finished = { new FinishMark { id = "golden-cage", at = 900 } },
                chapters = { Chapter("golden-cage-2", 950, "{\"line\":12}") },
                credited = { "GPA.1" },
            };
            var merged = Progress.Merge(Fresh(), cloud);

            Assert.That((merged.coins, merged.coinsAt, merged.giftDay), Is.EqualTo((340, 1000L, "2026-10-26")));
            Assert.That(merged.words, Is.EqualTo(new[] { "Калфак", "Бий" }));
            Assert.That(merged.finished.Select(f => (f.id, f.at)), Is.EqualTo(new[] { ("golden-cage", 900L) }));
            Assert.That(merged.chapters.Single().machine, Is.EqualTo("{\"line\":12}"));
            Assert.That(merged.credited, Is.EqualTo(new[] { "GPA.1" }));
        }

        [Test]
        public void APhoneThatPlayedBeforeTheCloudKeepsItsCoins()
        {
            // coins earned before this build have no time: against an empty cloud they still win
            var here = new Progress { coins = 150, coinsAt = 0 };
            Assert.That(Progress.Merge(here, null).coins, Is.EqualTo(150));
            Assert.That(Progress.Merge(here, new Progress()).coins, Is.EqualTo(150));
        }

        [Test]
        public void TheLaterPurseWins()
        {
            var older = new Progress { coins = 90, coinsAt = 100 };
            var newer = new Progress { coins = 50, coinsAt = 200 };
            Assert.That(Progress.Merge(older, newer).coins, Is.EqualTo(50));
            Assert.That(Progress.Merge(newer, older).coins, Is.EqualTo(50));
            Assert.That(Progress.Merge(newer, older).coinsAt, Is.EqualTo(200));
        }

        [Test]
        public void EachChapterKeepsItsLaterSave()
        {
            var here = new Progress { chapters = { Chapter("a", 5, "here"), Chapter("b", 3, "only here") } };
            var cloud = new Progress { chapters = { Chapter("a", 7, "cloud"), Chapter("c", 1, "only cloud") } };
            var merged = Progress.Merge(here, cloud).chapters.ToDictionary(c => c.story, c => c.machine);

            Assert.That(merged, Is.EquivalentTo(new Dictionary<string, string>
            {
                ["a"] = "cloud", ["b"] = "only here", ["c"] = "only cloud",
            }));
        }

        [Test]
        public void AChapterFinishedLaterDropsTheOlderSave()
        {
            // the phone finished the chapter (its save is gone); the cloud still has the reader in the middle of it
            var here = new Progress { finished = { new FinishMark { id = "a", at = 10 } } };
            var cloud = new Progress { chapters = { Chapter("a", 5) } };
            Assert.That(Progress.Merge(here, cloud).chapters, Is.Empty);
            Assert.That(Progress.Merge(cloud, here).chapters, Is.Empty);

            // read again after finishing: that save is newer than the mark and stays
            var reread = new Progress { chapters = { Chapter("a", 12) } };
            Assert.That(Progress.Merge(here, reread).chapters.Select(c => c.at), Is.EqualTo(new[] { 12L }));
        }

        [Test]
        public void AFinishMarkWithoutTimeDropsNoSave()
        {
            // marks made before this build carry no time: which came first cannot be known, so the save stays
            var here = new Progress { finished = { new FinishMark { id = "a", at = 0 } } };
            var cloud = new Progress { chapters = { Chapter("a", 5) } };
            Assert.That(Progress.Merge(here, cloud).chapters, Has.Count.EqualTo(1));
        }

        [Test]
        public void FinishMarksAreUnitedWithTheLaterTime()
        {
            var here = new Progress { finished = { new FinishMark { id = "a", at = 4 }, new FinishMark { id = "b", at = 1 } } };
            var cloud = new Progress { finished = { new FinishMark { id = "a", at = 9 }, new FinishMark { id = "c", at = 2 } } };
            var merged = Progress.Merge(here, cloud).finished.ToDictionary(f => f.id, f => f.at);
            Assert.That(merged, Is.EquivalentTo(new Dictionary<string, long> { ["a"] = 9, ["b"] = 1, ["c"] = 2 }));
        }

        [Test]
        public void WordsArePooledInTheOrderTheyWereMet()
        {
            var here = new Progress { words = { "Бий", "Калфак" } };
            var cloud = new Progress { words = { "Калфак", "Чура" } };
            Assert.That(Progress.Merge(here, cloud).words, Is.EqualTo(new[] { "Бий", "Калфак", "Чура" }));
        }

        [Test]
        public void CreditedPurchasesArePooledUpToTheLedgerSize()
        {
            // an order credited on one phone must not be credited again on the other
            var here = new Progress { credited = Enumerable.Range(0, Progress.CreditedKept).Select(i => "here." + i).ToList() };
            var cloud = new Progress { credited = { "here.5", "cloud.1" } };
            var merged = Progress.Merge(here, cloud).credited;

            Assert.That(merged, Has.Count.EqualTo(Progress.CreditedKept));
            Assert.That(merged, Is.Unique);
            Assert.That(merged.Last(), Is.EqualTo("cloud.1"));
            Assert.That(merged, Does.Not.Contain("here.0"), "the oldest goes first");
        }

        [Test]
        public void TheLaterGiftDayIsKept()
        {
            Assert.That(Progress.Merge(new Progress { giftDay = "2026-10-27" }, new Progress { giftDay = "2026-10-26" }).giftDay, Is.EqualTo("2026-10-27"));
            Assert.That(Progress.Merge(new Progress { giftDay = "" }, new Progress { giftDay = "2026-10-26" }).giftDay, Is.EqualTo("2026-10-26"));
            Assert.That(Progress.Merge(new Progress { giftDay = null }, null).giftDay, Is.EqualTo(""));
        }

        [Test]
        public void MergingLeavesBothCopiesAsTheyWere()
        {
            var here = new Progress { words = { "Бий" }, chapters = { Chapter("a", 1) } };
            var cloud = new Progress { words = { "Чура" }, chapters = { Chapter("a", 2) } };
            Progress.Merge(here, cloud);
            Assert.That(here.words, Is.EqualTo(new[] { "Бий" }));
            Assert.That(here.chapters.Single().at, Is.EqualTo(1));
            Assert.That(cloud.words, Is.EqualTo(new[] { "Чура" }));
        }
    }
}

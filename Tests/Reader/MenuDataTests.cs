using System;
using System.Linq;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    /// The data behind the new menu: chapters of the catalog, the words of the library, the rules of the shop.
    public class MenuDataTests
    {
        [Test]
        public void EveryWordTheEpisodeExplainsIsInTheLibraryOnce()
        {
            var terms = Glossary.FromStory(Project.EpisodeJson());
            Assert.That(terms.Count, Is.GreaterThanOrEqualTo(5));
            Assert.That(terms.Select(t => t.Name), Is.Unique);
            Assert.That(terms.Select(t => t.Name), Does.Contain("Калфак").And.Contain("Бий"));
            foreach (var t in terms)
            {
                Assert.That(t.Text, Is.Not.Empty, t.Name);
                Assert.That(char.IsUpper(t.Text[0]), t.Name + ": the explanation starts with a capital letter");
            }
        }

        [Test]
        public void ANoteSplitsIntoTheWordAndItsExplanation()
        {
            var t = Glossary.Parse("Чура — слуга при дворе.");
            Assert.That((t.Name, t.Text), Is.EqualTo(("Чура", "Слуга при дворе.")));
            Assert.That(Glossary.Parse("  "), Is.Null);
        }

        [Test]
        public void TheFirstChapterKeepsTheOldSaveSlot()
        {
            // saves made before there were chapters are keyed by the story: they must still open
            var story = StoryCatalog.Find("golden-cage");
            Assert.That(story.First.SaveId, Is.EqualTo(story.Id));
            Assert.That(story.First.Soon, Is.False);
            Assert.That(story.Chapters.Select(c => c.SaveId), Is.Unique);
            Assert.That(story.Chapters.Select(c => c.Number), Is.EqualTo(Enumerable.Range(1, story.Chapters.Count)));
            foreach (var soon in StoryCatalog.All.Where(s => s.Soon)) Assert.That(soon.Chapters, Is.Empty, soon.Id);
        }

        [Test]
        public void TheGiftComesOnceADay()
        {
            var now = new DateTime(2026, 10, 27, 21, 30, 0);
            Assert.That(ShopCatalog.GiftReady(null, now), Is.True);
            Assert.That(ShopCatalog.GiftReady("2026-10-26", now), Is.True);
            Assert.That(ShopCatalog.GiftReady(ShopCatalog.Day(now), now), Is.False);
            Assert.That(ShopCatalog.UntilNextGift(now), Is.EqualTo(TimeSpan.FromMinutes(150)));
        }

        [Test]
        public void ThePacksAreDistinctAndGrow()
        {
            Assert.That(ShopCatalog.Packs.Select(p => p.Id), Is.Unique);
            Assert.That(ShopCatalog.Packs.Select(p => p.Coins), Is.Ordered.And.All.GreaterThan(0));
            Assert.That(ShopCatalog.Find("coins_300").Coins, Is.EqualTo(300));
        }
    }
}

using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    public class TextRulesTests
    {
        [TestCase("А если я не буду?", "А\u00a0если я\u00a0не\u00a0буду?")]
        [TestCase("Он пахнет, и я в доме", "Он\u00a0пахнет, и\u00a0я\u00a0в\u00a0доме")]
        [TestCase("Калфак — убор", "Калфак\u00a0— убор")]
        [TestCase("Ветер", "Ветер")]
        public void NoBreaksGluesShortWordsAndDashes(string text, string expected) =>
            Assert.That(TextRules.NoBreaks(text), Is.EqualTo(expected));

        [TestCase("ПАМЯТЬ СТЕПИ +1", "+1 Память степи")]
        [TestCase("МУДРОСТЬ +1", "+1 Мудрость")]
        [TestCase("УВАЖЕНИЕ САФИИ +1", "+1 Уважение Сафии")]
        [TestCase("ВОЛЯ СТЕПЕЙ −2", "-2 Воля степей")]
        public void StatLinesBecomeBanners(string line, string banner) =>
            Assert.That(TextRules.StatToast(line), Is.EqualTo(banner));

        [TestCase("МУДРОСТЬ ХАНСТВА снижена. ВОЛЯ СТЕПЕЙ повышена.")]
        [TestCase("Просто текст +1 в середине")]
        [TestCase("ОТКРЫТА ВЕТКА «СОВМЕСТНОЕ РАССЛЕДОВАНИЕ».")]
        public void OtherLinesAreNotBanners(string line) => Assert.That(TextRules.StatToast(line), Is.Null);

        [TestCase("ОТКРЫТАЯ ИНФОРМАЦИЯ: «ДИВАН». В будущем", "Открытая информация: «Диван». В будущем")]
        [TestCase("УВАЖЕНИЕ САФИИ СНИЖЕНО.", "Уважение Сафии снижено.")]
        [TestCase("ЭПИЗОД 1. «ЗОЛОТАЯ КЛЕТКА»", "Эпизод 1. «Золотая клетка»")]
        [TestCase("Обычный текст остаётся", "Обычный текст остаётся")]
        public void ShoutingBecomesASentence(string text, string expected) =>
            Assert.That(TextRules.SentenceCase(text), Is.EqualTo(expected));

        [Test]
        public void TheTypingNeverTakesLongerThanTheCap()
        {
            foreach (var words in new[] { 1, 5, 20, 60, 200 })
                Assert.That(RevealPlan.Total(words), Is.LessThanOrEqualTo(RevealPlan.MaxTotal + 1e-4f), $"{words} words");
            Assert.That(RevealPlan.Step(3), Is.EqualTo(RevealPlan.DefaultStep), "a short text keeps the natural pace");
        }

        [Test]
        public void WordsAppearInOrderAndEndFullyVisible()
        {
            var step = RevealPlan.Step(10);
            for (var i = 0; i < 9; i++)
                for (float t = 0; t < RevealPlan.Total(10); t += .05f)
                    Assert.That(RevealPlan.Alpha(i, t, step), Is.GreaterThanOrEqualTo(RevealPlan.Alpha(i + 1, t, step)));
            Assert.That(RevealPlan.Alpha(0, 0, step), Is.EqualTo(0), "invisible at the start");
            Assert.That(RevealPlan.Alpha(9, RevealPlan.Total(10), step), Is.EqualTo(1).Within(1e-4), "the last word is whole at the end");
        }
    }
}

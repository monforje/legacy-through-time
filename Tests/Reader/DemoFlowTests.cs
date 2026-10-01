using System.Linq;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    public class DemoFlowTests
    {
        [Test]
        public void TheOrderNamesExistingScreensOnce()
        {
            Assert.That(DemoFlow.Order, Is.Unique);
            foreach (var id in DemoFlow.Order) Assert.That(DemoFlow.Get(id).Id, Is.EqualTo(id));
        }

        [Test]
        public void TheFlowEndsWithTheEndScreen() =>
            Assert.That(DemoFlow.Pages().Last(), Is.SameAs(DemoFlow.End));

        [Test]
        public void EveryScreenIsBuiltFromAtLeastOneBlock()
        {
            foreach (var page in DemoFlow.Pages())
                Assert.That(new object[] { page.Title, page.Plate, page.Choice, page.Picker }.Any(b => b != null), Is.True, page.Id);
        }

        [Test]
        public void ChoicesHaveOptionsAndPickersHaveValues()
        {
            foreach (var page in DemoFlow.Pages())
            {
                if (page.Choice != null) Assert.That(page.Choice.Items, Is.Not.Empty, page.Id);
                if (page.Picker != null) Assert.That(page.Picker.Values.Length, Is.GreaterThan(1), page.Id);
                if (page.Choice?.Seconds > 0) Assert.That(page.Plate, Is.Not.Null, "a timer sits on a plate: " + page.Id);
            }
        }
    }
}

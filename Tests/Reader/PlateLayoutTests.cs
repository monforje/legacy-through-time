using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    public class PlateLayoutTests
    {
        const float Floor = Metrics.OptionsLine;
        static readonly float Top = PlateLayout.TopLine(Metrics.RefHeight);

        [Test]
        public void TheTopLineIsAFixedShareOfTheScreen()
        {
            Assert.That(Top, Is.EqualTo(Metrics.PlateTop));
            Assert.That(PlateLayout.TopLine(600), Is.EqualTo(Metrics.PlateTop * 600 / 780).Within(1e-3));
        }

        [Test]
        public void ATextWithoutOptionsGrowsDownFromTheTopLine()
        {
            foreach (var height in new float[] { 110, 160, 220 })
            {
                var bottom = PlateLayout.Bottom(Top, height, Floor, optionsHeight: 0);
                Assert.That(bottom + height, Is.EqualTo(Top), $"top edge of a {height} px plate must stay on the line");
            }
        }

        [Test]
        public void ThePlateStopsAtTheFloorAndThenGrowsUp()
        {
            var tooTall = Top - Floor + 40;
            var bottom = PlateLayout.Bottom(Top, tooTall, Floor, 0);
            Assert.That(bottom, Is.EqualTo(Floor));
            Assert.That(bottom + tooTall, Is.GreaterThan(Top));
        }

        [Test]
        public void OptionsKeepTheirRoomAndPushAShortPlateUp()
        {
            var options = 185f;                                   // three options
            var bottom = PlateLayout.Bottom(Top, 110, Floor, options);
            Assert.That(bottom, Is.EqualTo(Floor + options - Metrics.OptionsOverlap), "the plate rests on the options, overlapping them");
            Assert.That(bottom + 110, Is.GreaterThan(Top), "so it stands higher than the reference line");
        }

        [Test]
        public void ATallPlateKeepsItsReferenceLineWhileTheOptionsHaveRoom()
        {
            var bottom = PlateLayout.Bottom(Top, 160, Floor, 40);
            Assert.That(bottom + 160, Is.EqualTo(Top));
        }

        [Test]
        public void TextNeverMakesAPlateShorterThanTheMinimum()
        {
            var skin = PlateSkins.For(PlateStyle.Narr);
            Assert.That(PlateLayout.Height(skin, 20), Is.EqualTo(PlateLayout.MinHeight));
            Assert.That(PlateLayout.Height(skin, 100), Is.EqualTo(100 + skin.PadTop + skin.PadBottom));
        }

        [Test]
        public void CloudsAreWholeTilesPlusCorners()
        {
            foreach (var style in new[] { PlateStyle.Thought, PlateStyle.ThoughtPuff })
            {
                var skin = PlateSkins.For(style);
                for (float text = 10; text < 300; text += 7)
                {
                    var h = PlateLayout.Height(skin, text);
                    Assert.That((h - skin.TileCorner) % skin.TileSize, Is.EqualTo(0).Within(1e-3), $"{style} at text {text}");
                    Assert.That(h, Is.GreaterThanOrEqualTo(text + skin.PadTop + skin.PadBottom));
                }
            }
        }

        [TestCase(0, 80, 40, 80)]
        [TestCase(80, 80, 40, 80)]
        [TestCase(81, 80, 40, 120)]
        [TestCase(150, 64, 32, 160)]
        public void RoundUpToTile(float height, float corners, float tile, float expected) =>
            Assert.That(PlateLayout.RoundUpToTile(height, corners, tile), Is.EqualTo(expected));

        [Test]
        public void TheStackIsItsButtonsPlusGaps()
        {
            Assert.That(PlateLayout.StackHeight(new float[0], 10), Is.EqualTo(0));
            Assert.That(PlateLayout.StackHeight(new float[] { 50 }, 10), Is.EqualTo(50));
            Assert.That(PlateLayout.StackHeight(new float[] { 65, 50, 50 }, 10), Is.EqualTo(185));
        }
    }
}

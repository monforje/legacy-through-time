using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    public class PlateSkinsTests
    {
        static readonly PlateStyle[] Styles = (PlateStyle[])Enum.GetValues(typeof(PlateStyle));

        [Test]
        public void EveryStyleHasASkin() =>
            Assert.That(Styles.Select(PlateSkins.For), Has.All.Not.Null);

        [Test]
        public void ASpeechPlateHasItsHornOnTheSideOppositeToItsTag()
        {
            var heroine = PlateSkins.For(PlateStyle.SpeechLeft);
            var others = PlateSkins.For(PlateStyle.SpeechRight);
            Assert.That((heroine.TagSide, heroine.Tail), Is.EqualTo((TagSide.Left, TailSide.Right)));
            Assert.That((others.TagSide, others.Tail), Is.EqualTo((TagSide.Right, TailSide.Left)));
            Assert.That(heroine.Headroom, Is.GreaterThan(0));
            Assert.That(PlateSkins.For(PlateStyle.Narr).Headroom, Is.EqualTo(0));
        }

        [Test]
        public void OnlyCloudsAreTiled()
        {
            foreach (var style in Styles)
            {
                var skin = PlateSkins.For(style);
                var cloud = style == PlateStyle.Thought || style == PlateStyle.ThoughtPuff;
                Assert.That(skin.Tiled, Is.EqualTo(cloud), style.ToString());
                if (cloud) Assert.That(skin.TileSize, Is.GreaterThan(0));
            }
        }

        [Test]
        public void TheTextNeverTouchesTheCornerOrnaments()
        {
            // the ornaments end 25 px from the plate edge (docs: heritage-ui-for-unity.md)
            foreach (var style in Styles) Assert.That(PlateSkins.For(style).PadX, Is.GreaterThanOrEqualTo(30), style.ToString());
        }

        [Test]
        public void EverySpriteTheCodeNamesExistsAsAPng()
        {
            var dir = Project.Path_("Assets", "Resources", "Heritage");
            if (!Directory.Exists(dir)) Assert.Ignore("run `task assets:unity` first");
            var existing = Directory.GetFiles(dir, "*.png").Select(Path.GetFileNameWithoutExtension).ToHashSet();

            // sprites named by the skins ...
            var named = Styles.Select(PlateSkins.For).SelectMany(s => new[] { s.Sprite, s.TagSprite }).ToHashSet();
            // ... and every literal sprite name the components pass to Art.Sprite / Ui.Icon / Ui.Sliced / PressButton.On
            var components = Directory.GetFiles(Project.Path_("Assets", "Scripts", "Prototype"), "*.cs", SearchOption.AllDirectories);
            var literal = new Regex(@"(?:Art\.Sprite|Ui\.Sliced|Ui\.Icon)\([^;]*?""([a-z][a-z0-9-]*)""");
            foreach (var file in components)
                foreach (Match m in literal.Matches(File.ReadAllText(file)))
                    named.Add(m.Groups[1].Value);
            foreach (var file in components)
                foreach (Match m in Regex.Matches(File.ReadAllText(file), @"""(btn-pressed|key-pressed|btn-chosen)"""))
                    named.Add(m.Groups[1].Value);

            var missing = named.Where(n => !existing.Contains(n)).ToList();
            Assert.That(missing, Is.Empty, "sprites used by the code but absent from Assets/Resources/Heritage");
        }

        [Test]
        public void TheBrandArtExists()
        {
            // the loading-screen logo and the launcher icon layers (task assets:brand)
            foreach (var path in new[]
            {
                Project.Path_("Assets", "Resources", "Brand", "logo.png"),
                Project.Path_("Assets", "Art", "Brand", "icon-bg.png"), Project.Path_("Assets", "Art", "Brand", "icon-fg.png"),
                Project.Path_("Assets", "Art", "Brand", "icon-round.png"), Project.Path_("Assets", "Art", "Brand", "icon-legacy.png"),
            })
                Assert.That(File.Exists(path), Is.True, path + " (run `task assets:brand`)");
        }
    }
}

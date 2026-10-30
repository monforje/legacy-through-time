using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    /// The two UI kits (Resources/Heritage from SVG, Resources/Heritage3D from Blender) replace each other one to one:
    /// Art.BlenderKit switches them without touching any layout.
    public class UiKitTests
    {
        static string Dir(string kit) => Project.Path_("Assets", "Resources", kit);

        /// name -> `left bottom right top sliced|tiled fill` of slices.txt
        static Dictionary<string, string[]> Slices(string kit) =>
            File.ReadAllLines(Path.Combine(Dir(kit), "slices.txt"))
                .Select(l => l.Trim().Split(' ')).Where(p => p.Length >= 7)
                .ToDictionary(p => p[0], p => p.Skip(1).ToArray());

        static void RequireBoth()
        {
            if (!File.Exists(Path.Combine(Dir("Heritage"), "slices.txt"))) Assert.Ignore("run `task assets:unity` first");
            if (!File.Exists(Path.Combine(Dir("Heritage3D"), "slices.txt"))) Assert.Ignore("run `task assets:blender` first");
        }

        [Test]
        public void TheBlenderKitFillsTheSameCentresAndTilesTheCloudsAsTheSvgKit()
        {
            RequireBoth();
            var svg = Slices("Heritage");
            foreach (var (name, b) in Slices("Heritage3D"))
            {
                if (!svg.TryGetValue(name, out var a)) continue;
                Assert.That(b[5], Is.EqualTo(a[5]), name + ": fill centre");
            }
            // a pattern may need a tile of its own, but the layout rounds the clouds to whole tiles of these borders
            foreach (var name in new[] { "thought", "thought-cloud", "info" })
                Assert.That(Slices("Heritage3D")[name].Take(4), Is.EqualTo(svg[name].Take(4)), name + ": borders");
        }

        [Test]
        public void EverySlicedSpriteOfTheBlenderKitHasItsBorders()
        {
            RequireBoth();
            var svg = Slices("Heritage");
            var blender = Slices("Heritage3D");
            var pngs = Directory.GetFiles(Dir("Heritage3D"), "*.png").Select(Path.GetFileNameWithoutExtension);
            var missing = pngs.Where(n => svg.ContainsKey(n) && !blender.ContainsKey(n)).ToList();
            Assert.That(missing, Is.Empty, "9-sliced in the SVG kit, without borders in the Blender kit");
        }

        [Test]
        public void TheHornOfASpeechPlateIsOnTheSideOppositeToItsNameTag()
        {
            RequireBoth();
            foreach (var style in new[] { PlateStyle.SpeechLeft, PlateStyle.SpeechRight })
            {
                var skin = PlateSkins.For(style);
                foreach (var kit in new[] { "Heritage", "Heritage3D" })
                {
                    // the horn lives in the wide, unstretched border of the sprite: `left bottom right top`
                    var b = Slices(kit)[skin.Sprite].Take(4).Select(int.Parse).ToArray();
                    var horn = b[2] > b[0] ? TailSide.Right : TailSide.Left;
                    Assert.That(horn, Is.EqualTo(skin.Tail), $"{style}: {kit}/{skin.Sprite} has its horn on the {horn}");
                    var tag = skin.TagSide == TagSide.Left ? TailSide.Left : skin.TagSide == TagSide.Right ? TailSide.Right : TailSide.None;
                    Assert.That(tag, Is.Not.EqualTo(horn), $"{style}: the name tag and the horn are on the same side");
                }
            }
        }

        /// A story kit (`sa-btn` for `btn`, Art.StoryKit) is drawn into the layout of the main kit: the same size of the
        /// borders, the same fill and tiling, or a plate would grow differently in that story.
        [TestCase("sa-")]
        public void AStoryKitHasTheBordersOfTheSpritesItReplaces(string prefix)
        {
            RequireBoth();
            var all = Slices("Heritage3D");
            var themed = all.Where(kv => kv.Key.StartsWith(prefix)).ToList();
            Assert.That(themed, Is.Not.Empty);
            foreach (var (name, b) in themed)
            {
                var plain = name.Substring(prefix.Length);
                Assert.That(all.ContainsKey(plain), $"{name}: there is no {plain} to replace");
                Assert.That(b, Is.EqualTo(all[plain]), $"{name}: borders, tiling and fill of {plain}");
                Assert.That(File.Exists(Path.Combine(Dir("Heritage3D"), name + ".png")), name);
            }
        }

        [Test]
        public void TheCloudsTileAsTheirSkinsSay()
        {
            RequireBoth();
            // the layout rounds a cloud to whole tiles (PlateLayout.RoundUpToTile): both kits must tile the same way
            foreach (var name in new[] { "thought", "thought-cloud", "info" })
            {
                var b = Slices("Heritage3D")[name];
                Assert.That(b[4], Is.EqualTo("tiled"), name);
            }
        }
    }
}

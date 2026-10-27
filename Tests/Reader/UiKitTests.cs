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
        public void TheBlenderKitKeepsTheBordersOfTheSvgKit()
        {
            RequireBoth();
            var svg = Slices("Heritage");
            foreach (var (name, b) in Slices("Heritage3D"))
            {
                if (!svg.TryGetValue(name, out var a)) continue;
                Assert.That(b.Take(4), Is.EqualTo(a.Take(4)), name + ": borders");
                Assert.That(b[5], Is.EqualTo(a[5]), name + ": fill centre");
            }
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

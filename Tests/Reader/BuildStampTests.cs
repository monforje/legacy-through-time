using System;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    public class BuildStampTests
    {
        [Test]
        public void AStampIsTheTimeLikeTheReleaseTags()
        {
            Assert.That(BuildStamp.Of(new DateTime(2026, 10, 2, 15, 4, 59)), Is.EqualTo("2026-10-02-1504"));
            Assert.That(BuildStamp.IsStamp("2026-10-02-0214"), Is.True);
            Assert.That(BuildStamp.IsStamp("1.0"), Is.False);
            Assert.That(BuildStamp.IsStamp(null), Is.False);
        }

        [TestCase("2026-10-02-1504", "2026-10-02-0214", true)]
        [TestCase("2026-10-02-0214", "2026-10-02-0214", false)]
        [TestCase("2026-10-01-2359", "2026-10-02-0214", false)]
        [TestCase("2026-10-02-0214", "1.0", true)]       // builds from before the stamps are older than any release
        [TestCase("v1", "2026-10-02-0214", false)]       // a tag that is not a stamp is never offered
        [TestCase(null, "1.0", false)]
        public void OnlyANewerStampIsOffered(string release, string current, bool newer) =>
            Assert.That(BuildStamp.IsNewer(release, current), Is.EqualTo(newer));

        [Test]
        public void TheVersionCodeGrowsAndFitsAnInt()
        {
            var a = BuildStamp.VersionCode(new DateTime(2026, 10, 2, 2, 14, 0));
            var b = BuildStamp.VersionCode(new DateTime(2026, 10, 2, 2, 15, 0));
            Assert.That(b, Is.GreaterThan(a));
            Assert.That(BuildStamp.VersionCode(new DateTime(2100, 1, 1)), Is.LessThan(int.MaxValue));
        }
    }
}

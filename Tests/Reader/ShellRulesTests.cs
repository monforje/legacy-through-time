using System;
using System.Linq;
using NUnit.Framework;

namespace LegacyThroughTime.Prototype.Tests
{
    /// The rules behind the shell screens: the timers of the rewards and the wheel of fortune.
    public class ShellRulesTests
    {
        [Test]
        public void ATimerCountsDownToZeroAndNeverBelow()
        {
            var taken = 1_000_000L;
            Assert.That(Rewards.Remaining(taken, 1800, taken), Is.EqualTo(1800));
            Assert.That(Rewards.Remaining(taken, 1800, taken + 8_000), Is.EqualTo(1792));
            Assert.That(Rewards.Remaining(taken, 1800, taken + 1_800_000), Is.EqualTo(0));
            Assert.That(Rewards.Remaining(taken, 1800, taken + 9_999_999), Is.EqualTo(0));
        }

        [Test]
        public void ARewardNeverTakenIsReadyAtOnce()
        {
            Assert.That(Rewards.Remaining(0, 1800, 5_000), Is.EqualTo(0));
            Assert.That(Rewards.Remaining(1_000, 0, 5_000), Is.EqualTo(0), "no period, no timer");
        }

        [Test]
        public void TheClockIsHoursMinutesSecondsWithZeros()
        {
            Assert.That(Rewards.Clock(0), Is.EqualTo("00:00:00"));
            Assert.That(Rewards.Clock(8 * 3600 + 45), Is.EqualTo("08:00:45"));
            Assert.That(Rewards.Clock(2 * 3600 + 59 * 60 + 26), Is.EqualTo("02:59:26"));
            Assert.That(Rewards.Clock(-5), Is.EqualTo("00:00:00"));
        }

        [Test]
        public void TheDailyRewardWaitsForMidnight()
        {
            Assert.That(Rewards.UntilMidnight(new DateTime(2026, 10, 30, 23, 59, 0)), Is.EqualTo(60));
            Assert.That(Rewards.UntilMidnight(new DateTime(2026, 10, 30, 0, 0, 0)), Is.EqualTo(24 * 3600));
        }

        [Test]
        public void EveryRewardCardHasATitleAndAPayout()
        {
            Assert.That(Rewards.All, Has.Length.EqualTo(4));
            Assert.That(Rewards.All.Select(k => k.Id).Distinct().Count(), Is.EqualTo(4));
            Assert.That(Rewards.All.All(k => k.Title.Length > 0 && k.Gems + k.Energy > 0), Is.True);
            Assert.That(Rewards.Find("ad").Seconds, Is.EqualTo(30 * 60));
            Assert.That(Rewards.Find("energy").NeedsTap, Is.False, "the free energy is given by itself");
        }

        [Test]
        public void TheWheelHasTenSectorsAndTheChancesAddUpToAHundred()
        {
            Assert.That(Wheel.Prizes, Has.Length.EqualTo(Wheel.Sectors));
            Assert.That(Wheel.Prizes.Sum(p => p.Weight), Is.EqualTo(100).Within(.5));
            Assert.That(Wheel.Prizes.Select(p => p.Id).Distinct().Count(), Is.EqualTo(Wheel.Sectors));
        }

        [Test]
        public void ThePickFollowsTheWeightsAtTheEdges()
        {
            Assert.That(Wheel.Pick(0), Is.EqualTo(0));
            Assert.That(Wheel.Pick(.9999999), Is.EqualTo(Wheel.Sectors - 1));
            Assert.That(Wheel.Pick(-3), Is.EqualTo(0), "out of range is clamped");
            Assert.That(Wheel.Pick(7), Is.EqualTo(Wheel.Sectors - 1));
            var total = Wheel.Prizes.Sum(p => p.Weight);
            Assert.That(Wheel.Pick(Wheel.Prizes[0].Weight / total + 1e-6), Is.EqualTo(1), "just past the first share is the second prize");
        }

        [Test]
        public void AThousandSpinsLandWhereTheChancesSay()
        {
            var counts = new int[Wheel.Sectors];
            var rng = new Random(7);
            for (var i = 0; i < 100_000; i++) counts[Wheel.Pick(rng.NextDouble())]++;
            var total = Wheel.Prizes.Sum(p => p.Weight);
            for (var i = 0; i < Wheel.Sectors; i++)
                Assert.That(counts[i] / 100_000.0 * 100, Is.EqualTo(Wheel.Prizes[i].Weight / total * 100).Within(.5), Wheel.Prizes[i].Id);
        }

        [Test]
        public void TheDiscStopsWithTheChosenSectorAtTheTop()
        {
            Assert.That(Wheel.Angle(0, 4), Is.EqualTo(1440f));
            Assert.That(Wheel.Angle(4, 4), Is.EqualTo(1440f - 4 * 36f));
            Assert.That((Wheel.Angle(4, 4) % 360 + 360) % 360, Is.EqualTo(216f), "the same 216 degrees the mockup stopped at");
        }
    }
}

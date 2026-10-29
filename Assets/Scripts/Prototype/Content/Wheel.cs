using System;
using System.Linq;

namespace LegacyThroughTime.Prototype
{
    /// The wheel of fortune: ten prizes with weights (the chances written under them), the pick of a prize and the
    /// turn of the disc that ends on it. Plain data and rules without Unity: tested by `task test:reader`.
    static class Wheel
    {
        public sealed class Prize
        {
            public string Id, Label, Pic;
            public int Amount;
            public double Weight;      // in percent: the chance shown under the prize
            public string Kind;        // "coin", "gem", "energy", "gift": what the sector pays out
        }

        public static readonly Prize[] Prizes =
        {
            new Prize { Id = "coin-50", Label = "50 монет", Pic = "coin", Amount = 50, Weight = 29.59, Kind = "coin" },
            new Prize { Id = "energy-1", Label = "1 энергия", Pic = "energy", Amount = 1, Weight = 4.3, Kind = "energy" },
            new Prize { Id = "gem-3", Label = "3 алмаза", Pic = "gem", Amount = 3, Weight = 19, Kind = "gem" },
            new Prize { Id = "coin-100", Label = "100 монет", Pic = "coin", Amount = 100, Weight = 19, Kind = "coin" },
            new Prize { Id = "gift", Label = "Подарок", Pic = "gift", Amount = 0, Weight = 19, Kind = "gift" },
            new Prize { Id = "gem-10", Label = "10 алмазов", Pic = "gem", Amount = 10, Weight = 4.5, Kind = "gem" },
            new Prize { Id = "gem-25", Label = "25 алмазов", Pic = "gem", Amount = 25, Weight = 2.3, Kind = "gem" },
            new Prize { Id = "energy-3", Label = "3 энергии", Pic = "energy", Amount = 3, Weight = 1.1, Kind = "energy" },
            new Prize { Id = "gem-50", Label = "50 алмазов", Pic = "gem", Amount = 50, Weight = 0.76, Kind = "gem" },
            new Prize { Id = "gem-150", Label = "150 алмазов", Pic = "gem", Amount = 150, Weight = 0.45, Kind = "gem" },
        };

        public const int Sectors = 10;
        public const float SectorDegrees = 360f / Sectors;

        /// The sector for a random number in [0, 1): the weights are shares of their sum.
        public static int Pick(double r)
        {
            var total = Prizes.Sum(p => p.Weight);
            var x = Math.Min(Math.Max(r, 0), 0.9999999999) * total;
            for (var i = 0; i < Prizes.Length; i++)
            {
                x -= Prizes[i].Weight;
                if (x < 0) return i;
            }
            return Prizes.Length - 1;
        }

        /// The disc turns this many degrees clockwise (counted as `-angle` on the screen): whole `turns` and then the chosen
        /// sector under the pointer at the top. The sector 0 is centred at the top when the disc is at rest.
        public static float Angle(int sector, int turns) => turns * 360f - sector * SectorDegrees;
    }
}

using System;

namespace LegacyThroughTime.Prototype
{
    /// What the shop offers: packs of coins sold through Google Play (the ids are the product ids of the Play
    /// Console) and a free gift once a day. Plain data and rules: tested by `task test:reader`.
    static class ShopCatalog
    {
        public sealed class Pack
        {
            public string Id;          // product id in Google Play (consumable)
            public string Title;
            public int Coins;
            public string Icon;        // sprite of the kit
        }

        public static readonly Pack[] Packs =
        {
            new Pack { Id = "coins_100", Title = "Горсть монет", Coins = 100, Icon = "coins-s" },
            new Pack { Id = "coins_300", Title = "Кошель купца", Coins = 300, Icon = "coins-m" },
            new Pack { Id = "coins_800", Title = "Казна хана", Coins = 800, Icon = "coins-l" },
        };

        /// An offer of the shell shop (templates/shell/shop.html): gems and energy, one starter set. `Placeholder` is the
        /// price shown until the store has the real one (editor, no connection).
        public sealed class Offer
        {
            public string Id, Title, Section, Pic, Placeholder;
            public int Gems, Energy;
            public bool Best;       // "Лучшая выгода" of its section
        }

        public const string SectionStarter = "starter", SectionEnergy = "energy", SectionGems = "gems";

        public static readonly Offer[] Offers =
        {
            new Offer { Id = "starter", Title = "Для начинающих", Section = SectionStarter, Pic = "gift", Gems = 100, Energy = 5, Placeholder = "6,99 $" },
            new Offer { Id = "energy_30", Title = "Лучшая выгода", Section = SectionEnergy, Pic = "energy-pack-l", Energy = 30, Best = true, Placeholder = "19,99 $" },
            new Offer { Id = "energy_15", Title = "15 энергии", Section = SectionEnergy, Pic = "energy-pack-s", Energy = 15, Placeholder = "10,99 $" },
            new Offer { Id = "energy_5", Title = "5 энергии", Section = SectionEnergy, Pic = "energy-pack-s", Energy = 5, Placeholder = "3,99 $" },
            new Offer { Id = "gems_270", Title = "Лучшая выгода", Section = SectionGems, Pic = "gem-pack-l", Gems = 270, Best = true, Placeholder = "19,99 $" },
            new Offer { Id = "gems_130", Title = "130 алмазов", Section = SectionGems, Pic = "gem-pack-s", Gems = 130, Placeholder = "9,99 $" },
            new Offer { Id = "gems_60", Title = "60 алмазов", Section = SectionGems, Pic = "gem-pack-s", Gems = 60, Placeholder = "4,99 $" },
        };

        /// "алмаз", "алмаза", "алмазов" by the number.
        public static string Plural(int n, string one, string few, string many)
        {
            var d = Math.Abs(n) % 100;
            if (d >= 11 && d <= 14) return many;
            switch (d % 10) { case 1: return one; case 2: case 3: case 4: return few; default: return many; }
        }

        /// "100 алмазов, 5 энергии": what an offer pays out, for the toast.
        public static string Describe(Offer o)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (o.Gems > 0) parts.Add(o.Gems + " " + Plural(o.Gems, "алмаз", "алмаза", "алмазов"));
            if (o.Energy > 0) parts.Add(o.Energy + " " + Plural(o.Energy, "энергия", "энергии", "энергии"));
            return string.Join(", ", parts);
        }

        public static Offer FindOffer(string id) => Array.Find(Offers, o => o.Id == id);

        public static Pack Find(string id) => Array.Find(Packs, p => p.Id == id);

        /// Coins of the daily gift, and the coins a new reader starts with (the story's `VAR gems`).
        public const int DailyCoins = 15, StartCoins = 20;

        /// The gift is there when it was last taken on another (earlier) day, or never; `lastDay` is yyyy-MM-dd.
        public static bool GiftReady(string lastDay, DateTime now) =>
            string.IsNullOrEmpty(lastDay) || string.CompareOrdinal(lastDay, Day(now)) < 0;

        public static string Day(DateTime now) => now.ToString("yyyy-MM-dd");

        /// Time until the next gift: the next local midnight.
        public static TimeSpan UntilNextGift(DateTime now) => now.Date.AddDays(1) - now;
    }
}

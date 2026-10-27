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

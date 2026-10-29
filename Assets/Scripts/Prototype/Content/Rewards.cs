using System;

namespace LegacyThroughTime.Prototype
{
    /// The free rewards of the shell (docs/brainstorm/shell-ux.md): a timer, a reward that is ready when it runs out.
    /// Plain data and rules without Unity: tested by `task test:reader`.
    static class Rewards
    {
        public sealed class Kind
        {
            public string Id;
            public string Title;
            public string Text;
            public string Pic;           // picture of the shell kit
            public int Seconds;          // the period; 0 - the reward has no timer of its own (a chapter)
            public int Gems, Energy;     // what it gives
            public bool NeedsTap;        // false: given by itself when the timer ends (the free energy)
        }

        /// The four cards of the "Награды" popup, in the order of the screen.
        public static readonly Kind[] All =
        {
            new Kind { Id = "energy", Title = "Бесплатная энергия", Text = "Получайте порцию энергии каждые 3 часа", Pic = "energy", Seconds = 3 * 3600, Energy = 3, NeedsTap = false },
            new Kind { Id = "ad", Title = "Бесплатные алмазы", Text = "Получайте 3 алмаза каждые полчаса", Pic = "gem", Seconds = 30 * 60, Gems = 3, NeedsTap = true },
            new Kind { Id = "daily", Title = "Ежедневный вход", Text = "Заходите каждый день и получайте больше наград: до 10 алмазов в день", Pic = "calendar", Seconds = 0, Gems = 10, NeedsTap = true },
            new Kind { Id = "chapter", Title = "Награда за главу", Text = "Дочитайте любую главу до конца и заберите 5 алмазов", Pic = "star", Seconds = 0, Gems = 5, NeedsTap = true },
        };

        public static Kind Find(string id) => Array.Find(All, k => k.Id == id);

        /// Seconds left until the reward is ready: `lastAtMs` is when it was taken (unix ms, 0 - never), the period in seconds.
        public static int Remaining(long lastAtMs, int periodSeconds, long nowMs)
        {
            if (lastAtMs <= 0 || periodSeconds <= 0) return 0;
            var left = (lastAtMs + periodSeconds * 1000L - nowMs + 999) / 1000;
            return (int)Math.Max(0, Math.Min(periodSeconds, left));
        }

        /// The time of the timer badge: HH:MM:SS (more than 99 hours is not a thing here).
        public static string Clock(int seconds)
        {
            seconds = Math.Max(0, seconds);
            return $"{seconds / 3600:00}:{seconds % 3600 / 60:00}:{seconds % 60:00}";
        }

        /// Time until the next local midnight in seconds: the timer of the daily reward.
        public static int UntilMidnight(DateTime now) => (int)Math.Ceiling(ShopCatalog.UntilNextGift(now).TotalSeconds);
    }
}

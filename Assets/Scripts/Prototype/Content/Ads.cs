namespace LegacyThroughTime.Prototype
{
    /// The ad rules of the financial model (`pitch/Наследие_финмодель_v5.xlsx`, "Допущения", п. 3): an interstitial at most once in
    /// 30 minutes and never for a reader who has paid, a clip for a reward whenever the reader asks for it. No ad provider is
    /// connected yet, so the screens show a stub (`App/Ads.cs`). Plain data and rules without Unity: tested by `task test:reader`.
    static class AdRules
    {
        public const int InterstitialGapSeconds = 30 * 60;
        public const int RewardEnergy = 3;          // a clip in the chat: this much energy
        public const int ClipSeconds = 5;           // how long the stub's "clip" runs

        /// An interstitial is due when the reader has never paid and the last one was shown at least the gap ago.
        public static bool InterstitialDue(long lastAtMs, long nowMs, bool paid) =>
            !paid && (lastAtMs <= 0 || nowMs - lastAtMs >= InterstitialGapSeconds * 1000L);
    }
}

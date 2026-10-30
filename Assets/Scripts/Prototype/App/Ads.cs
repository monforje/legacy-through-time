using System;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The ads of the app. There is no provider yet: both calls show the stub dialog (`AdStub`) where a clip will play. The
    /// rules (the gap, who is not shown) are `AdRules`.
    static class Ads
    {
        const string LastKey = "ads:interstitial:at", PaidKey = "ads:paid";

        /// A reader who has bought anything is not shown interstitials.
        public static bool Paid => PlayerPrefs.GetInt(PaidKey, 0) == 1;

        public static void MarkPaid() { PlayerPrefs.SetInt(PaidKey, 1); PlayerPrefs.Save(); }

        static long LastAt => long.TryParse(PlayerPrefs.GetString(LastKey, ""), out var at) ? at : 0;

        /// An interstitial when one is due; true when it was shown (`done` runs when it is closed).
        public static bool Interstitial(Transform parent, IAnimator anim, Action done = null)
        {
            var now = Progress.Now();
            if (!AdRules.InterstitialDue(LastAt, now, Paid)) return false;
            PlayerPrefs.SetString(LastKey, now.ToString()); PlayerPrefs.Save();
            AdStub.Show(parent, anim, "Реклама", false, done);
            return true;
        }

        /// A clip the reader asked for: `reward` runs only when the clip is watched to the end.
        public static void Rewarded(Transform parent, IAnimator anim, Action reward) =>
            AdStub.Show(parent, anim, "Ролик за награду", true, reward);
    }
}

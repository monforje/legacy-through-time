using System;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// What the pills of the shell show and what the shop, the rewards and the wheel pay out: gems are the purse of
    /// the stories (`Wallet.Coins`), energy is the talk's (`ChatStore.Energy`). One place for the screens to read and
    /// change them, and one event for the pills to listen to.
    static class ShellPurse
    {
        public static event Action Changed;

        public static int Gems => Wallet.Coins;
        public static int Energy => ChatStore.Energy.value;

        public static void AddGems(int amount) { if (amount > 0) Wallet.Add(amount); Changed?.Invoke(); }

        public static void AddEnergy(int amount)
        {
            if (amount <= 0) return;
            var energy = ChatStore.Energy;
            energy.Add(amount);
            ChatStore.Write(energy);
            Changed?.Invoke();
        }

        /// Pays out an offer of the shop (a purchase confirmed by the store, or the editor's stand-in).
        public static void Grant(ShopCatalog.Offer offer) { AddGems(offer.Gems); AddEnergy(offer.Energy); }

        // ------------------------------------------------------------ rewards: when each was last taken
        static string AtKey(string id) => "reward:" + id + ":at";

        public static long TakenAt(string id) => long.TryParse(PlayerPrefs.GetString(AtKey(id), ""), out var at) ? at : 0;

        public static int Remaining(Rewards.Kind kind, DateTime now)
        {
            if (kind.Id == "daily") return Wallet.GiftReady ? 0 : Rewards.UntilMidnight(now);
            return Rewards.Remaining(TakenAt(kind.Id), kind.Seconds, Progress.Now());
        }

        /// Takes a reward that is ready: pays it out and starts its timer. False when it is not ready yet.
        public static bool Take(Rewards.Kind kind, DateTime now)
        {
            if (Remaining(kind, now) > 0) return false;
            if (kind.Id == "daily") Wallet.TakeGift();
            PlayerPrefs.SetString(AtKey(kind.Id), Progress.Now().ToString());
            PlayerPrefs.Save();
            AddGems(kind.Gems);
            AddEnergy(kind.Energy);
            return true;
        }
    }
}

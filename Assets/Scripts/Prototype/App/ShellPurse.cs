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

        /// Starts the timer of a reward without paying it out (the first look at the popup).
        public static void Start(Rewards.Kind kind) { PlayerPrefs.SetString(AtKey(kind.Id), Progress.Now().ToString()); PlayerPrefs.Save(); }

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

        // ------------------------------------------------------------ the wheel: spins and prizes
        const string SpinsKey = "wheel:spins", SpinDayKey = "wheel:day";

        /// Spins left: one free spin a day, more may be bought.
        public static int Spins
        {
            get
            {
                var today = ShopCatalog.Day(DateTime.Now);
                if (PlayerPrefs.GetString(SpinDayKey, "") != today)
                {
                    PlayerPrefs.SetString(SpinDayKey, today);
                    PlayerPrefs.SetInt(SpinsKey, Math.Max(1, PlayerPrefs.GetInt(SpinsKey, 1)));
                    PlayerPrefs.Save();
                }
                return PlayerPrefs.GetInt(SpinsKey, 1);
            }
        }

        public static bool TrySpin()
        {
            var left = Spins;
            if (left <= 0) return false;
            PlayerPrefs.SetInt(SpinsKey, left - 1);
            PlayerPrefs.Save();
            return true;
        }

        public static void AddSpins(int n) { PlayerPrefs.SetInt(SpinsKey, Spins + Math.Max(0, n)); PlayerPrefs.Save(); }

        /// Pays a prize out. The wheel's coins are a stand-in until there are coins apart from gems: ten of them make a gem;
        /// a gift is ten gems.
        public static string Pay(Wheel.Prize prize)
        {
            switch (prize.Kind)
            {
                case "energy": AddEnergy(prize.Amount); return prize.Label;
                case "gem": AddGems(prize.Amount); return prize.Label;
                case "coin": AddGems(prize.Amount / 10); return prize.Label + " = " + prize.Amount / 10 + " алмазов";
                default: AddGems(10); return "Подарок: 10 алмазов";
            }
        }
    }
}

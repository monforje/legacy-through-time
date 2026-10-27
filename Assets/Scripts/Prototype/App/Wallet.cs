using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// The reader's coins, one purse for every story (PlayerPrefs). A story keeps its own `gems` variable: the player
    /// puts the purse into it when a chapter opens and takes it back after every step, so that coins bought in the
    /// shop or got as a gift can be spent in any story, and coins spent there are gone from the purse. Every change
    /// keeps its time: of two purses (the phone's and the cloud's) the later one wins.
    static class Wallet
    {
        const string Key = "wallet:coins", AtKey = "wallet:at", GiftKey = "wallet:gift-day";

        public static event Action<int> Changed;

        public static int Coins
        {
            get => PlayerPrefs.GetInt(Key, ShopCatalog.StartCoins);
            set
            {
                value = Mathf.Max(0, value);
                if (value == Coins) return;
                Restore(value, Progress.Now());
            }
        }

        /// When the purse last changed (unix ms); 0 when it never did since the cloud came.
        public static long ChangedAt => long.TryParse(PlayerPrefs.GetString(AtKey, ""), out var at) ? at : 0;

        /// Puts a purse in place with the time it had (from the cloud), or now (Coins).
        public static void Restore(int coins, long at)
        {
            PlayerPrefs.SetInt(Key, coins);
            PlayerPrefs.SetString(AtKey, at.ToString());
            PlayerPrefs.Save();
            Changed?.Invoke(coins);
        }

        public static void Add(int coins) => Coins += coins;

        // ------------------------------------------------------------ the daily gift
        public static bool GiftReady => ShopCatalog.GiftReady(GiftDay, DateTime.Now);

        /// The day the gift was last taken ("yyyy-MM-dd"), "" before the first one.
        public static string GiftDay
        {
            get => PlayerPrefs.GetString(GiftKey, "");
            set { PlayerPrefs.SetString(GiftKey, value ?? ""); PlayerPrefs.Save(); }
        }

        /// Takes today's gift: false when it was already taken today.
        public static bool TakeGift()
        {
            if (!GiftReady) return false;
            PlayerPrefs.SetString(GiftKey, ShopCatalog.Day(DateTime.Now));
            Add(ShopCatalog.DailyCoins);
            return true;
        }
    }

    /// The words of the library the reader has met (PlayerPrefs, one line each): a note shown in a story opens its word.
    static class Library
    {
        const string Key = "library:open";

        public static event Action Changed;

        public static bool IsOpen(string name) => Array.IndexOf(Open(), name) >= 0;

        public static string[] Open() => PlayerPrefs.GetString(Key, "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        /// Opens the word of a note (`Калфак — …`).
        public static void Unlock(string note)
        {
            var term = Glossary.Parse(note);
            if (term == null || IsOpen(term.Name)) return;
            var all = PlayerPrefs.GetString(Key, "");
            PlayerPrefs.SetString(Key, all.Length == 0 ? term.Name : all + "\n" + term.Name);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        /// Puts the words met on any phone in place (from the cloud).
        public static void Restore(IEnumerable<string> words)
        {
            PlayerPrefs.SetString(Key, string.Join("\n", words));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}

using System;

namespace LegacyThroughTime.Prototype
{
    /// Coin purse of the screens demo. (The story keeps its coins in the ink variable `gems`.)
    sealed class Bank
    {
        public const int Start = 20;
        public int Coins { get; private set; } = Start;
        public event Action Changed;

        public void Add(int delta) => Set(Coins + delta);
        public void Set(int value) { Coins = value; Changed?.Invoke(); }

        /// False (and nothing spent) when the purse is too thin.
        public bool TrySpend(int cost)
        {
            if (cost > Coins) return false;
            Set(Coins - cost);
            return true;
        }
    }
}

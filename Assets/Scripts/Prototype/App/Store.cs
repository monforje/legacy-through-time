using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LegacyThroughTime.Prototype
{
    /// Coins for money through Google Play (Unity IAP 5): the packs of ShopCatalog, consumables. The two-step flow:
    /// a pending order credits the purse, the purse is saved, and only then the order is confirmed - an unconfirmed
    /// one comes back on the next launch, a confirmed one that was never credited would be lost. Transactions already
    /// credited are remembered, so an order delivered twice is credited once. When the store cannot connect (no Google
    /// Play, no network, products not set up in the Play Console yet) the shop shows the packs as unavailable.
    static class Store
    {
        public enum State { Connecting, Ready, Unavailable }

        const string LedgerKey = "store:credited";
        const int LedgerSize = Progress.CreditedKept;

        public static State Status { get; private set; } = State.Connecting;
        /// The status or the prices changed: the shop redraws its buttons.
        public static event Action Changed;
        /// A purchase ended: (product id, success, a line for the reader).
        public static event Action<string, bool, string> Finished;

        static StoreController store;
        static bool started;

        /// Connects once, early in the app's life (pending orders of an earlier session are credited then).
        public static async void Start()
        {
            if (started) return;
            started = true;
            try
            {
                store = UnityIAPServices.StoreController();
                // every event before Connect(): a pending order of a previous session may arrive at once
                store.OnStoreConnected += OnConnected;
                store.OnStoreDisconnected += failure => SetStatus(State.Unavailable, "store disconnected: " + failure.Message);
                store.OnProductsFetched += products => SetStatus(products.Count > 0 ? State.Ready : State.Unavailable, $"{products.Count} products");
                store.OnProductsFetchFailed += failure => SetStatus(State.Unavailable, "products: " + failure.FailureReason);
                store.OnPurchasesFetched += orders => Debug.Log("[store] purchases fetched");
                store.OnPurchasesFetchFailed += failure => Debug.LogWarning("[store] purchases: " + failure.Message);
                store.OnPurchasePending += OnPending;
                store.OnPurchaseConfirmed += OnConfirmed;
                store.OnPurchaseFailed += failed => Finished?.Invoke(IdOf(failed), false, Reason(failed.FailureReason));
                store.OnPurchaseDeferred += deferred => Finished?.Invoke(IdOf(deferred), false, "Покупка ждёт подтверждения");
                await store.Connect();
            }
            catch (Exception e)
            {
                SetStatus(State.Unavailable, "connect: " + e.Message);
            }
        }

        static void OnConnected()
        {
            var products = new List<ProductDefinition>();
            foreach (var pack in ShopCatalog.Packs) products.Add(new ProductDefinition(pack.Id, ProductType.Consumable));
            foreach (var offer in ShopCatalog.Offers) products.Add(new ProductDefinition(offer.Id, ProductType.Consumable));
            store.FetchProducts(products);
            store.FetchPurchases();
        }

        static void SetStatus(State status, string why)
        {
            Debug.Log($"[store] {status}: {why}");
            Status = status;
            Changed?.Invoke();
        }

        /// The price of a pack in the reader's currency, or null when it cannot be bought now.
        public static string Price(string id)
        {
            var product = Status == State.Ready ? store?.GetProductById(id) : null;
            return product != null && product.availableToPurchase ? product.metadata.localizedPriceString : null;
        }

        public static void Buy(string id)
        {
            var product = Status == State.Ready ? store?.GetProductById(id) : null;
            if (product == null || !product.availableToPurchase) { Finished?.Invoke(id, false, "Покупка сейчас недоступна"); return; }
            store.PurchaseProduct(product);
        }

        static void OnPending(PendingOrder order)
        {
            var id = IdOf(order);
            var pack = ShopCatalog.Find(id);
            var offer = pack == null ? ShopCatalog.FindOffer(id) : null;
            var transaction = order.Info?.TransactionID ?? "";
            if (pack == null && offer == null)
            {
                Debug.LogWarning("[store] an order of an unknown product: " + id);
                return;                                   // not confirmed: it stays with the store
            }
            if (!Credited(transaction))
            {
                if (pack != null) Wallet.Add(pack.Coins);  // saved by Wallet (PlayerPrefs.Save) ...
                else ShellPurse.Grant(offer);
                Remember(transaction);                     // ... and the ledger, before the confirmation
            }
            store.ConfirmPurchase(order);
            Finished?.Invoke(id, true, pack != null ? $"+{pack.Coins} монет. Спасибо!" : "Спасибо! Покупка зачислена");
        }

        static void OnConfirmed(Order order)
        {
            if (order is FailedOrder failed) Debug.LogWarning($"[store] confirmation failed: {failed.FailureReason} {failed.Details}");
        }

        static string IdOf(Order order)
        {
            var items = order?.CartOrdered?.Items();
            return items != null && items.Count > 0 ? items[0].Product.definition.id : null;
        }

        static string Reason(PurchaseFailureReason reason) => reason == PurchaseFailureReason.UserCancelled
            ? "Покупка отменена"
            : "Не получилось купить. Попробуйте позже";

        // ------------------------------------------------------------ the ledger of credited transactions
        /// Shared through the cloud: an order credited on one phone is not credited again on another.
        public static List<string> Ledger() => new(PlayerPrefs.GetString(LedgerKey, "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

        public static void RestoreLedger(IEnumerable<string> transactions)
        {
            PlayerPrefs.SetString(LedgerKey, string.Join("\n", transactions));
            PlayerPrefs.Save();
        }

        static bool Credited(string transaction) => transaction.Length > 0 && Ledger().Contains(transaction);

        static void Remember(string transaction)
        {
            if (transaction.Length == 0) return;
            var ledger = Ledger();
            ledger.Add(transaction);
            if (ledger.Count > LedgerSize) ledger.RemoveRange(0, ledger.Count - LedgerSize);
            PlayerPrefs.SetString(LedgerKey, string.Join("\n", ledger));
            PlayerPrefs.Save();
        }
    }
}

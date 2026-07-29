using System;
using System.Threading;
using System.Threading.Tasks;

namespace SDKPro.IAP
{
    public interface IIapService : IDisposable
    {
        IapServiceState State { get; }
        bool CanPurchase { get; }

        event Action<IapServiceState> StateChanged;
        event Action StoreConnected;
        event Action ProductsChanged;
        event Action EntitlementsChanged;
        event Action<IapFulfillmentContext> FulfillmentRequired;
        event Action<IapPurchaseResult> PurchaseCompleted;
        event Action<IapPurchaseResult> PurchaseFailed;
        event Action<IapPurchaseResult> PurchaseDeferred;
        event Action<string> ConfirmationFailed;
        event Action<string> StoreDisconnected;

        Task<IapInitializationResult> InitializeAsync(CancellationToken token = default);
        Task<IapPurchaseResult> PurchaseAsync(
            string productKey,
            string placement = null,
            CancellationToken token = default);
        Task<IapRestoreResult> RestoreAsync(CancellationToken token = default);

        IDisposable RegisterFulfillmentHandler(
            string productKey,
            Func<IapFulfillmentContext, CancellationToken, Task<IapFulfillmentResult>> handler);

        bool TryGetProduct(string productKey, out IapProductInfo product);
        string GetDisplayPrice(string productKey);
        bool IsOwned(string productKey);
        void RefreshPurchases();
        void RetryPendingFulfillments();
    }
}

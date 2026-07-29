using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace SDKPro.IAP
{
    public sealed class IapService : IIapService
    {
        private sealed class ActiveRequest
        {
            public string ProductKey;
            public string Placement;
            public TaskCompletionSource<IapPurchaseResult> Completion;
            public CancellationTokenRegistration Cancellation;
        }

        private sealed class FulfillmentRegistration : IDisposable
        {
            private readonly IapService m_Owner;
            private readonly string m_Key;
            private readonly Delegate m_Handler;

            public FulfillmentRegistration(IapService owner, string key, Delegate handler)
            {
                m_Owner = owner;
                m_Key = key;
                m_Handler = handler;
            }

            public void Dispose()
            {
                m_Owner.UnregisterFulfillmentHandler(m_Key, m_Handler);
            }
        }

        private readonly IapCatalog m_Catalog;
        private readonly bool m_UseFakeStore;
        private readonly IIapFulfillmentStore m_FulfillmentStore;
        private readonly Dictionary<string, Product> m_Products =
            new(StringComparer.Ordinal);
        private readonly HashSet<string> m_OwnedProducts =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string,
            Func<IapFulfillmentContext, CancellationToken, Task<IapFulfillmentResult>>>
            m_FulfillmentHandlers = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ActiveRequest> m_ActiveRequests =
            new(StringComparer.Ordinal);
        private readonly List<PendingOrder> m_WaitingOrders = new();
        private readonly HashSet<string> m_ProcessingOrders =
            new(StringComparer.Ordinal);
        private readonly CancellationTokenSource m_DisposeCancellation = new();

        private StoreController m_StoreController;
        private Task<IapInitializationResult> m_InitializationTask;
        private TaskCompletionSource<IapInitializationResult> m_InitializationCompletion;
        private bool m_Disposed;

        public IapServiceState State { get; private set; } =
            IapServiceState.Uninitialized;
        public bool CanPurchase =>
            State is IapServiceState.Ready or IapServiceState.ReadyWithoutEntitlements;

        public event Action<IapServiceState> StateChanged;
        public event Action StoreConnected;
        public event Action ProductsChanged;
        public event Action EntitlementsChanged;
        public event Action<IapFulfillmentContext> FulfillmentRequired;
        public event Action<IapPurchaseResult> PurchaseCompleted;
        public event Action<IapPurchaseResult> PurchaseFailed;
        public event Action<IapPurchaseResult> PurchaseDeferred;
        public event Action<string> ConfirmationFailed;
        public event Action<string> StoreDisconnected;

        public IapService(
            IapCatalog catalog,
            bool useFakeStore = false,
            IIapFulfillmentStore fulfillmentStore = null)
        {
            m_Catalog = catalog;
            m_UseFakeStore = useFakeStore;
            m_FulfillmentStore =
                fulfillmentStore ?? new PlayerPrefsIapFulfillmentStore();
        }

        public Task<IapInitializationResult> InitializeAsync(
            CancellationToken token = default)
        {
            ThrowIfDisposed();
            m_InitializationTask ??= InitializeInternalAsync();
            return m_InitializationTask.WithCancellation(token);
        }

        private async Task<IapInitializationResult> InitializeInternalAsync()
        {
            if (m_Catalog == null)
            {
                return FailInitialization("No IAP catalog is assigned.");
            }

            if (!m_Catalog.TryValidate(out string catalogError))
            {
                return FailInitialization(catalogError);
            }

            SetState(IapServiceState.Initializing);
            m_InitializationCompletion =
                new TaskCompletionSource<IapInitializationResult>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                m_StoreController = UnityIAPServices.StoreController(
                    m_UseFakeStore ? "fake" : null);
                Subscribe();
                m_StoreController.ProcessPendingOrdersOnPurchasesFetched(true);

                await m_StoreController.Connect();

                string storeName = m_UseFakeStore
                    ? "fake"
                    : DefaultStoreHelper.GetDefaultStoreName();
                m_StoreController.FetchProductsWithNoRetries(
                    m_Catalog.BuildDefinitions(storeName));
            }
            catch (Exception exception)
            {
                return FailInitialization(
                    $"IAP initialization failed: {exception.Message}");
            }

            return await m_InitializationCompletion.Task;
        }

        public async Task<IapPurchaseResult> PurchaseAsync(
            string productKey,
            string placement = null,
            CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (!CanPurchase)
            {
                return IapPurchaseResult.Unavailable(
                    productKey,
                    $"IAP is not ready. Current state: {State}.");
            }

            if (!m_Products.TryGetValue(productKey ?? string.Empty, out Product product) ||
                !product.availableToPurchase)
            {
                return IapPurchaseResult.Unavailable(
                    productKey,
                    $"Product '{productKey}' is not available from the store.");
            }

            if (!m_FulfillmentHandlers.ContainsKey(productKey))
            {
                return IapPurchaseResult.Unavailable(
                    productKey,
                    $"Product '{productKey}' has no fulfillment handler.");
            }

            if (m_ActiveRequests.ContainsKey(productKey))
            {
                return IapPurchaseResult.Unavailable(
                    productKey,
                    $"A purchase for '{productKey}' is already in progress.");
            }

            var completion = new TaskCompletionSource<IapPurchaseResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var request = new ActiveRequest
            {
                ProductKey = productKey,
                Placement = placement,
                Completion = completion
            };
            m_ActiveRequests.Add(productKey, request);

            if (token.CanBeCanceled)
            {
                request.Cancellation = token.Register(() =>
                {
                    if (m_ActiveRequests.Remove(productKey))
                    {
                        completion.TrySetResult(new IapPurchaseResult(
                            IapPurchaseStatus.Cancelled,
                            productKey,
                            null,
                            placement,
                            "Purchase wait cancelled by the caller.",
                            false,
                            new IapProductInfo(product)));
                    }
                });
            }

            try
            {
                m_StoreController.PurchaseProduct(product);
            }
            catch (Exception exception)
            {
                CompleteActiveRequest(
                    productKey,
                    new IapPurchaseResult(
                        IapPurchaseStatus.Failed,
                        productKey,
                        null,
                        placement,
                        exception.Message,
                        false,
                        new IapProductInfo(product)));
            }

            return await completion.Task;
        }

        public Task<IapRestoreResult> RestoreAsync(
            CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (m_StoreController == null)
            {
                return Task.FromResult(
                    new IapRestoreResult(false, "IAP is not initialized."));
            }

            var completion = new TaskCompletionSource<IapRestoreResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            CancellationTokenRegistration cancellation = default;
            if (token.CanBeCanceled)
            {
                cancellation = token.Register(() =>
                    completion.TrySetCanceled(token));
            }

            m_StoreController.RestoreTransactions((success, error) =>
            {
                cancellation.Dispose();
                completion.TrySetResult(new IapRestoreResult(success, error));
            });
            return completion.Task;
        }

        public IDisposable RegisterFulfillmentHandler(
            string productKey,
            Func<IapFulfillmentContext, CancellationToken, Task<IapFulfillmentResult>>
                handler)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(productKey))
            {
                throw new ArgumentException(
                    "A product key is required.",
                    nameof(productKey));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (m_FulfillmentHandlers.ContainsKey(productKey))
            {
                throw new InvalidOperationException(
                    $"A fulfillment handler is already registered for '{productKey}'.");
            }

            m_FulfillmentHandlers.Add(productKey, handler);
            RetryPendingFulfillments();
            return new FulfillmentRegistration(this, productKey, handler);
        }

        public bool TryGetProduct(string productKey, out IapProductInfo product)
        {
            if (m_Products.TryGetValue(productKey ?? string.Empty, out Product value))
            {
                product = new IapProductInfo(value);
                return true;
            }

            product = null;
            return false;
        }

        public string GetDisplayPrice(string productKey)
        {
            if (TryGetProduct(productKey, out IapProductInfo product) &&
                !string.IsNullOrWhiteSpace(product.LocalizedPriceString))
            {
                return product.LocalizedPriceString;
            }

            return m_Catalog != null &&
                   m_Catalog.TryGet(productKey, out IapProductConfig config)
                ? config.EditorFallbackPrice
                : null;
        }

        public bool IsOwned(string productKey)
        {
            return m_OwnedProducts.Contains(productKey ?? string.Empty);
        }

        public void RefreshPurchases()
        {
            ThrowIfDisposed();
            m_StoreController?.FetchPurchases();
        }

        public void RetryPendingFulfillments()
        {
            if (m_Disposed || m_WaitingOrders.Count == 0)
            {
                return;
            }

            PendingOrder[] waiting = m_WaitingOrders.ToArray();
            foreach (PendingOrder order in waiting)
            {
                ProcessPendingOrder(order);
            }
        }

        public void Dispose()
        {
            if (m_Disposed)
            {
                return;
            }

            m_Disposed = true;
            m_DisposeCancellation.Cancel();
            m_DisposeCancellation.Dispose();
            Unsubscribe();

            foreach (ActiveRequest request in m_ActiveRequests.Values)
            {
                request.Cancellation.Dispose();
                request.Completion.TrySetResult(
                    IapPurchaseResult.Unavailable(
                        request.ProductKey,
                        "IAP service was disposed."));
            }

            m_ActiveRequests.Clear();
            m_FulfillmentHandlers.Clear();
            m_WaitingOrders.Clear();
            SetState(IapServiceState.Disposed);
        }

        private void Subscribe()
        {
            m_StoreController.OnStoreConnected += HandleStoreConnected;
            m_StoreController.OnStoreDisconnected += HandleStoreDisconnected;
            m_StoreController.OnProductsFetched += HandleProductsFetched;
            m_StoreController.OnProductsFetchFailed += HandleProductsFetchFailed;
            m_StoreController.OnPurchasesFetched += HandlePurchasesFetched;
            m_StoreController.OnPurchasesFetchFailed += HandlePurchasesFetchFailed;
            m_StoreController.OnPurchasePending += HandlePurchasePending;
            m_StoreController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
            m_StoreController.OnPurchaseFailed += HandlePurchaseFailed;
            m_StoreController.OnPurchaseDeferred += HandlePurchaseDeferred;
        }

        private void Unsubscribe()
        {
            if (m_StoreController == null)
            {
                return;
            }

            m_StoreController.OnStoreConnected -= HandleStoreConnected;
            m_StoreController.OnStoreDisconnected -= HandleStoreDisconnected;
            m_StoreController.OnProductsFetched -= HandleProductsFetched;
            m_StoreController.OnProductsFetchFailed -= HandleProductsFetchFailed;
            m_StoreController.OnPurchasesFetched -= HandlePurchasesFetched;
            m_StoreController.OnPurchasesFetchFailed -= HandlePurchasesFetchFailed;
            m_StoreController.OnPurchasePending -= HandlePurchasePending;
            m_StoreController.OnPurchaseConfirmed -= HandlePurchaseConfirmed;
            m_StoreController.OnPurchaseFailed -= HandlePurchaseFailed;
            m_StoreController.OnPurchaseDeferred -= HandlePurchaseDeferred;
        }

        private void HandleStoreConnected()
        {
            StoreConnected?.Invoke();
        }

        private void HandleStoreDisconnected(
            StoreConnectionFailureDescription failure)
        {
            string error = failure?.message ?? "The store disconnected.";
            StoreDisconnected?.Invoke(error);

            if (State == IapServiceState.Initializing && m_Products.Count == 0)
            {
                FailInitialization(error);
            }
        }

        private void HandleProductsFetched(List<Product> products)
        {
            m_Products.Clear();
            foreach (Product product in products)
            {
                m_Products[product.definition.id] = product;
            }

            ProductsChanged?.Invoke();
            m_StoreController.FetchPurchases();
        }

        private void HandleProductsFetchFailed(ProductFetchFailed failure)
        {
            foreach (Product product in m_StoreController.GetProducts())
            {
                m_Products[product.definition.id] = product;
            }

            ProductsChanged?.Invoke();
            if (m_Products.Count > 0)
            {
                m_StoreController.FetchPurchases();
                return;
            }

            FailInitialization(
                $"Product fetch failed: {failure?.FailureReason}");
        }

        private void HandlePurchasesFetched(Orders orders)
        {
            RebuildOwnedProducts(orders);
            CompleteInitialization(true);
        }

        private void HandlePurchasesFetchFailed(
            PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning(
                $"IAP purchase history fetch failed: {failure?.Message}");
            CompleteInitialization(false);
        }

        private void HandlePurchasePending(PendingOrder order)
        {
            ProcessPendingOrder(order);
        }

        private async void ProcessPendingOrder(PendingOrder order)
        {
            string orderKey = BuildOrderKey(order);
            if (string.IsNullOrWhiteSpace(orderKey))
            {
                QueueWaitingOrder(order);
                Debug.LogError(
                    "IAP pending order has no transaction ID or receipt. " +
                    "It was left unconfirmed to avoid duplicate fulfillment.");
                return;
            }

            if (!m_ProcessingOrders.Add(orderKey))
            {
                return;
            }

            bool allFulfilled = true;
            try
            {
                foreach (CartItem item in order.CartOrdered.Items())
                {
                    Product product = item.Product;
                    string productKey = product.definition.id;
                    string fulfillmentKey = $"{orderKey}|{productKey}";
                    string placement = m_ActiveRequests.TryGetValue(
                        productKey,
                        out ActiveRequest active)
                        ? active.Placement
                        : null;
                    var context = new IapFulfillmentContext(
                        product,
                        order.Info.TransactionID,
                        order.Info.Receipt,
                        placement);

                    bool alreadyFulfilled =
                        m_FulfillmentStore.IsFulfilled(fulfillmentKey);
                    if (!alreadyFulfilled)
                    {
                        if (!m_FulfillmentHandlers.TryGetValue(
                                productKey,
                                out var handler))
                        {
                            allFulfilled = false;
                            QueueWaitingOrder(order);
                            FulfillmentRequired?.Invoke(context);
                            continue;
                        }

                        IapFulfillmentResult fulfillment;
                        try
                        {
                            fulfillment = await handler(
                                context,
                                m_DisposeCancellation.Token);
                        }
                        catch (Exception exception)
                        {
                            fulfillment = IapFulfillmentResult.Retry(
                                exception.Message);
                        }

                        if (!fulfillment.Succeeded)
                        {
                            allFulfilled = false;
                            QueueWaitingOrder(order);
                            CompleteActiveRequest(
                                productKey,
                                new IapPurchaseResult(
                                    IapPurchaseStatus.Failed,
                                    productKey,
                                    order.Info.TransactionID,
                                    placement,
                                    fulfillment.Error ??
                                    "Purchase fulfillment requested a retry.",
                                    false,
                                    context.Product));
                            continue;
                        }

                        m_FulfillmentStore.MarkFulfilled(fulfillmentKey);
                    }

                    var result = new IapPurchaseResult(
                        IapPurchaseStatus.Succeeded,
                        productKey,
                        order.Info.TransactionID,
                        placement,
                        null,
                        alreadyFulfilled,
                        context.Product);
                    CompleteActiveRequest(productKey, result);
                    PurchaseCompleted?.Invoke(result);
                }

                if (allFulfilled)
                {
                    RemoveWaitingOrder(orderKey);
                    m_StoreController.ConfirmPurchase(order);
                }
            }
            finally
            {
                m_ProcessingOrders.Remove(orderKey);
            }
        }

        private void HandlePurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failed)
            {
                ConfirmationFailed?.Invoke(
                    $"Purchase confirmation failed: {failed.FailureReason}. " +
                    failed.Details);
                return;
            }

            if (order is ConfirmedOrder)
            {
                foreach (CartItem item in order.CartOrdered.Items())
                {
                    Product product = item.Product;
                    if (product.definition.type != ProductType.Consumable)
                    {
                        m_OwnedProducts.Add(product.definition.id);
                    }
                }

                EntitlementsChanged?.Invoke();
            }
        }

        private void HandlePurchaseFailed(FailedOrder order)
        {
            foreach (CartItem item in order.CartOrdered.Items())
            {
                string productKey = item.Product.definition.id;
                IapPurchaseStatus status =
                    order.FailureReason == PurchaseFailureReason.UserCancelled
                        ? IapPurchaseStatus.Cancelled
                        : IapPurchaseStatus.Failed;
                var result = new IapPurchaseResult(
                    status,
                    productKey,
                    order.Info?.TransactionID,
                    GetPlacement(productKey),
                    $"{order.FailureReason}: {order.Details}",
                    false,
                    new IapProductInfo(item.Product));
                CompleteActiveRequest(productKey, result);
                PurchaseFailed?.Invoke(result);
            }
        }

        private void HandlePurchaseDeferred(DeferredOrder order)
        {
            foreach (CartItem item in order.CartOrdered.Items())
            {
                string productKey = item.Product.definition.id;
                var result = new IapPurchaseResult(
                    IapPurchaseStatus.Deferred,
                    productKey,
                    order.Info?.TransactionID,
                    GetPlacement(productKey),
                    "The store deferred this purchase.",
                    false,
                    new IapProductInfo(item.Product));
                CompleteActiveRequest(productKey, result);
                PurchaseDeferred?.Invoke(result);
            }
        }

        private void RebuildOwnedProducts(Orders orders)
        {
            m_OwnedProducts.Clear();
            if (orders?.ConfirmedOrders != null)
            {
                foreach (ConfirmedOrder order in orders.ConfirmedOrders)
                {
                    foreach (CartItem item in order.CartOrdered.Items())
                    {
                        Product product = item.Product;
                        if (product.definition.type != ProductType.Consumable)
                        {
                            m_OwnedProducts.Add(product.definition.id);
                        }
                    }
                }
            }

            EntitlementsChanged?.Invoke();
        }

        private void CompleteInitialization(bool entitlementsLoaded)
        {
            if (State != IapServiceState.Initializing)
            {
                return;
            }

            SetState(
                entitlementsLoaded
                    ? IapServiceState.Ready
                    : IapServiceState.ReadyWithoutEntitlements);
            m_InitializationCompletion.TrySetResult(
                IapInitializationResult.Ready(entitlementsLoaded));
        }

        private IapInitializationResult FailInitialization(string error)
        {
            SetState(IapServiceState.Failed);
            IapInitializationResult result =
                IapInitializationResult.Failed(error);
            m_InitializationCompletion?.TrySetResult(result);
            return result;
        }

        private string GetPlacement(string productKey)
        {
            return m_ActiveRequests.TryGetValue(productKey, out ActiveRequest request)
                ? request.Placement
                : null;
        }

        private void CompleteActiveRequest(
            string productKey,
            IapPurchaseResult result)
        {
            if (!m_ActiveRequests.Remove(productKey, out ActiveRequest request))
            {
                return;
            }

            request.Cancellation.Dispose();
            request.Completion.TrySetResult(result);
        }

        private void QueueWaitingOrder(PendingOrder order)
        {
            string key = BuildOrderKey(order);
            if (m_WaitingOrders.All(existing => BuildOrderKey(existing) != key))
            {
                m_WaitingOrders.Add(order);
            }
        }

        private void RemoveWaitingOrder(string orderKey)
        {
            m_WaitingOrders.RemoveAll(order => BuildOrderKey(order) == orderKey);
        }

        private static string BuildOrderKey(PendingOrder order)
        {
            if (!string.IsNullOrWhiteSpace(order?.Info?.TransactionID))
            {
                return order.Info.TransactionID;
            }

            if (!string.IsNullOrWhiteSpace(order?.Info?.Receipt))
            {
                return order.Info.Receipt;
            }

            return null;
        }

        private void UnregisterFulfillmentHandler(string key, Delegate handler)
        {
            if (m_FulfillmentHandlers.TryGetValue(key, out var current) &&
                Delegate.Equals(current, handler))
            {
                m_FulfillmentHandlers.Remove(key);
            }
        }

        private void SetState(IapServiceState value)
        {
            if (State == value)
            {
                return;
            }

            State = value;
            StateChanged?.Invoke(value);
        }

        private void ThrowIfDisposed()
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException(nameof(IapService));
            }
        }
    }
}

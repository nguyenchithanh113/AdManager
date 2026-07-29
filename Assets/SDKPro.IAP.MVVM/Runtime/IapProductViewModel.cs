using System;
using System.Threading;
using System.Threading.Tasks;
using R3;

namespace SDKPro.IAP.MVVM
{
    public abstract class IapProductViewModel : IDisposable
    {
        private readonly IIapService m_Service;
        private readonly string m_ProductKey;
        private readonly CancellationTokenSource m_LifetimeCancellation = new();
        private IDisposable m_FulfillmentRegistration;
        private bool m_Initialized;

        public ReactiveProperty<string> PriceText { get; } = new();
        public ReactiveProperty<bool> IsAvailable { get; } = new();
        public ReactiveProperty<bool> IsOwned { get; } = new();
        public ReactiveProperty<bool> IsBusy { get; } = new();
        public Subject<IapPurchaseResult> PurchaseFinished { get; } = new();

        public string ProductKey => m_ProductKey;

        protected IapProductViewModel(IIapService service, string productKey)
        {
            m_Service = service ?? throw new ArgumentNullException(nameof(service));
            m_ProductKey = string.IsNullOrWhiteSpace(productKey)
                ? throw new ArgumentException(
                    "A product key is required.",
                    nameof(productKey))
                : productKey;
        }

        public void Initialize()
        {
            if (m_Initialized)
            {
                return;
            }

            m_Initialized = true;
            m_FulfillmentRegistration = m_Service.RegisterFulfillmentHandler(
                m_ProductKey,
                FulfillAsync);
            m_Service.ProductsChanged += Refresh;
            m_Service.EntitlementsChanged += Refresh;
            m_Service.StateChanged += HandleStateChanged;
            Refresh();
        }

        public async Task<IapPurchaseResult> BuyAsync(
            string placement = null,
            CancellationToken token = default)
        {
            if (!m_Initialized)
            {
                Initialize();
            }

            if (IsBusy.Value)
            {
                return IapPurchaseResult.Unavailable(
                    m_ProductKey,
                    $"A purchase for '{m_ProductKey}' is already in progress.");
            }

            IsBusy.Value = true;
            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                    token,
                    m_LifetimeCancellation.Token);
                IapPurchaseResult result = await m_Service.PurchaseAsync(
                    m_ProductKey,
                    placement,
                    linked.Token);
                PurchaseFinished.OnNext(result);
                OnPurchaseFinished(result);
                return result;
            }
            finally
            {
                IsBusy.Value = false;
                Refresh();
            }
        }

        protected abstract Task<IapFulfillmentResult> FulfillAsync(
            IapFulfillmentContext purchase,
            CancellationToken token);

        protected virtual void OnPurchaseFinished(IapPurchaseResult result)
        {
        }

        protected virtual void Refresh()
        {
            IsAvailable.Value =
                m_Service.TryGetProduct(m_ProductKey, out IapProductInfo product) &&
                product.AvailableToPurchase;
            PriceText.Value = m_Service.GetDisplayPrice(m_ProductKey) ?? string.Empty;
            IsOwned.Value = m_Service.IsOwned(m_ProductKey);
        }

        public void Dispose()
        {
            m_LifetimeCancellation.Cancel();
            m_LifetimeCancellation.Dispose();
            m_FulfillmentRegistration?.Dispose();
            m_Service.ProductsChanged -= Refresh;
            m_Service.EntitlementsChanged -= Refresh;
            m_Service.StateChanged -= HandleStateChanged;
            PriceText.Dispose();
            IsAvailable.Dispose();
            IsOwned.Dispose();
            IsBusy.Dispose();
            PurchaseFinished.Dispose();
        }

        private void HandleStateChanged(IapServiceState state)
        {
            Refresh();
        }
    }
}

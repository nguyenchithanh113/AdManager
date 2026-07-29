using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SDKPro.IAP
{
    [DisallowMultipleComponent]
    public sealed class IapManager : MonoBehaviour
    {
        [SerializeField] private IapCatalog m_Catalog;
        [SerializeField] private bool m_UseFakeStore;
        [SerializeField] private bool m_InitializeOnAwake = true;
        [SerializeField] private bool m_DontDestroyOnLoad = true;
        [SerializeField, Min(1)] private int m_MaximumProductFetchAttempts = 3;

        private CancellationTokenSource m_LifetimeCancellation;
        private IapService m_Service;

        public static IapManager Instance { get; private set; }
        public IIapService Service => m_Service;
        public bool IsReady => m_Service?.CanPurchase == true;
        public bool HasCompleteCatalog => m_Service?.HasCompleteCatalog == true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (m_DontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            m_LifetimeCancellation = new CancellationTokenSource();
            m_Service = new IapService(
                m_Catalog,
                m_UseFakeStore,
                maximumProductFetchAttempts: m_MaximumProductFetchAttempts);

            if (m_InitializeOnAwake)
            {
                ObserveInitialization();
            }
        }

        public Task<IapInitializationResult> InitializeAsync(
            CancellationToken token = default)
        {
            return m_Service.InitializeAsync(token);
        }

        public IDisposable RegisterFulfillmentHandler(
            string productKey,
            Func<IapFulfillmentContext, CancellationToken, Task<IapFulfillmentResult>>
                handler)
        {
            return m_Service.RegisterFulfillmentHandler(productKey, handler);
        }

        public Task<IapPurchaseResult> BuyAsync(
            string productKey,
            string placement = null,
            CancellationToken token = default)
        {
            return m_Service.PurchaseAsync(productKey, placement, token);
        }

        public async void Buy(
            string productKey,
            Action success,
            Action<string> failure,
            string placement = null)
        {
            try
            {
                IapPurchaseResult result = await BuyAsync(
                    productKey,
                    placement,
                    m_LifetimeCancellation.Token);
                if (result.Succeeded)
                {
                    success?.Invoke();
                }
                else
                {
                    failure?.Invoke(result.Error ?? result.Status.ToString());
                }
            }
            catch (OperationCanceledException)
            {
                failure?.Invoke("Purchase cancelled because the IAP manager was destroyed.");
            }
            catch (Exception exception)
            {
                failure?.Invoke(exception.Message);
            }
        }

        public Task<IapRestoreResult> RestoreAsync(
            CancellationToken token = default)
        {
            return m_Service.RestoreAsync(token);
        }

        public bool TryGetProduct(string productKey, out IapProductInfo product)
        {
            return m_Service.TryGetProduct(productKey, out product);
        }

        public string GetDisplayPrice(string productKey)
        {
            return m_Service.GetDisplayPrice(productKey);
        }

        public bool IsOwned(string productKey)
        {
            return m_Service.IsOwned(productKey);
        }

        public void RefreshProducts()
        {
            m_Service.RefreshProducts();
        }

        private async void ObserveInitialization()
        {
            try
            {
                IapInitializationResult result =
                    await InitializeAsync(m_LifetimeCancellation.Token);
                if (!result.Succeeded)
                {
                    Debug.LogError($"SDKPro IAP initialization failed: {result.Error}");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            m_LifetimeCancellation?.Cancel();
            m_LifetimeCancellation?.Dispose();
            m_Service?.Dispose();
            Instance = null;
        }
    }
}

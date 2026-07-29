using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using SDKPro.Core.Firebase;
using SDKPro.Core.GDPR;
using SDKPro.Core.Mmp;
using SDKPro.Core.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace SDKPro.Core.Mockups
{
    public class SDKManagerTemplate : Singleton<SDKManagerTemplate>
    {
        [FormerlySerializedAs("m_GdprProxy")]
        [SerializeField]
        [Tooltip("Fallback GDPR flow for ads providers without an integrated flow. Assign GoogleGDPRProxy.")]
        private GDPRProxy m_DefaultGoogleMobileAdsGdprProxy;

        private CompositeDisposable m_Bindings = new();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            m_Bindings.Clear();
            m_Bindings.Dispose();
        }

        public async UniTask StartAsync(CancellationToken token)
        {
            AdsManagerTemplate adsManager = AdsManagerTemplate.Instance;

            // Integrated providers run GDPR inside ads initialization.
            // Other providers await the default Google GDPR flow first.
            await adsManager.Init(
                m_DefaultGoogleMobileAdsGdprProxy,
                token);
            
            await FirebaseManager.Instance.Init(RemoteConfigTemplate.Instance, token);

            await UniTask.WaitForSeconds(0.5f, cancellationToken: token);
            
            MmpManager.Instance.Init(gameObject.GetCancellationTokenOnDestroy()).Forget();

            FirebaseManager.Instance.TokenResult.Subscribe(val =>
            {
                if (val.fetched)
                {
                    MmpManager.Instance.TrackTokenReceived(val.value);
                }
            }).AddTo(m_Bindings);

        }
    }
}

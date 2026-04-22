using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using SDKPro.Core.Firebase;
using SDKPro.Core.GDPR;
using SDKPro.Core.Mmp;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Core.Mockups
{
    public class SDKManagerTemplate : Singleton<SDKManagerTemplate>
    {
        [SerializeField] private GDPRProxy m_GdprProxy;

        private CompositeDisposable m_Bindings = new();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            m_Bindings.Clear();
            m_Bindings.Dispose();
        }

        public async UniTask StartAsync(CancellationToken token)
        {
            await m_GdprProxy.Get().WaitForConsent(token);
            
            await FirebaseManager.Instance.Init(RemoteConfigTemplate.Instance, token);
            
            MmpManager.Instance.Init(gameObject.GetCancellationTokenOnDestroy()).Forget();

            FirebaseManager.Instance.TokenResult.Subscribe(val =>
            {
                if (val.fetched)
                {
                    MmpManager.Instance.TrackTokenReceived(val.value);
                }
            }).AddTo(m_Bindings);

            await AdsManagerTemplate.Instance.Init();
            
        }
    }
}
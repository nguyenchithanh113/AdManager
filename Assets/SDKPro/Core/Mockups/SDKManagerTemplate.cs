using System.Threading;
using Cysharp.Threading.Tasks;
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
        
        
        public async UniTask StartAsync(CancellationToken token)
        {
            FirebaseManager.Instance.onTokenReceived -= MmpManager.Instance.TrackTokenReceived;
            FirebaseManager.Instance.onTokenReceived += MmpManager.Instance.TrackTokenReceived;
            
            await m_GdprProxy.Get().WaitForConsent(token);
            
            await FirebaseManager.Instance.Init(RemoteConfigTemplate.Instance, token);
            
            MmpManager.Instance.Init(gameObject.GetCancellationTokenOnDestroy()).Forget();

            await AdsManagerTemplate.Instance.Init();
            
        }
    }
}
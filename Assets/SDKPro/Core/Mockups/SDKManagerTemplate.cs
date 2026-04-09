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
            await m_GdprProxy.Get().WaitForConsent(token);
            
            MmpManager.Instance.Init(gameObject.GetCancellationTokenOnDestroy()).Forget();

            await FirebaseManager.Instance.Init(RemoteConfigTemplate.Instance, token);

            await AdsManagerTemplate.Instance.Init();
            
        }
    }
}
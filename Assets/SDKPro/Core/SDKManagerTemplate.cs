using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;
using SDKPro.Core.Ads.Proxy;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Core
{
    public class SDKManagerTemplate : Singleton<SDKManagerTemplate>
    {
        [SerializeField] private AdsServiceProxy m_DummyAd;

        private IAdsService m_AdsService;

        public async UniTask StartAsync()
        {
            var token = gameObject.GetCancellationTokenOnDestroy();
        }
    }
}
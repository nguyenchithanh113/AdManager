using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Core.Mmp
{
    public class MmpManager : Singleton<MmpManager>
    {
        [SerializeField] private MmpServiceProxy m_ServiceProxy;

        private IMmpService m_Service;
        
        public async UniTask Init(CancellationToken token)
        {
            m_Service = m_ServiceProxy.Get();

            await m_Service.Init();
        }

        public string GetUserID() => m_Service.GetUserID();
        public void TrackAdEvent(AdsValue adsValue) => m_Service.TrackAdEvent(adsValue);

        public void TrackCustomEvent(string eventKey, Dictionary<string, string> eventValues) =>
            m_Service.TrackCustomEvent(eventKey, eventValues);
    }
}
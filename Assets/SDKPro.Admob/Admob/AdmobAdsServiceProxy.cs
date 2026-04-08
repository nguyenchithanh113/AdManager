using SDKPro.Core.Ads;
using SDKPro.Core.Ads.Proxy;
using UnityEngine;

namespace SDKPro.Admob
{
    public class AdmobAdsServiceProxy : AdsServiceProxy
    {
        [SerializeField] private AdmobConfig m_Config;

        private AdmobAdsService m_AdsService;
        
        public override IAdsService GetService()
        {
            if (m_AdsService == null)
            {
                m_AdsService = new AdmobAdsService(m_Config);
            }

            return m_AdsService;
        }

        private void OnDestroy()
        {
            if (m_AdsService != null)
            {
                m_AdsService.Dispose();
            }
        }
    }
}
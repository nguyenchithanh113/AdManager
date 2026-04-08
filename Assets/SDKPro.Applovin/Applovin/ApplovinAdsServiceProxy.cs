using System;
using SDKPro.Core.Ads;
using SDKPro.Core.Ads.Proxy;
using UnityEngine;

namespace SDKPro.Applovin
{
    public class ApplovinAdsServiceProxy : AdsServiceProxy
    {
        [SerializeField] private ApplovinConfig m_Config;

        private ApplovinAdsService m_AdsService;
        
        public override IAdsService GetService()
        {
            if (m_AdsService == null)
            {
                m_AdsService = new ApplovinAdsService(m_Config);
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
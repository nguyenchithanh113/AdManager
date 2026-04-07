using UnityEngine;

namespace SDKPro.Core.Ads.Proxy
{
    public abstract class AdsServiceProxy : MonoBehaviour
    {
        [SerializeField] private AdsLoadSetting m_AdsLoadSetting;
        public abstract IAdsService GetService();

        public AdsLoadSetting GetAdsLoadSetting() => m_AdsLoadSetting;
    }
}
using UnityEngine;

namespace SDKPro.Core.Ads.Proxy
{
    public abstract class AdsServiceProxy : MonoBehaviour
    {
        [SerializeField] private AdsLoadSetting m_AdsLoadSetting;
        public abstract IAdsService GetService();

        protected AdsLoadSetting AdsLoadSetting => m_AdsLoadSetting;

        public virtual AdsLoadSetting GetAdsLoadSetting() => m_AdsLoadSetting;
    }
}

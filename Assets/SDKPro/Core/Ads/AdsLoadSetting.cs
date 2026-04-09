using UnityEngine;

namespace SDKPro.Core.Ads
{
    [CreateAssetMenu(menuName = "SDKPro/AdsLoadSetting")]
    public class AdsLoadSetting : ScriptableObject
    {
        public bool loadInter;
        public bool loadReward;
        public bool loadBanner;
        public bool loadMrec;
        public bool loadAOA;

        public bool hideBannerWhenFirstCreated = true;
        public bool hideMrecWhenFirstCreated = true;
    }
}
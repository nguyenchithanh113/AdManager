using UnityEngine;

namespace SDKPro.Core.Parameters
{
    [System.Serializable]
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
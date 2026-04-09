using GoogleMobileAds.Api;
using UnityEngine;

namespace SDKPro.Admob
{
    [CreateAssetMenu(menuName = "SDKPro/AdmobConfig")]
    public class AdmobConfig : ScriptableObject
    {
        public string interID;
        public string rewardID;
        public string mrecID;
        public string aoaID;
        public string bannerID;

        public bool shouldDestroyBannerWhenLoad = false;
        public bool onlyShowAoaOnce = false;

        public AdPosition mrecAdPosition = AdPosition.Bottom;
        public AdPosition bannerPosition = AdPosition.Bottom;
    }
}
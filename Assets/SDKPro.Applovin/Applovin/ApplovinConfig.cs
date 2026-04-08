using UnityEngine;

namespace SDKPro.Applovin
{
    [CreateAssetMenu(menuName = "SDKPro/ApplovinConfig")]
    public class ApplovinConfig : ScriptableObject
    {
        public string interID;
        public string rewardID;
        public string bannerID;
        public string aoaID;
        public string mrecID;
        
        public MaxSdkBase.AdViewPosition bannerPosition = MaxSdkBase.AdViewPosition.BottomCenter;
        public MaxSdkBase.AdViewPosition mrecPosition = MaxSdkBase.AdViewPosition.BottomCenter;
    }
}
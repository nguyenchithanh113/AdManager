using UnityEngine;

namespace SDKPro.Applovin
{
    [CreateAssetMenu(menuName = "SDKPro/ApplovinConfig")]
    public class ApplovinConfig : ScriptableObject
    {
        [Header("Privacy")]
        [Tooltip("Enable only when the MAX Terms and Privacy Policy Flow is configured. SDK initialization must not finish before its GDPR flow completes.")]
        public bool gdprHandledDuringInitialization = true;

        [Header("Ad Units")]
        public string interID;
        public string rewardID;
        public string bannerID;
        public string aoaID;
        public string mrecID;
        
        public MaxSdkBase.AdViewPosition bannerPosition = MaxSdkBase.AdViewPosition.BottomCenter;
        public MaxSdkBase.AdViewPosition mrecPosition = MaxSdkBase.AdViewPosition.BottomCenter;
    }
}

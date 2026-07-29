using System;
using UnityEngine;

namespace SDKPro.Core.Mockups
{
    [Serializable]
    public class FakeAdsConfig
    {
        [Min(0f)] public float initializationDelay = 0.15f;
        [Min(0f)] public float loadDelay = 0.25f;

        [Header("Failure simulation")]
        public bool failInterstitialLoad;
        public bool failRewardLoad;
        public bool failBannerLoad;
        public bool failMrecLoad;
        public bool failAppOpenLoad;
        public bool failFullscreenDisplay;

        [Header("Format simulation")]
        [Tooltip("Simulate a provider-owned GDPR flow so FakeAdsService remains dependency-free.")]
        public bool simulateGdprDuringInitialization = true;
        public bool simulateCollapsibleBanner = true;
        [Min(0f)] public double simulatedRevenue = 0.001d;
    }
}

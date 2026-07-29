using System;

namespace SDKPro.Core.Ads
{
    [Flags]
    public enum AdsServiceCapabilities
    {
        None = 0,
        Interstitial = 1 << 0,
        Rewarded = 1 << 1,
        Banner = 1 << 2,
        CollapsibleBanner = 1 << 3,
        Mrec = 1 << 4,
        AppOpen = 1 << 5,

        /// <summary>
        /// The provider runs its GDPR flow inside provider initialization.
        /// InitInternal must not complete until that flow is complete.
        /// </summary>
        GdprDuringInitialization = 1 << 6
    }

    public static class AdsServiceCapabilitiesExtensions
    {
        public static bool Supports(
            this AdsServiceCapabilities capabilities,
            AdsServiceCapabilities capability)
        {
            return (capabilities & capability) == capability;
        }
    }
}

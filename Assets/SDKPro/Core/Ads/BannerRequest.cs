using System;

namespace SDKPro.Core.Ads
{
    public enum CollapsibleBannerPlacement
    {
        Bottom,
        Top
    }

    [Serializable]
    public struct BannerRequest
    {
        public bool collapsible;
        public CollapsibleBannerPlacement collapsiblePlacement;

        public static BannerRequest Standard => new BannerRequest();

        public static BannerRequest Collapsible(
            CollapsibleBannerPlacement placement = CollapsibleBannerPlacement.Bottom)
        {
            return new BannerRequest
            {
                collapsible = true,
                collapsiblePlacement = placement
            };
        }
    }
}

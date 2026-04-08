namespace SDKPro.Core.Ads
{
    public class AdsValue
    {
        public double value;
        public AdType adType;
        public string adPlatform;
        public string adNetwork;
        public string adIdentifier;
        public string adCurrency;
        public string placement;
    }

    public enum AdType
    {
        Interstitial,
        Reward,
        Banner,
        BannerCollapsible,
        AOA,
        Mrec
    }
}
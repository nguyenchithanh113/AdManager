using SDKPro.Core.Ads;

namespace SDKPro.Admob
{
    public static class AdmobExtension
    {
        public static AdsValue ConvertToBaseAdValue(this GoogleMobileAds.Api.AdValue adValue, AdType adType, string identifier)
        {
            AdsValue baseAdValue = new AdsValue
            {
                value = (adValue.Value) / 1000000.0,
                adType = adType,
                adCurrency = "USD",
                adNetwork = "admob",
                adIdentifier = identifier,
                adPlatform = "Admob",
            };

            return baseAdValue;
        }
    }
}
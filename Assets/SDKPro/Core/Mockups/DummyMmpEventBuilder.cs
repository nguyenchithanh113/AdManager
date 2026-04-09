using SDKPro.Core.Ads;

namespace SDKPro.Core.Mockups
{
    public class DummyMmpEventBuilder : AdsEventMmpBuilder
    {
        public override EventBuilderResult OnInterDisplayed(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterDisplayedFailed(EventErrorInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterHidden(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterClicked(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterLoadRequest(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterLoadedSuccess(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterLoadedFail(EventErrorInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardDisplayed(RewardEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardDisplayedFailed(EventErrorInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardReceive(RewardEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardLoadRequest(RewardEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardLoadedSuccess(RewardEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardLoadedFail(EventErrorInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardClicked(RewardEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnBannerDisplayed(BannerEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnBannerHidden(BannerEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnBannerClicked(BannerEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnBannerLoadedFail(EventErrorInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnBannerLoadedSuccess(BannerEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnAOADisplayed(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnAOAHidden(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnAOAClicked(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnAOALoadedSuccess(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnAOALoadedFail(EventErrorInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnMrecDisplayed(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnMrecClicked(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnMrecLoadedFail(EventErrorInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnMrecLoadedSuccess(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterCallShow(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterCallShowPassCapping(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterCallShowFailCapping(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterCallShowAdsReady(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnInterCallShowAdsNotReady(EventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardCallShow(RewardEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardCallShowAdsReady(RewardEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnRewardCallShowAdsNotReady(RewardEventInfo info)
        {
            return EventBuilderResult.Fail;
        }

        public override EventBuilderResult OnAdPaid(AdsValue adsValue)
        {
            return EventBuilderResult.Fail;
        }
    }
}
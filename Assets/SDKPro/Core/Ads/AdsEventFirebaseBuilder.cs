using System;
using System.Collections.Generic;
using SDKPro.Core.Firebase;
using UnityEngine;

namespace SDKPro.Core.Ads
{
    public struct EventInfo
    {
        public string mediation;
        public int accumulatedCount;
        public string connectionString;
        public string placement;
    }

    public struct EventErrorInfo
    {
        public string mediation;
        public int accumulatedCount;
        public string connectionString;
        public string placement;
        public string error;
        public bool isCollapsible;
    }

    public struct BannerEventInfo
    {
        public string mediation;
        public int accumulatedCount;
        public string connectionString;
        public string placement;
        public bool isCollapsible;
    }

    public struct RewardEventInfo
    {
        public string mediation;
        public int accumulatedCount;
        public string connectionString;
        public string placement;
        public string reward;
    }

    public struct EventBuilderResult
    {
        public bool fail;
        public string eventName;
        public EventParameter[] parameters;

        public static EventBuilderResult Build(string eventName, params EventParameter[] parameters)
        {
            EventBuilderResult eventBuilderResult = new EventBuilderResult()
            {
                eventName = eventName,
                parameters = parameters
            };
            return eventBuilderResult;
        }

        public static readonly EventBuilderResult Fail = new EventBuilderResult() { fail = true };
    }
    
    public abstract class AdsEventFirebaseBuilder : MonoBehaviour
    {
        public abstract EventBuilderResult OnInterDisplayed(EventInfo info);
        public abstract EventBuilderResult OnInterDisplayedFailed(EventErrorInfo info);
        public abstract EventBuilderResult OnInterHidden(EventInfo info);
        public abstract EventBuilderResult OnInterClicked(EventInfo info);
        public abstract EventBuilderResult OnInterLoadRequest(EventInfo info);
        public abstract EventBuilderResult OnInterLoadedSuccess(EventInfo info);
        public abstract EventBuilderResult OnInterLoadedFail(EventErrorInfo info);

        public abstract EventBuilderResult OnRewardDisplayed(RewardEventInfo info);
        public abstract EventBuilderResult OnRewardDisplayedFailed(EventErrorInfo info);
        public abstract EventBuilderResult OnRewardReceive(RewardEventInfo info);
        public abstract EventBuilderResult OnRewardHidden(RewardEventInfo info);
        public abstract EventBuilderResult OnRewardLoadRequest(RewardEventInfo info);
        public abstract EventBuilderResult OnRewardLoadedSuccess(RewardEventInfo info);
        public abstract EventBuilderResult OnRewardLoadedFail(EventErrorInfo info);
        public abstract EventBuilderResult OnRewardClicked(RewardEventInfo info);

        public abstract EventBuilderResult OnBannerDisplayed(BannerEventInfo info);
        public abstract EventBuilderResult OnBannerHidden(BannerEventInfo info);
        public abstract EventBuilderResult OnBannerClicked(BannerEventInfo info);
        public abstract EventBuilderResult OnBannerLoadedFail(EventErrorInfo info);
        public abstract EventBuilderResult OnBannerLoadedSuccess(BannerEventInfo info);

        public abstract EventBuilderResult OnAOADisplayed(EventInfo info);
        public abstract EventBuilderResult OnAOAHidden(EventInfo info);
        public abstract EventBuilderResult OnAOAClicked(EventInfo info);
        public abstract EventBuilderResult OnAOALoadedSuccess(EventInfo info);
        public abstract EventBuilderResult OnAOALoadedFail(EventErrorInfo info);

        public abstract EventBuilderResult OnMrecDisplayed(EventInfo info);
        public abstract EventBuilderResult OnMrecClicked(EventInfo info);
        public abstract EventBuilderResult OnMrecLoadedFail(EventErrorInfo info);
        public abstract EventBuilderResult OnMrecLoadedSuccess(EventInfo info);

        public abstract EventBuilderResult OnInterCallShow(EventInfo info);
        public abstract EventBuilderResult OnInterCallShowPassCapping(EventInfo info);
        public abstract EventBuilderResult OnInterCallShowFailCapping(EventInfo info);
        public abstract EventBuilderResult OnInterCallShowAdsReady(EventInfo info);
        public abstract EventBuilderResult OnInterCallShowAdsNotReady(EventInfo info);

        public abstract EventBuilderResult OnRewardCallShow(RewardEventInfo info);
        public abstract EventBuilderResult OnRewardCallShowAdsReady(RewardEventInfo info);
        public abstract EventBuilderResult OnRewardCallShowAdsNotReady(RewardEventInfo info);

        public abstract EventBuilderResult OnAdPaid(AdsValue adsValue);
    }
}
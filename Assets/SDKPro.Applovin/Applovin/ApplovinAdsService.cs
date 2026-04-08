using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Applovin
{
    public class ApplovinAdsService : AdsServiceBase
    {
        public override string Mediation { get; } = "Applovin";
        private ApplovinConfig m_Config;

        private MaxSdkBase.AdViewConfiguration m_BannerAdViewConfiguration;
        private MaxSdkBase.AdViewConfiguration m_MrecAdViewConfiguration;
        
        private Vector2 _mrecCustomPosition = new Vector2(-10000, -10000);
        private Vector2 _invalidPosition = new Vector2(-10000, -10000);

        private bool _isMrecLoaded;
        private bool _bannerIsLoaded;

        private bool m_Initialized;
        
        public ApplovinAdsService(ApplovinConfig config)
        {
            m_Config = config;
            MaxSdk.InvokeEventsOnUnityMainThread = true;

            m_BannerAdViewConfiguration = new MaxSdkBase.AdViewConfiguration(config.bannerPosition);
            m_MrecAdViewConfiguration = new MaxSdkBase.AdViewConfiguration(config.mrecPosition);
        }

        void OnSdkInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            #region inter callback

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += InterstitialOnAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += InterstitialOnAdLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += InterstitialOnAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialOnAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += InterstitialOnAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += InterstitialOnAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += InterstitialOnAdPaidEvent;

            #endregion

            #region reward callback

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += RewardedOnAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += RewardedOnAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += RewardedOnAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += RewardedOnAdDisplayFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += RewardedOnAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += RewardedOnAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += RewardedOnAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += RewardedOnAdPaidEvent;

            #endregion

            #region banner callback

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += BannerOnAdPaidEvent;

            #endregion

            #region mrec callback

            MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += MrecOnAdPaidEvent;

            #endregion

            #region aoa callback

            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAoaAdLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAoaAdLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAoaAdClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAoaDisplayEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAoaDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAoaHiddenEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += AoaOnAdPaidEvent;

            #endregion

            if (!MaxSdk.HasUserConsent())
            {
                MaxSdk.SetHasUserConsent(true);
                MaxSdk.SetDoNotSell(false);
            }
            
            m_Initialized = true;
            OnAdServiceInitializeFinished?.Invoke();
        }

        public override void UpdateUserID(string id)
        {
            MaxSdk.SetUserId(id);
        }

        protected override async UniTask InitInternal(CancellationToken token)
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;
            
            MaxSdk.InitializeSdk();

            await UniTask.WaitUntil(() => m_Initialized, cancellationToken: token);
        }

        private void InterstitialOnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            _interRetryAttempt = 0;
            
            Debug.Log("Applovin Inter loaded");
            OnInterLoadedSuccess?.Invoke();
        }

        private void InterstitialOnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            ScheduleReloadInterstitial(m_SessionToken.Token).Forget();
            
            Debug.Log("Applovin Inter loaded Failed"+errorInfo);
            OnInterLoadedFail?.Invoke(errorInfo.Message);
        }

        private void InterstitialOnAdDisplayedEvent(string adUnitID, MaxSdkBase.AdInfo adInfo)
        {
            OnInterDisplayed?.Invoke();
        }

        private void InterstitialOnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            OnInterDisplayedFail?.Invoke(errorInfo.Message);
            ActionUtility.StartActionDelay(LoadInterstitial, 0.5f).Forget();
        }

        private void InterstitialOnAdClickedEvent(string adUnitID, MaxSdkBase.AdInfo adInfo)
        {
            OnInterClicked?.Invoke();
        }

        private void InterstitialOnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnInterHidden?.Invoke();
            ActionUtility.StartActionDelay(LoadInterstitial, 0.5f).Forget();
        }
        
        void InterstitialOnAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAdsPaid?.Invoke(new AdsValue()
            {
                adPlatform = Mediation,
                adNetwork = adInfo.NetworkName,
                value = adInfo.Revenue,
                adIdentifier = adUnitId,
                adCurrency = "USD",
                adType = AdType.Interstitial
            });
        }

        private void RewardedOnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
            Debug.Log("Applovin Rewarded ad loaded");
            // Reset retry attempt
            _rewardRetryAttempt = 0;
            OnRewardLoadedSuccess?.Invoke();
        }

        private void RewardedOnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            ScheduleReloadReward(m_SessionToken.Token).Forget();
            
            Debug.Log("Applovin Rewarded ad loaded Failed "+errorInfo);
            OnRewardLoadedFail?.Invoke(errorInfo.Message);
        }

        private void RewardedOnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            ActionUtility.StartActionDelay(LoadReward, 0.5f).Forget();
            
            OnRewardDisplayedFail?.Invoke(errorInfo.Message);
        }

        private void RewardedOnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnRewardDisplayed?.Invoke();
        }

        private void RewardedOnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad clicked");
            OnRewardClicked?.Invoke();
        }

        private void RewardedOnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ActionUtility.StartActionDelay(LoadReward, 0.5f).Forget();
            OnRewardAdClose?.Invoke();
        }

        private void RewardedOnAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward,
            MaxSdkBase.AdInfo adInfo)
        {
            OnRewardReceive?.Invoke();
        }
        
        void RewardedOnAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAdsPaid?.Invoke(new AdsValue()
            {
                adPlatform = Mediation,
                adNetwork = adInfo.NetworkName,
                value = adInfo.Revenue,
                adIdentifier = adUnitId,
                adCurrency = "USD",
                adType = AdType.Reward
            });
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _bannerIsLoaded = true;
            Debug.Log("Applovin Banner ad loaded");
            
            OnBannerLoadedSuccess?.Invoke(false);
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Debug.Log("Applovin Banner ad loaded Failed: "+errorInfo);
            OnBannerLoadedFail.Invoke(false, errorInfo.Message);
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnBannerClicked?.Invoke(false);
        }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnBannerDisplayed?.Invoke(false);
        }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnBannerHidden?.Invoke(false);
        }
        
        void BannerOnAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAdsPaid?.Invoke(new AdsValue()
            {
                adPlatform = Mediation,
                adNetwork = adInfo.NetworkName,
                value = adInfo.Revenue,
                adIdentifier = adUnitId,
                adCurrency = "USD",
                adType = AdType.Banner
            });
        }

        public void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _isMrecLoaded = true; Debug.Log("Applovin Mrec ad loaded");
            OnMrecLoadedSuccess.Invoke();
        }

        public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
        {
            Debug.Log("Applovin Mrec ad loaded Failed: "+error);
            OnMrecLoadedFail.Invoke(error.Message);
        }

        public void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnMrecClicked.Invoke();
        }
        

        public void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        public void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
        
        void MrecOnAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAdsPaid?.Invoke(new AdsValue()
            {
                adPlatform = Mediation,
                adNetwork = adInfo.NetworkName,
                value = adInfo.Revenue,
                adIdentifier = adUnitId,
                adCurrency = "USD",
                adType = AdType.Mrec
            });
        }

        public void OnAoaAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Applovin Aoa ad loaded");
            OnAOALoadedSuccess.Invoke();
        }

        public void OnAoaAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
        {
            Debug.Log("Applovin Rewarded ad loaded Failed: " + error);
            ActionUtility.StartActionDelay(LoadAOA, 7f).Forget();
            OnAOALoadedFail.Invoke(error.Message);
        }

        public void OnAoaAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAOAClicked.Invoke();
        }

        private void OnAoaHiddenEvent(string adUnitId, MaxSdkBase.AdInfo arg2)
        {
            ActionUtility.StartActionOnMainThread(LoadAOA).Forget();
            OnAOAHidden.Invoke();
        }

        private void OnAoaDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo arg2, MaxSdkBase.AdInfo arg3)
        {
            ActionUtility.StartActionOnMainThread(LoadAOA).Forget();
        }

        private void OnAoaDisplayEvent(string adUnitId, MaxSdkBase.AdInfo arg2)
        {
            OnAOADisplayed.Invoke();
        }
        
        void AoaOnAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAdsPaid?.Invoke(new AdsValue()
            {
                adPlatform = Mediation,
                adNetwork = adInfo.NetworkName,
                value = adInfo.Revenue,
                adIdentifier = adUnitId,
                adCurrency = "USD",
                adType = AdType.AOA
            });
        }
        

        public override void LoadInterstitial()
        {
            if (string.IsNullOrEmpty(m_Config.interID)) return;
            Debug.Log("Applovin Load Interstitial");
            if (MaxSdk.IsInitialized())
            {
                if (!MaxSdk.IsInterstitialReady(m_Config.interID))
                {
                    MaxSdk.LoadInterstitial(m_Config.interID);
                    OnInterLoadRequest?.Invoke();
                }
                else
                {
                    Debug.Log("Applovin Load Interstitial - AdsIsReady - Not Load");
                }
            }
        }

        public override bool IsInterstitialReady()
        {
            return MaxSdk.IsInterstitialReady(m_Config.interID);
        }

        public override void ShowInterstitial()
        {
            MaxSdk.ShowInterstitial(m_Config.interID);
        }

        public override void LoadReward()
        {
            if (string.IsNullOrEmpty(m_Config.rewardID)) return;
            Debug.Log("Applovin Load Reward");
            if (MaxSdk.IsInitialized())
            {
                if (!MaxSdk.IsRewardedAdReady(m_Config.rewardID))
                {
                    MaxSdk.LoadRewardedAd(m_Config.rewardID);
                    OnRewardLoadRequest?.Invoke();
                }
                else
                {
                    Debug.Log("Applovin Load Reward - AdsIsReady - Not Load");
                }
            }
        }

        public override bool IsRewardReady()
        {
            return MaxSdk.IsRewardedAdReady(m_Config.rewardID);
        }

        public override void ShowReward()
        {
            MaxSdk.ShowRewardedAd(m_Config.rewardID);
        }
        
        /*public Rect GetBannerScreenLayout()
        {
#if UNITY_EDITOR
            var bannerWidthDPI = 320;
            var bannerHeightDPI = 50;

            var screenWidthDPI = ScreenUtility.ScreenToDPI(Screen.width);
            var screenHeightDPI = ScreenUtility.ScreenToDPI(Screen.height);

            int x = (int)((screenWidthDPI / 2) - (bannerWidthDPI / 2));
            int y = (int)(screenHeightDPI - bannerHeightDPI);
            
            var bannerPos = new Vector2(x, y);
            var bannerSize = new Vector2(bannerWidthDPI, bannerHeightDPI);
            return LocalToScreenLayout(bannerPos, bannerSize);
#elif UNITY_ANDROID && !UNITY_EDITOR
            return MaxSdk.GetBannerLayout(_config.bannerID); 
#endif
        }

        Rect LocalToScreenLayout(Vector2 pos, Vector2 size)
        {
            var screenWidthDPI = ScreenUtility.ScreenToDPI(Screen.width);
            var screenHeightDPI = ScreenUtility.ScreenToDPI(Screen.height);
            var dpX = pos.x;
            var dpY = screenHeightDPI - (pos.y + size.y);

            var spX = ScreenUtility.DPIToScreen(dpX);
            var spY = ScreenUtility.DPIToScreen(dpY);
            var spSize = ScreenUtility.DPIToScreen(size);

            return new Rect(spX, spY, spSize.x, spSize.y);
        }*/

        public override void CreateBanner()
        {
            if (string.IsNullOrEmpty(m_Config.bannerID)) return;
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
            MaxSdk.CreateBanner(m_Config.bannerID, m_BannerAdViewConfiguration);
            //MaxSdk.SetBannerExtraParameter(_config.bannerID, "adaptive_banner", "false");
            // Set background or background color for banners to be fully functional.
            MaxSdk.SetBannerBackgroundColor(m_Config.bannerID, Color.white);
            //MaxSdk.SetBannerWidth(_config.bannerID, (float)Screen.width);
        }

        public override void LoadBanner()
        {
            
        }

        public override void ShowBanner()
        {
            MaxSdk.ShowBanner(m_Config.bannerID);
        }

        public override void HideBanner()
        {
            MaxSdk.HideBanner(m_Config.bannerID);
        }

        public override void DestroyBanner()
        {
            try
            {
                MaxSdk.DestroyBanner(m_Config.bannerID);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public override void CreateMrec()
        {
            if (string.IsNullOrEmpty(m_Config.mrecID)) return;
            
            MaxSdk.CreateMRec(m_Config.mrecID, m_MrecAdViewConfiguration);
            
            Debug.Log("Applovin Start Load Mrec");
        }

        public override void LoadMrec()
        {
            
        }

        public override void ShowMrec()
        {
            MaxSdk.ShowMRec(m_Config.mrecID);
        }

        public override void HideMrec()
        {
            MaxSdk.HideMRec(m_Config.mrecID);
        }

        public override bool IsMrecReady()
        {
            return _isMrecLoaded;
        }

        public override void SetMrecPosition(Vector2 dpPos)
        {
            _mrecCustomPosition = dpPos;
            MaxSdk.UpdateMRecPosition(m_Config.mrecID, dpPos.x, dpPos.y);
        }

        public override void LoadAOA()
        {
            if (string.IsNullOrEmpty(m_Config.aoaID)) return;

            MaxSdk.LoadAppOpenAd(m_Config.aoaID);
            
            Debug.Log("Applovin Start Load Aoa");
        }

        public override void ShowAOA()
        {
            MaxSdk.ShowAppOpenAd(m_Config.aoaID);
        }

        public override bool IsAOAReady()
        {
            return MaxSdk.IsAppOpenAdReady(m_Config.aoaID);
        }
    }
}
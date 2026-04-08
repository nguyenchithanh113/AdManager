using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Api.AdManager;
using GoogleMobileAds.Common;
using SDKPro.Core.Ads;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Admob
{
    public class AdmobAdsService : AdsServiceBase
    {
        private readonly AdmobConfig _admobConfig;
        
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        private BannerView _bannerView;
        private BannerView _mrecView;
        private AppOpenAd _appOpenAd;

        private bool m_IsInitialized;
        
        private bool _isMrecLoaded;

        public override string Mediation { get; } = "Admob";

        public AdmobAdsService(
            AdmobConfig admobConfig)
        {
            _admobConfig = admobConfig;
        }

        public override void UpdateUserID(string id)
        {
            
        }

        protected override async UniTask InitInternal(CancellationToken token)
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };
            
            RequestConfiguration requestConfiguration = new RequestConfiguration();
            requestConfiguration.TagForChildDirectedTreatment = TagForChildDirectedTreatment.Unspecified;
            requestConfiguration.TestDeviceIds = deviceIds;
            
            MobileAds.SetRequestConfiguration(requestConfiguration);
            
            MobileAds.Initialize(OnInitComplete);

            await UniTask.WaitUntil(() => m_IsInitialized, cancellationToken: token);
        }
        
        void OnInitComplete(InitializationStatus initializationStatus)
        {
            MobileAdsEventExecutor.ExecuteInUpdate((() =>
            {
                Debug.Log("Initialization Admob Complete");

                Dictionary<string, AdapterStatus> map = initializationStatus.getAdapterStatusMap();
                foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
                {
                    string className = keyValuePair.Key;
                    AdapterStatus status = keyValuePair.Value;
                    switch (status.InitializationState)
                    {
                        case AdapterState.NotReady:
                            // The adapter initialization did not complete.
                            MonoBehaviour.print("Adapter: " + className + " not ready.");
                            break;
                        case AdapterState.Ready:
                            // The adapter was successfully initialized.
                            MonoBehaviour.print("Adapter: " + className + " is initialized.");
                            break;
                    }
                }
                
                OnAdServiceInitializeFinished?.Invoke();
                m_IsInitialized = true;
            }));
        }

        #region Interstitial Ad

        public override void LoadInterstitial()
        {
            if(string.IsNullOrEmpty(_admobConfig.interID)) return;
            
            var adRequest = new AdRequest();
            var id = _admobConfig.interID;

            if (_interstitialAd != null)
            {
                DestroyInterstitial();
            }
            
            InterstitialAd.Load(id, adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    ScheduleReloadInterstitial(m_SessionToken.Token).Forget();
                    Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
                    OnInterLoadedFail?.Invoke(error.GetMessage());
                    return;
                }
                
                if (ad == null)
                {
                    ScheduleReloadInterstitial(m_SessionToken.Token).Forget();
                    Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
                    OnInterLoadedFail?.Invoke("Unexpected error");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                _interRetryAttempt = 0;
                _interstitialAd = ad;
                OnInterLoadedSuccess?.Invoke();

                RegisterEventHandlersInterstitial(ad);
            });
            
            OnInterLoadRequest?.Invoke();
        }

        public override bool IsInterstitialReady()
        {
            return _interstitialAd != null && _interstitialAd.CanShowAd();
        }

        public override void ShowInterstitial()
        {
            if (IsInterstitialReady())
            {
                _interstitialAd.Show();
            }
        }
        
        public void DestroyInterstitial()
        {
            if (_interstitialAd != null)
            {
                Debug.Log("Destroying interstitial ad.");
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
        }

        private void RegisterEventHandlersInterstitial(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
                
                OnAdsPaid?.Invoke(adValue.ConvertToBaseAdValue(AdType.Interstitial, _admobConfig.interID));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                OnInterClicked?.Invoke();
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Admob: inters displayed");
                OnInterDisplayed?.Invoke();
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                ActionUtility.StartActionDelay(LoadInterstitial, 0.5f).Forget();
                OnInterHidden?.Invoke();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content with error : "
                               + error);
                ActionUtility.StartActionDelay(LoadInterstitial, 0.5f).Forget();
                OnInterDisplayedFail?.Invoke(error.GetMessage());
            };
        }

        #endregion
        
        #region Reward ad

        public override void LoadReward()
        {
            if (string.IsNullOrEmpty(_admobConfig.rewardID)) return;
            
            // Clean up the old ad before loading a new one.
            if (_rewardedAd != null)
            {
                DestroyRewarded();
            }

            Debug.Log("Loading rewarded ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();
            var id = _admobConfig.rewardID;

            // Send the request to load the ad.
            RewardedAd.Load(id, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    ScheduleReloadReward(m_SessionToken.Token).Forget();
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                    OnRewardLoadedFail.Invoke(error.GetMessage());
                    return;
                }

                // If the operation failed for unknown reasons.
                // This is an unexpected error, please report this bug if it happens.
                if (ad == null)
                {
                    ScheduleReloadReward(m_SessionToken.Token).Forget();
                    Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
                    OnRewardLoadedFail.Invoke("Unexpected error");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                _rewardRetryAttempt = 0;
                _rewardedAd = ad;
                OnRewardLoadedSuccess?.Invoke();

                // Register to ad events to extend functionality.
                RegisterEventHandlersRewarded(ad);
            });
        }

        public override bool IsRewardReady()
        {
            return _rewardedAd != null && _rewardedAd.CanShowAd();
        }

        public override void ShowReward()
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show(OnRewardReceiveCallback);
            }
        }

        void OnRewardReceiveCallback(Reward reward)
        {
            OnRewardReceive?.Invoke();
        }
        
        public void DestroyRewarded()
        {
            if (_rewardedAd != null)
            {
                Debug.Log("Destroying rewarded ad.");
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
        }
        
        private void RegisterEventHandlersRewarded(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
                
                OnAdsPaid?.Invoke(adValue.ConvertToBaseAdValue(AdType.Reward, _admobConfig.rewardID));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () => { Debug.Log("Rewarded ad recorded an impression."); };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                OnRewardClicked?.Invoke();
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when the ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                OnRewardDisplayed?.Invoke();
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                OnRewardAdClose?.Invoke();

                ActionUtility.StartActionDelay(LoadReward, 0.5f).Forget();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                OnRewardDisplayedFail?.Invoke(error.GetMessage());
                Debug.LogError("Rewarded ad failed to open full screen content with error : "
                               + error);

                ActionUtility.StartActionDelay(LoadReward, 0.5f).Forget();
            };
        }

        #endregion

        #region Banner

        public override void LoadBanner()
        {
            if (_admobConfig.shouldDestroyBannerWhenLoad)
            {
                CreateBanner();
            }
            
            if (_bannerView == null)
            {
                CreateBanner();
            }
            
            var adRequest = new AdRequest();
            
            Debug.Log("Loading banner ad.");
            _bannerView.LoadAd(adRequest);
        }

        public override void ShowBanner()
        {
            if (_bannerView != null)
            {
                _bannerView.Show();
            }
        }

        public override void HideBanner()
        {
            if (_bannerView != null)
            {
                _bannerView.Hide();
            }
        }

        public override void DestroyBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("Destroying banner view.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }
        
        public override void CreateBanner()
        {
            DestroyBanner();
            
            var id = _admobConfig.bannerID;
            _bannerView = new AdManagerBannerView(id, AdSize.Banner, _admobConfig.bannerPosition);
            
            RegisterEventHandlersBanner(_bannerView);
        }

        bool IsBannerCollapsible()
        {
            return _bannerView.IsCollapsible();
            return false;
        }
        
        private void RegisterEventHandlersBanner(BannerView ad)
        {
            // Raised when an ad is loaded into the banner view.
            ad.OnBannerAdLoaded += (() =>
            {
                OnBannerLoadedSuccess.Invoke(IsBannerCollapsible());
                Debug.Log("Ad Banner Loaded Success");
            });
            // Raised when an ad fails to load into the banner view.
            ad.OnBannerAdLoadFailed += (error =>
            {
                OnBannerLoadedFail.Invoke(IsBannerCollapsible(), error.GetMessage());
                Debug.Log("Ad Banner Loaded Failed with error: "+error);
            });
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (adValue =>
            {
                OnAdsPaid?.Invoke(adValue.ConvertToBaseAdValue(IsBannerCollapsible() ? AdType.BannerCollapsible : AdType.Banner, _admobConfig.bannerID));
            });
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += (() =>
            {
                
            });
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                OnBannerClicked?.Invoke(IsBannerCollapsible());
                //_firebaseService.LogEvent("banner_collap_click", AdManager.GetInternetEvent(), GetCheckBannerCollapsibleEvent());
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                OnBannerDisplayed?.Invoke(IsBannerCollapsible());
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                OnBannerHidden?.Invoke(IsBannerCollapsible());
            };
        }

        #endregion

        #region Mrec

        public override void ShowMrec()
        {
            if (_mrecView!=null)
            {
                _mrecView.Show();
            }
        }

        public override void HideMrec()
        {
            if (_mrecView!=null)
            {
                _mrecView.Hide();
            }
        }

        public override void SetMrecPosition(Vector2 dpPos)
        {
            if (_mrecView!=null)
            {
                _mrecView.SetPosition((int)dpPos.x, (int)dpPos.y);
            }
        }

        public override bool IsMrecReady()
        {
            return _isMrecLoaded;
        }

        public override void LoadMrec()
        {
            if (_mrecView == null)
            {
                CreateMrec();
            }

            var adRequest = new AdRequest();
            
            _mrecView.LoadAd(adRequest);
        }

        public void DestroyMrecView()
        {
            if (_mrecView != null)
            {
                Debug.Log("Destroy mrec view");
                _mrecView.Destroy();
                _isMrecLoaded = false;
                _mrecView = null;
            }
        }
        
        public override void CreateMrec()
        {
            DestroyMrecView();
            var id = _admobConfig.mrecID;
            
            _mrecView = new AdManagerBannerView(id, AdSize.MediumRectangle, _admobConfig.mrecAdPosition);
            
            RegisterEventHandlersMrec(_mrecView);
        }
        
        private void RegisterEventHandlersMrec(BannerView ad)
        {
            // Raised when an ad is loaded into the banner view.
            ad.OnBannerAdLoaded += (() =>
            {
                OnMrecLoadedSuccess?.Invoke();
                _isMrecLoaded = true;
                Debug.Log("Ad Mrec Loaded Success");
            });
            // Raised when an ad fails to load into the banner view.
            ad.OnBannerAdLoadFailed += (error =>
            {
                OnMrecLoadedFail?.Invoke(error.GetMessage());
                Debug.Log("Ad Mrec Loaded Failed with error: "+error);
            });
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (adValue =>
            {
                OnAdsPaid?.Invoke(adValue.ConvertToBaseAdValue(AdType.Mrec, _admobConfig.mrecID));
            });
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += (() =>
            {
                
            });
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                OnMrecClicked?.Invoke();   
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                OnMrecDisplayed?.Invoke();
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                
            };
        }

        #endregion
        
        public override void LoadAOA()
        {
            if(string.IsNullOrEmpty(_admobConfig.aoaID)) return;
            var adRequest = new AdRequest();
            var id = _admobConfig.aoaID;
            
            AppOpenAd.Load(id, adRequest, (ad, error) =>
            {
                if (error != null)
                {
                    ScheduleReloadAOA().Forget();
                    Debug.LogError("App open ad failed to load an ad with error : "
                                   + error);
                    OnAOALoadedFail?.Invoke(error.GetMessage());
                    return;
                }
                
                if (ad == null)
                {
                    ScheduleReloadAOA().Forget();
                    Debug.LogError("Unexpected error: App open ad load event fired with " +
                                   " null ad and null error.");
                    OnAOALoadedFail?.Invoke("Unexpected error");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());
                OnAOALoadedSuccess?.Invoke();
                _appOpenAd = ad;

                RegisterEventHandlersAOA(ad);
            });
        }

        async UniTaskVoid ScheduleReloadAOA()
        {
            await UniTask.WaitForSeconds(15f);
            LoadAOA();
        }

        public override void ShowAOA()
        {
            if (_appOpenAd != null && _appOpenAd.CanShowAd())
            {
                Debug.Log("Showing app open ad.");
                _appOpenAd.Show();
            }
            else
            {
                Debug.LogError("App open ad is not ready yet.");
            }
        }

        public override bool IsAOAReady()
        {
            return _appOpenAd != null && _appOpenAd.CanShowAd();
        }

        private void RegisterEventHandlersAOA(AppOpenAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (adValue) =>
            {
                OnAdsPaid?.Invoke(adValue.ConvertToBaseAdValue(AdType.AOA, _admobConfig.aoaID));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () => { Debug.Log("App open ad recorded an impression."); };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                OnAOAClicked?.Invoke();
                Debug.Log("App open ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("App open ad full screen content opened.");
                OnAOADisplayed?.Invoke();
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("App open ad full screen content closed.");

                // It may be useful to load a new ad when the current one is complete.
                if (!_admobConfig.onlyShowAoaOnce)
                {
                    LoadAOA();
                }
                
                OnAOAHidden?.Invoke();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("App open ad failed to open full screen content with error : "
                               + error);
            };
        }
    }
}
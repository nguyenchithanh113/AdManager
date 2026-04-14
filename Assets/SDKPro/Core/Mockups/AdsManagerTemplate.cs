using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;
using SDKPro.Core.Ads.Proxy;
using SDKPro.Core.Firebase;
using SDKPro.Core.Mmp;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Core.Mockups
{
    public class AdsManagerTemplate : Singleton<AdsManagerTemplate>
    {
        [SerializeField] private AdsServiceProxy m_AdsServiceProxy;
        [SerializeField] private AdsEventFirebaseBuilder m_AdsEventFirebaseBuilder;
        [SerializeField] private AdsEventMmpBuilder m_AdsEventMmpBuilder;

        private IAdsService m_AdsService;

        private float m_InterCappingTime = 30f;

        private float _timer;
        private float _lastTimeShowFullScreenAd = -100;
        private float _lastTimeShowInterAd = -100;
        private float _lastTimeShowAoa = -100;
        private float _lastTimeLoadCollapsibleBanner = -100;

        private string _interPlacement = "";
        private string _rewardPlacement = "";
        private string _reward = "";

        private Action _interSuccessCallback;
        private Action _rewardSuccessCallback;

        private Action _interFailCallback;
        private Action _rewardFailCallback;
        private bool _isPause;
        private bool _blockInit = false;
        
        public List<Func<bool>> InterShowRules = new();

        public async UniTask Init()
        {
            m_AdsService = m_AdsServiceProxy.GetService();
            RegisterAdsBaseEvents(m_AdsService);

            var tasks = new List<UniTask>();
            tasks.Add(m_AdsService.Init(m_AdsServiceProxy.GetAdsLoadSetting()));

            await UniTask.WhenAll(tasks);
        }

        void RegisterAdsBaseEvents(IAdsService adsService)
        {
            adsService.OnInterDisplayed += () => OnInterDisplayed(adsService);
            adsService.OnInterDisplayedFail += error => OnInterDisplayedFail(error, adsService);
            adsService.OnInterHidden += () => OnInterHidden(adsService);
            adsService.OnInterClicked += () => OnInterClicked(adsService);
            adsService.OnInterLoadRequest += () => OnInterLoadRequest(adsService);
            adsService.OnInterLoadedSuccess += () => OnInterLoadedSuccess(adsService);
            adsService.OnInterLoadedFail += error => OnInterLoadedFail(error, adsService);

            adsService.OnRewardDisplayed += () => OnRewardDisplayed(adsService);
            adsService.OnRewardDisplayedFail += error => OnRewardDisplayedFail(error, adsService);
            adsService.OnRewardReceive += () => OnRewardReceive(adsService);
            adsService.OnRewardAdClose += () => OnRewardHidden(adsService);
            adsService.OnRewardClicked += () => OnRewardClicked(adsService);
            adsService.OnRewardLoadRequest += () => OnRewardLoadRequest(adsService);
            adsService.OnRewardLoadedSuccess += () => OnRewardLoadedSuccess(adsService);
            adsService.OnRewardLoadedFail += error => OnRewardLoadedFail(error, adsService);

            adsService.OnBannerDisplayed += collapsible => OnBannerDisplayed(collapsible, adsService);
            adsService.OnBannerHidden += collapsible => OnBannerHidden(collapsible, adsService);
            adsService.OnBannerClicked += collapsible => OnBannerClicked(collapsible, adsService);
            adsService.OnBannerLoadedFail += (collapsible, error) => OnBannerLoadedFail(error, collapsible, adsService);
            adsService.OnBannerLoadedSuccess += collapsible => OnBannerLoadedSuccess(collapsible, adsService);

            adsService.OnAOADisplayed += () => OnAOADisplayed(adsService);
            adsService.OnAOAHidden += () => OnAOAHidden(adsService);
            adsService.OnAOAClicked += () => OnAOAClicked(adsService);
            adsService.OnAOALoadedSuccess += () => OnAOALoadedSuccess(adsService);
            adsService.OnAOALoadedFail += error => OnAOALoadedFail(error, adsService);

            adsService.OnMrecDisplayed += () => OnMrecDisplayed(adsService);
            adsService.OnMrecClicked += () => OnMrecClicked(adsService);
            adsService.OnMrecLoadedFail += error => OnMrecLoadedFail(error, adsService);
            adsService.OnMrecLoadedSuccess += () => OnMrecLoadedSuccess(adsService);

            adsService.OnAdsPaid += value => OnAdPaid(value);
        }

        private void Update()
        {
            if (_isPause) return;
            _timer += Time.unscaledDeltaTime;
        }

        bool CanShowFullScreenAds()
        {
            return _timer - _lastTimeShowFullScreenAd >= 1.2f;
        }

        public void MarkAsShowFullScreenAds()
        {
            _lastTimeShowFullScreenAd = _timer;
        }

        public bool IsInterstitialPassCapping()
        {
            float cappingTime = m_InterCappingTime;

            return (_timer - _lastTimeShowInterAd) >= cappingTime;
        }

        public bool ShowInterstitial(Action successCallback, Action failCallback, string placement,
            bool ignoreCapping = false)
        {
            _interSuccessCallback = successCallback;
            _interFailCallback = failCallback;
            _interPlacement = placement;

#if NO_ADS
            _interSuccessCallback?.Invoke();
            return false;
#endif
            
            foreach (var interShowRule in InterShowRules)
            {
                if (interShowRule() == false)
                {
                    _interSuccessCallback?.Invoke();
                    return false;
                }
            }

            if (!CanShowFullScreenAds())
            {
                _interSuccessCallback?.Invoke();
                return false;
            }

            OnInterCallShow(m_AdsService);

            if (!IsInterstitialPassCapping() && !ignoreCapping)
            {
                _interSuccessCallback?.Invoke();
                OnInterCallShowFailCapping(m_AdsService);
                return false;
            }

            OnInterCallShowPassCapping(m_AdsService);

            if (m_AdsService.IsInterstitialReady())
            {
                OnInterCallShowAdsReady(m_AdsService);

                _lastTimeShowFullScreenAd = _timer;
                _lastTimeShowInterAd = _timer;
                m_AdsService.ShowInterstitial();
                return true;
            }
            else
            {
                OnInterCallShowAdsNotReady(m_AdsService);
                m_AdsService.ScheduleReloadInterstitial(gameObject.GetCancellationTokenOnDestroy()).Forget();
                _interSuccessCallback?.Invoke();
                return false;
            }
        }

        public void ShowReward(Action successCallback, Action failCallback, string placement, string reward)
        {
            _rewardPlacement = placement;
            _reward = reward;

            if (!CanShowFullScreenAds())
            {
                return;
            }

            OnRewardCallShow(m_AdsService);

            if (m_AdsService.IsRewardReady())
            {
                _rewardSuccessCallback = successCallback;
                _rewardFailCallback = failCallback;
                _rewardPlacement = placement;

                _lastTimeShowFullScreenAd = _timer;
                OnRewardCallShowAdsReady(m_AdsService);
                m_AdsService.ShowReward();
            }
            else
            {
                OnRewardCallShowAdsNotReady(m_AdsService);
            }
        }

        public void ShowBanner()
        {
            m_AdsService.ShowBanner();
        }

        public void HideBanner()
        {
            m_AdsService.HideBanner();
        }

        #region Event Handler API

        void IncrementAccumulateEvent(string key, out int current)
        {
            current = PlayerPrefs.GetInt(key, 1);
            PlayerPrefs.SetInt(key, current + 1);
        }

        Dictionary<string, string> EventParametersToDictionary(EventParameter[] parameters)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(parameters.Length);
            foreach (var eventParameter in parameters)
            {
                dictionary.TryAdd(eventParameter.key, eventParameter.value.ToString());
            }

            return dictionary;
        }

        void HandleLogFirebaseEvent(string eventName, params EventParameter[] parameters)
        {
            if (parameters.Length > 0)
            {
                FirebaseManager.Instance.LogEvent(eventName, parameters);
            }
            else
            {
                FirebaseManager.Instance.LogEvent(eventName);
            }
        }

        void HandleLogMmpEvent(string eventName, params EventParameter[] parameters)
        {
            MmpManager.Instance.TrackCustomEvent(eventName, EventParametersToDictionary(parameters));
        }

        void ProcessEventBuilder<T1>(T1 param, Func<T1, EventBuilderResult> factory,
            Action<string, EventParameter[]> logAction)
        {
            var result = factory(param);
            if (result.fail) return;
            logAction.Invoke(result.eventName, result.parameters);
        }

        void HandleLogIncrementalEvent(string sourceId, string placement, IAdsService adsService,
            Func<EventInfo, EventBuilderResult> firebaseBuilder, Func<EventInfo, EventBuilderResult> mmpBuilder)
        {
            IncrementAccumulateEvent(sourceId, out var current);
            EventInfo eventInfo = new EventInfo()
            {
                accumulatedCount = current,
                connectionString = IsConnectToInternetString(),
                placement = placement,
                mediation = adsService.Mediation,
            };
            ProcessEventBuilder(eventInfo, firebaseBuilder, HandleLogFirebaseEvent);
            ProcessEventBuilder(eventInfo, mmpBuilder, HandleLogMmpEvent);
        }

        void HandleLogIncrementalBannerEvent(string sourceId, bool isCollapsible, IAdsService adsService,
            Func<BannerEventInfo, EventBuilderResult> firebaseBuilder, Func<BannerEventInfo, EventBuilderResult> mmpBuilder)
        {
            IncrementAccumulateEvent(sourceId, out var current);
            BannerEventInfo eventInfo = new BannerEventInfo()
            {
                accumulatedCount = current,
                connectionString = IsConnectToInternetString(),
                placement = "",
                mediation = adsService.Mediation,
                isCollapsible = isCollapsible,
            };
            ProcessEventBuilder(eventInfo, firebaseBuilder, HandleLogFirebaseEvent);
            ProcessEventBuilder(eventInfo, mmpBuilder, HandleLogMmpEvent);
        }

        void HandleLogIncrementalErrorEvent(string sourceId, string placement, string error, IAdsService adsService,
            Func<EventErrorInfo, EventBuilderResult> firebaseBuilder, Func<EventErrorInfo, EventBuilderResult> mmpBuilder)
        {
            IncrementAccumulateEvent(sourceId, out var current);
            EventErrorInfo eventInfo = new EventErrorInfo()
            {
                accumulatedCount = current,
                connectionString = IsConnectToInternetString(),
                error = error,
                placement = placement,
                mediation = adsService.Mediation
            };

            ProcessEventBuilder(eventInfo, firebaseBuilder, HandleLogFirebaseEvent);
            ProcessEventBuilder(eventInfo, mmpBuilder, HandleLogMmpEvent);
        }

        void HandleLogIncrementalErrorBannerEvent(string sourceId, bool isCollapsible, string error,
            IAdsService adsService,
            Func<EventErrorInfo, EventBuilderResult> firebaseBuilder, Func<EventErrorInfo, EventBuilderResult> mmpBuilder)
        {
            IncrementAccumulateEvent(sourceId, out var current);
            EventErrorInfo eventInfo = new EventErrorInfo()
            {
                accumulatedCount = current,
                connectionString = IsConnectToInternetString(),
                error = error,
                mediation = adsService.Mediation,
                isCollapsible = isCollapsible,
            };

            ProcessEventBuilder(eventInfo, firebaseBuilder, HandleLogFirebaseEvent);
            ProcessEventBuilder(eventInfo, mmpBuilder, HandleLogMmpEvent);
        }

        void HandleLogIncrementalInterEvent(string sourceId, IAdsService adsService,
            Func<EventInfo, EventBuilderResult> firebaseBuilder, Func<EventInfo, EventBuilderResult> mmpBuilder)
        {
            IncrementAccumulateEvent(sourceId, out var current);
            EventInfo eventInfo = new EventInfo()
            {
                accumulatedCount = current,
                connectionString = IsConnectToInternetString(),
                placement = _interPlacement,
                mediation = adsService.Mediation
            };
            ProcessEventBuilder(eventInfo, firebaseBuilder, HandleLogFirebaseEvent);
            ProcessEventBuilder(eventInfo, mmpBuilder, HandleLogMmpEvent);
        }

        void HandleLogIncrementalRewardEvent(string sourceId, IAdsService adsService,
            Func<RewardEventInfo, EventBuilderResult> firebaseBuilder, Func<RewardEventInfo, EventBuilderResult> mmpBuilder)
        {
            IncrementAccumulateEvent(sourceId, out var current);
            RewardEventInfo eventInfo = new RewardEventInfo()
            {
                accumulatedCount = current,
                connectionString = IsConnectToInternetString(),
                placement = _rewardPlacement,
                reward = _reward,
                mediation = adsService.Mediation,
            };
            ProcessEventBuilder(eventInfo, firebaseBuilder, HandleLogFirebaseEvent);
            ProcessEventBuilder(eventInfo, mmpBuilder, HandleLogMmpEvent);
        }

        #endregion

        #region Base Ads events

        void OnInterDisplayed(IAdsService adsService)
        {
            string sourceId = "InterDisplayed";
            HandleLogIncrementalInterEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnInterDisplayed, m_AdsEventMmpBuilder.OnInterDisplayed);
        }

        void OnInterDisplayedFail(string error, IAdsService adsService)
        {
            string sourceId = "InterDisplayedFail";
            HandleLogIncrementalErrorEvent(sourceId, _interPlacement, error, adsService,
                m_AdsEventFirebaseBuilder.OnInterDisplayedFailed, m_AdsEventMmpBuilder.OnInterDisplayedFailed);
        }

        void OnInterHidden(IAdsService adsService)
        {
            string sourceId = "InterHidden";
            HandleLogIncrementalInterEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnInterHidden, m_AdsEventMmpBuilder.OnInterHidden);
        }

        void OnInterClicked(IAdsService adsService)
        {
            string sourceId = "InterClicked";
            HandleLogIncrementalInterEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnInterClicked, m_AdsEventMmpBuilder.OnInterClicked);
        }

        void OnInterLoadRequest(IAdsService adsService)
        {
            string sourceId = "InterLoadRequest";
            HandleLogIncrementalInterEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnInterLoadRequest, m_AdsEventMmpBuilder.OnInterLoadRequest);
        }

        void OnInterLoadedSuccess(IAdsService adsService)
        {
            string sourceId = "InterLoadedSuccess";
            HandleLogIncrementalInterEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnInterLoadedSuccess, m_AdsEventMmpBuilder.OnInterLoadedSuccess);
        }

        void OnInterLoadedFail(string error, IAdsService adsService)
        {
            string sourceId = "InterLoadedFail";
            HandleLogIncrementalErrorEvent(sourceId, _interPlacement, error, adsService,
                m_AdsEventFirebaseBuilder.OnInterLoadedFail, m_AdsEventMmpBuilder.OnInterLoadedFail);
        }

        void OnRewardDisplayed(IAdsService adsService)
        {
            string sourceId = "RewardDisplayed";
            HandleLogIncrementalRewardEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnRewardDisplayed, m_AdsEventMmpBuilder.OnRewardDisplayed);
        }

        void OnRewardDisplayedFail(string error, IAdsService adsService)
        {
            ActionUtility.StartActionOnMainThread((() =>
            {
                _rewardSuccessCallback?.Invoke();
                _rewardSuccessCallback = null;
            })).Forget();

            string sourceId = "RewardDisplayedFail";
            HandleLogIncrementalErrorEvent(sourceId, _rewardPlacement, error, adsService,
                m_AdsEventFirebaseBuilder.OnRewardDisplayedFailed, m_AdsEventMmpBuilder.OnRewardDisplayedFailed);
        }

        void OnRewardReceive(IAdsService adsService)
        {
            ActionUtility.StartActionOnMainThread((() =>
            {
                _rewardSuccessCallback?.Invoke();
                _rewardSuccessCallback = null;
            })).Forget();

            string sourceId = "RewardReceive";
            HandleLogIncrementalRewardEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnRewardReceive, m_AdsEventMmpBuilder.OnRewardReceive);
        }

        void OnRewardHidden(IAdsService adsService)
        {
            string sourceId = "RewardHidden";
            HandleLogIncrementalRewardEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnRewardReceive, m_AdsEventMmpBuilder.OnRewardReceive);
        }

        void OnRewardClicked(IAdsService adsService)
        {
            string sourceId = "RewardClicked";
            HandleLogIncrementalRewardEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnRewardClicked, m_AdsEventMmpBuilder.OnRewardClicked);
        }

        void OnRewardLoadRequest(IAdsService adsService)
        {
            string sourceId = "RewardLoadRequest";
            HandleLogIncrementalRewardEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnRewardLoadRequest, m_AdsEventMmpBuilder.OnRewardLoadRequest);
        }

        void OnRewardLoadedSuccess(IAdsService adsService)
        {
            string sourceId = "RewardLoadedSuccess";
            HandleLogIncrementalRewardEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnRewardLoadedSuccess, m_AdsEventMmpBuilder.OnRewardLoadedSuccess);
        }

        void OnRewardLoadedFail(string error, IAdsService adsService)
        {
            string sourceId = "RewardLoadedFail";
            HandleLogIncrementalErrorEvent(sourceId, _rewardPlacement, error, adsService,
                m_AdsEventFirebaseBuilder.OnRewardLoadedFail, m_AdsEventMmpBuilder.OnRewardLoadedFail);
        }

        void OnBannerDisplayed(bool isCollapsible, IAdsService adsService)
        {
            string sourceId = "BannerDisplayed";
            HandleLogIncrementalBannerEvent(sourceId, isCollapsible, adsService,
                m_AdsEventFirebaseBuilder.OnBannerDisplayed, m_AdsEventMmpBuilder.OnBannerDisplayed);
        }

        void OnBannerHidden(bool isCollapsible, IAdsService adsService)
        {
            string sourceId = "BannerHidden";
            HandleLogIncrementalBannerEvent(sourceId, isCollapsible, adsService,
                m_AdsEventFirebaseBuilder.OnBannerHidden, m_AdsEventMmpBuilder.OnBannerHidden);
        }

        void OnBannerClicked(bool isCollapsible, IAdsService adsService)
        {
            string sourceId = "BannerClicked";
            HandleLogIncrementalBannerEvent(sourceId, isCollapsible, adsService,
                m_AdsEventFirebaseBuilder.OnBannerClicked, m_AdsEventMmpBuilder.OnBannerClicked);
        }

        void OnBannerLoadedFail(string error, bool isCollapsible, IAdsService adsService)
        {
            string sourceId = "BannerLoadedFail";
            HandleLogIncrementalErrorBannerEvent(sourceId, isCollapsible, error, adsService,
                m_AdsEventFirebaseBuilder.OnBannerLoadedFail, m_AdsEventMmpBuilder.OnBannerLoadedFail);
        }

        void OnBannerLoadedSuccess(bool isCollapsible, IAdsService adsService)
        {
            string sourceId = "BannerLoadedSuccess";
            HandleLogIncrementalBannerEvent(sourceId, isCollapsible, adsService,
                m_AdsEventFirebaseBuilder.OnBannerLoadedSuccess, m_AdsEventMmpBuilder.OnBannerLoadedSuccess);
        }

        void OnAOADisplayed(IAdsService adsService)
        {
            string sourceId = "AOADisplayed";
            HandleLogIncrementalEvent(sourceId, "", adsService, m_AdsEventFirebaseBuilder.OnAOADisplayed, m_AdsEventMmpBuilder.OnAOADisplayed);
        }

        void OnAOAHidden(IAdsService adsService)
        {
            string sourceId = "AOAHidden";
            HandleLogIncrementalEvent(sourceId, "", adsService, m_AdsEventFirebaseBuilder.OnAOAHidden, m_AdsEventMmpBuilder.OnAOAHidden);
        }

        void OnAOAClicked(IAdsService adsService)
        {
            string sourceId = "AOAClicked";
            HandleLogIncrementalEvent(sourceId, "", adsService, m_AdsEventFirebaseBuilder.OnAOAClicked, m_AdsEventMmpBuilder.OnAOAClicked);
        }

        void OnAOALoadedSuccess(IAdsService adsService)
        {
            string sourceId = "AOALoadedSuccess";
            HandleLogIncrementalEvent(sourceId, "", adsService, m_AdsEventFirebaseBuilder.OnAOALoadedSuccess, m_AdsEventMmpBuilder.OnAOALoadedSuccess);
        }

        void OnAOALoadedFail(string error, IAdsService adsService)
        {
            string sourceId = "AOALoadedFail";
            HandleLogIncrementalErrorEvent(sourceId, "", error, adsService, m_AdsEventFirebaseBuilder.OnAOALoadedFail, m_AdsEventMmpBuilder.OnAOALoadedFail);
        }

        void OnMrecDisplayed(IAdsService adsService)
        {
            string sourceId = "MrecDisplayed";
            HandleLogIncrementalEvent(sourceId, "", adsService, m_AdsEventFirebaseBuilder.OnMrecDisplayed, m_AdsEventMmpBuilder.OnMrecDisplayed);
        }

        void OnMrecClicked(IAdsService adsService)
        {
            string sourceId = "MrecClicked";
            HandleLogIncrementalEvent(sourceId, "", adsService, m_AdsEventFirebaseBuilder.OnMrecClicked, m_AdsEventMmpBuilder.OnMrecClicked);
        }

        void OnMrecLoadedFail(string error, IAdsService adsService)
        {
            string sourceId = "MrecLoadedFail";
            HandleLogIncrementalErrorEvent(sourceId, "", error, adsService, m_AdsEventFirebaseBuilder.OnMrecLoadedFail, m_AdsEventMmpBuilder.OnMrecLoadedFail);
        }

        void OnMrecLoadedSuccess(IAdsService adsService)
        {
            string sourceId = "MrecLoadedSuccess";
            HandleLogIncrementalEvent(sourceId, "", adsService, m_AdsEventFirebaseBuilder.OnMrecLoadedSuccess, m_AdsEventMmpBuilder.OnMrecLoadedSuccess);
        }

        void OnAdPaid(AdsValue adsValue)
        {
            var result = m_AdsEventFirebaseBuilder.OnAdPaid(adsValue);
            if (result.fail) return;
            HandleLogFirebaseEvent(result.eventName, result.parameters);
            MmpManager.Instance.TrackAdEvent(adsValue);
            //HandleLogMmpEvent(result.eventName, result.parameters);
        }

        #endregion

        #region Ads logic base events

        void OnInterCallShow(IAdsService adsService)
        {
            string sourceId = "InterCallShow";
            HandleLogIncrementalEvent(sourceId, _interPlacement, adsService, m_AdsEventFirebaseBuilder.OnInterCallShow, m_AdsEventMmpBuilder.OnInterCallShow);
        }

        void OnInterCallShowPassCapping(IAdsService adsService)
        {
            string sourceId = "InterCallShowPassCapping";
            HandleLogIncrementalEvent(sourceId, _interPlacement, adsService,
                m_AdsEventFirebaseBuilder.OnInterCallShowPassCapping, m_AdsEventMmpBuilder.OnInterCallShowPassCapping);
        }

        void OnInterCallShowFailCapping(IAdsService adsService)
        {
            string sourceId = "InterCallShowFailCapping";
            HandleLogIncrementalEvent(sourceId, _interPlacement, adsService,
                m_AdsEventFirebaseBuilder.OnInterCallShowFailCapping, m_AdsEventMmpBuilder.OnInterCallShowFailCapping);
        }

        void OnInterCallShowAdsReady(IAdsService adsService)
        {
            string sourceId = "InterCallShowAdsReady";
            HandleLogIncrementalEvent(sourceId, _interPlacement, adsService,
                m_AdsEventFirebaseBuilder.OnInterCallShowAdsReady, m_AdsEventMmpBuilder.OnInterCallShowAdsReady);
        }

        void OnInterCallShowAdsNotReady(IAdsService adsService)
        {
            string sourceId = "InterCallShowAdsNotReady";
            HandleLogIncrementalEvent(sourceId, _interPlacement, adsService,
                m_AdsEventFirebaseBuilder.OnInterCallShowAdsNotReady, m_AdsEventMmpBuilder.OnInterCallShowAdsNotReady);
        }

        void OnRewardCallShow(IAdsService adsService)
        {
            string sourceId = "RewardCallShow";
            HandleLogIncrementalRewardEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnRewardCallShow, m_AdsEventMmpBuilder.OnRewardCallShow);
        }

        void OnRewardCallShowAdsReady(IAdsService adsService)
        {
            string sourceId = "RewardCallShowAdsReady";
            HandleLogIncrementalRewardEvent(sourceId, adsService, m_AdsEventFirebaseBuilder.OnRewardCallShowAdsReady, m_AdsEventMmpBuilder.OnRewardCallShowAdsReady);
        }

        void OnRewardCallShowAdsNotReady(IAdsService adsService)
        {
            string sourceId = "RewardCallShowAdsNotReady";
            HandleLogIncrementalRewardEvent(sourceId, adsService,
                m_AdsEventFirebaseBuilder.OnRewardCallShowAdsNotReady, m_AdsEventMmpBuilder.OnRewardCallShowAdsNotReady);
        }

        #endregion


        static string IsConnectToInternetString()
        {
            return Application.internetReachability != NetworkReachability.NotReachable ? "online" : "offline";
        }

        public static EventParameter GetInternetEvent()
        {
            return new EventParameter("connection", IsConnectToInternetString());
        }
    }
}
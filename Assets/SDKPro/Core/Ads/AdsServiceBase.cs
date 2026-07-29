using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.GDPR;
using UnityEngine;

namespace SDKPro.Core.Ads
{
    public abstract class AdsServiceBase : IAdsService, IDisposable
    {
        public Action OnAdServiceInitializeFinished { get; set; }
        public Action OnInterLoadRequest { get; set; }
        public Action OnInterLoadedSuccess { get; set; }
        public Action<string> OnInterLoadedFail { get; set; }
        public Action OnInterClicked { get; set; }
        public Action OnInterDisplayed { get; set; }
        public Action<string> OnInterDisplayedFail { get; set; }
        public Action OnInterHidden { get; set; }
        public Action OnRewardLoadRequest { get; set; }
        public Action OnRewardLoadedSuccess { get; set; }
        public Action<string> OnRewardLoadedFail { get; set; }
        public Action OnRewardClicked { get; set; }
        public Action OnRewardDisplayed { get; set; }
        public Action<string> OnRewardDisplayedFail { get; set; }
        public Action OnRewardAdClose { get; set; }
        public Action OnRewardReceive { get; set; }
        
        public Action<bool> OnBannerDisplayed { get; set; }
        public Action<bool> OnBannerHidden { get; set; }
        public Action<bool> OnBannerClicked { get; set; }
        public Action<bool,string> OnBannerLoadedFail { get; set; }
        public Action<bool> OnBannerLoadedSuccess { get; set; }
        
        public Action OnAOADisplayed { get; set; }
        public Action<string> OnAOADisplayedFail { get; set; }
        public Action OnAOAHidden { get; set; }
        public Action OnAOAClicked { get; set; }
        public Action<string> OnAOALoadedFail { get; set; }
        public Action OnAOALoadedSuccess { get; set; }
        
        public Action OnMrecDisplayed { get; set; }
        public Action OnMrecClicked { get; set; }
        public Action<string> OnMrecLoadedFail { get; set; }
        public Action OnMrecLoadedSuccess { get; set; }
        
        public Action<AdsValue> OnAdsPaid { get; set; }
        
        public abstract string Mediation { get; }
        public abstract AdsServiceCapabilities Capabilities { get; }
        public bool IsGdprFlowCompleted { get; private set; }
        
        protected bool _isScheduleReloadReward;
        protected bool _isScheduleReloadInter;

        protected int _rewardRetryAttempt = 0;
        protected int _interRetryAttempt = 0;

        protected CancellationTokenSource m_SessionToken;
        private bool _isProviderInitialized;
        
        public bool IsInit { get; protected set; }
        public async UniTaskVoid ScheduleReloadInterstitial(CancellationToken token)
        {
            if(_isScheduleReloadInter) return;
            _isScheduleReloadInter = true;

            _interRetryAttempt = Mathf.Min(_interRetryAttempt + 1, 5);
            float duration = Mathf.Pow(2f, _interRetryAttempt);

            await UniTask.WaitForSeconds(duration, cancellationToken: token);
            _isScheduleReloadInter = false;
            
            if(IsInterstitialReady()) return;
            LoadInterstitial();
        }
        public async UniTaskVoid ScheduleReloadReward(CancellationToken token)
        {
            if(_isScheduleReloadReward) return;
            _isScheduleReloadReward = true;

            _rewardRetryAttempt = Mathf.Min(_rewardRetryAttempt + 1, 5);
            float duration = Mathf.Pow(2f, _rewardRetryAttempt);

            await UniTask.WaitForSeconds(duration, cancellationToken: token);
            _isScheduleReloadReward = false;
            
            if(IsRewardReady()) return;
            LoadReward();
        }

        public async UniTask Init(
            AdsLoadSetting adsLoadSetting,
            IGDPR defaultGdpr,
            CancellationToken token)
        {
            if (IsInit)
            {
                return;
            }

            if (adsLoadSetting == null)
            {
                throw new ArgumentNullException(nameof(adsLoadSetting));
            }

            EnsureSessionToken(token);

            if (Capabilities.Supports(AdsServiceCapabilities.GdprDuringInitialization))
            {
                // The provider contract guarantees that InitInternal returns
                // only after its integrated GDPR flow is complete.
                await EnsureProviderInitialized(m_SessionToken.Token);
                IsGdprFlowCompleted = true;
            }
            else
            {
                if (defaultGdpr == null)
                {
                    throw new InvalidOperationException(
                        $"{Mediation} does not handle GDPR during initialization. " +
                        "Assign a default GoogleGDPRProxy before initializing this service.");
                }

                await defaultGdpr.WaitForConsent(m_SessionToken.Token);
                IsGdprFlowCompleted = true;
                await EnsureProviderInitialized(m_SessionToken.Token);
            }
            
            if(adsLoadSetting.loadInter) LoadInterstitial();
            if(adsLoadSetting.loadReward) LoadReward();
            if (adsLoadSetting.loadBanner)
            {
                CreateBanner();
                if(adsLoadSetting.hideBannerWhenFirstCreated) HideBanner();
                LoadBanner();
            }

            if (adsLoadSetting.loadMrec)
            {
                CreateMrec();
                if(adsLoadSetting.hideMrecWhenFirstCreated) HideMrec();
                LoadMrec();
            }

            if (adsLoadSetting.loadAOA)
            {
                LoadAOA();
            }
            
            IsInit = true;
        }

        protected abstract UniTask InitInternal(CancellationToken token);

        private async UniTask EnsureProviderInitialized(CancellationToken token)
        {
            if (_isProviderInitialized)
            {
                return;
            }

            await InitInternal(token);
            _isProviderInitialized = true;
        }

        private void EnsureSessionToken(CancellationToken token)
        {
            if (m_SessionToken != null)
            {
                return;
            }

            m_SessionToken = token.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(token)
                : new CancellationTokenSource();
        }

        public virtual void Dispose()
        {
            if (m_SessionToken != null)
            {
                if (!m_SessionToken.IsCancellationRequested)
                {
                    m_SessionToken.Cancel();
                }

                m_SessionToken.Dispose();
            }

            m_SessionToken = null;
            _isProviderInitialized = false;
            IsGdprFlowCompleted = false;
            IsInit = false;
        }

        public abstract void UpdateUserID(string id);

        public abstract void LoadInterstitial();
        public abstract bool IsInterstitialReady();
        public abstract void ShowInterstitial();
        public abstract void LoadReward();
        public abstract bool IsRewardReady();
        public abstract void ShowReward();
        public abstract void CreateBanner();
        public abstract void LoadBanner();
        public virtual void LoadBanner(BannerRequest request)
        {
            WarnIfCollapsibleBannerIsUnsupported(request);
            LoadBanner();
        }

        public abstract void ShowBanner();
        public virtual void ShowBanner(BannerRequest request)
        {
            WarnIfCollapsibleBannerIsUnsupported(request);
            ShowBanner();
        }

        public abstract void HideBanner();
        public abstract void DestroyBanner();
        public abstract void CreateMrec();
        public abstract void LoadMrec();
        public abstract void ShowMrec();
        public abstract void HideMrec();
        public abstract bool IsMrecReady();
        public abstract void SetMrecPosition(Vector2 dpPos);
        public abstract void LoadAOA();
        public abstract void ShowAOA();
        public abstract bool IsAOAReady();

        private void WarnIfCollapsibleBannerIsUnsupported(BannerRequest request)
        {
            if (request.collapsible &&
                !Capabilities.Supports(AdsServiceCapabilities.CollapsibleBanner))
            {
                Debug.LogWarning(
                    $"{Mediation} does not support collapsible banners. Falling back to a normal banner.");
            }
        }
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Parameters;
using SDKPro.Core.Services.Interfaces;
using UnityEngine;

namespace SDKPro.Core.Services
{
    public abstract class AdServiceBase : IAdService, IDisposable
    {
        public Action OnAdServiceInitializeFinished { get; set; }
        public Action OnInterLoadRequest { get; set; }
        public Action OnInterLoadedSuccess { get; set; }
        public Action OnInterLoadedFail { get; set; }
        public Action OnInterClicked { get; set; }
        public Action OnInterAdDisplay { get; set; }
        public Action OnInterAdDisplayFail { get; set; }
        public Action OnInterAdClose { get; set; }
        public Action OnRewardLoadRequest { get; set; }
        public Action OnRewardLoadedSuccess { get; set; }
        public Action OnRewardLoadedFail { get; set; }
        public Action OnRewardClicked { get; set; }
        public Action OnRewardAdDisplay { get; set; }
        public Action OnRewardAdDisplayFail { get; set; }
        public Action OnRewardAdClose { get; set; }
        public Action OnRewardReceive { get; set; }
        public Action OnBannerClicked { get; set; }
        public Action OnBannerDisplayed { get; set; }
        public Action OnAOADisplay { get; set; }
        
        public Action<AdValue> OnAdPaid { get; set; }
        
        protected bool _isScheduleReloadReward;
        protected bool _isScheduleReloadInter;

        protected int _rewardRetryAttempt = 0;
        protected int _interRetryAttempt = 0;

        private CancellationTokenSource m_SessionToken;
        
        public bool IsInit { get; protected set; }
        public async UniTaskVoid ScheduleReloadInterstitial(CancellationToken token)
        {
            if(_isScheduleReloadInter) return;
            _isScheduleReloadInter = true;

            _interRetryAttempt = Mathf.Min(_interRetryAttempt + 1, 5);
            float duration = Mathf.Pow(2f, _interRetryAttempt);

            await UniTask.WaitForSeconds(duration);
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

            await UniTask.WaitForSeconds(duration);
            _isScheduleReloadReward = false;
            
            if(IsRewardReady()) return;
            LoadReward();
        }

        public async UniTask Init(AdsLoadSetting adsLoadSetting)
        {
            m_SessionToken = new CancellationTokenSource();
            await InitInternal(m_SessionToken.Token);
            
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

        public void Dispose()
        {
            if (m_SessionToken is {IsCancellationRequested:false})
            {
                m_SessionToken.Cancel();
                m_SessionToken.Dispose();
            }
        }

        public abstract void LoadInterstitial();
        public abstract bool IsInterstitialReady();
        public abstract void ShowInterstitial();
        public abstract void LoadReward();
        public abstract bool IsRewardReady();
        public abstract void ShowReward();
        public abstract void CreateBanner();
        public abstract void LoadBanner();
        public abstract void ShowBanner();
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
    }
}
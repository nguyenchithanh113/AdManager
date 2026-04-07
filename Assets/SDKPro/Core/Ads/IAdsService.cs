using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SDKPro.Core.Ads
{
    public interface IAdsService
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
        public Action OnAOAHidden { get; set; }
        public Action OnAOAClicked { get; set; }
        public Action<string> OnAOALoadedFail { get; set; }
        public Action OnAOALoadedSuccess { get; set; }
        
        public Action OnMrecDisplayed { get; set; }
        public Action OnMrecClicked { get; set; }
        public Action<string> OnMrecLoadedFail { get; set; }
        public Action OnMrecLoadedSuccess { get; set; }

        public Action<AdsValue> OnAdsPaid { get; set; }
        
        public string Mediation { get; }
        
        public UniTask Init(AdsLoadSetting adsLoadSetting);

        public UniTaskVoid ScheduleReloadInterstitial(CancellationToken token);
        public UniTaskVoid ScheduleReloadReward(CancellationToken token);

        public void LoadInterstitial();
        public bool IsInterstitialReady();
        public void ShowInterstitial();
        
        public void LoadReward();
        public bool IsRewardReady();
        public void ShowReward();

        public void CreateBanner();
        public void LoadBanner();
        public void ShowBanner();
        public void HideBanner();

        public void DestroyBanner();

        public void CreateMrec();
        public void LoadMrec();
        public void ShowMrec();
        public void HideMrec();
        public bool IsMrecReady();
        public void SetMrecPosition(Vector2 dpPos);
        
        public void LoadAOA();
        public void ShowAOA();

        public bool IsAOAReady();
    }
}
using System;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Parameters;
using UnityEngine;

namespace SDKPro.Core.Services.Interfaces
{
    public interface IAdService
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
        
        public UniTask Init(AdsLoadSetting adsLoadSetting);

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
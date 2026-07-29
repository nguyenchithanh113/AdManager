using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;
using UnityEngine;

namespace SDKPro.Core.Mockups
{
    public sealed class FakeAdsService : AdsServiceBase
    {
        private readonly FakeAdsConfig _config;
        private readonly FakeAdsOverlay _overlay;

        private bool _interstitialReady;
        private bool _rewardReady;
        private bool _bannerReady;
        private bool _mrecReady;
        private bool _appOpenReady;
        private bool _bannerVisible;
        private bool _mrecVisible;
        private bool _showBannerAfterLoad;
        private BannerRequest _bannerRequest;

        public override string Mediation => "Fake";

        public override AdsServiceCapabilities Capabilities
        {
            get
            {
                AdsServiceCapabilities capabilities =
                    AdsServiceCapabilities.Interstitial |
                    AdsServiceCapabilities.Rewarded |
                    AdsServiceCapabilities.Banner |
                    AdsServiceCapabilities.Mrec |
                    AdsServiceCapabilities.AppOpen;

                if (_config.simulateCollapsibleBanner)
                {
                    capabilities |= AdsServiceCapabilities.CollapsibleBanner;
                }

                if (_config.simulateGdprDuringInitialization)
                {
                    capabilities |= AdsServiceCapabilities.GdprDuringInitialization;
                }

                return capabilities;
            }
        }

        public FakeAdsService(FakeAdsConfig config, FakeAdsOverlay overlay)
        {
            _config = config ?? new FakeAdsConfig();
            _overlay = overlay;
        }

        protected override async UniTask InitInternal(CancellationToken token)
        {
            await Delay(_config.initializationDelay, token);
            OnAdServiceInitializeFinished?.Invoke();
        }

        public override void UpdateUserID(string id)
        {
        }

        public override void LoadInterstitial()
        {
            OnInterLoadRequest?.Invoke();
            LoadInterstitialAsync().Forget();
        }

        private async UniTaskVoid LoadInterstitialAsync()
        {
            await DelayForLoad();
            _interstitialReady = !_config.failInterstitialLoad;
            if (_interstitialReady)
            {
                _interRetryAttempt = 0;
                OnInterLoadedSuccess?.Invoke();
            }
            else
            {
                OnInterLoadedFail?.Invoke("Fake interstitial load failure");
            }
        }

        public override bool IsInterstitialReady() => _interstitialReady;

        public override void ShowInterstitial()
        {
            if (!_interstitialReady || _config.failFullscreenDisplay)
            {
                _interstitialReady = false;
                OnInterDisplayedFail?.Invoke("Fake interstitial display failure");
                LoadInterstitial();
                return;
            }

            _interstitialReady = false;
            OnInterDisplayed?.Invoke();
            EmitPaid(AdType.Interstitial, "fake_interstitial");
            _overlay.ShowInterstitial(
                () => OnInterClicked?.Invoke(),
                () =>
                {
                    OnInterHidden?.Invoke();
                    LoadInterstitial();
                });
        }

        public override void LoadReward()
        {
            OnRewardLoadRequest?.Invoke();
            LoadRewardAsync().Forget();
        }

        private async UniTaskVoid LoadRewardAsync()
        {
            await DelayForLoad();
            _rewardReady = !_config.failRewardLoad;
            if (_rewardReady)
            {
                _rewardRetryAttempt = 0;
                OnRewardLoadedSuccess?.Invoke();
            }
            else
            {
                OnRewardLoadedFail?.Invoke("Fake rewarded load failure");
            }
        }

        public override bool IsRewardReady() => _rewardReady;

        public override void ShowReward()
        {
            if (!_rewardReady || _config.failFullscreenDisplay)
            {
                _rewardReady = false;
                OnRewardDisplayedFail?.Invoke("Fake rewarded display failure");
                LoadReward();
                return;
            }

            _rewardReady = false;
            OnRewardDisplayed?.Invoke();
            EmitPaid(AdType.Reward, "fake_rewarded");
            _overlay.ShowRewarded(
                () => OnRewardClicked?.Invoke(),
                () =>
                {
                    OnRewardAdClose?.Invoke();
                    LoadReward();
                },
                () => OnRewardReceive?.Invoke());
        }

        public override void CreateBanner()
        {
            _bannerReady = false;
        }

        public override void LoadBanner()
        {
            LoadBanner(BannerRequest.Standard);
        }

        public override void LoadBanner(BannerRequest request)
        {
            _bannerRequest = NormalizeBannerRequest(request);
            LoadBannerAsync().Forget();
        }

        private async UniTaskVoid LoadBannerAsync()
        {
            await DelayForLoad();
            _bannerReady = !_config.failBannerLoad;
            if (_bannerReady)
            {
                OnBannerLoadedSuccess?.Invoke(_bannerRequest.collapsible);
                if (_showBannerAfterLoad)
                {
                    _showBannerAfterLoad = false;
                    ShowLoadedBanner();
                }
            }
            else
            {
                _showBannerAfterLoad = false;
                OnBannerLoadedFail?.Invoke(
                    _bannerRequest.collapsible,
                    "Fake banner load failure");
            }
        }

        public override void ShowBanner()
        {
            if (_bannerReady)
            {
                ShowLoadedBanner();
            }
        }

        public override void ShowBanner(BannerRequest request)
        {
            _showBannerAfterLoad = true;
            LoadBanner(request);
        }

        private void ShowLoadedBanner()
        {
            _bannerVisible = true;
            _overlay.ShowBanner(
                _bannerRequest.collapsible,
                () => OnBannerClicked?.Invoke(_bannerRequest.collapsible));
            OnBannerDisplayed?.Invoke(_bannerRequest.collapsible);
            EmitPaid(
                _bannerRequest.collapsible ? AdType.BannerCollapsible : AdType.Banner,
                "fake_banner");
        }

        public override void HideBanner()
        {
            if (!_bannerVisible)
            {
                return;
            }

            _bannerVisible = false;
            _overlay.HideBanner();
            OnBannerHidden?.Invoke(_bannerRequest.collapsible);
        }

        public override void DestroyBanner()
        {
            HideBanner();
            _bannerReady = false;
        }

        public override void CreateMrec()
        {
            _mrecReady = false;
        }

        public override void LoadMrec()
        {
            LoadMrecAsync().Forget();
        }

        private async UniTaskVoid LoadMrecAsync()
        {
            await DelayForLoad();
            _mrecReady = !_config.failMrecLoad;
            if (_mrecReady)
            {
                OnMrecLoadedSuccess?.Invoke();
            }
            else
            {
                OnMrecLoadedFail?.Invoke("Fake MREC load failure");
            }
        }

        public override void ShowMrec()
        {
            if (!_mrecReady)
            {
                return;
            }

            _mrecVisible = true;
            _overlay.ShowMrec(() => OnMrecClicked?.Invoke());
            OnMrecDisplayed?.Invoke();
            EmitPaid(AdType.Mrec, "fake_mrec");
        }

        public override void HideMrec()
        {
            if (!_mrecVisible)
            {
                return;
            }

            _mrecVisible = false;
            _overlay.HideMrec();
        }

        public override bool IsMrecReady() => _mrecReady;

        public override void SetMrecPosition(Vector2 dpPos)
        {
            _overlay.SetMrecPosition(dpPos);
        }

        public override void LoadAOA()
        {
            LoadAoaAsync().Forget();
        }

        private async UniTaskVoid LoadAoaAsync()
        {
            await DelayForLoad();
            _appOpenReady = !_config.failAppOpenLoad;
            if (_appOpenReady)
            {
                OnAOALoadedSuccess?.Invoke();
            }
            else
            {
                OnAOALoadedFail?.Invoke("Fake app-open load failure");
            }
        }

        public override void ShowAOA()
        {
            if (!_appOpenReady || _config.failFullscreenDisplay)
            {
                _appOpenReady = false;
                OnAOADisplayedFail?.Invoke("Fake app-open display failure");
                LoadAOA();
                return;
            }

            _appOpenReady = false;
            OnAOADisplayed?.Invoke();
            EmitPaid(AdType.AOA, "fake_app_open");
            _overlay.ShowAppOpen(
                () => OnAOAClicked?.Invoke(),
                () =>
                {
                    OnAOAHidden?.Invoke();
                    LoadAOA();
                });
        }

        public override bool IsAOAReady() => _appOpenReady;

        public override void Dispose()
        {
            _overlay.Clear();
            base.Dispose();
        }

        private BannerRequest NormalizeBannerRequest(BannerRequest request)
        {
            if (request.collapsible && !_config.simulateCollapsibleBanner)
            {
                Debug.LogWarning(
                    "FakeAdsService collapsible simulation is disabled. Falling back to a normal fake banner.");
                request.collapsible = false;
            }

            return request;
        }

        private async UniTask DelayForLoad()
        {
            CancellationToken token = m_SessionToken != null
                ? m_SessionToken.Token
                : CancellationToken.None;
            await Delay(_config.loadDelay, token);
        }

        private static async UniTask Delay(float seconds, CancellationToken token)
        {
            if (seconds <= 0f)
            {
                token.ThrowIfCancellationRequested();
                return;
            }

            await UniTask.WaitForSeconds(seconds, cancellationToken: token);
        }

        private void EmitPaid(AdType adType, string identifier)
        {
            OnAdsPaid?.Invoke(new AdsValue
            {
                value = _config.simulatedRevenue,
                adType = adType,
                adPlatform = Mediation,
                adNetwork = Mediation,
                adIdentifier = identifier,
                adCurrency = "USD"
            });
        }
    }
}

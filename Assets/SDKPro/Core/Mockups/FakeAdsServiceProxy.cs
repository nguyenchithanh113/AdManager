using SDKPro.Core.Ads;
using SDKPro.Core.Ads.Proxy;
using UnityEngine;

namespace SDKPro.Core.Mockups
{
    [DisallowMultipleComponent]
    public sealed class FakeAdsServiceProxy : AdsServiceProxy
    {
        [SerializeField] private FakeAdsConfig m_Config = new FakeAdsConfig();

        private FakeAdsService _service;
        private AdsLoadSetting _runtimeLoadSetting;

        public override IAdsService GetService()
        {
            if (_service != null)
            {
                return _service;
            }

            FakeAdsOverlay overlay = GetComponent<FakeAdsOverlay>();
            if (overlay == null)
            {
                overlay = gameObject.AddComponent<FakeAdsOverlay>();
            }

            _service = new FakeAdsService(m_Config, overlay);
            return _service;
        }

        public override AdsLoadSetting GetAdsLoadSetting()
        {
            if (AdsLoadSetting != null)
            {
                return AdsLoadSetting;
            }

            if (_runtimeLoadSetting == null)
            {
                _runtimeLoadSetting = ScriptableObject.CreateInstance<AdsLoadSetting>();
                _runtimeLoadSetting.loadInter = true;
                _runtimeLoadSetting.loadReward = true;
                _runtimeLoadSetting.loadBanner = true;
                _runtimeLoadSetting.loadMrec = true;
                _runtimeLoadSetting.loadAOA = true;
                _runtimeLoadSetting.hideBannerWhenFirstCreated = true;
                _runtimeLoadSetting.hideMrecWhenFirstCreated = true;
            }

            return _runtimeLoadSetting;
        }

        private void OnDestroy()
        {
            _service?.Dispose();
            _service = null;

            if (_runtimeLoadSetting != null)
            {
                Destroy(_runtimeLoadSetting);
                _runtimeLoadSetting = null;
            }
        }
    }
}

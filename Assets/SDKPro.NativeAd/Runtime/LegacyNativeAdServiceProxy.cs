using SDKPro.Core.NativeAds;
using UnityEngine;

namespace SDKPro.NativeAd
{
    [DisallowMultipleComponent]
    public sealed class LegacyNativeAdServiceProxy : NativeAdServiceProxy
    {
        [SerializeField] private LegacyNativeAdSlot[] m_Slots;

        private LegacyNativeAdService _service;

        public override INativeAdService GetService()
        {
            return _service ??= new LegacyNativeAdService(m_Slots);
        }

        private void OnDestroy()
        {
            _service?.Dispose();
            _service = null;
        }
    }
}

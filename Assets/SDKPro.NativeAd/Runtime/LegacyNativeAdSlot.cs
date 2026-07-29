using System;
using UnityEngine;

namespace SDKPro.NativeAd
{
    public abstract class LegacyNativeAdSlot : MonoBehaviour
    {
        [SerializeField] private string m_Placement = "default";

        public string Placement => m_Placement;
        public bool IsReady { get; private set; }

        public event Action<LegacyNativeAdSlot> Loaded;
        public event Action<LegacyNativeAdSlot, string> LoadFailed;
        public event Action<LegacyNativeAdSlot> Displayed;
        public event Action<LegacyNativeAdSlot> Hidden;
        public event Action<LegacyNativeAdSlot> Clicked;

        public abstract void Load();
        public abstract void Show();
        public abstract void Hide();
        public abstract void DestroyAd();

        protected void RaiseLoaded()
        {
            IsReady = true;
            Loaded?.Invoke(this);
        }

        protected void RaiseLoadFailed(string error)
        {
            IsReady = false;
            LoadFailed?.Invoke(this, error);
        }

        protected void RaiseDisplayed()
        {
            Displayed?.Invoke(this);
        }

        protected void RaiseHidden()
        {
            Hidden?.Invoke(this);
        }

        protected void RaiseClicked()
        {
            Clicked?.Invoke(this);
        }

        protected void MarkConsumed()
        {
            IsReady = false;
        }
    }
}

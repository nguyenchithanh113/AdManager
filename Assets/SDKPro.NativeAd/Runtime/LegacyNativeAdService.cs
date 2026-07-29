using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SDKPro.Core.NativeAds;
using UnityEngine;

namespace SDKPro.NativeAd
{
    public sealed class LegacyNativeAdService : INativeAdService
    {
        private readonly IReadOnlyList<LegacyNativeAdSlot> _configuredSlots;
        private readonly Dictionary<string, LegacyNativeAdSlot> _slots = new();

        public Action OnInitialized { get; set; }
        public Action<string> OnLoaded { get; set; }
        public Action<string, string> OnLoadFailed { get; set; }
        public Action<string> OnDisplayed { get; set; }
        public Action<string> OnHidden { get; set; }
        public Action<string> OnClicked { get; set; }

        public LegacyNativeAdService(IReadOnlyList<LegacyNativeAdSlot> configuredSlots)
        {
            _configuredSlots = configuredSlots;
        }

        public UniTask Init()
        {
            DisposeSlots();
            _slots.Clear();

            if (_configuredSlots != null)
            {
                foreach (LegacyNativeAdSlot slot in _configuredSlots)
                {
                    if (slot == null || string.IsNullOrWhiteSpace(slot.Placement))
                    {
                        continue;
                    }

                    if (_slots.ContainsKey(slot.Placement))
                    {
                        Debug.LogWarning(
                            $"Duplicate legacy native-ad placement '{slot.Placement}' was ignored.");
                        continue;
                    }

                    _slots.Add(slot.Placement, slot);
                    slot.Loaded += HandleLoaded;
                    slot.LoadFailed += HandleLoadFailed;
                    slot.Displayed += HandleDisplayed;
                    slot.Hidden += HandleHidden;
                    slot.Clicked += HandleClicked;
                }
            }

            OnInitialized?.Invoke();
            return UniTask.CompletedTask;
        }

        public void Load(string placement)
        {
            if (TryGetSlot(placement, out LegacyNativeAdSlot slot))
            {
                slot.Load();
            }
        }

        public bool IsReady(string placement)
        {
            return TryGetSlot(placement, out LegacyNativeAdSlot slot, false) &&
                   slot.IsReady;
        }

        public void Show(string placement)
        {
            if (TryGetSlot(placement, out LegacyNativeAdSlot slot))
            {
                slot.Show();
            }
        }

        public void Hide(string placement)
        {
            if (TryGetSlot(placement, out LegacyNativeAdSlot slot))
            {
                slot.Hide();
            }
        }

        public void Destroy(string placement)
        {
            if (TryGetSlot(placement, out LegacyNativeAdSlot slot))
            {
                slot.DestroyAd();
            }
        }

        public void Dispose()
        {
            DisposeSlots();
            _slots.Clear();
        }

        private bool TryGetSlot(
            string placement,
            out LegacyNativeAdSlot slot,
            bool logMissing = true)
        {
            if (_slots.TryGetValue(placement ?? string.Empty, out slot))
            {
                return true;
            }

            if (logMissing)
            {
                Debug.LogWarning(
                    $"No legacy native-ad slot is configured for placement '{placement}'.");
            }

            return false;
        }

        private void DisposeSlots()
        {
            foreach (LegacyNativeAdSlot slot in _slots.Values)
            {
                if (slot == null)
                {
                    continue;
                }

                slot.Loaded -= HandleLoaded;
                slot.LoadFailed -= HandleLoadFailed;
                slot.Displayed -= HandleDisplayed;
                slot.Hidden -= HandleHidden;
                slot.Clicked -= HandleClicked;
            }
        }

        private void HandleLoaded(LegacyNativeAdSlot slot)
        {
            OnLoaded?.Invoke(slot.Placement);
        }

        private void HandleLoadFailed(LegacyNativeAdSlot slot, string error)
        {
            OnLoadFailed?.Invoke(slot.Placement, error);
        }

        private void HandleDisplayed(LegacyNativeAdSlot slot)
        {
            OnDisplayed?.Invoke(slot.Placement);
        }

        private void HandleHidden(LegacyNativeAdSlot slot)
        {
            OnHidden?.Invoke(slot.Placement);
        }

        private void HandleClicked(LegacyNativeAdSlot slot)
        {
            OnClicked?.Invoke(slot.Placement);
        }
    }
}

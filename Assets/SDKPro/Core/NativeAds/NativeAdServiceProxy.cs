using UnityEngine;

namespace SDKPro.Core.NativeAds
{
    public abstract class NativeAdServiceProxy : MonoBehaviour
    {
        public abstract INativeAdService GetService();
    }
}

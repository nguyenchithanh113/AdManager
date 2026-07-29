using System;
using Cysharp.Threading.Tasks;

namespace SDKPro.Core.NativeAds
{
    public interface INativeAdService : IDisposable
    {
        Action OnInitialized { get; set; }
        Action<string> OnLoaded { get; set; }
        Action<string, string> OnLoadFailed { get; set; }
        Action<string> OnDisplayed { get; set; }
        Action<string> OnHidden { get; set; }
        Action<string> OnClicked { get; set; }

        UniTask Init();
        void Load(string placement);
        bool IsReady(string placement);
        void Show(string placement);
        void Hide(string placement);
        void Destroy(string placement);
    }
}

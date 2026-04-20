using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;

namespace SDKPro.Core.Mmp
{
    public interface IMmpService
    {
        public UniTask Init();
        public string GetUserID();
        public void TrackAdEvent(AdsValue adsValue);
        public void TrackCustomEvent(string eventKey, Dictionary<string, string> eventValues);
        public void TrackUninstallToken(string token);
        public void Dispose();
    }
}
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;
using SDKPro.Core.Mmp;

namespace SDKPro.Core.Mockups
{
    public class DummyMmp : IMmpService
    {
        public async UniTask Init()
        {
            
        }

        public string GetUserID()
        {
            return "";
        }

        public void TrackAdEvent(AdsValue adsValue)
        {
            
        }

        public void TrackCustomEvent(string eventKey, Dictionary<string, string> eventValues)
        {
            
        }

        public void TrackUninstallToken(string token)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}
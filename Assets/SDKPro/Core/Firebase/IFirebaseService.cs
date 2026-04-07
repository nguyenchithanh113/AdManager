using System;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;

namespace SDKPro.Core.Firebase
{
    public interface IFirebaseService
    {
        public UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider);
        public Action OnInit { get; } 

        public void LogEvent(string eventName, params EventParameter[] parameters);
        public void LogUniqueEvent(string eventName, params EventParameter[] parameters);
        public void LogAdPaidEvent(AdsValueEvent adsValue);
    }
}
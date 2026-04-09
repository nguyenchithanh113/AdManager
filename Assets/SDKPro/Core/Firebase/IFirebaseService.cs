using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;

namespace SDKPro.Core.Firebase
{
    public interface IFirebaseService
    {
        public UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider, CancellationToken token);
        public Action OnInit { get; } 

        public void LogEvent(string eventName, params EventParameter[] parameters);
        public void LogEvent(string eventName);
        public void LogUniqueEvent(string eventName, params EventParameter[] parameters);
        public void LogUniqueEvent(string eventName);

        public void Dispose();
    }
}
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;

namespace SDKPro.Core.Firebase
{
    public interface IFirebaseService
    {
        public delegate void OnFetchFailHandler(string error);
        public delegate void OnFetchSuccessHandler();
        public delegate void OnTokenReceivedHandler(string token);
        
        public UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider, CancellationToken token);
        public Action OnInit { get; set; } 
        public Action OnStartFetchingConfig { get; set; }
        public event OnFetchFailHandler OnFetchFail;
        public event OnFetchSuccessHandler OnFetchSuccess;
        public event OnTokenReceivedHandler OnTokenReceived;

        public void LogEvent(string eventName, params EventParameter[] parameters);
        public void LogEvent(string eventName);
        public void LogUniqueEvent(string eventName, params EventParameter[] parameters);
        public void LogUniqueEvent(string eventName);

        public void Dispose();
    }
}
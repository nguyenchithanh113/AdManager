using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using SDKPro.Core.Utilities;

namespace SDKPro.Core.Firebase
{
    public struct TokenResult
    {
        public bool fetched;
        public string value;
    }
    
    public class FirebaseManager : Singleton<FirebaseManager>
    {
        public FirebaseServiceProxyBase ServiceProxy;

        private IFirebaseService m_Service;

        public Action onStartFetchingConfig;
        public event IFirebaseService.OnFetchFailHandler onFetchFail;
        public event IFirebaseService.OnFetchSuccessHandler onFetchSuccess;
        //public event IFirebaseService.OnTokenReceivedHandler onTokenReceived;
        public ReactiveProperty<TokenResult> TokenResult => m_Service.TokenResult;

        public Action onInit;

        public Action onLoggingEvent;

        public async UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider, CancellationToken token)
        {
            m_Service = ServiceProxy.Get();

            m_Service.OnFetchFail += onFetchFail;
            m_Service.OnFetchSuccess += onFetchSuccess;
            m_Service.OnInit += onInit;
            m_Service.OnStartFetchingConfig += onStartFetchingConfig;

            await m_Service.Init(remoteConfigVariableProvider, token);
        }

        public void LogEvent(string eventName, params EventParameter[] parameters)
        {
            m_Service.LogEvent(eventName, parameters);
            onLoggingEvent?.Invoke();
        }

        public void LogEvent(string eventName)
        {
            m_Service.LogEvent(eventName);
            onLoggingEvent?.Invoke();
        }


        public void LogUniqueEvent(string eventName, params EventParameter[] parameters)
        {
            m_Service.LogUniqueEvent(eventName, parameters);
            onLoggingEvent?.Invoke();
        }


        public void LogUniqueEvent(string eventName)
        {
            m_Service.LogUniqueEvent(eventName);
            onLoggingEvent?.Invoke();
        }
            
    }
}
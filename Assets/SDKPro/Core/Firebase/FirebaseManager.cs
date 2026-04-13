using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Utilities;

namespace SDKPro.Core.Firebase
{
    public class FirebaseManager : Singleton<FirebaseManager>
    {
        public FirebaseServiceProxyBase ServiceProxy;

        private IFirebaseService m_Service;

        public Action onStartFetchingConfig;
        public event IFirebaseService.OnFetchFailHandler onFetchFail;
        public event IFirebaseService.OnFetchSuccessHandler onFetchSuccess;

        public async UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider, CancellationToken token)
        {
            m_Service = ServiceProxy.Get();

            m_Service.OnFetchFail += onFetchFail;
            m_Service.OnFetchSuccess += onFetchSuccess;
            m_Service.OnStartFetchingConfig += onStartFetchingConfig;

            await m_Service.Init(remoteConfigVariableProvider, token);
        }

        public void LogEvent(string eventName, params EventParameter[] parameters) =>
            m_Service.LogEvent(eventName, parameters);
        public void LogEvent(string eventName) =>
            m_Service.LogEvent(eventName);

        public void LogUniqueEvent(string eventName, params EventParameter[] parameters) =>
            m_Service.LogUniqueEvent(eventName, parameters);
        
        public void LogUniqueEvent(string eventName) =>
            m_Service.LogUniqueEvent(eventName);
    }
}
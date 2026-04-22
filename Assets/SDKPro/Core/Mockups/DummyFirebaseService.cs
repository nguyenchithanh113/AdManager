using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using SDKPro.Core.Firebase;

namespace SDKPro.Core.Mockups
{
    public class DummyFirebaseService : IFirebaseService
    {
        private IRemoteConfigVariableProvider m_RemoteConfigVariableProvider;
        private Dictionary<string, object> m_RemoteVariableMap = new();
        public async UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider, CancellationToken token)
        {
            m_RemoteConfigVariableProvider = remoteConfigVariableProvider;
            m_RemoteVariableMap =
                RemoteConfigVariableProviderHelper.ToDictionary(m_RemoteConfigVariableProvider.GetVariableInfos());
            
            m_RemoteConfigVariableProvider.Update(new UpdateResult()
            {
                resultValues = m_RemoteVariableMap,
                success = true,
            });
            OnStartFetchingConfig?.Invoke();
            OnFetchSuccess?.Invoke();
            OnInit?.Invoke();
        }

        public Action OnInit { get; set; }
        public Action OnStartFetchingConfig { get; set; }
        public event IFirebaseService.OnFetchFailHandler OnFetchFail;
        public event IFirebaseService.OnFetchSuccessHandler OnFetchSuccess;

        public ReactiveProperty<TokenResult> TokenResult { get; } =
            new ReactiveProperty<TokenResult>(new TokenResult() { fetched = false, value = "" });

        public void LogEvent(string eventName, params EventParameter[] parameters)
        {
            
        }

        public void LogEvent(string eventName)
        {
            
        }

        public void LogUniqueEvent(string eventName, params EventParameter[] parameters)
        {
            
        }

        public void LogUniqueEvent(string eventName)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}
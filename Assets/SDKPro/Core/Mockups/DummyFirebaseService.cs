using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Firebase;

namespace SDKPro.Core.Mockups
{
    public class DummyFirebaseService : IFirebaseService
    {
        public async UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider, CancellationToken token)
        {
            
        }

        public Action OnInit { get; }
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
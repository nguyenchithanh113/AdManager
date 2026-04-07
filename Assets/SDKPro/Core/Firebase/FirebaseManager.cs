using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Utilities;

namespace SDKPro.Core.Firebase
{
    public class FirebaseManager : Singleton<FirebaseManager>
    {
        public FirebaseServiceProxy m_ServiceProxy;

        private IFirebaseService m_Service;

        public async UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider, CancellationToken token)
        {
            m_Service = m_ServiceProxy.Get();

            await m_Service.Init(remoteConfigVariableProvider);
        }

        public void LogEvent(string eventName, params EventParameter[] parameters) =>
            m_Service.LogEvent(eventName, parameters);

        public void LogUniqueEvent(string eventName, params EventParameter[] parameters) =>
            m_Service.LogUniqueEvent(eventName, parameters);
    }
}
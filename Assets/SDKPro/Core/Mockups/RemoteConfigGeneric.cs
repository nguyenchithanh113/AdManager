using System.Collections.Generic;
using R3;
using SDKPro.Core.Firebase;

namespace SDKPro.Core.Mockups
{
    public class RemoteConfigGeneric<T> : IRemoteConfigVariableProvider where T : RemoteConfigGeneric<T>, new()
    {
        public enum Status
        {
            Pending,
            Success,
            FailAndUseDefaultValues
        }
        
        public class UpdateInfo
        {
            public Status status;
            public T instance;
        }
        
        public ReactiveProperty<UpdateInfo> updateInfo = new();
        
        private static T m_Instance;
        
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new T();
                    m_Instance.updateInfo.Value = new UpdateInfo()
                    {
                        instance = m_Instance,
                        status = Status.Pending
                    };
                }

                return m_Instance;
            }
        }
        
        public List<RemoteVariableInfo> GetVariableInfos()
        {
            return RemoteConfigVariableProviderHelper.BuildVariableInfos<T>(this as T);
        }

        public void Update(UpdateResult updateResult)
        {
            RemoteConfigVariableProviderHelper.Update(updateResult.resultValues, this);
            updateInfo.Value = new UpdateInfo()
            {
                instance = this as T,
                status = updateResult.success ? Status.Success : Status.FailAndUseDefaultValues
            };
        }
    }
}
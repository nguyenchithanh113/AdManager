using System.Collections.Generic;
using R3;
using SDKPro.Core.Attributes;
using SDKPro.Core.Firebase;

namespace SDKPro.Core.Mockups
{
    public class RemoteConfigTemplate : IRemoteConfigVariableProvider
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
            public RemoteConfigTemplate instance;
        }
        
        [RemoteVariable]
        public int interCapping = 30;

        public ReactiveProperty<UpdateInfo> updateInfo = new();
        
        private static RemoteConfigTemplate m_Instance;
        
        public static RemoteConfigTemplate Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new RemoteConfigTemplate();
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
            return RemoteConfigVariableProviderHelper.BuildVariableInfos<RemoteConfigTemplate>();
        }

        public void Update(UpdateResult updateResult)
        {
            RemoteConfigVariableProviderHelper.Update(updateResult.resultValues, this);
            updateInfo.Value = new UpdateInfo()
            {
                instance = this,
                status = updateResult.success ? Status.Success : Status.FailAndUseDefaultValues
            };
        }
    }
}
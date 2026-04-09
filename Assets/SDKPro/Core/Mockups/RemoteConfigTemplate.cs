using System.Collections.Generic;
using R3;
using SDKPro.Core.Attributes;
using SDKPro.Core.Firebase;

namespace SDKPro.Core.Mockups
{
    public class RemoteConfigTemplate : IRemoteConfigVariableProvider
    {
        [RemoteVariable]
        public int interCapping = 30;

        public Subject<RemoteConfigTemplate> OnUpdate = new();
        
        private static RemoteConfigTemplate m_Instance;
        
        public static RemoteConfigTemplate Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new RemoteConfigTemplate();
                }

                return m_Instance;
            }
        }

        public List<RemoteVariableInfo> GetVariableInfos()
        {
            return RemoteConfigVariableProviderHelper.BuildVariableInfos<RemoteConfigTemplate>();
        }

        public void Update(List<RemoteVariableInfo> updatedValues)
        {
            RemoteConfigVariableProviderHelper.Update(updatedValues, this);
            OnUpdate.OnNext(this); 
        }

        public void Update(Dictionary<string, object> updatedValues)
        {
            RemoteConfigVariableProviderHelper.Update(updatedValues, this);
            OnUpdate.OnNext(this); 
        }
    }
}
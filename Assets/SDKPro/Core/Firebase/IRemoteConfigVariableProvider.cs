using System.Collections.Generic;

namespace SDKPro.Core.Firebase
{
    public interface IRemoteConfigVariableProvider
    {
        public List<RemoteVariableInfo> GetVariableInfos();

        public void Update(List<RemoteVariableInfo> updatedValues);
    }

    public struct RemoteVariableInfo
    {
        public string key;
        public object boxedValue;
    }

    public enum VariableType
    {
        String,
        Boolean,
        Float
    }
}
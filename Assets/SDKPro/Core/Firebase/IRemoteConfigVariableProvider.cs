using System.Collections.Generic;

namespace SDKPro.Core.Firebase
{
    public interface IRemoteConfigVariableProvider
    {
        public List<RemoteVariableInfo> GetVariableInfos();
    }

    public struct RemoteVariableInfo
    {
        public string key;
        public object boxedValue;
        public VariableType variableType;
    }

    public enum VariableType
    {
        String,
        Boolean,
        Float
    }
}
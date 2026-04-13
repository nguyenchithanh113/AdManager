using System.Collections.Generic;

namespace SDKPro.Core.Firebase
{
    public interface IRemoteConfigVariableProvider
    {
        public List<RemoteVariableInfo> GetVariableInfos();
        
        public void Update(UpdateResult updateResult);
    }

    public class UpdateResult
    {
        public Dictionary<string, object> resultValues;
        public bool success;
        public string error;
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
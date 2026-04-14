using System.Collections.Generic;
using System.Reflection;
using SDKPro.Core.Attributes;

namespace SDKPro.Core.Firebase
{
    public static class RemoteConfigVariableProviderHelper
    {
        public static List<RemoteVariableInfo> BuildVariableInfos<T>(T target)
            where T : class, IRemoteConfigVariableProvider
        {
            List<RemoteVariableInfo> variableInfos = new();
            
            var fieldInfos = typeof(T).GetFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                var fieldInfo = fieldInfos[i];

                if (fieldInfo.GetCustomAttribute<RemoteVariableAttribute>() != null)
                {
                    var fieldName = fieldInfo.Name;
                    object value = fieldInfo.GetValue(target);
                    RemoteVariableInfo info = new RemoteVariableInfo()
                    {
                        key = fieldName,
                        boxedValue = value
                    };
                    variableInfos.Add(info);
                }
            }

            return variableInfos;
        }

        public static Dictionary<string, object> ToDictionary(List<RemoteVariableInfo> variableInfos)
        {
            Dictionary<string, object> map = new();
            foreach (var remoteVariableInfo in variableInfos)
            {
                map.Add(remoteVariableInfo.key, remoteVariableInfo.boxedValue);
            }

            return map;
        }

        public static void Update<T>(List<RemoteVariableInfo> variableInfos, T provider)
            where T : class, IRemoteConfigVariableProvider
        {
            Dictionary<string, object>
                map = new Dictionary<string, object>(variableInfos.Count);
            foreach (var remoteVariableInfo in variableInfos)
            {
                map.Add(remoteVariableInfo.key, remoteVariableInfo.boxedValue);
            }
            
            var fieldInfos = typeof(T).GetFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                var fieldInfo = fieldInfos[i];

                if (fieldInfo.GetCustomAttribute<RemoteVariableAttribute>() != null)
                {
                    if (map.TryGetValue(fieldInfo.Name, out var value))
                    {
                        fieldInfo.SetValue(provider, value);
                    }
                }
            }
        }
        
        public static void Update<T>(Dictionary<string, object> variableInfos, T provider)
            where T : class, IRemoteConfigVariableProvider
        {
            var fieldInfos = typeof(T).GetFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                var fieldInfo = fieldInfos[i];

                if (fieldInfo.GetCustomAttribute<RemoteVariableAttribute>() != null)
                {
                    if (variableInfos.TryGetValue(fieldInfo.Name, out var value))
                    {
                        fieldInfo.SetValue(provider, value);
                    }
                }
            }
        }
    }
}
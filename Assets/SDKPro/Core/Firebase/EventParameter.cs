using System.Collections.Generic;
using UnityEngine;

namespace SDKPro.Core.Firebase
{
    public struct EventParameter
    {
        public object value;
        public string key;

        public EventParameter(string key, object value)
        {
            this.key = key;
            this.value = value;

            if (value is not int && value is not string && value is not double && value is not float && value is not long)
            {
                Debug.LogError($"Value is unsupported type {value.GetType()}");
            }
        }
        

        public static explicit operator EventParameter(KeyValuePair<string, string> keyValuePair) =>
            new EventParameter(keyValuePair.Key, keyValuePair.Value);
    }
}
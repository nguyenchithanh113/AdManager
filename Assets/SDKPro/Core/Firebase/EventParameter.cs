using System.Collections.Generic;

namespace SDKPro.Core.Firebase
{
    public struct EventParameter
    {
        public string value;
        public string key;

        public EventParameter(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public static explicit operator EventParameter(KeyValuePair<string, string> keyValuePair) =>
            new EventParameter(keyValuePair.Key, keyValuePair.Value);
    }
}
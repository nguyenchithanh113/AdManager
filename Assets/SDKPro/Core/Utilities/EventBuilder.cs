using System;
using System.Collections.Generic;

namespace SDKPro.Core.Utilities
{
    public struct EventBuilder
    {
        public string key;
        private Func<string> valueGetter;
        public string Value => valueGetter();

        public KeyValuePair<string, string> ToKeyValuePair() => new KeyValuePair<string, string>(key, Value);

        public EventBuilder(string key, Func<string> valueGetter)
        {
            this.key = key;
            this.valueGetter = valueGetter;
        }
    }
}
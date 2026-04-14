using System.Collections.Generic;
using R3;
using SDKPro.Core.Attributes;
using SDKPro.Core.Firebase;

namespace SDKPro.Core.Mockups
{
    public class RemoteConfigTemplate : RemoteConfigGeneric<RemoteConfigTemplate>
    {
        [RemoteVariable]
        public int interCapping = 30;
    }
}
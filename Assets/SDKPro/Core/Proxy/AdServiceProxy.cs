using SDKPro.Core.Services.Interfaces;
using UnityEngine;

namespace SDKPro.Core.Proxy
{
    public abstract class AdServiceProxy : MonoBehaviour
    {
        public abstract IAdService GetService();
    }
}
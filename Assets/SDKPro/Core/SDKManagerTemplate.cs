using Cysharp.Threading.Tasks;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Core
{
    public class SDKManagerTemplate : Singleton<SDKManagerTemplate>
    {
        public async UniTask StartAsync()
        {
            var token = gameObject.GetCancellationTokenOnDestroy();
        }
    }
}
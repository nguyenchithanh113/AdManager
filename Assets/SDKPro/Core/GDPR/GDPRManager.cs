using System.Threading;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Core.GDPR
{
    public class GDPRManager : Singleton<GDPRManager>
    {
        [SerializeField] private GDPRProxy m_GDPRProxy;

        public async UniTask Init(CancellationToken token)
        {
            await m_GDPRProxy.Get().WaitForConsent(token);
        }
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SDKPro.Core.GDPR
{
    public interface IGDPR
    {
        public UniTask WaitForConsent(CancellationToken token);
    }
}
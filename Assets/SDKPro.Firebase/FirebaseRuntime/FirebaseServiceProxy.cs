using SDKPro.Core.Firebase;
using UnityEngine;

namespace SDKPro.FirebaseRuntime
{
    public class FirebaseServiceProxy : FirebaseServiceProxyBase
    {
        [SerializeField] private bool m_VerboseLogging;
        protected override IFirebaseService Create()
        {
            return new FirebaseService(m_VerboseLogging);
        }
    }
}
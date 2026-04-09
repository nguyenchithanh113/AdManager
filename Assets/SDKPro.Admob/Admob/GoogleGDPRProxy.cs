using SDKPro.Core.GDPR;
using UnityEngine;

namespace SDKPro.Admob
{
    public class GoogleGDPRProxy : GDPRProxy
    {
        [SerializeField] private GoogleGDPR m_GoogleGdpr;
        
        protected override IGDPR Create()
        {
            return m_GoogleGdpr;
        }
    }
}
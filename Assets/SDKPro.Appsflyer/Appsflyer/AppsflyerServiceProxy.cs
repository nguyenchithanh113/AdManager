using SDKPro.Core.Mmp;
using UnityEngine;

namespace SDKPro.Appsflyer
{
    public class AppsflyerServiceProxy : MmpServiceProxy
    {
        [SerializeField] AppsflyerService m_AppsflyerService;

        protected override IMmpService Create()
        {
            return m_AppsflyerService;
        }
    }
}
using System;
using SDKPro.Core.Utilities;
using UnityEngine;

namespace SDKPro.Core.Providers
{
    public class TimeProvider : MonoBehaviour
    {
        public float Timer { get; private set; }

        private bool m_Paused;

        private void Update()
        {
            if (m_Paused) return;
            Timer += Time.unscaledDeltaTime;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                ActionUtility.StartActionDelay((() =>
                {
                    m_Paused = false;
                }), 0.5f).Forget();
            }
            else
            {
                m_Paused = true;
            }
        }
    }
}
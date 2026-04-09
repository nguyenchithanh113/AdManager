using UnityEngine;

namespace SDKPro.Core
{
    public abstract class Proxy<T> : MonoBehaviour where T : class
    {
        private T m_Target;
        
        public virtual T Get()
        {
            if (m_Target == null)
            {
                m_Target = Create();
            }

            return m_Target;
        }

        protected abstract T Create();
    }
}
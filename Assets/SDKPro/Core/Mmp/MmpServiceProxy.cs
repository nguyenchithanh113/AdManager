using UnityEngine;

namespace SDKPro.Core.Mmp
{
    public abstract class MmpServiceProxy : MonoBehaviour
    {
        public abstract IMmpService Get();
    }
}
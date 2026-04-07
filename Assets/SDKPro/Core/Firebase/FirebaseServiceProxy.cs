using UnityEngine;

namespace SDKPro.Core.Firebase
{
    public abstract class FirebaseServiceProxy : MonoBehaviour
    {
        public abstract IFirebaseService Get();
    }
}
using SDKPro.Core.Firebase;

namespace SDKPro.FirebaseRuntime
{
    public class FirebaseServiceProxy : FirebaseServiceProxyBase
    {
        protected override IFirebaseService Create()
        {
            return new FirebaseService();
        }
    }
}
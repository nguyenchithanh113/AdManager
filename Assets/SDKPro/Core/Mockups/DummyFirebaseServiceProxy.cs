using SDKPro.Core.Firebase;

namespace SDKPro.Core.Mockups
{
    public class DummyFirebaseServiceProxy : FirebaseServiceProxyBase
    {
        protected override IFirebaseService Create()
        {
            return new DummyFirebaseService();
        }
    }
}
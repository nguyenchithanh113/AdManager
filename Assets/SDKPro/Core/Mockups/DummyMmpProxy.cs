using SDKPro.Core.Mmp;

namespace SDKPro.Core.Mockups
{
    public class DummyMmpProxy : MmpServiceProxy
    {
        protected override IMmpService Create()
        {
            return new DummyMmp();
        }
    }
}
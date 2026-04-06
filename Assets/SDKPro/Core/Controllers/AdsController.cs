using System;
using SDKPro.Core.Services.Interfaces;

namespace SDKPro.Core.Controllers
{
    public class AdsController
    {
        private IAdService m_AdsService;
        
        private float _timer;
        private float _lastTimeShowFullScreenAd = -100;
        private float _lastTimeShowInterAd = -100;
        private float _lastTimeShowAoa = -100;
        
        private string _interPlacement;
        private string _rewardPlacement;
        private string _reward;
        
        private Action _interSuccessCallback;
        private Action _rewardSuccessCallback;

        private Action _interFailCallback;
        private Action _rewardFailCallback;
        
        public AdsController(IAdService adService)
        {
            m_AdsService = adService;
        }
    }
}
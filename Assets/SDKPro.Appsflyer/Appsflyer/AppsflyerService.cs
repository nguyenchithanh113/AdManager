using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using Cysharp.Threading.Tasks;
using SDKPro.Core.Ads;
using SDKPro.Core.Mmp;
using UnityEngine;

namespace SDKPro.Appsflyer
{
    public class AppsflyerService : MonoBehaviour, IMmpService, IAppsFlyerConversionData,
        IAppsFlyerPurchaseValidation,               // For purchase validation callbacks  
        IAppsFlyerPurchaseRevenueDataSource,        // For StoreKit 1 additional parameters 
        IAppsFlyerPurchaseRevenueDataSourceStoreKit2 // For StoreKit 2 additional parameters 
    {
        // These fields are set from the editor so do not modify!
        //******************************//
        public string devKey;
        public string appID;
        public string UWPAppID;
        public string macOSAppID;
        public bool isDebug;
        public bool getConversionData;
        //******************************//

        public bool initPurchaseConnector = true;

        public async UniTask Init()
        {
            //Note: Must be call after GDPR, user is ensured to agree, otherwise he can't play
            //or at least most logic goes this way.
            
            // These fields are set from the editor so do not modify!
            //******************************//
            AppsFlyer.setIsDebug(isDebug);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
    AppsFlyer.initSDK(devKey, macOSAppID, getConversionData ? this : null);
#else
            AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif
            AppsFlyer.enableTCFDataCollection(true);
            //******************************/

            if (initPurchaseConnector)
            {
                AppsFlyerPurchaseConnector.init(this, Store.GOOGLE);
                AppsFlyerPurchaseConnector.setStoreKitVersion(StoreKitVersion.SK2);
                AppsFlyerPurchaseConnector.setIsSandbox(isDebug);
        
                AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
                    AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
                    AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases
                );
        
                AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
                AppsFlyerPurchaseConnector.setPurchaseRevenueDataSource(this);
                AppsFlyerPurchaseConnector.setPurchaseRevenueDataSourceStoreKit2(this);

                // 3. Build and start
                AppsFlyerPurchaseConnector.build();
                AppsFlyerPurchaseConnector.startObservingTransactions();
            }
            
            //ToDo: probably doesn't need to set consent data manually since we use enableTCFDataCollection
            /*var consent = new AppsFlyerConsent(
                isUserSubjectToGDPR:            true,
                hasConsentForDataUsage:         false,
                hasConsentForAdsPersonalization:false,
                hasConsentForAdStorage:         false
            );

            AppsFlyer.setConsentData(consent);*/
            
            AppsFlyer.startSDK();
            
#if UNITY_IOS
            StartCoroutine(RequestAuthorization());
#endif
        }
        
#if UNITY_IOS
    IEnumerator RequestAuthorization()
    {
      
        using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
        {

            while (!req.IsFinished)
            {
                yield return null;
            }
             if (req.Granted && req.DeviceToken != "")
             {
                  byte[] tokenBytes = ConvertHexStringToByteArray(req.DeviceToken);
                  AppsFlyer.registerUninstall(tokenBytes);
             }
        }
    }

    private byte[] ConvertHexStringToByteArray(string hexString)
    {

        byte[] data = new byte[hexString.Length / 2];
        for (int index = 0; index < data.Length; index++)
        {
            string byteValue = hexString.Substring(index * 2, 2);
            data[index] = System.Convert.ToByte(byteValue, 16);
        }
        return data;
    }
#endif

        public void TrackUninstallToken(string token)
        {
#if UNITY_ANDROID
            AppsFlyer.updateServerUninstallToken(token);
#endif
        }

        public string GetUserID()
        {
            return AppsFlyer.getAppsFlyerId();
        }

        public void TrackAdEvent(AdsValue adsValue)
        {
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AdRevenueScheme.AD_UNIT, adsValue.adIdentifier);
            additionalParams.Add(AdRevenueScheme.AD_TYPE, adsValue.adType.ToString());
            additionalParams.Add(AdRevenueScheme.PLACEMENT, adsValue.placement);
            AFAdRevenueData revenueData = new AFAdRevenueData(adsValue.adNetwork, GetMediationType(adsValue.adPlatform),
                adsValue.adCurrency, adsValue.value);
            AppsFlyer.logAdRevenue(revenueData, additionalParams);
        }

        public void TrackCustomEvent(string eventKey, Dictionary<string, string> eventValues)
        {
            AppsFlyer.sendEvent(eventKey, eventValues);
        }
        
        MediationNetwork GetMediationType(string adPlatform)
        {
            MediationNetwork mediationNetworkType = default;
            
            switch (adPlatform)
            {
                case "Admob":
                    mediationNetworkType = MediationNetwork.GoogleAdMob;
                    break;
                case "Applovin":
                    mediationNetworkType = MediationNetwork.ApplovinMax;
                    break;
                case "IronSource":
                    mediationNetworkType = MediationNetwork.IronSource;
                    break;
                default:
                    mediationNetworkType = MediationNetwork.Unity;
                    Debug.Log($"Fail To detect adPlatform {adPlatform}, default to Unity");
                    break;
            }
        
            return mediationNetworkType;
        }

        public void Dispose()
        {
            
        }
        
        // --- Purchase Revenue Data Sources ---
        public Dictionary<string, object> PurchaseRevenueAdditionalParametersForProducts(
            HashSet<object> products, 
            HashSet<object> transactions)
        {
            return new Dictionary<string, object>
            {
                ["storekit_version"] = "1.0",
                ["additional_param"] = "sk1_value",
                ["product_count"] = products.Count,
                ["transaction_count"] = transactions.Count
            };
        }

        public Dictionary<string, object> PurchaseRevenueAdditionalParametersStoreKit2ForProducts(
            HashSet<object> products, 
            HashSet<object> transactions)
        {
            return new Dictionary<string, object>
            {
                ["storekit_version"] = "2.0", 
                ["additional_param"] = "sk2_value",
                ["product_count"] = products.Count,
                ["transaction_count"] = transactions.Count
            };
        }

        // --- Purchase Validation Callbacks ---
        public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
        {
            AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo);
            Debug.Log("Purchase validation success: " + validationInfo);
        
            // Parse and handle validation info
            var dict = AFMiniJSON.Json.Deserialize(validationInfo) as Dictionary<string, object>;
        
#if UNITY_ANDROID
        if (dict.ContainsKey("productPurchase"))
        {
            Debug.Log("Android in-app purchase validated");
        }
        else if (dict.ContainsKey("subscriptionPurchase"))
        {
            Debug.Log("Android subscription validated");
        }
#endif
        }

        public void didReceivePurchaseRevenueError(string error)
        {
            AppsFlyer.AFLog("didReceivePurchaseRevenueError", error);
            Debug.LogError("Purchase validation error: " + error);
        }
        
        // Mark AppsFlyer CallBacks
        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            // add deferred deeplink logic here
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }
    }
}
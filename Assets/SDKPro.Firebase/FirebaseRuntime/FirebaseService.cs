using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Messaging;
using Firebase.RemoteConfig;
using SDKPro.Core.Firebase;
using UnityEngine;

namespace SDKPro.FirebaseRuntime
{
    public class FirebaseService : IFirebaseService
    {
        private bool m_IsInitalized;

        private IRemoteConfigVariableProvider m_RemoteConfigVariableProvider;
        private Dictionary<string, object> m_RemoteVariableMap = new();

        private bool m_VerboseLogging;

        public FirebaseService(bool verboseLogging)
        {
            m_VerboseLogging = verboseLogging;
        }

        public Action OnStartFetchingConfig { get; set; }
        public event IFirebaseService.OnFetchFailHandler OnFetchFail;
        public event IFirebaseService.OnFetchSuccessHandler OnFetchSuccess;

        public async UniTask Init(IRemoteConfigVariableProvider remoteConfigVariableProvider, CancellationToken token)
        {
            Debug.Log("Firebase Initializing");
            m_RemoteConfigVariableProvider = remoteConfigVariableProvider;
            m_RemoteVariableMap =
                RemoteConfigVariableProviderHelper.ToDictionary(m_RemoteConfigVariableProvider.GetVariableInfos());
            
            await Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    InitializeFirebase();
                    FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(m_RemoteVariableMap)
                        .ContinueWithOnMainThread(task => { FetchDataAsync(); }, token);
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    m_IsInitalized = true;
                    OnInit?.Invoke();
                    Debug.Log("Firebase Initialized");
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            }, token);

            await UniTask.WaitUntil(() => m_IsInitalized, cancellationToken: token);
            OnInit?.Invoke();
        }
        
        protected void InitializeFirebase()
        {
            /*var idfv = MappingUserIdTracking.GetIdfv();
            if (!string.IsNullOrEmpty(idfv))
            {
                FirebaseAnalytics.SetUserId(idfv);
            }*/
#if !UNITY_EDITOR
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Dictionary<ConsentType, ConsentStatus> consentMap = new Dictionary<ConsentType, ConsentStatus>();
            consentMap.Add(ConsentType.AnalyticsStorage, ConsentStatus.Granted);
            consentMap.Add(ConsentType.AdStorage, ConsentStatus.Granted);
            consentMap.Add(ConsentType.AdUserData, ConsentStatus.Granted);
            consentMap.Add(ConsentType.AdPersonalization, ConsentStatus.Granted);

            FirebaseAnalytics.SetConsent(consentMap);
            
            FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            FirebaseMessaging.TokenReceived -= OnTokenReceived;
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
#endif
        }
        
        public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            Debug.Log("Received Registration Token: " + token.Token);
        }

        public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.Log("Received a new message from: " + e.Message.From);
        }

        public void Dispose()
        {
#if !UNITY_EDITOR
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            FirebaseMessaging.TokenReceived -= OnTokenReceived;
#endif
        }

        public Task FetchDataAsync()
        {
            OnStartFetchingConfig?.Invoke();
            Debug.Log("Fetching data...");
            System.Threading.Tasks.Task fetchTask =
                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                    TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }
        
        private void FetchComplete(Task fetchTask)
        {
            string error = "Undefined";
            
            if (fetchTask.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
                error = "Fetch canceled";
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
                error = "Fetch encountered an error";
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }

            var info = FirebaseRemoteConfig.DefaultInstance.Info;

            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                        .ContinueWithOnMainThread(task =>
                        {
                            var keyList = m_RemoteVariableMap.Keys;
                            foreach (var key in keyList)
                            {
                                var remoteConfig = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                                if (remoteConfig.Source == ValueSource.RemoteValue)
                                {
                                    try
                                    {
                                        if (m_RemoteVariableMap[key] is bool)
                                        {
                                            var value = remoteConfig.BooleanValue;
                                            m_RemoteVariableMap[key] = value;
                                            Debug.Log($"Fetch Remote: {key} {value.ToString()}");
                                        }
                                        else if (m_RemoteVariableMap[key] is float)
                                        {
                                            var value = (float)remoteConfig.DoubleValue;
                                            m_RemoteVariableMap[key] = value;
                                            Debug.Log($"Fetch Remote: {key} {value.ToString()}");
                                        }
                                        else if (m_RemoteVariableMap[key] is string)
                                        {
                                            var value = remoteConfig.StringValue;
                                            m_RemoteVariableMap[key] = value;
                                            Debug.Log($"Fetch Remote: {key} {value.ToString()}");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogException(e);
                                        error = e.ToString();
                                    }
                                }
                            }

                            m_RemoteConfigVariableProvider.Update(new UpdateResult()
                            {
                                resultValues = m_RemoteVariableMap,
                                success = true
                            });
                            OnFetchSuccess?.Invoke();
                        });
                    break;
                case LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            SchedulingRefetchRemote().Forget();
                            Debug.Log("Error");
                            error = "Fetch encountered an error";
                            break;
                        case FetchFailureReason.Throttled:
                            SchedulingRefetchRemote().Forget();
                            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            error = "Fetch throttled";
                            break;
                    }

                    break;
                case LastFetchStatus.Pending:
                    Debug.Log("Latest Fetch call still pending.");
                    error = "Latest Fetch call still pending";
                    break;
            }
            
            m_RemoteConfigVariableProvider.Update(new UpdateResult()
            {
                resultValues = m_RemoteVariableMap,
                success = false,
                error = error
            });
            
            OnFetchFail?.Invoke(error);
        }
        
        private bool _isSchedulingRefetch;
        async UniTask SchedulingRefetchRemote()
        {
            if(_isSchedulingRefetch) return;
            _isSchedulingRefetch = true;
            await UniTask.WaitUntil((() => Application.internetReachability != NetworkReachability.NotReachable));
            await UniTask.WaitForSeconds(2f);
            FetchDataAsync();
            _isSchedulingRefetch = false;
        }

        public Action OnInit { get; set; }
        public void LogEvent(string eventName, params EventParameter[] parameters)
        {
            VerboseLogging(eventName, parameters);
            var parsedParams = ParseEventParameters(parameters).ToArray();
            if (parsedParams.Length > 0)
            {
                FirebaseAnalytics.LogEvent(eventName, parsedParams);
            }
            else
            {
                Debug.LogError($"Fail to fire {eventName} due to failing to parse parameters, check again");
            }
        }

        void VerboseLogging(string eventName, EventParameter[] parameters)
        {
            if (!m_VerboseLogging) return;
            StringBuilder builder = new StringBuilder();
            builder.Append($"Event: {eventName}");
            builder.AppendLine("{");

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var valueStr = param.value == null ? "null" : param.value.ToString();
                builder.AppendLine($"{param.key} : {valueStr}");
            }

            builder.AppendLine("}");
            
            Debug.Log(builder.ToString());
        }

        void VerboseLogging(string eventName)
        {
            if (!m_VerboseLogging) return;
            Debug.Log($"Event: {eventName}");
        }

        public void LogEvent(string eventName)
        {
            VerboseLogging(eventName);
            FirebaseAnalytics.LogEvent(eventName);
        }

        public void LogUniqueEvent(string eventName, params EventParameter[] parameters)
        {
            if (PlayerPrefs.GetInt(eventName, 0) == 0)
            {
                VerboseLogging(eventName, parameters);
                var parsedParams = ParseEventParameters(parameters).ToArray();
                if (parsedParams.Length > 0)
                {
                    FirebaseAnalytics.LogEvent(eventName, parsedParams);
                    PlayerPrefs.SetInt(eventName, 1);
                }
                else
                {
                    Debug.LogError($"Fail to fire {eventName} due to failing to parse parameters, check again");
                }
            }
        }

        public void LogUniqueEvent(string eventName)
        {
            if (PlayerPrefs.GetInt(eventName, 0) == 0)
            {
                VerboseLogging(eventName);
                FirebaseAnalytics.LogEvent(eventName);
                PlayerPrefs.SetInt(eventName, 1);
            }
        }


        List<Parameter> ParseEventParameters(EventParameter[] parameters)
        {
            List<Parameter> parsed = new List<Parameter>(parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var key = param.key;

                if (param.value is null)
                {
                    Debug.LogError($"Value with key {key} is null");
                    continue;
                }

                if (param.value is string str)
                {
                    parsed.Add(new Parameter(key, str));
                }
                else if (param.value is int integer)
                {
                    parsed.Add(new Parameter(key, integer));
                }
                else if (param.value is long longInteger)
                {
                    parsed.Add(new Parameter(key, longInteger));
                }
                else if (param.value is double doubleValue)
                {
                    parsed.Add(new Parameter(key, doubleValue));
                }
                else
                {
                    Debug.LogError($"Value type {param.value.GetType()} is not supported");
                }
            }

            return parsed;
        }
    }
}
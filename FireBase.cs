namespace App.RemoteConfig
{
    using Firebase.Extensions;
    using System;
    using System.Threading.Tasks;
    using UnityEngine;

    public
    class FireBase : MonoBehaviour
    {
        Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
        protected bool isFirebaseInitialized = false;

        protected virtual void Start()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                dependencyStatus = task.Result;
                if(dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError(
                      "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }
        void InitializeFirebase()
        {
            System.Collections.Generic.Dictionary<string, object> defaults =
              new System.Collections.Generic.Dictionary<string, object>();

            // These are the values that are used if we haven't fetched data from the
            // server
            // yet, or if we ask for values that the server doesn't have:
            defaults.Add("config_test_string", "default local string");
            defaults.Add("config_test_int", 1);
            defaults.Add("config_test_float", 1.0);
            defaults.Add("config_test_bool", false);

            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
              .ContinueWithOnMainThread(task =>
              {
                  DebugLog("RemoteConfig configured and ready!");
                  isFirebaseInitialized = true;
              });

        }


        // Display the currently loaded data.  If fetch has been called, this will be
        // the data fetched from the server.  Otherwise, it will be the defaults.
        // Note:  Firebase will cache this between sessions, so even if you haven't
        // called fetch yet, if it was called on a previous run of the program, you
        //  will still have data from the last time it was run.
        public void DisplayData()
        {
            DebugLog("Current Data:");
            DebugLog("config_test_string: " +
                     Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                     .GetValue("config_test_string").StringValue);
            DebugLog("config_test_int: " +
                     Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                     .GetValue("config_test_int").LongValue);
            DebugLog("config_test_float: " +
                     Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                     .GetValue("config_test_float").DoubleValue);
            DebugLog("config_test_bool: " +
                     Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                     .GetValue("config_test_bool").BooleanValue);
        }

        public void DisplayAllKeys()
        {
            DebugLog("Current Keys:");
            System.Collections.Generic.IEnumerable<string> keys =
                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Keys;
            foreach(string key in keys) DebugLog("    " + key);
        }

        // FetchAsync only fetches new data if the current data is older than the provided
        // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
        // By default the timespan is 12 hours, and for production apps, this is a good
        // number. For this example though, it's set to a timespan of zero, so that
        // changes in the console will always show up immediately.
        public Task FetchDataAsync()
        {
            DebugLog("Fetching data...");
            System.Threading.Tasks.Task fetchTask =
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }

        void FetchComplete(Task fetchTask)
        {
            if(fetchTask.IsCanceled)
            {
                DebugLog("Fetch canceled.");
            }
            else if(fetchTask.IsFaulted)
            {
                DebugLog("Fetch encountered an error.");
            }
            else if(fetchTask.IsCompleted)
            {
                DebugLog("Fetch completed successfully!");
            }

            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            switch(info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        DebugLog(String.Format("Remote data loaded and ready (last fetch time {0}).",
                                   info.FetchTime));
                    });

                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    switch(info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            DebugLog("Fetch failed for unknown reason");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            DebugLog("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    DebugLog("Latest Fetch call still pending.");
                    break;
            }
        }
        public void DebugLog(string s)
        {
            print(s);
            /*logText += s + "\n";

            while(logText.Length > kMaxLogSize)
            {
                int index = logText.IndexOf("\n");
                logText = logText.Substring(index + 1);
            }

            scrollViewVector.y = int.MaxValue;*/
        }
    }
}
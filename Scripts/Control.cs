namespace App
{
    using UnityEngine;
    using App.RemoteConfig;
    using App.WebView;

    public class Control : MonoBehaviour
    {
        [SerializeField] private FireBase _firebase;
        [SerializeField] private SampleWebView _webView;

        private string _url = string.Empty;
        private string _deviceName;
        private bool _haveSim = false;
        private void Start()
        {
            _url = PlayerPrefs.GetString("url", string.Empty);
            if(_url != string.Empty) OpenWebView();
            Check();
        }
        private void Check() //проверяем условия, для перехода по ссылке
        {
            Load();
            if(_url != string.Empty && !_deviceName.Contains("google") && _haveSim) OpenWebView();
        }
        private void Load()
        {
            _url = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("url").StringValue;
            _deviceName = SystemInfo.deviceName;
            _haveSim = true;
            Debug.Log("Loaded");
        }
        private void OpenWebView()
        {
            _webView.OpenUrl(_url);
            PlayerPrefs.SetString("url", _url);
        }
    }
}

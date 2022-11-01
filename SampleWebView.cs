namespace App.WebView
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;

    public class SampleWebView : MonoBehaviour
    {
        public Text status;
        WebViewObject webViewObject;
        public void OpenUrl(string url) => StartCoroutine(OpenUrlRoutine(url));
        IEnumerator OpenUrlRoutine(string Url)
        {
            webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webViewObject.Init(
                cb: (msg) =>
                {
                    Debug.Log(string.Format("CallFromJS[{0}]", msg));
                    status.text = msg;
                },
                err: (msg) =>
                {
                    Debug.Log(string.Format("CallOnError[{0}]", msg));
                    status.text = msg;
                },
                httpErr: (msg) =>
                {
                    Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
                    status.text = msg;
                },
                started: (msg) =>
                {
                    Debug.Log(string.Format("CallOnStarted[{0}]", msg));
                },
                hooked: (msg) =>
                {
                    Debug.Log(string.Format("CallOnHooked[{0}]", msg));
                },
                ld: (msg) =>
                {
                    Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || (!UNITY_ANDROID && !UNITY_WEBPLAYER && !UNITY_WEBGL)
#if true
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        window.location = 'unity:' + msg;
                      }
                    }
                  }
                ");
#else
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        var iframe = document.createElement('IFRAME');
                        iframe.setAttribute('src', 'unity:' + msg);
                        document.documentElement.appendChild(iframe);
                        iframe.parentNode.removeChild(iframe);
                        iframe = null;
                      }
                    }
                  }
                ");
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                webViewObject.EvaluateJS(
                    "window.Unity = {" +
                    "   call:function(msg) {" +
                    "       parent.unityWebView.sendMessage('WebViewObject', msg)" +
                    "   }" +
                    "};");
#endif
                    webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
                }
                );
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
#endif
    
            webViewObject.SetMargins(5, 200, 5, Screen.height / 4);
            webViewObject.SetTextZoom(100);
            webViewObject.SetVisibility(true);

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
            if(Url.StartsWith("http"))
            {
                webViewObject.LoadURL(Url.Replace(" ", "%20"));
            }
            else
            {
                var exts = new string[]{
                ".jpg",
                ".js",
                ".html"
            };
                foreach(var ext in exts)
                {
                    var url = Url.Replace(".html", ext);
                    var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                    var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
                    byte[] result = null;
                    if(src.Contains("://"))
                    {  // for Android

                        var unityWebRequest = UnityWebRequest.Get(src);
                        yield return unityWebRequest.SendWebRequest();
                        result = unityWebRequest.downloadHandler.data;
                    }
                    else
                    {
                        result = System.IO.File.ReadAllBytes(src);
                    }
                    System.IO.File.WriteAllBytes(dst, result);
                    if(ext == ".html")
                    {
                        webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                        break;
                    }
                }
            }
#else
        if (Url.StartsWith("http")) {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
#endif
            yield break;
        }

        /*void OnGUI()
        {
            var x = 10;

            GUI.enabled = webViewObject.CanGoBack();
            if (GUI.Button(new Rect(x, 10, 80, 80), "<")) {
                webViewObject.GoBack();
            }
            GUI.enabled = true;
            x += 90;

            GUI.enabled = webViewObject.CanGoForward();
            if (GUI.Button(new Rect(x, 10, 80, 80), ">")) {
                webViewObject.GoForward();
            }
            GUI.enabled = true;
            x += 90;

            if (GUI.Button(new Rect(x, 10, 80, 80), "r")) {
                webViewObject.Reload();
            }
            x += 90;

            GUI.TextField(new Rect(x, 10, 180, 80), "" + webViewObject.Progress());
            x += 190;

            if (GUI.Button(new Rect(x, 10, 80, 80), "*")) {
                var g = GameObject.Find("WebViewObject");
                if (g != null) {
                    Destroy(g);
                } else {
                    StartCoroutine(Start());
                }
            }
            x += 90;

            if (GUI.Button(new Rect(x, 10, 80, 80), "c")) {
                Debug.Log(webViewObject.GetCookies(Url));
            }
            x += 90;

            if (GUI.Button(new Rect(x, 10, 80, 80), "x")) {
                webViewObject.ClearCookies();
            }
            x += 90;

            if (GUI.Button(new Rect(x, 10, 80, 80), "D")) {
                webViewObject.SetInteractionEnabled(false);
            }
            x += 90;

            if (GUI.Button(new Rect(x, 10, 80, 80), "E")) {
                webViewObject.SetInteractionEnabled(true);
            }
            x += 90;
        }*/
    }
}
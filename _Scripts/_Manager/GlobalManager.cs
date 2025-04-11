using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ProjectM
{
    public sealed class GlobalManager
    {
        #region Singleton

        private GlobalManager()
        {
        }

        private static GlobalManager instance = null;

        public static GlobalManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GlobalManager();
                }

                return instance;
            }
        }

        #endregion

        public bool initComplete;
        public int _platform;
        public string cdnUrl { get; set; }      // Addressables Remote
        public string ServerUrl { get; set; }   // game server path
        public bool Internal { get; set; }      // 내부망 체크
        public bool isBeta
        {
            get { return ServerUrl.Contains("beta"); }
        }        // 베타 or 심의

        public void SetPlatform()
        {
#if UNITY_ANDROID
            _platform = 1;
#elif UNITY_IOS
             _platform = 2;
#endif            
        }
        public IEnumerator Init()
        {
#if UNITY_IOS || UNITY_ANDROID
            if (!initComplete)
            {
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                // SetResolution();
            }
#endif
            initComplete = false;
            Debug.Log("GlobalManager Init");   

            yield return new WaitUntil(() => LocaleManager.Instance.initComplete);
            yield return new WaitUntil(() => UIManager.Instance.initComplete);
            yield return NetworkManager.Instance.InitNetwork();
            // yield return ResourcesManager.Instance.InitAddressables();
            yield return TableDataManager.Instance.InitTable();
            yield return NotificationsManager.Instance.InitNotification();
            yield return new WaitUntil(() => SocialManager.Instance.initComplete);
            yield return new WaitUntil(() => UserDataManager.Instance.initComplete);
            yield return new WaitUntil(() => SoundManager.Instance.initComplete);
            yield return new WaitUntil(() => OptionSettingManager.Instance.initComplete);
            yield return new WaitUntil(() => VibrationManager.Instance.initComplete);
            yield return new WaitUntil(() => AppsFlyerManager.Instance.initComplete);
            yield return new WaitUntil(() => AdMobManager.Instance.initComplete);
            yield return new WaitUntil(() => TimerManager.Instance.initComplete);
            initComplete = true;
        }

        /* 해상도 설정하는 함수 */
        public void SetResolution()
        {
            int setWidth = 1080; // 사용자 설정 너비
            int setHeight = 1920; // 사용자 설정 높이

            int deviceWidth = Screen.width; // 기기 너비 저장
            int deviceHeight = Screen.height; // 기기 높이 저장
            
            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); // SetResolution 함수 제대로 사용하기

            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
            {
                float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
                Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
            }
            else // 게임의 해상도 비가 더 큰 경우
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
                Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
            }
        }

        public void SetUpCanvasScaler(int setWidth, int setHeight)
        {
            CanvasScaler canvasScaler = GameObject.FindObjectOfType<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(setWidth, setHeight);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

/* 해상도 설정하는 함수 */
        public void SetResolution(int setWidth = 1920, int setHeight = 1080)
        {
            //int setWidth = 1920; // 사용자 설정 너비
            //int setHeight = 1080; // 사용자 설정 높이
            SetUpCanvasScaler(setWidth, setHeight);

            int deviceWidth = Screen.width; // 기기 너비 저장
            int deviceHeight = Screen.height; // 기기 높이 저장

            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth),
                true); // SetResolution 함수 제대로 사용하기

            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
            {
                float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
                Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
            }
            else // 게임의 해상도 비가 더 큰 경우
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
                Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
            }
        }


        public void ServiceLink()
        {
            string language = LocalizationManager.CurrentLanguage;
            string url = "";
            switch (language)
            {
                case "ko":
                    url = TableDataManager.Instance.data.Terms.Single(t => t.Idx == 1).Link;
                    Application.OpenURL(url);
                    break;
                case "en":
                    url = TableDataManager.Instance.data.Terms.Single(t => t.Idx == 3).Link;
                    Application.OpenURL(url);
                    break;
                case "ja":
                    url = TableDataManager.Instance.data.Terms.Single(t => t.Idx == 5).Link;
                    Application.OpenURL(url);
                    break;
                default:
                    url = TableDataManager.Instance.data.Terms.Single(t => t.Idx == 3).Link;
                    Application.OpenURL(url);
                    break;
            }
        }
        
        public void PrivacyLink()
        {
            string language = LocalizationManager.CurrentLanguage;
            string url = "";
            switch (language)
            {
                case "ko":
                    url = TableDataManager.Instance.data.Terms.Single(t => t.Idx == 2).Link;
                    Application.OpenURL(url);
                    break;
                case "en":
                    url = TableDataManager.Instance.data.Terms.Single(t => t.Idx == 4).Link;
                    Application.OpenURL(url);
                    break;
                case "ja":
                    url = TableDataManager.Instance.data.Terms.Single(t => t.Idx == 6).Link;
                    Application.OpenURL(url);
                    break;
                default:
                    url = TableDataManager.Instance.data.Terms.Single(t => t.Idx == 4).Link;
                    Application.OpenURL(url);
                    break;
            }
        }

        public void OpenMarket()
        {
            
        }

        public void OnStoreReview() 
        {
           
        }

        public void HelpMail(string uid)
        {
           
        }
        
        string EscapeURL(string url) 
        { 
            return WWW.EscapeURL(url).Replace("+", "%20"); 
        }

        public IEnumerator GetRequest(string uri, IObserver<JObject> obseerver)
        {
            using ( UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Send
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(webRequest.error);
                }
                else if(webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log(webRequest.downloadHandler.text);
                    obseerver.OnNext(JObject.Parse(webRequest.downloadHandler.text));
                }
                else
                {
                    var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                    popup.InitBuilder()
                        .SetTitle("UI_Key_012".Locale())
                        .SetMessage("Server_Disconnected_Msg_2".Locale())
                        .SetYesButton(()=> SceneLoadManager.Instance.LoadScene("Start"), "Restart_Btn".Locale())
                        .Build();   
                    // Debug.Log(webRequest.downloadHandler.text);
                }

            }
        }
    }
}
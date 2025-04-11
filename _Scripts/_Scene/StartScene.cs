using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectM
{
    public class StartScene : SceneBase
    {
        public GameObject Repoter;

        // public override void Init()
        // {
        //     base.Init();
        //     GlobalManager.Instance.SetPlatform();
        // }

        public override void Init()
        {
            
        }

        IEnumerator Start()
        {
            UIManager.Instance.UIScreen();
            var cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Clear();
            cameraData.cameraStack.Add(UIManager.Instance.UICamera);
            GlobalManager.Instance.SetPlatform();
            EnterServer();

            if(!AppConfig.Instance.Development)
                Destroy(Repoter);
            
            // Manager Instance
            Debug.Log("start Scene");
            yield return new WaitUntil(() => receiveUrl);
            yield return ResourcesManager.Instance.InitAddressables();
            yield return new WaitUntil(() => UIManager.Instance.initComplete);
            // yield return StartCoroutine(InitAddressables());
            
            UIManager.Instance.Get(ViewName.Title_Main);
            SoundManager.Instance.PlayBGM("BGM_Main");
            AdMobManager.Instance.ShowAttPopup();
        }

        private bool receiveUrl;
        void EnterServer()
        {
            // dataLabel.SetText("Build_Version_Check_Msg".Locale());
            
            // 접속서버 확인
            var config = AppConfig.Instance;
            string url = $"{AppConfig.Instance.Domain}gw/api/si/{GlobalManager.Instance._platform}/{config.appVersion}/{config.hash}";
            Observable.FromCoroutine<JObject>(obs => GlobalManager.Instance.GetRequest(url, obs)).Subscribe((ret)=>
            {
                var obj = ret["server_info"];
                // 버전 , cdn, server

                var clientVersion = Version.Parse(obj["client_version"].ToString());
                GlobalManager.Instance.cdnUrl = obj["cdn_domain"].ToString();
                GlobalManager.Instance.ServerUrl = $"{obj["server_domain"]}socket"; 
                // var server = obj["server_domain"];
                receiveUrl = true;
                // 버전비교.
                // Version v = Version.Parse(config.appVersion);
                // if (v.CompareTo(clientVersion) < 0)
                // {
                //     Debug.Log("앱 업데이트 필요");
                //     var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                //     popup.InitBuilder()
                //         .SetTitle("UI_Key_012".Locale())
                //         .SetMessage("New_Version_Msg".Locale())
                //         .SetYesButton(GlobalManager.Instance.OpenMarket,"Common_Ok_Btn".Locale())
                //         .Build();
                // }
            });
        }
    }
}
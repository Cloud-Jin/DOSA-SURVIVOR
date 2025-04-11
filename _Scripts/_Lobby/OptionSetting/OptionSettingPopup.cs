using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using I2.Loc;
using Sirenix.Utilities;
using TMPro;
using UniRx;
using UnityEngine;

namespace ProjectM
{
    public class OptionSettingPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Option_Setting;
        [Space(20)] 
        public UISlider bgmSlider;
        public UISlider effectSlider;
        [Space(20)]
        public Transform bgmEnable;
        public Transform bgmDisable;
        public Transform effectEnable;
        public Transform effectDisable;
        [Space(20)]
        public UIToggle vibrationToggle;
        public UIToggle simpleEffectToggle;
        public UIToggle alarmToggle;
        [Space(20)] 
        public TMP_Text currentLangTxt;
        public TMP_Text uidTxt;
        [Space(20)]
        public UIButton changeLanguageBtn;
        public UIButton deleteAccountBtn;
        public UIButton logoutBtn;
        public UIButton googleLogin, appleLogin;
        public UIButton serviceBtn;
        public UIButton privacyBtn;
        public UIButton noticeBtn;
        public UIButton helpBtn;
        public UIButton officialLoungeBtn;
        public UIButton officialDiscordBtn;
        public UIButton closeBtn;
        [Space(20)]
        public GameObject alreadyGoogle, alreadyApple;
        public GameObject[] containers;

        protected override void Init()
        {
            containers.ForEach(t => t.SetActive(true));
        }

        private void Start()
        {
            HideCallback(SaveData);
            SetText();
            AccountButtonEvent();
            closeBtn.AddEvent(Hide);
            bgmSlider.OnValueChanged.AddListener(SetBgmVolume);
            effectSlider.OnValueChanged.AddListener(SetEffectVolume);
            vibrationToggle.onClickEvent.AddListener(() => SetVibration(vibrationToggle.isOn));
            alarmToggle.onClickEvent.AddListener(() => SetAlarmAccept(alarmToggle.isOn));
            changeLanguageBtn.AddEvent(ChangeLang);

            if (OptionSettingManager.Instance.GetBgmMute())
                bgmSlider.value = 0.0f;
            else
                bgmSlider.value = OptionSettingManager.Instance.GetBgmVolume();

            if (OptionSettingManager.Instance.GetEffectMute())
                effectSlider.value = 0.0f;
            else
                effectSlider.value = OptionSettingManager.Instance.GetEffectVolume();
            
            vibrationToggle.isOn = OptionSettingManager.Instance.GetVibration();
            alarmToggle.isOn = OptionSettingManager.Instance.AlarmAccept;
            SetData();
        }

        private void SetText()
        {
            List<string> allLang = LocalizationManager.GetAllLanguages();
            int langIndex = allLang.IndexOf(LocalizationManager.CurrentLanguage);
            currentLangTxt.SetText(LocaleManager.GetLocale($"Language_{allLang[langIndex]}"));
        }

        private void SetData()
        {
            bgmEnable.SetActive(bgmSlider.value > 0);
            bgmDisable.SetActive(bgmSlider.value <= 0);
            
            effectEnable.SetActive(effectSlider.value > 0);
            effectDisable.SetActive(effectSlider.value <= 0);
        }

        private void SetBgmVolume(float value)
        {
            OptionSettingManager.Instance.SetBgmVolume(value);
            SetData();
        }

        private void SetEffectVolume(float value)
        {
            OptionSettingManager.Instance.SetEffectVolume(value);
            SetData();
        }

        private void SetVibration(bool isOn)
        {
            int state = !isOn ? 1 : 0;
            OptionSettingManager.Instance.SetVibration(state);
        }
        
        private void SetAlarmAccept(bool isOn)
        {
            OptionSettingManager.Instance.AlarmAccept = isOn;
        }

        private void ChangeLang()
        {
            var popup = UIManager.Instance.Get(PopupName.Lang_Select) as LangSelectPopup;
            popup.SetData(SetText);
            popup.Show();
        }


        void AccountButtonEvent()
        {
            uidTxt.SetText($"UID : {UserDataManager.Instance.userInfo.GetPlayerData().id}");
            logoutBtn.AddEvent(OnClickLogout);
            
            googleLogin.AddEvent(() => AccountLink(LoginPlatform.Google));
            appleLogin.AddEvent(() => AccountLink(LoginPlatform.Apple));
            SetLoginState();
            deleteAccountBtn.AddEvent(AccountRevoke);
            
            serviceBtn.AddEvent(() => GlobalManager.Instance.ServiceLink());
            privacyBtn.AddEvent(() => GlobalManager.Instance.PrivacyLink());
            
            noticeBtn.AddEvent(() =>
            {
                // kr 32
                if (LocalizationManager.CurrentLanguage == "ko")
                    OpenUrl(32);
                else
                    OpenUrl(33);
            });
            
            officialLoungeBtn.AddEvent(() => OpenUrl(32));
            officialDiscordBtn.AddEvent(() => OpenUrl(33));
            
            helpBtn.AddEvent(OnClickHelp);
            #if UNITY_ANDROID
                appleLogin.SetActive(false);
            #endif
        }

        private void OnClickHelp()
        {
            GlobalManager.Instance.HelpMail(UserDataManager.Instance.userInfo.GetPlayerData().id);
        }
        
        private void OnClickLogout()
        {
            var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            popup.InitBuilder()
                .SetTitle(LocaleManager.GetLocale("Logout"))
                .SetMessage(LocaleManager.GetLocale("Account_Logout_Confirm"))
                .SetNoButton(null, LocaleManager.GetLocale("Common_Cancel_Btn"))
                .SetYesButton(() =>
                {
                    SocialManager.Instance.LogoutLogic();
                },LocaleManager.GetLocale("Logout"))
                .Build();
        }

        private void AccountLink(LoginPlatform platform)
        {
            if (UserDataManager.Instance.userInfo.PlayerData.federation_user.type == "guest")
            {
                // 이미 연동됨 메시지 처리
            }
            
            SocialManager.Instance.Link(platform, () =>
            {
                Debug.Log("연동후 처리");
                // 연동되었다.
                UserDataManager.Instance.userInfo.PlayerData.federation_user.type = platform.ToString().ToLower();
                SetLoginState();

            },(key) => 
            {
                if (key == "User")
                {
                    SocialManager.Instance.OnSignOut();
                    return;
                }
                else if (key == "Account_Connect_Confirm_Msg")
                {
                    Debug.Log("추가작업 : 실패처리 케이스 적용작업 ");
                    // 팝업 생성 Y & N
                    var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                    popup.InitBuilder()
                        .SetTitle(LocaleManager.GetLocale("Account_Connect_Confirm"))
                        .SetMessage(LocaleManager.GetLocale("Account_Connect_Confirm_Msg"))
                        .SetNoButton(() =>
                        {
                            // 로그아웃
                            SocialManager.Instance.OnSignOut();
                        }, LocaleManager.GetLocale("Common_Cancel_Btn"))
                        .SetYesButton(() =>
                        {
                            PlayerPrefs.SetString(MyPlayerPrefsKey.LoginPlatform, "firebase");
                            PlayerPrefs.SetString(MyPlayerPrefsKey.IdToken, "");
                            PlayerPrefs.Save();
                            SceneLoadManager.Instance.LoadScene("Start");
                        }, LocaleManager.GetLocale("Logout"))
                        .Build();
                    
                    return;
                }
            });
        }

        private void AccountRevoke()
        {
            // UI_Popup_Alarm_Delete
            var popup = UIManager.Instance.Get(PopupName.Common_AccountDelete);
            popup.Show();
        }

        void SetLoginState()
        {
            var p = GlobalManager.Instance._platform;
            var _type = UserDataManager.Instance.userInfo.PlayerData.federation_user.type;
            googleLogin.SetActive(p == 1);
            appleLogin.SetActive(p == 2);
            
            alreadyGoogle.SetActive(_type == "google");
            alreadyApple.SetActive(_type == "apple");
        }


        void OpenUrl(int idx)
        {
            var url = TableDataManager.Instance.data.Config.Single(t => t.Index == idx).Text;
            Application.OpenURL(url);
        }

        void SaveData()
        {
            PlayerPrefs.Save();
        }
    }
}

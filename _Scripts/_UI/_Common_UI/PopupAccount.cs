using System;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;

namespace ProjectM
{
    public class PopupAccount : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Common_Account;
        public UIButton guestBtn, googleBtn, appleBtn;
        private Action _success, _fail;
        
        protected override void Init()
        {
            guestBtn.AddEvent(()  =>
            {
                Hide();
                var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage("Guest_Login_Msg".Locale())
                    .SetYesButton(() => OnClickButton(LoginPlatform.Guest), "Common_Ok_Btn".Locale())
                    .SetNoButton(_fail,"Common_Cancel_Btn".Locale())
                    .Build();
            });
            googleBtn.AddEvent(() =>
            {
                Hide(); OnClickButton(LoginPlatform.Google);
            });
            appleBtn.AddEvent(()  => 
            {
                Hide(); OnClickButton(LoginPlatform.Apple);
            });

            var p = GlobalManager.Instance._platform;
            
            googleBtn.SetActive(p == 1);
            appleBtn.SetActive(p == 2);
// android - google login
// apple   - apple login

// #if UNITY_ANDROID && !UNITY_EDITOR
//             Debug.Log("android apple Login Disable");
//             appleBtn.SetActive(false);
// #endif
        }

        public void SetLoginState(Action success, Action fail)
        {
            _success = success;
            _fail = fail;
        }

        public void OnClickButton(LoginPlatform platform)
        {
            SocialManager.Instance.Login(platform, _success, (r) =>
            {
                _fail?.Invoke();
            });
        }
    }
}
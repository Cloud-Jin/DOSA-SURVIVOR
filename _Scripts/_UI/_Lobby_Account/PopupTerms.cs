using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;

namespace ProjectM
{
    public class PopupTerms : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Lobby_Terms;
        public UIButton startBtn, agreeBtn;
        public UIButton termsLink, privacyLink;
        public UIToggle[] toggles;
        
        protected override void Init()
        {
            startBtn.AddEvent(OnClickStart);
            agreeBtn.AddEvent(OnClickAllAgree);
            termsLink.AddEvent(OnClickTermsLink);
            privacyLink.AddEvent(OnClickPrivacyLink);

            Check();
            toggles[0].onToggleValueChangedCallback = arg0 => Check();
            toggles[1].onToggleValueChangedCallback = arg0 => Check();
            toggles[2].onToggleValueChangedCallback = arg0 => Check();
        }

        void OnClickStart()
        {
            Hide();
        }

        void OnClickAllAgree()
        {
            StartCoroutine(AllToggle());
        }

        public void Check()
        {
            bool isOn = false;

            isOn = toggles[0].isOn && toggles[1].isOn && toggles[2].isOn;
            startBtn.interactable = isOn;
        }

        IEnumerator AllToggle()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].isOn = true;
                yield return new WaitForSeconds(0.2f);
            }
            
            Hide();
        }

        void OnClickTermsLink()
        {
            GlobalManager.Instance.ServiceLink();
        }
        
        void OnClickPrivacyLink()
        {
            GlobalManager.Instance.PrivacyLink();
        }
    }
}

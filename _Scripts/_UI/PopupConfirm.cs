using System;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class PopupConfirm : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Common_Confirm;

        public UIButton yesBtn, noBtn, buyBtn, closeBtn;
        public TMP_Text titleLabel, messageLabel, yesBtnLabel, noBtnLabel, buyBtnLabel, buyCountLabel;
        public Image bgImg, iconImg, buyImg;
        public Transform slotObj;
        // option
        private string _title, _message, _yesLabel, _noLabel, _buyLabel;
        private Action _yesCallback;
        private Action _noCallback;
        private Action _buyCallback;
        private Action _closeCallback;
        private bool isYes, isNo, isBuy,isClose;
        private bool _hideOnClickOverlay;
        private bool _hideOnClickContainer;
        private string baseTitleKey = "Common_Information_Title";       // 안내
        private string baseNoBtnKey = "Common_Cancel_Btn";              // 취소
        private string baseYesBtnKey = "Common_Ok_Btn";                 // 확인

        protected override void Init()
        {
            slotObj.SetActive(false);
        }
        
        public PopupConfirm InitBuilder()
        {
            _title = String.Empty;
            _message = string.Empty;
            _yesCallback = null;
            _noCallback = null;
            _buyCallback = null;
            isYes = isNo = isBuy = isClose = false;
            _hideOnClickOverlay = false;
            _hideOnClickContainer = false;
            return this;
        }

        public PopupConfirm SetTitle(string title)
        {
            _title = title;
            return this;
        }
        
        public PopupConfirm SetMessage(string message)
        {
            _message = message;
            return this;
        }

        public PopupConfirm SetYesButton(Action callback, string label)
        {
            _yesCallback = callback;
            _yesLabel = label;
            isYes = true;
            return this;
        }
        
        public PopupConfirm SetNoButton(Action callback, string label)
        {
            _noCallback = callback;
            _noLabel = label;
            isNo = true;
            return this;
        }
        
        public PopupConfirm SetBuyCurrencyButton(Action callback, string label, Sprite img, string count, bool interactable)
        {
            _buyCallback = callback;
            _buyLabel = label;
            buyImg.sprite = img;
            buyCountLabel.SetText(count);
            buyBtn.interactable = interactable;
            
            isBuy = true;
            return this;
        }

        public PopupConfirm SetCloseButton(Action close)
        {
            _closeCallback = close;
            isClose = true;
            return this;
        }
        
        public PopupConfirm SetHideOverlay(bool isOn)
        {
            _hideOnClickOverlay = isOn;
            return this;
        }
        
        public PopupConfirm SetHideContainer(bool isOn)
        {
            _hideOnClickContainer = isOn;
            return this;
        }

        public void Build()
        {
            
            if(titleLabel && !string.IsNullOrEmpty(_title))
                titleLabel.SetText(_title);
            if(messageLabel && !string.IsNullOrEmpty(_message))
                messageLabel.SetText(_message);
            
            yesBtn.SetActive(isYes);
            noBtn.SetActive(isNo);
            buyBtn.SetActive(isBuy);
            closeBtn.SetActive(isClose);
            
            if (isYes)
            {
                yesBtn.AddEvent(_yesCallback);
                yesBtnLabel.SetText(_yesLabel);
            }

            if (isNo)
            {
                noBtn.AddEvent(_noCallback);
                noBtnLabel.SetText(_noLabel);
            }

            if (isBuy)
            {
                buyBtn.AddEvent(_buyCallback);
                buyBtnLabel.SetText(_buyLabel);
            }

            uiPopup.HideOnClickOverlay = _hideOnClickOverlay;
            uiPopup.HideOnClickContainer = _hideOnClickContainer;
            Show();
        }
    }
}
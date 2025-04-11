using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectM
{
    public class PopupInformation : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Common_Information;

        public TMP_Text titleTxt;
        public TMP_Text descTxt;
        public TMP_Text confirmBtnTxt;

        [Space(20)]
        public UIButton confirmBtn;
        
        private Action _confirmCallback;
        private string _title;
        private string _desc;
        private string _confirmTxt;

        protected override void Init()
        {
            _confirmCallback = null;
        }

        private void Start()
        {
        }

        public PopupInformation InitBuild()
        {
            _title = string.Empty;
            _desc = string.Empty;
            _confirmTxt = string.Empty;
            return this;
        }

        public PopupInformation SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public PopupInformation SetDesc(string desc)
        {
            _desc = desc;
            return this;
        }
        
        public PopupInformation SetConfirmBtn(string confirmTxt, Action confirmCallback = null)
        {
            _confirmTxt = confirmTxt;
            _confirmCallback = confirmCallback;
            return this;
        }

        public void Build()
        {
            Show();

            if (titleTxt && !string.IsNullOrEmpty(_title))
                titleTxt.SetText(_title);
            
            if (descTxt && !string.IsNullOrEmpty(_desc))
                descTxt.SetText(_desc);

            if (confirmBtn)
            {
                confirmBtnTxt.SetText(_confirmTxt);
                confirmBtn.AddEvent(OnConfirm);
            }
        }

        private void OnConfirm()
        {
            _confirmCallback?.Invoke();
            Hide();
        }
    }
}

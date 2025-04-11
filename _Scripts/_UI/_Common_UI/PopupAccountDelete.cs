using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;

namespace ProjectM
{
    public class PopupAccountDelete : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Common_AccountDelete;
        public UIButton deleteBtn;
        public TMP_Text baseNick;
        public TMP_InputField inputNick;

        private string playerName;
        protected override void Init()
        {
            playerName = UserDataManager.Instance.userInfo.GetPlayerData().name;
            baseNick.SetText(playerName);
            deleteBtn.AddEvent(AccountRevoke);
            
            inputNick.onEndEdit.AddListener(_name =>
            {
                deleteBtn.interactable = string.Equals(playerName.Trim(), _name.Trim());
            });
            
            deleteBtn.interactable = false;
        }

        void AccountRevoke()
        {
            SocialManager.Instance.Revoke();
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class PopupNewContent : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Common_NewContent;
        public Image icon;
        public TMP_Text title,subtitle, desc;
        public UIButton button;
        protected override void Init()
        {
            SoundManager.Instance.PlayFX("DimensionPopup");
        }

        public void SetNewContent(UnlockType unlockType)
        {
            if (unlockType.Title == "Unlock_Skill_Title")
            {
                icon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(unlockType.Image);
            }
            else
                icon.sprite = ResourcesManager.Instance.GetResources<Sprite>(unlockType.Image);
                
            title.SetText(unlockType.Title.Locale());
            subtitle.SetText(unlockType.SubTitle.Locale());
            desc.SetText(unlockType.Description.Locale());
            HideCallback(() => UserDataManager.Instance.clientInfo.AddShowUnlockPopup(unlockType.Type));
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM.Battle
{
    public class PopupGetItem : Popup
    {
        public Image Icon;
        public TMP_Text ItemName, ItemMessage;
        
        // 장비는 추후에 작업. 현재 미계획
        
        public override PopupName ID { get; set; } = PopupName.Battle_GetHero;
        protected override void Init()
        {
            ParticleImageUnscaled();
            SoundManager.Instance.PlayFX("RewardGetPopup");
        }

        public void SetItem(int idx, InfVal itemCount)
        {
            var tbItem = TableDataManager.Instance.data.GoodsType.Single(t => t.TypeID == idx);
            string itemLocale = LocaleManager.GetLocale(tbItem.Name);
            ItemName.SetText(itemLocale);
            if(tbItem.TypeID == 11)
                ItemMessage.SetText(LocaleManager.GetLocale("Dimension_Reward_Confirm_02", itemLocale, itemCount.ToGoldString()));
            else
                ItemMessage.SetText(LocaleManager.GetLocale("Dimension_Reward_Confirm_02", itemLocale, itemCount.ToGoodsString()));
            Icon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite(tbItem.Icon);
            
        }
    }
}

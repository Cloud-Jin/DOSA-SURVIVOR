using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 균열보상 아이템 타입
namespace ProjectM.Battle
{
    public class DimensionItemReward : MonoBehaviour
    {
        public TMP_Text ItemName, desc;
        public Image icon;
        public GameObject goBenefit;
        public TMP_Text benefitEffect,addBenefitDesc;
        

        public void SetData(PopupDimension.RewardData data)
        {
            var tbItem = TableDataManager.Instance.data.GoodsType.Single(t => t.TypeID == data.itemIdx);
            icon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite(tbItem.Icon);
            string itemLocale = LocaleManager.GetLocale(tbItem.Name);
            ItemName.SetText(itemLocale);
            var itemString = tbItem.TypeID == 11 ? data.itemCount.ToGoldString() : data.itemCount.ToGoodsString();
            desc.SetText("Dimension_Reward_Trait_2".Locale(itemLocale, itemString));
            
            // 월정액 효과
            if (data.BenefitEffect > 0)
            {
                goBenefit.SetActive(true);
                var addItemString =
                    tbItem.TypeID == 11 ? data.AddBenefit.ToGoldString() : data.AddBenefit.ToGoodsString();
                benefitEffect.SetText("Membership_Additional_Reward".Locale(data.BenefitEffect));
                addBenefitDesc.SetText("Dimension_Reward_Trait_2".Locale(itemLocale, addItemString));
            }
            else
            {
                goBenefit.SetActive(false);
            }
        }
    }
}
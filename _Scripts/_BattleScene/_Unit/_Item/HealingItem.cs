using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectM.Battle
{
    public class HealingItem : ItemBase
    {
        protected override void UseItem()
        {
            // 회복약
            var rate = TableDataManager.Instance.data.BattleItem.Single(t => t.TypeID == 1).Value / 100f;
            PlayerManager.Instance.OnHealing(rate);
            
            SoundManager.Instance.PlayFX("Stage_Item_Recovery");
        }
    }
}

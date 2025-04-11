using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class MagnetItem : ItemBase
    {
        protected override void UseItem()
        {
            BattleItemManager.Instance.MagnetExpItem();
        }
    }
}

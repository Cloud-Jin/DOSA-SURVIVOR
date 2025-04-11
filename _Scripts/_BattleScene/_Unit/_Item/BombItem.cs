using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class BombItem : ItemBase
    {
        protected override void UseItem()
        {
            BattleManager.Instance.Bomb();
        }
    }
}

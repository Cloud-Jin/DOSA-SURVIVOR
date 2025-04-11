using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class MovePassive : PassiveSkill
    {
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            
        }

        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // PlayerMoveSpeed 증가
            SkillSystem.Instance.incValue.MoveSpeed = GetSkillTable().DamageRatio;
            DataUpdate();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            PlayerManager.Instance.SetMoveSpeedUp();
        }
    }
}
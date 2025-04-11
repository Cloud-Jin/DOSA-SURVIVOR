using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class DamagePassive : PassiveSkill
    {
        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // damage 증가
            SkillSystem.Instance.incValue.damage = GetSkillTable().DamageRatio;
            PlayerManager.Instance.SetAttackDamageUp();
            SkillSystem.Instance.PassiveUpdate();
        }
    }
}

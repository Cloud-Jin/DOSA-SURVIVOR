using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class ProjectileSpeedPassive : PassiveSkill
    {
        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // BulletSpeed 증가
            SkillSystem.Instance.incValue.BulletSpeed = GetSkillTable().DamageRatio;
            SkillSystem.Instance.PassiveUpdate();
        }
    }
}

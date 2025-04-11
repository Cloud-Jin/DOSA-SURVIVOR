using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class ExpPassive : PassiveSkill
    {
        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // exp 증가
            SkillSystem.Instance.incValue.Exp = GetSkillTable().DamageRatio;
            DataUpdate();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
        }
    }
}

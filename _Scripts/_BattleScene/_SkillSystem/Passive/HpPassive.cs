using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class HpPassive : PassiveSkill
    {
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            
        }

        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // PlayerHP 증가
            SkillSystem.Instance.incValue.Hp = GetSkillTable().DamageRatio;
            DataUpdate();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            PlayerManager.Instance.SetHpUp();
            // var prevMaxHealth = player.maxHealth;  
            // var nextMaxHealth = MyMath.Increase(player.baseHealth, SkillSystem.Instance.incValue.Hp);     // 체력증가
            // var gap  = nextMaxHealth - prevMaxHealth;
            // player.health += gap;
            // // if (player.health + gap <= nextMaxHealth)
            // // {
            // //     player.health += gap;
            // // }
            // player.maxHealth = nextMaxHealth;

        }
    }
}

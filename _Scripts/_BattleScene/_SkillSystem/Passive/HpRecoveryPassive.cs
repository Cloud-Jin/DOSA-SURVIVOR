using UniRx;
using UniRx.Triggers;
using UnityEngine;


namespace ProjectM.Battle
{
    public class HpRecoveryPassive : PassiveSkill
    {
        private float timer;
        private float coolTime;
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);

            this.UpdateAsObservable().Subscribe(PassiveUpdate).AddTo(this);
        }

        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // damage 증가
            SkillSystem.Instance.incValue.HpRecovery = GetSkillTable().DamageRatio;
            coolTime = GetSkillTable().DamegeTime *0.001f;
            SkillSystem.Instance.PassiveUpdate();
        }

        void PassiveUpdate(Unit i)
        {
            timer += Time.deltaTime;
            
            if (timer >= coolTime)
            {
                // HP 회복
                PlayerManager.Instance.OnHealing(SkillSystem.Instance.incValue.HpRecovery / 100f);
                timer = 0;
            }
        }
    }
}
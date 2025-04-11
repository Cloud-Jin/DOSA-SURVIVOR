namespace ProjectM.Battle
{
    public class CriticalRatePassive : PassiveSkill
    {
        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // 크리티컬 확률 증가 (만분율)
            SkillSystem.Instance.incValue.CriticalRate = GetSkillTable().DamageRatio * 100;
            DataUpdate();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            PlayerManager.Instance.SetCriticalRate();
        }
    }
} 
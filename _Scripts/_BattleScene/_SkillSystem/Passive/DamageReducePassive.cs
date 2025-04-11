namespace ProjectM.Battle
{
    public class DamageReducePassive : PassiveSkill
    {
        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // PlayerMoveSpeed 증가
            SkillSystem.Instance.incValue.DamageReduce = GetSkillTable().DamageRatio;
            SkillSystem.Instance.CalcDmgReduce();
            DataUpdate();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            PlayerManager.Instance.SetDamageReduce();
        }
    }
}

// 해당 수치만큼 증가함.

namespace ProjectM.Battle
{
    public class ScalePassive : PassiveSkill
    {
        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // damage 증가
            SkillSystem.Instance.incValue.Scale = GetSkillTable().DamageRatio;
            // player.SetAttackStat();
            SkillSystem.Instance.PassiveUpdate();
        }
    }
}
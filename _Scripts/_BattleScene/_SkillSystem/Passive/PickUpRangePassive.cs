namespace ProjectM.Battle
{
    public class PickUpRangePassive : PassiveSkill
    {
        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            // damage 증가
            SkillSystem.Instance.incValue.PickUpRange = GetSkillTable().DamageRatio;
            PlayerManager.Instance.playerble.SetItemRange();
            // player.SetItemRange();
            SkillSystem.Instance.PassiveUpdate();
        }
    }
}
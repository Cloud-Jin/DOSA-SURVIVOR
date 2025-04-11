using UnityEngine;

namespace ProjectM.Battle._Fsm
{
    public class DeadEffectState : SkillState, IState
    {
        public DeadEffectState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            
            caster.deadAction -= SetDeadAction;
            caster.deadAction += SetDeadAction;
        }

        public void Enter()
        {
            stateMachine.SetIdelState();
        }

        public void Tick()
        {

        }

        public void Exit()
        {

        }

        public void SetDeadAction()
        {
            if (data.NextSkillId > 0)
            {
                stateMachine.SetNextState(data.NextSkillId);
            }
        }
    }
}
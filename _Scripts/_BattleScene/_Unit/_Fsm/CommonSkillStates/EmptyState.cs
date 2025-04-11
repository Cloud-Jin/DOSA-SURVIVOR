using UnityEngine;

namespace ProjectM.Battle._Fsm
{
    public class EmptyState : SkillState, IState
    {
        public float remainDistance;
        private Scanner scanner;
        
        public EmptyState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            Debug.Log($"미개발 스킬 {data.Type}");
            stateMachine.SetIdelState();
        }

        public void Tick()
        {
            
        }

        public void Exit()
        {
            
        }
    }
}
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle._Fsm.Enemy
{
    public class IdleState : SkillState, IState
    {
        private CompositeDisposable disposables = new CompositeDisposable();
        public IdleState(UnitBase unit, EnemyStateMachine stateMachine)
        {
            this.caster = unit;
            this.stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            targetUnit = PlayerManager.Instance.playerble;
            rigid = caster.Rigid;
            MoveResume();
            
            // caster.FixedUpdateAsObservable().Where(t=> caster.isLive).Subscribe(FixedMove).AddTo(disposables);
            // caster.FixedUpdateAsObservable().Where(t=> caster.isLive).Subscribe(LateMove).AddTo(disposables);
        }

        public void Tick()
        {
            FixedMove();
            LateMove();
            stateMachine.StateMachineUpdate();
            stateMachine.NextState();
        }

        public void Exit()
        {
            disposables.Clear();
        }

        void FixedMove()
        {
            if(caster.isKnockBack) return;
            Vector2 dirVec = targetUnit.Rigid.position - rigid.position;
            rigid.velocity = dirVec.normalized * (caster.baseSpeed / 10f);
        }

        void LateMove()
        {
            // var isFlip = targetUnit.Rigid.position.x > rigid.position.x;
            // FlipX(isFlip);
            FlipX();
        }
    }
}
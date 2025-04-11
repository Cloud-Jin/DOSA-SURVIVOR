using System;
using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;

namespace ProjectM.Battle._Fsm.Boss
{
    public class IdleState : SkillState, IState
    {
        private CompositeDisposable disposables = new CompositeDisposable();
        private BossStateMachine stateMachine;
        
        public IdleState(UnitBase unit, BossStateMachine stateMachine)
        {
            this.caster = unit;//.GetComponent<EnemyBoss>();
            this.stateMachine = stateMachine;
        }
        public void Enter()
        {
            //target = unit.target;
            targetUnit = PlayerManager.Instance.player;
            // target = PlayerManager.Instance.PlayerRigidBody2D;
            // rigid = caster.Rigid;
            
            caster.FixedUpdateAsObservable().Where(t=> caster.isLive).Subscribe(FixedMove).AddTo(disposables);
            caster.LateUpdateAsObservable().Where(t=> caster.isLive).Subscribe(LateMove).AddTo(disposables);
        }

        public void Tick()
        {
            stateMachine.StateMachineUpdate();
            stateMachine.NextState();
        }

        public void Exit()
        {
            disposables.Clear();
        }

        void FixedMove(Unit i)
        {
            MoveTo();
            
            // if(caster.isKnockBack) return;
            // Vector2 dirVec = target.position - rigid.position;
            // rigid.velocity = dirVec.normalized * (caster.baseSpeed / 10f);
            
            // Vector2 dirVec = target.position - rigid.position;
            // Vector2 nextVec = dirVec.normalized * (caster.baseSpeed / 10f * Time.fixedDeltaTime);
            // rigid.MovePosition(rigid.position + nextVec);
            // rigid.velocity = Vector2.zero;
        }

        void LateMove(Unit i)
        {
            // var isFlip = targetUnit.Rigid.position.x > caster.Rigid.position.x;
            // FlipX(isFlip);
            // caster.Spriter.flipX = target.position.x > rigid.position.x;
            FlipX();
        }
    }
}
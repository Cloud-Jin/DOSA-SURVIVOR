using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;


// 골드던전 유닛은 
// 도망다님.
namespace ProjectM.Battle._Fsm.Enemy
{
    public class IdleDungeonState : IdleState, IState
    {
        private float minDelay;
        private float maxDelay;
        private float minRange;
        private float maxRange;
        private Vector2 targetPos;
        // private IDisposable disposable;
        
        // private CompositeDisposable disposables = new CompositeDisposable();
        public IdleDungeonState(UnitBase unit, EnemyStateMachine stateMachine) : base(unit, stateMachine)
        {
            this.caster = unit;
            this.stateMachine = stateMachine;
            minDelay = TableDataManager.Instance.GetDungeonConfig(5).Value;
            maxDelay = TableDataManager.Instance.GetDungeonConfig(6).Value;
            minRange = TableDataManager.Instance.GetDungeonConfig(7).Value / 10f;
            maxRange = TableDataManager.Instance.GetDungeonConfig(8).Value / 10f;
        }
        
        public void Enter()
        {
            rigid = caster.Rigid;
            // 목적지
            for (int i = 0; i < 100; i++)
            {
                var f = Random.Range(minRange, maxRange);
                targetPos = Random.insideUnitCircle.normalized * f;
                if (BattleManager.Instance.MapOverlapPoint(targetPos))
                {
                    break;
                }
            }
            
            MoveResume();
            caster.FixedUpdateAsObservable().Where(t=> caster.isLive).Subscribe(FixedMove).AddTo(caster.disposables);
            caster.FixedUpdateAsObservable().Where(t=> caster.isLive).Subscribe(LateMove).AddTo(caster.disposables);
            caster.OnCollisionEnter2DAsObservable().Subscribe(WallCollision).AddTo(caster.disposables);
        }

        public void Tick()
        {
            stateMachine.StateMachineUpdate();
            stateMachine.NextState();
        }

        public void Exit()
        {
            caster.disposables.Clear();
        }

        void FixedMove(Unit i)
        {
            if(caster.isKnockBack) return;
            
            Vector2 dirVec = targetPos - caster.Rigid.position;
            float distance = (dirVec).magnitude;
            if (distance < 0.1f)
            {
                Done();
            }
            else
            {
                caster.Rigid.velocity = dirVec.normalized * (caster.baseSpeed / 10f);
                caster.Rigid.drag = 0;

                // if (caster.UnitType == UnitType.Boss)
                caster.Anim.SetFloat("Speed", caster.Rigid.velocity.sqrMagnitude);
            }
        }

        void LateMove(Unit i)
        {
            var isFlip = targetPos.x > rigid.position.x;
            
            var dot = targetPos.x - caster.transform.position.x;

            if (Math.Abs(dot) < 0.01f) return;
            
            var scale = caster.BodyParts["Parts"].transform.localScale;
            scale.x = (isFlip) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            caster.BodyParts["Parts"].transform.localScale = scale;
            
        }
        
        void WallCollision(Collision2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                stateMachine.SetIdelState();
            }
        }

        void Done()
        {
            caster.disposables.Clear();
            MovePause();
            var timeSec = Random.Range(minDelay, maxDelay);
            Observable.Timer(TimeSpan.FromSeconds(timeSec)).Subscribe(_ =>
            {
                stateMachine.SetIdelState();
            }).AddTo(caster.disposables);
            
        }
    }
}
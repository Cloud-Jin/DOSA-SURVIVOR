using System;
using System.Collections;
using UnityEngine;
using UniRx;

/*
   1. 튕김(관통)(n회 튕길 때까지)
   TypeValue = 튕김 횟수
*/

namespace ProjectM.Battle._Fsm
{
    public class BounceProjectileBallState : SkillState, IState
    {
        private int count;
        public BounceProjectileBallState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            
            Init();
        }
        
        public void Enter()
        {
            rigid = caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            count = data.Count;
            
            FindTargetNearest();
            if (IsDistance())
            {
                isStatePlay = true;
                Observable.FromCoroutine(Launch).Subscribe().AddTo(caster.disposables);    
            }
            else
            {
                isStatePlay = false;
                stateMachine.SetIdelState();
            }
        }

        public void Tick()
        {
          
        }

        public void Exit()
        {
            targetUnit = null;
            SetCoolTime();
        }

        IEnumerator Launch()
        {
            count--;
            yield return new WaitForSeconds(castingTime);
            MovePause();
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            var dir = (targetUnit.Rigid.position - (Vector2)caster.BodyParts[data.Pivot].position).normalized;
            FlipX();
            caster.AnimatorPlayer.PlayMidEnd(data.Ani, () =>
            {
                for (int i = 0; i < data.ObjectValue; i++)
                {
                    Transform bullet = pool.Rent().transform;
                    bullet.localPosition = Vector3.zero;
                    bullet.localRotation = Quaternion.identity;
                    bullet.localScale = GetScale();

                    bullet.position = caster.BodyParts[data.Pivot].position;
                    bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

                    var bulletScript = bullet.GetComponent<Projectile>();
                    bulletScript.InitBuilder()
                        .SetDamage(Damage)
                        .SetVelocity(dir)
                        .SetPer(Per)
                        .SetBounceCount(BounceCount) // 0 무한.
                        .SetBoundType(data.BounceType)
                        .SetSpeed(data.Speed / 10f)
                        .SetDuration(data.DurationTime / 1000f)
                        .SetUnit(caster)
                        .SetAccDamageFunc(AccDamageFunc)
                        .Build();

                   
                }
            }, () =>
            {
                if (count > 0)
                {
                    castingTime = data.CountTime / 1000f;
                    Observable.FromCoroutine(Launch).Subscribe().AddTo(caster.disposables);
                }
                else if (data.NextSkillId > 0)
                {
                    stateMachine.SetNextState(data.NextSkillId);
                }
                else
                {
                    stateMachine.SetIdelState();
                }   
            });
        }

        // 해당 각도로 던짐
        public Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }
    }
}
using System;
using System.Collections;
using UnityEngine;
using UniRx;

// 360 방사패턴
namespace ProjectM.Battle._Fsm
{
    public class RadiationBounceProjectileState : SkillState, IState
    {
        private int count;
        public RadiationBounceProjectileState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
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
            SetCoolTime();
        }

        IEnumerator Launch()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            MovePause();
            yield return new WaitForSeconds(castingTime);
            count--;
            var dir = MyMath.GetDirection(targetUnit.Rigid.position, caster.Rigid.position);            
            caster.PlayAnim(data.Ani);
            MovePause();
            FlipX();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);

                bullet.position = caster.BodyParts[data.Pivot].position;
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                
                var projectileScript = bullet.GetComponent<Projectile>();
                // projectileScript.Pool = pool;
                // projectileScript.Init(Damage, dir, Per, BounceCount,  data.Speed / 10f );
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetPer(Per)
                    .SetBounceCount(BounceCount)
                    .SetBoundType(data.BounceType)
                    .SetVelocity(dir)
                    .SetSpeed(data.Speed / 10f)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(caster)
                    .Build();
                
                dir = Quaternion.AngleAxis(360f/data.ObjectValue, Vector3.forward) * dir;
            }

            if (count > 0)
            {
                yield return new WaitForSeconds(data.CountTime / 1000f);
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
        }
    }
}
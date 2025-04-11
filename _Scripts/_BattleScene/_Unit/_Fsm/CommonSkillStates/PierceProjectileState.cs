using System;
using System.Collections;
using UnityEngine;
using UniRx;

// 관통 투사체
namespace ProjectM.Battle._Fsm
{
    public class PierceProjectileState : SkillState, IState
    {
        private int count;
        public PierceProjectileState(UnitBase unit, SkillAI data, StateMachine stateMachine)
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
            SetCoolTime();
        }

        IEnumerator Launch()
        {
            MovePause();
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            caster.PlayAnim(data.Ani);
            MovePause();
            FlipX();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = GetScale();

                bullet.position = caster.BodyParts[data.Pivot].position;
                var dir = MyMath.GetDirection(bullet.position, targetUnit.Rigid.position);
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                
                var projectileScript = bullet.GetComponent<Projectile>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetPer(Per)
                    .SetVelocity(dir)
                    .SetSpeed(data.Speed / 10f)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(caster)
                    .SetAccDamageFunc(AccDamageFunc)
                    .Build();
            }

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
        }
    }
}
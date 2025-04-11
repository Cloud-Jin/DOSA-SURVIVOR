using System;
using System.Collections;
using UnityEngine;
using UniRx;

// 자체유도형 스킬

namespace ProjectM.Battle._Fsm
{
   public class GuideRadarProjectileState : SkillState, IState
    {
        private int count;
        
        public GuideRadarProjectileState(UnitBase unit, SkillAI data, StateMachine stateMachine)
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
            --count;
            MovePause();
            yield return new WaitForSeconds(castingTime);
            
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            caster.PlayAnim(data.Ani);
            MovePause();
            FlipX();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);

                // 데이터에 따른 발사 위치
                bullet.position = caster.BodyParts[data.Pivot].position;
                var dir = MyMath.GetDirection(bullet.position, targetUnit.Rigid.position);
                
                var projectileScript = bullet.GetComponent<Projectile>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetAutoTarget(targetUnit, data.Speed/10f)
                    .SetPer(Per)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(caster)
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
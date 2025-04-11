using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;

/*"
    타겟 대상 기준 n범위 랜덤한 곳에 함정이 소환됨
    TypeValue 값이 0이라면 타겟대상 위치
    지속시간 종료 시, 대미지를 안줌
*/
namespace ProjectM.Battle._Fsm
{
    public class TargetTrapState : SkillState, IState
    {
        public TargetTrapState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            Init();
        }
        
        public void Enter()
        {
            castingTime = data.CastingTime * 0.001f;
            count = data.Count;
            
            FindTargetNearest();
            if (IsDistance())
            {
                isStatePlay = true;
                Observable.FromCoroutine(Trap).Subscribe().AddTo(caster.disposables);    
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
        
        IEnumerator Trap()
        {
            --count;
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            MovePause();
            yield return new WaitForSeconds(castingTime);
            
            // while (!IsDistance())
            // {
            //     MoveTo();
            //     yield return null;
            // }
            
            MovePause();
            
            caster.PlayAnim(data.Ani);
            FlipX();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                if (data.PriorityTarget > 0)
                {
                    targetUnit = PriorityTarget(0);
                    if (targetUnit == null)
                        targetUnit = caster;
                }

                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = GetScale();
                bullet.position = targetUnit.Rigid.position + MyMath.GetCircleSize(data.TypeValue / 10f);
                
                var projectileScript = bullet.GetComponent<DamageOverTime>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetTick(data.DamegeTime *0.001f)
                    .SetDuration(data.DurationTime *0.001f, null)
                    .SetBlockType(data.ProjectileBlockType, data.ProjectileBlockedType)
                    .SetAccDamageFunc(AccDamageFunc)
                    .SetUnit(caster)
                    .Build();
                // yield return new WaitForSeconds(data.CountTime / 1000f);
            }

            if (count > 0)
            {
                castingTime = data.CountTime * 0.001f;
                Observable.FromCoroutine(Trap).Subscribe().AddTo(caster.disposables);
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
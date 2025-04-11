using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

/*
    시전자 기준 n범위 랜덤한 곳에 함정이 소환됨
    TypeValue 값이 0이라면 시전자 위치
    지속시간 종료 시, 대미지를 안줌
*/
namespace ProjectM.Battle._Fsm
{
    public class TrapState : SkillState, IState
    {
        public TrapState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
        }

        public void Enter()
        {
            castingTime = data.CastingTime * 0.001f;
            count = data.Count;
            
            FindTargetNearest();
            Observable.FromCoroutine(Trap).Subscribe().AddTo(caster);
        }

        public void Tick()
        {
            
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
        }
        
        IEnumerator Trap()
        {
            --count;
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            MovePause();
            yield return new WaitForSeconds(castingTime);
            caster.PlayAnim(data.Ani);
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);
                bullet.position = caster.Rigid.position + MyMath.GetCircleSize(data.TypeValue / 10f);
                
                var projectileScript = bullet.GetComponent<DamageOverTime>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetTick(data.DamegeTime *0.001f)
                    .SetDuration(data.DurationTime *0.001f, null)
                    .SetBlockType(data.ProjectileBlockType, data.ProjectileBlockedType)
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
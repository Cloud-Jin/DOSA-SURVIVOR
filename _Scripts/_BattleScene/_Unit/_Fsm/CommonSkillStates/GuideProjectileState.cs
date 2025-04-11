using System;
using System.Collections;
using UnityEngine;
using UniRx;

// 유도형 스킬

namespace ProjectM.Battle._Fsm
{
    public class GuideProjectileState : SkillState, IState
    {
        private int count;
        private DamageOverTime bullet;
        public GuideProjectileState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            Init();
        }

        public void Enter()
        {
            rigid = caster.Rigid;
            castingTime = data.CastingTime *0.001f;
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
            if (targetUnit && bullet)
            {
                var dir = MyMath.GetDirection(bullet.transform.position, targetUnit.Rigid.position);
                bullet.SetVelocity(dir * data.Speed / 10f);
            }
        }

        public void Exit()
        {
            SetCoolTime();
        }

        IEnumerator Launch()
        {
            --count;
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
                bullet.localScale = Vector3.one * (data.Scale / 100f);

                // 데이터에 따른 발사 위치
                bullet.position = caster.BodyParts[data.Pivot].position;
                var dir = MyMath.GetDirection(bullet.position, targetUnit.Rigid.position);
                
                var projectileScript = bullet.GetComponent<DamageOverTime>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetVelocity(dir)
                    .SetSpeed(data.Speed/10f)
                    .SetTick(data.DamegeTime *0.001f)
                    .SetDuration(data.DurationTime *0.001f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(caster)
                    .Build();

                this.bullet = projectileScript;
                Observable.FromCoroutine(() => Trace(projectileScript)).Subscribe().AddTo(projectileScript.disposables);
            }
            
            if (count > 0)
            {
                castingTime = data.CountTime *0.001f;
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

        IEnumerator Trace(DamageOverTime bullet)
        {
            while (true)
            {
                var dir = MyMath.GetDirection(bullet.transform.position, targetUnit.Rigid.position);
                bullet.SetVelocity(dir * data.Speed / 10f);
                bullet.rigid.velocity = (dir * data.Speed / 10f);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
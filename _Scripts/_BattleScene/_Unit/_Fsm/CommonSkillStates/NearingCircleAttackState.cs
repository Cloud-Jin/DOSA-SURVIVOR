using System;
using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 근접 평타.

namespace ProjectM.Battle._Fsm
{
    public class NearingCircleAttackState : SkillState, IState
    {
        private Scanner scanner;
        private float _durationtime;
        private float _damegeTime;
        private float timer;
        private bool running;

        public NearingCircleAttackState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            PoolManager.Instance.CreatePool("Effect_Skill_Warning_01b", 1); // 더미
            PoolManager.Instance.CreatePool(data.ObjectResource, 1); // 더미
        }

        public void Enter()
        {
            rigid = caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            _durationtime = data.DurationTime / 1000f;
            _damegeTime = data.DamegeTime / 1000f;
            running = true;
            FindTargetNearest();
            if (IsDistance())
            {
                isStatePlay = true;
                Observable.FromCoroutine(Attack).Subscribe().AddTo(caster.disposables);    
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
            caster.Rigid.drag = 0;
            running = false;
            SetCoolTime();
        }

        IEnumerator Attack()
        {
            MovePause();
            // var dir = MyMath.GetDirection(caster.transform.position, targetUnit.Rigid.position);
            // var angle = MyMath.GetAngle(caster.transform.position, targetUnit.transform.position) - 90f;
            // 인디케이터 범위생성
            // var indicatorPool = PoolManager.Instance.GetPool("Arc 1 Region");
            // var indicator = indicatorPool.Rent().GetComponent<ArcIndicator>();
            // indicator.transform.position = caster.Rigid.position;
            // indicator.transform.localRotation = Quaternion.identity;
            //
            // indicator.InitBuilder()
            //     .SetPool(indicatorPool)
            //     .SetAngle(-angle)
            //     .SetArc(data.TypeValue)
            //     .SetRadius(data.Scale / 100f)
            //     .SetDuration(castingTime)
            //     .Build();

            //castingTime 원 게이지
            {
                var pool = PoolManager.Instance.GetPool("Effect_Skill_Warning_01b");
                var effect = pool.Rent().GetComponent<ParticleBase>();
                effect.transform.position = caster.BodyParts[data.Pivot].transform.position;
                effect.transform.localScale = Vector3.one * (data.Scale / 100f);
                effect.Pool = pool;
                effect.SetStartLifeTime(castingTime);
                effect.SetReturnTime(castingTime, null);
            }

            yield return new WaitForSeconds(castingTime);

            caster.AnimatorPlayer.PlayMidEnd(data.Ani, () =>
            {
                // 원형 공격
                var pool = PoolManager.Instance.GetPool(data.ObjectResource);
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);

                // 데이터에 따른 발사 위치
                bullet.position = caster.BodyParts[data.Pivot].transform.position;
                
                var projectileScript = bullet.GetComponent<Projectile>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetPer(99)
                    .SetDuration(0.001f)
                    .SetKnockBack(data.KnockBack / 10f)
                    .SetUnit(caster)
                    .Build();

            }, () =>
            {
                if (count > 0)
                {
                    castingTime = data.CountTime / 1000f;
                    Observable.FromCoroutine(Attack).Subscribe().AddTo(caster.disposables);
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
    }
}
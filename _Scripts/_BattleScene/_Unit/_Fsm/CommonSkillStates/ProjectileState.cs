using System;
using System.Collections;
using UnityEngine;
using UniRx;

namespace ProjectM.Battle._Fsm
{
    public class ProjectileState : SkillState, IState
    {
        public ProjectileState(UnitBase unit, SkillAI data, StateMachine stateMachine)
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
            --count;
            MovePause();
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            
            var target = targetUnit;
            MovePause();
            FlipX();
            caster.AnimatorPlayer.PlayMidEnd(data.Ani, () =>
            {
             
                for (int i = 0; i < data.ObjectValue; i++)
                {
                    Transform bullet = pool.Rent().transform;
                    bullet.gameObject.SetActive(false);
                    // bullet.localPosition = Vector3.zero;
                    bullet.localRotation = Quaternion.identity;
                    bullet.localScale = GetScale();

                    // 데이터에 따른 발사 위치
                    bullet.position = caster.BodyParts[data.Pivot].position;
                    
                    bullet.gameObject.SetActive(true);
                    var dir = MyMath.GetDirection(bullet.position, target.Rigid.position);
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
                        .SetBlockType(data.ProjectileBlockType, data.ProjectileBlockedType)
                        .SetUnit(caster)
                        .SetAccDamageFunc(AccDamageFunc)
                        .Build();
                }
            }, () =>
            {
                MoveResume();
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
    }
}
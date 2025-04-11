using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;


namespace ProjectM.Battle._Fsm
{
    public class CrossProjectileState : SkillState, IState
    {
        private int count;
        List<Vector2> dirs = new List<Vector2>()
        {
          Vector2.up,
          Vector2.down,
          Vector2.left,
          Vector2.right
        };

        public CrossProjectileState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
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
            var pivotPos = Pivot.Dequeue();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);

                // 데이터에 따른 발사 위치
                bullet.position = pivotPos.position; //GetPivot().position;//caster.BodyParts[data.Pivot].position;
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dirs[i]);
                
                var projectileScript = bullet.GetComponent<Projectile>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetPer(Per)
                    .SetVelocity(dirs[i])
                    .SetSpeed(data.Speed / 10f)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(caster)
                    .Build();
                
                // if(data.AddExplosionAble > 0)
                //     projectileScript.AddExplosion(() =>
                //     {
                //         caster.BodyParts[$"{data.Index}"] = bullet;
                //         stateMachine.SetNextState(data.AddExplosionSkillId);
                //     });
                
                yield return new WaitForSeconds(data.CountTime / 1000f);
            }

            // Pivot.Dequeue();
            
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
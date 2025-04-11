using System;
using System.Collections;
using UnityEngine;
using UniRx;

namespace ProjectM.Battle._Fsm
{
    public class TargetBombState : SkillState, IState
    {
        public TargetBombState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            Init();
        }

        public void Enter()
        {
            rigid = caster.Rigid;
            castingTime = data.CastingTime * 0.001f;
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
            MovePause();
            yield return new WaitForSeconds(castingTime);
            
            MovePause();
            caster.PlayAnim(data.Ani);
            FlipX();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);

                // 데이터에 따른 발사 위치
                var endPosition = targetUnit.Rigid.position + MyMath.GetCircleSize(data.TypeValue / 10f);
                bullet.position = endPosition;
                // var dir = MyMath.GetDirection(bullet.position, targetUnit.Rigid.position);
                // bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                
                var projectileScript = bullet.GetComponent<ProjectileTrap>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetPer(Per)
                    // .SetVelocity(dir* data.Speed / 10f)
                    .SetbombScale(data.Scale / 100f)
                    .SetDuration(data.DurationTime *0.001f)
                    .SetKnockBack(data.KnockBack / 10f)
                    .SetAimResource("Effect_Skill_Warning_01", endPosition, (data.Scale / 100f) * projectileScript.Radius * 2)
                    // .SetBlockType(data.ProjectileBlockType, data.ProjectileBlockedType)
                    .SetUnit(caster)
                    .Build();

                if(data.AddExplosionAble > 0)
                    projectileScript.AddExplosion(() =>
                    {
                        caster.BodyParts[$"{data.Index}"] = bullet;
                        stateMachine.SetNextState(data.AddExplosionSkillId);
                    });
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
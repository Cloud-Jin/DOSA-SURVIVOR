using System;
using System.Collections;
using UnityEngine;
using UniRx;

namespace ProjectM.Battle._Fsm
{
    public class TargetTraceBombState : SkillState, IState
    {
        public TargetTraceBombState(UnitBase unit, SkillAI data, StateMachine stateMachine)
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

            isStatePlay = true;
            Observable.FromCoroutine(Launch).Subscribe().AddTo(caster.disposables);
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
            
            caster.PlayAnim(data.Ani);
            
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);

                // 데이터에 따른 발사 위치
                var endPostion = caster.Rigid.position + MyMath.GetCircleSize(data.TypeValue / 10f);
                bullet.position = endPostion;
                // var dir = MyMath.GetDirection(bullet.position, targetUnit.Rigid.position);
                // bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                
                var projectileScript = bullet.GetComponent<ProjectileTrap>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetPer(Per)
                    .SetDuration(data.DurationTime*0.001f)
                    // .SetKnockBack(data.KnockBack / 10f)
                    // .SetAimResource("Effect_Skill_Warning_01", endPostion)
                    .SetAutoTarget(targetUnit, data.Speed/10f)
                    .SetUnit(caster)
                    .Build();

                // if(data.AddExplosionAble > 0)
                    projectileScript.AddExplosion(() =>
                    {
                        projectileScript.SetAnimation("Play", null);
                    });
            }
            
            if (count > 0)
            {
                castingTime = data.CountTime*0.001f;
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
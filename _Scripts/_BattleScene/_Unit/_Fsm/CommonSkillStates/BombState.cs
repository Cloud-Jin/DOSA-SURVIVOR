using System;
using System.Collections;
using UnityEngine;
using UniRx;

namespace ProjectM.Battle._Fsm
{
    public class BombState : SkillState, IState
    {
        private int count;

        public BombState(UnitBase unit, SkillAI data, StateMachine stateMachine)
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

            isStatePlay = true;
            Observable.FromCoroutine(Launch).Subscribe().AddTo(caster);
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
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);

                // 데이터에 따른 발사 위치
                bullet.position = caster.Rigid.position + MyMath.GetCircleSize(data.TypeValue / 10f);
                // var dir = MyMath.GetDirection(bullet.position, targetUnit.Rigid.position);
                // bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                
                var projectileScript = bullet.GetComponent<ProjectileTrap>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetPer(Per)
                    .SetbombScale(data.Scale / 100f)
                    .SetDuration(data.DurationTime *0.001f)
                    .SetKnockBack(data.KnockBack / 10f)
                    .SetUnit(caster)
                    .Build();

                if(data.AddExplosionAble > 0)
                    projectileScript.AddExplosion(() =>
                    {
                        caster.BodyParts[$"{data.Index}"] = bullet;
                        stateMachine.SetNextState(data.AddExplosionSkillId);
                    });
                
                yield return new WaitForSeconds(data.CountTime *0.001f);
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
        
    }
}
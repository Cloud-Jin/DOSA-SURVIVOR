using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace ProjectM.Battle._Fsm
{
    public class MultiAngleProjectileState : SkillState, IState
    {
        List<float> convertAngle = new List<float>();
        
        public MultiAngleProjectileState(UnitBase unit, SkillAI data, StateMachine stateMachine)
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
            convertAngle = MyMath.CalcAngleCount(data.Angle, data.ObjectValue);
            
            
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
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = GetScale();

                // 데이터에 따른 발사 위치
                bullet.position = caster.BodyParts[data.Pivot].position;
                var dir = MyMath.GetDirection(bullet.position, targetUnit.Rigid.position);
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, Quaternion.AngleAxis(convertAngle[i], Vector3.forward) * dir);
                
                
                var projectileScript = bullet.GetComponent<Projectile>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetBoundType(data.BounceType)
                    .SetBounceCount(BounceCount)
                    .SetPer(Per)
                    .SetBlockType(data.ProjectileBlockType, data.ProjectileBlockedType)
                    .SetVelocity(Quaternion.AngleAxis(convertAngle[i], Vector3.forward) * dir)
                    .SetSpeed(data.Speed / 10f)
                    .SetDuration(data.DurationTime *0.001f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(caster)
                    .SetAccDamageFunc(AccDamageFunc)
                    .Build();
                
                if(data.AddExplosionAble > 0)
                    projectileScript.AddExplosion(() =>
                    {
                        // caster.BodyParts[$"{data.Index}"] = bullet;
                        stateMachine.GetState(data.AddExplosionSkillId).Pivot.Enqueue(bullet);
                        stateMachine.SetNextState(data.AddExplosionSkillId);
                    });
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
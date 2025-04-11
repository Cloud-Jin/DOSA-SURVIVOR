using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 근접 직선 공격

namespace ProjectM.Battle._Fsm
{
    public class NearingStraightAttackState : SkillState, IState
    {
        private Scanner scanner;
        private float _durationtime;
        private float _damegeTime;
        private float timer;
        
        List<float> convertAngle = new List<float>();
        
        public NearingStraightAttackState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            Init();

            convertAngle.Clear();
            convertAngle = MyMath.CalcAngleCount(data.Angle, data.ObjectValue);
        }

        public void Enter()
        {
            rigid = caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            _durationtime = data.DurationTime / 1000f;
            _damegeTime = data.DamegeTime / 1000f;
            
            if (targetUnit == null)
            {
                var scanner = caster.GetComponent<Scanner>();
                if (scanner.nearestTarget == null)
                {
                    stateMachine.SetIdelState();
                    return;
                }

                targetUnit = scanner.nearestTarget.GetComponent<UnitBase>();
            }

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
            SetCoolTime();
        }

        IEnumerator Attack()
        {
            MovePause();
            yield return new WaitForSeconds(castingTime);
            
            MovePause();
            caster.AnimatorPlayer.Play(data.Ani, EndCallback);
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            var dir = MyMath.GetDirection(caster.BodyParts[data.Pivot].position, targetUnit.Rigid.position);
            FlipX();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = Quaternion.identity;
                projectile.position = caster.BodyParts[data.Pivot].position;
                projectile.rotation = Quaternion.FromToRotation(Vector3.up, Quaternion.AngleAxis(convertAngle[i], Vector3.forward) * dir);
                projectile.transform.SetParent(caster.transform);
                
                var projectileScript = projectile.GetComponent<DamageOverTime>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(Damage)
                    .SetTick(data.DamegeTime / 1000f)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetKnockBack(data.KnockBack / 10f)
                    .SetUnit(caster)
                    .SetAccDamageFunc(AccDamageFunc)
                    .Build();
            }
        }

        void EndCallback()
        {
            if (count > 0)
            {
                castingTime = data.CountTime *0.001f;
                Observable.FromCoroutine(Attack).Subscribe().AddTo(caster.disposables);
            }
            else if (data.NextSkillId > 0)
            {
                stateMachine.SetNextState(data.NextSkillId);
            }
            else
            {
                MoveResume();
                stateMachine.SetIdelState();
            }
        }
    }
}
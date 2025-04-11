using System;
using System.Collections;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

/*
DurationTime = 장판 지속시간 설정
ㄴ 0이면 지속 시간 없이 장판 계속 존재
ㄴ 1이상이면 지속 시간 동안만 장판 존재

Scale = 장판 크기 설정
DamageTime = 대미지 주기 설정
Speed = 장판 이동 속도
*/

namespace ProjectM.Battle._Fsm
{
    public class BounceProjectilePassState : SkillState, IState
    {
        private int count;
        public BounceProjectilePassState(UnitBase unit, SkillAI data, StateMachine stateMachine)
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
            count--;
            yield return new WaitForSeconds(castingTime);
            
            MovePause();
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            var dir = (targetUnit.Rigid.position - (Vector2)GetPivot().position).normalized;
            
            FlipX();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = GetScale();
                
                var endPosition = targetUnit.Rigid.position + MyMath.GetCircleSize(data.TypeValue / 10f);
                bullet.position = endPosition;
                // bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

                var bulletScript = bullet.GetComponent<DamageOverTime>();
                bulletScript.InitBuilder()
                    .SetDamage(Damage)
                    .SetBounceCount(BounceCount)
                    .SetPer(Per)
                    .SetVelocity(Random.insideUnitCircle.normalized)
                    .SetDuration(data.DurationTime/1000f)
                    .SetSpeed(data.Speed/10f)
                    .SetTick(data.DamegeTime/1000f)
                    .SetAccDamageFunc(AccDamageFunc)
                    .SetDamageTime(data.DamegeTime/1000f)
                    .SetUnit(caster)
                    .Build();
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
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

// 영웅 평타.

namespace ProjectM.Battle._Fsm
{
    public class NearingAttackState : SkillState, IState
    {
        private Scanner scanner;
        
        public NearingAttackState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;

            var hero = caster as Hero;
            if (hero)
            {
                AccDamageFunc = hero.AccDamageFunc;
            }
            // Init();
        }
        
        public void Enter()
        {
            rigid = caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            
            
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
            SetCoolTime();
        }

        IEnumerator Attack()
        {
            MovePause();
            yield return new WaitForSeconds(castingTime);
            
            MovePause();
            if (targetUnit.isLive && targetUnit.unitState == UnitState.Normal)
            {
                caster.PlayAnim(data.Ani);
                FlipX();
                targetUnit.GetComponent<IDamageable>().TakeDamage(Damage, HitType.Normal);
                AccDamageFunc?.Invoke(Damage);
            }
            
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
                stateMachine.SetIdelState();
            }
        }
    }
}
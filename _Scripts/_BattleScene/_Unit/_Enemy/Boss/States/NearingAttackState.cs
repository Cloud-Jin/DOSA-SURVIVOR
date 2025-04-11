/*using System;
using System.Collections;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

// 근접공격
namespace ProjectM.Battle._Fsm.Boss
{
    public class NearingAttackState : SkillState, IState
    {
        public float remainDistance;
        private Scanner scanner;
        
        public NearingAttackState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            Rigid = caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            
            scanner = caster.GetComponent<Scanner>();
            if (scanner.nearestTarget == null)
            {
                stateMachine.SetIdelState();
                return;
            }

            targetUnit = scanner.nearestTarget.GetComponent<UnitBase>();
            Observable.FromCoroutine(Attack).Subscribe().AddTo(caster);
        }

        public void Tick()
        {
            if(targetUnit.isLive)
                caster.Spriter.flipX = targetUnit.Rigid.position.x > Rigid.position.x;
        }

        public void Exit()
        {
            Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        IEnumerator Attack()
        {
            yield return new WaitForSeconds(castingTime);
            remainDistance = (targetUnit.Rigid.position - Rigid.position).magnitude;
            Vector2 dirVec = Vector2.zero;
            
            while (remainDistance > data.Range / 10f && targetUnit.isLive)
            {
                dirVec = (targetUnit.Rigid.position - Rigid.position).normalized;
                Vector2 newPosition = dirVec * (20f / 10f * Time.fixedDeltaTime); // 몬스터 스피드?
                Rigid.MovePosition(Rigid.position + newPosition);
                remainDistance = (targetUnit.Rigid.position - Rigid.position).magnitude;
                yield return new WaitForFixedUpdate();
            }
            var hero = caster.GetComponent<Hero>();
            Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            if (targetUnit.isLive)
            {
                hero.PlayAnim("Attack");
                targetUnit.GetComponent<IDamageable>().TakeDamage(Damage, HitType.Normal);
                stateMachine.SetCoolTime(data);
            }
            
            stateMachine.SetIdelState();
        }
    }
}*/
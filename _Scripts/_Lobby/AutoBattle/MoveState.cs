using System;
using UnityEngine;

namespace ProjectM.AutoBattle
{
    public class MoveState : IState
    {
        AutoUnit caster;
        private AutoBattleSkillAI Data => caster._battleSkillAI;
        
        public MoveState(AutoUnit unit)
        {
            caster = unit;
        }
        
        public void Enter()
        {
            
        }

        public void Stay()
        {
            AutoUnit target = caster.target;
            if (target == null || !target.isLive)
            {
                caster.State = AutoUnit.AutoState.Idle;
                return;
            }
            var isFlip =  target.transform.position.x > caster.transform.position.x;
            FlipX(isFlip);
            
            // 이동
            caster.transform.position = Vector3.MoveTowards(caster.transform.position, target.transform.position, Time.deltaTime * caster.moveSpeed); // 거리체크
            caster.AnimatorPlayer.Animator.SetFloat("Speed", 1);
            
            if (caster._battleSkillAI == null)
                return;
            
            if (Vector3.Distance(caster.transform.position, target.transform.position) <= Data.Range / 10f)  // 사거리안에 들어오면 공격
                caster.State = AutoUnit.AutoState.Attack;
        }

        public void Exit()
        {
            caster.AnimatorPlayer.Animator.SetFloat("Speed", 0);
        }
        
        void FlipX(bool isFlip)
        {
            var scale = caster.BodyParts["Parts"].transform.localScale;
            scale.x = (isFlip) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            caster.BodyParts["Parts"].transform.localScale = scale;
        }
    }
}
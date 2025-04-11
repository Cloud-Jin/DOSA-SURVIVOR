using System;
using System.Collections;
using System.Collections.Generic;
using ProjectM.AutoBattle;
using TMPro;
using UnityEngine;

namespace ProjectM.AutoBattle
{
    public class IdleState : IState
    {
        AutoUnit caster;

        public IdleState(AutoUnit unit)
        {
            caster = unit;
        }
        public void Enter()
        {
            // idle 애니
        }

        public void Stay()
        {
            if (caster.target == null || !caster.target.isLive)
            {
                ReturnPos();
            }
            else
            {
                caster.State = AutoUnit.AutoState.Move;
            }
        }

        public void Exit()
        {
            caster.AnimatorPlayer.Animator.SetFloat("Speed", 0);
        }

        void ReturnPos()
        {  
            var isFlip =  caster.returnPos.x > caster.transform.position.x;
            FlipX(isFlip);
            caster.transform.position = Vector3.MoveTowards(caster.transform.position, caster.returnPos, Time.deltaTime * 2f); // 거리체크
            caster.AnimatorPlayer.Animator.SetFloat("Speed", 1);
            if (Vector3.Distance(caster.transform.position, caster.returnPos) <= 0.1) 
                caster.AnimatorPlayer.Animator.SetFloat("Speed", 0);
            
            
            if (caster.returnPos.x == caster.transform.position.x && caster.returnPos.x > 0)
            {
                FlipX(true);
            }
        }
        
        void FlipX(bool isFlip)
        {
            var scale = caster.BodyParts["Parts"].transform.localScale;
            scale.x = (isFlip) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            caster.BodyParts["Parts"].transform.localScale = scale;
        }
        void FindTarget()
        {
            // var collider = Physics2D.OverlapCircleAll(caster.transform.position, 10f, LayerMask.GetMask("Enemy"));
            // if (collider.Length <= 0) return;
            //
            // AutoUnit nearestTarget = null;
            // float minDist = Mathf.Infinity;
            //
            // for (int i = 0; i < collider.Length; i++)
            // {
            //     AutoUnit temp = collider[i].GetComponent<AutoUnit>();
            //     if (temp.isLive) // 같은편 체크?
            //     {
            //         float dist = Vector3.Distance(caster.transform.position, temp.transform.position);
            //         if (dist < minDist)
            //         {
            //             minDist = dist;
            //             nearestTarget = temp;
            //         }
            //     }
            // }
            //
            // if (nearestTarget != null)
            // {
            //     caster.target = nearestTarget;  // 타겟 설정
            //     caster.State = AutoUnit.AutoState.Move;
            // }
        }
        
        
    }
}

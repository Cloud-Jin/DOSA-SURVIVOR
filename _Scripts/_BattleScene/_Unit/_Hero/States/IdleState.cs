
using System;
using UnityEngine;

namespace ProjectM.Battle._Fsm
{
    [Serializable]
    public class IdleState : IState
    {
        private Hero hero;
        private Player player;
        // public float FollowDist = 3f;
        // public float MaxFollowDist = 6.5f;
        // public float dist;
        
        //skill bar
        public float value;
        public int TargetIndex;
        
        private HeroStateMachine stateMachine;
         
        public IdleState(UnitBase hero, HeroStateMachine heroStateMachine)
        {
            this.hero = hero.GetComponent<Hero>();
            this.stateMachine = heroStateMachine;
        }
        public void Enter()
        {
            player = PlayerManager.Instance.player;
        }

        public void Tick()
        {
            stateMachine.StateMachineUpdate();
            
            // hero.Rigid.velocity = Vector2.zero;
            // hero.Anim.SetFloat("Speed", 0);
            
            // 영웅 쿨타임
            // if(hero.uiFollow)
            //     hero.TimerBar.Value = SetTimeValue();
            
            // dist = (player.transform.position - hero.transform.position).magnitude;
            // if (dist > MaxFollowDist)
            // {
            //     hero.StateMachine.TransitionTo(hero.StateMachine.teleportState);
            //     // 7.5 ~ 8 (화면 밖 )거리면 순간이동
            //     // 웨이브모드시 화면 증가하면 거리도 증가 ? 
            // }

            for (int i = 0; i < stateMachine.SkillStates.Count; i++)
            {
                if (stateMachine.coolTimes[i] <= 0)
                {
                    stateMachine.SkillStates[i].SetTarget(null);
                    stateMachine.TransitionTo(stateMachine.SkillStates[i]);
                    break;
                }
            }
            // else if (dist > FollowDist)
            // {
            //     hero.StateMachine.TransitionTo(hero.StateMachine.comebackState);
            // }
        }

        public void Exit()
        {
  
        }
        
        float SetTimeValue()
        {
            return Mathf.Abs(stateMachine.coolTimes[TargetIndex] - value) / value;
        }
    }
}
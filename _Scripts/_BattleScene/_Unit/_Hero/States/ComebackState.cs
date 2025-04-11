using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle._Fsm
{
    public class ComebackState : IState
    {
        private Hero hero;
        private float xDistance = -1.0f;// x값
        private float yDistance = 1.0f; // y값
        public ComebackState(UnitBase hero)
        {
            this.hero = hero.GetComponent<Hero>();
        }

        private Rigidbody2D target;
        private Rigidbody2D Rigid;

        public void Enter()
        {
            target = PlayerManager.Instance.PlayerRigidBody2D;
            Rigid = hero.Rigid;

            xDistance = Random.Range(-1.5f, 1.5f);
            yDistance = Random.Range(-1.5f, 1.5f);

        }

        public void Tick()
        {
            Vector2 dirVec = (target.position - Rigid.position + new Vector2(xDistance, yDistance));
            Vector2 nextVec = dirVec.normalized * (30f /10f * Time.fixedDeltaTime);
            Rigid.MovePosition(Rigid.position + nextVec);
            Rigid.velocity = Vector2.zero;
            
            hero.Spriter.flipX = target.position.x > Rigid.position.x;
            
            float dist = (target.position - Rigid.position).magnitude;
            
            if (dist <= 2.2f)
            {
                hero.StateMachine.TransitionTo(hero.StateMachine.idleState);
            }
            
        }

        public void Exit()
        {
            
        }
    }

}
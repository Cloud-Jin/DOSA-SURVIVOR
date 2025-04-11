using UnityEngine;

namespace ProjectM.Battle._Fsm
{
    public class TeleportState : IState
    {
        private Hero hero;
        
        private Rigidbody2D target;
        private Rigidbody2D Rigid;
        
        public TeleportState(UnitBase hero)
        {
            this.hero = hero.GetComponent<Hero>();
        }
        public void Enter()
        {
            target = PlayerManager.Instance.playerble.Rigid;
            hero.disposables.Clear();
            Rigid = hero.Rigid;
            
            
            
            for (int i = 0; i < 100; i++)
            {
                var pos = MyMath.GetCircleSize(3f);
                var newPos = target.position + pos;
                if (BattleManager.Instance.MapOverlapPoint(newPos))
                {
                    Rigid.MovePosition(newPos);
                    hero.transform.position = newPos;
                    break;
                }
            }
            
            hero.gameObject.SetActive(true);
            hero.StateMachine.TransitionTo(hero.StateMachine.idleState);
        }

        public void Tick()
        {
            // hero.StateMachine.TransitionTo(hero.StateMachine.idleState);
        }

        public void Exit()
        {
        }
    }
}
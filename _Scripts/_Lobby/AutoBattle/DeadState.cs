using UnityEngine;

namespace ProjectM.AutoBattle
{
    public class DeadState : IState
    {
        AutoUnit caster;
        
        public DeadState(AutoUnit unit)
        {
            caster = unit;
        }
        
        public void Enter()
        {
            caster.isLive = false;
            caster.AnimatorPlayer.Play("Dead", OnAnimationEnd);
        }

        public void Stay()
        {
         
        }

        public void Exit()
        {
         
        }
        
        void OnAnimationEnd()
        {
            caster.gameObject.SetActive(false);
            caster.DeadAction?.Invoke();
        }
    }
}
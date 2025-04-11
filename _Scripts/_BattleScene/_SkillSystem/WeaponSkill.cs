using UnityEngine;
using UniRx;

namespace ProjectM.Battle
{
    public class WeaponSkill : ActiveSkill
    {
        protected float timer;
        protected UIFollow uiFollow;
        protected ReactiveProperty<float> TimerBar;
        
        protected virtual void Start()
        {
            uiFollow = PlayerManager.Instance.player.UIFollow;
            // TimerBar.Subscribe(v => uiFollow.SetTimeBar(v)).AddTo(this);
        }
        protected void Running(Unit i)
        {
            timer += Time.deltaTime;
            // 기본공격 타임바
            TimerBar.Value = (timer / coolTime);
            
            if (timer >= coolTime)
            {
                Fire();
                timer = 0;
            }
        }
    }
}
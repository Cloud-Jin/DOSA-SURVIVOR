using System;
using UniRx;

namespace ProjectM.Battle
{
    public class AlarmBoss : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_AlarmBoss;
        public override PopupType PopupType => PopupType.Alarm;

        protected override void Init()
        {
            ParticleImageUnscaled();
            SoundManager.Instance.PlayFX("Boss_Alarm");
            Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(_ => Hide()).AddTo(this);
        }
    }
}

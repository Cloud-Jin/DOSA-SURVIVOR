using System;
using UniRx;

namespace ProjectM.Battle
{
    public class AlarmEnemyWave : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_AlarmEnemyWave;
        public override PopupType PopupType => PopupType.Alarm;

        protected override void Init()
        {
            ParticleImageUnscaled();
            SoundManager.Instance.PlayFX("Boss_Alarm");
            Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(_ => Hide()).AddTo(this);
        }
    }

}
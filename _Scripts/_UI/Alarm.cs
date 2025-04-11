using System;
using DTT.Utils.Extensions;
using TMPro;
using UniRx;
using UnityEngine;

// ShowPendingPopup
// 예약 팝업노출x
namespace ProjectM
{
    public class Alarm : Popup
    {
        public override PopupName ID { get; set; }
        public override PopupType PopupType => PopupType.Alarm;
        
        //
        public TMP_Text messageLabel;
        public Transform msgPosition;
        
        // option
        private float _duration;
        private string _message;
        private bool _particleUnscaled;
        private string _openSound;
        
        protected override void Init()
        {
            
        }
        
        public Alarm InitBuilder()
        {
            _duration = 2f;
            _message = string.Empty;
            _openSound = string.Empty;
            _particleUnscaled = false;
            
            return this;
        }

        public void SetPositionY(float posY)
        {
            msgPosition.GetRectTransform().anchoredPosition = new Vector2(0, posY);
        }

        public Alarm SetDuration(float time)
        {
            _duration = time;
            return this;
        }
        
        public Alarm SetMessage(string message)
        {
            _message = message;
            return this;
        }
        
        public Alarm SetParticleUnscaled(bool value)
        {
            _particleUnscaled = value;
            return this;
        }

        public Alarm SetOpenSound(string sound)
        {
            _openSound = sound;
            return this;
        }
        
        public void Build()
        {
            Show();
            ShowPendingPopup = false;
            if(messageLabel && !string.IsNullOrEmpty(_message))
                messageLabel.SetText(_message);
            if(_particleUnscaled)
                ParticleImageUnscaled();
            if(!string.IsNullOrEmpty(_openSound))
                SoundManager.Instance.PlayFX(_openSound);
            
            // 기본 2초
            if(_duration > 0)
                Observable.Timer(TimeSpan.FromSeconds(_duration)).Subscribe(_ => Hide()).AddTo(this);
        }
    }
}
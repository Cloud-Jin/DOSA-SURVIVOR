using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using UniRx;
using UnityEngine;

// 플레이어 사망시 노출 팝업
namespace ProjectM.Battle
{
    public class PopupRevive : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_Revive;
        public UIButton AdButton, GemButton, GiveUpButton;
        public Action revive, giveUp;
        protected override void Init()
        {
            AdButton.AddEvent(AdRevive);
            GemButton.AddEvent(Revive);
            GiveUpButton.AddEvent(GiveUp);
        }
        
        

        void Revive()
        {
            Hide();
            revive?.Invoke();
        }

        void AdRevive()
        {
            AdMobManager.Instance.ShowAD(() =>
            {
                Hide();
                APIRepository.RequestStageRevive(t => { revive?.Invoke(); });
            },
            () =>
            {
                Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale("No_Ad"))
                    .Build();
            },
            () =>
            {
                Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale("Ad_Not_Completed"))
                    .Build();
            });
        }

        void GiveUp()
        {
            Hide();
            ShowPendingPopup = false;
            BattleManager.Instance.Lose();
        }
    }
}

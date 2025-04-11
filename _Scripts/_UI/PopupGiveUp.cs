using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;

namespace ProjectM.Battle
{
    public class PopupGiveUp : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_Giveup;
        public UIButton GiveUpBtn, ContinueBtn;
        protected override void Init()
        {
            SetShowPendingPopup(false);
            GiveUpBtn.AddEvent(OnClickGiveUp);
            ContinueBtn.AddEvent(OnClickContinue);
        }
        
        void OnClickGiveUp()
        {
            Time.timeScale = 1;
            Hide();
            UIManager.Instance.PendingPopupClear();
            BlackBoard.Instance.ResetData();
            QuickUI();
            SceneLoadManager.Instance.LoadScene("Lobby");
            
            
        }

        void QuickUI()
        {
            // var dIdx = BattleManager.Instance.tbStage.StageType;
            // if (dIdx == 1)
            // {
            //     // 던전
            //     UIManager.Instance.ReservePopup(() =>
            //     {
            //         var popup = UIManager.Instance.Get(PopupName.Dungeon_Select) as PopupDungeonSelect;
            //         popup.SetDungen(dIdx);
            //         popup.Show();
            //     });
            // }

            switch (BattleManager.Instance.tbStage.StageType)
            {
                case 1:
                    break;
                case 2:
                    UIManager.Instance.ReservePopup(() =>
                    {
                        var popup = UIManager.Instance.Get(PopupName.Dungeon_Select) as PopupDungeonSelect;
                        popup.SetDungen(1);
                        popup.Show();
                    });
                    break;
                case 3:
                    UIManager.Instance.ReservePopup(() =>
                    {
                        var popup = UIManager.Instance.Get(PopupName.HardDungeonSelect) as PopupHardDungeonSelect;
                        popup.Show();
                    });
                    break;
                default:
                    Debug.LogError("추가작업 필요");
                    break;
            }
        }

        void OnClickContinue()
        {
            Hide();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace ProjectM.Battle.Dungeon
{
    public class PopupResultDungeon : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_ResultDungeon;
        public UIButton yesBtn, damageBtn;
        public TMP_Text level;
        // public GameObject goVictory, goLose;
        public List<GameObject> victoryList, LoseList;
        // 타이틀, 난이도, 난이도단계, 확인.
        // public PopupResultDungeonScrollView rewardController;
        public CommonRewardScrollView rewardController;
        protected override void Init()
        {
            ShowPendingPopup = false;
            uiPopup.Buttons.Add(yesBtn);
            
            level.SetText("Gold_Dungeon_Stage".Locale(BlackBoard.Instance.data.dungeonLevel));
            
            uiPopup.OnShowCallback.Event.AddListener(()=>rewardController.ReloadScrollView());
            
            AddHideCallback(OnHide);
        }
        
        public void SetTitle(int idx)
        {
            // 0 Victory
            victoryList.ForEach(t => t.SetActive(idx == 0));
            LoseList.ForEach(t => t.SetActive(idx == 1));
            // goVictory.SetActive(idx == 0);
            // goLose.SetActive(idx == 1);
            if (idx == 0)
            {
                SoundManager.Instance.PlayFX("Stage_Win");
                // title.SetText("Dungeon_Clear".Locale());
            }
            else
            {
                SoundManager.Instance.PlayFX("Stage_Lose");
                // title.SetText("Dungeon_Fail".Locale());
            }
        }
        
        void OnHide()
        {
            SceneLoadManager.Instance.LoadScene("Lobby");
        }
        
        void OnClickDamage()
        {
            var popup = UIManager.Instance.Get(PopupName.Battle_Damage);
            popup.Show();
        }
        
        public void SetRewardData(List<ScrollDataModel> _data)
        {
            rewardController.SetData(_data);
        }
    }
}
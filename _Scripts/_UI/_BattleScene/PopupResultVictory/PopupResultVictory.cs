using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace ProjectM.Battle
{
    public class PopupResultVictory : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_ResultVictory;
        public UIButton YesBtn, damageBtn;
        public TMP_Text Chapter, Stage, KillScore, TimeScore, CrackKill;
        public GameObject goVictory, goLose, goHardMode;
        public PopupResultVictoryScrollView baseRewardController;
        public PopupResultVictoryScrollView addRewardController;
        // public GameObject goBenefit;
        // public TMP_Text benefitEffect;
        protected override void Init()
        {
            ShowPendingPopup = false;
            
            YesBtn.AddEvent(OnClickYesBtn);
            damageBtn.AddEvent(OnClickDamage);
            uiPopup.OnShowCallback.Event.AddListener(()=>baseRewardController.ReloadScrollView());
            uiPopup.OnShowCallback.Event.AddListener(()=>addRewardController.ReloadScrollView());
            var tbStage = TableDataManager.Instance.GetStageData(BlackBoard.Instance.data.mapIndex);
            Chapter.SetText($"{tbStage.ChapterID}");
            Stage.SetText($"{tbStage.StageLevel}");
            KillScore.SetText(BattleManager.Instance.kill.Value.ToString());
            float remainTime = BattleManager.Instance.gameTime.Value;
            int min = Mathf.FloorToInt(remainTime / 60);
            int sec = Mathf.FloorToInt(remainTime % 60);
            TimeScore.SetText($"{min:D2}:{sec:D2}");
            BattleManager.Instance.CrackKill.Subscribe(x=> CrackKill.SetText(x.ToString())).AddTo(this);
            
            
            goHardMode.SetActive(false);
            // 삭제 로직
            // var bb = BlackBoard.Instance.data;
            // if (bb.benefitEffect > 0)
            // {
            //     goBenefit.SetActive(true);
            //     benefitEffect.SetText("Membership_Additional_Reward".Locale(bb.benefitEffect));
            // }
            // else
            //     goBenefit.SetActive(false);
        }

        public void SetTitle(int idx)
        {
            // 0 Victory
            goVictory.SetActive(idx == 0);
            goLose.SetActive(idx == 1);
            
            if (idx == 0)
            {
                SoundManager.Instance.PlayFX("Stage_Win");
            }
            else
            {
                SoundManager.Instance.PlayFX("Stage_Lose");
            }
        }

        public void SetHardMode()
        {
            goHardMode.SetActive(true);
        }

        void OnClickYesBtn()
        {
            // UIManager.Instance.PendingPopupClear();
            SceneLoadManager.Instance.LoadScene("Lobby");
        }
        
        void OnClickDamage()
        {
            var popup = UIManager.Instance.Get(PopupName.Battle_Damage);
            popup.Show();
        }
        
        public void SetRewardData(List<ScrollDataModel> _data)
        {
            baseRewardController.SetData(_data);
        }
        
        public void SetAddRewardData(List<ScrollDataModel> _data)
        {
            addRewardController.SetData(_data);
        }
    }
}

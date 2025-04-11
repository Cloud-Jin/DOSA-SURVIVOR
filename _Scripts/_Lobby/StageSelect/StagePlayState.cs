using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class StagePlayState : MonoBehaviour
    {
        [SerializeField] private TMP_Text chapterNameTxt;
        
        [Space(20)] 
        
        [SerializeField] private List<Image> stageIcon;
        [SerializeField] private List<TMP_Text> stageTxt;
        [SerializeField] private List<Image> stageFill;
        
        [Space(20)] 
        
        [SerializeField] private UIButton chapterSelectBtn;
        [SerializeField] private UIButton goMaxStageBtn;

        private readonly int MAX_UI_LEVEL = 5;
        
        private void Start()
        {
            chapterSelectBtn.AddEvent(ShowStageSelectPopup);
            goMaxStageBtn.AddEvent(GoMaxStage);
            
            UserDataManager.Instance.stageInfo.SelectStage.Subscribe(_ => SetData()).AddTo(this);
            OptionSettingManager.Instance.curLangCode.Subscribe(_ => SetData()).AddTo(this);
        }

        public void SetData()
        {
            stageFill.ForEach(d => { d.SetActive(false); });
            stageIcon.ForEach(d => { d.SetActive(false); });

            for (int i = 0; i < MAX_UI_LEVEL; ++i)
                stageTxt[i].SetText($"{i + 1}");

            // 현재 선택중인 스테이지 정보 (초기값은 마지막 클리어한 스테이지)
            List<Stage> stageList = TableDataManager.Instance.data.Stage.Where(t => t.StageType == 1).ToList();
            Stage selectStage = stageList.SingleOrDefault(t => t.Index == UserDataManager.Instance.stageInfo.SelectStage.Value);
            Stage clearStage = stageList.SingleOrDefault(t => t.ChapterID == UserDataManager.Instance.stageInfo.StageData.chapter_id_cap
                                                              && t.StageLevel == UserDataManager.Instance.stageInfo.StageData.level_cap);
            
            if (selectStage == null) return;
            
            chapterNameTxt.SetText(LocaleManager.GetLocale(selectStage.Name));

            // 클리어한 스테이지가 없음?
            if (clearStage == null) return;
            
            if (selectStage.ChapterID < clearStage.ChapterID || clearStage.NextStage <= 0)
            {
                // 모든 챕터 클리어 및 클리어 한 챕터를 선택했을때...
                stageFill.ForEach(d => { d.SetActive(true); });

                if (selectStage.StageLevel <= MAX_UI_LEVEL)
                {
                    for (int i = 0; i < MAX_UI_LEVEL; ++i)
                        stageTxt[i].SetText($"{i + 1}");

                    stageIcon[selectStage.StageLevel - 1].SetActive(true);
                }
                else
                {
                    for (int i = 0; i < MAX_UI_LEVEL; ++i)
                        stageTxt[i].SetText($"{i + MAX_UI_LEVEL + 1}");

                    stageIcon[selectStage.StageLevel - MAX_UI_LEVEL - 1].SetActive(true);
                }
            }
            else if (clearStage.ChapterID < selectStage.ChapterID)
            {
                // 새로운 챕터 첫 스테이지 플레이 가능
                stageFill.ForEach(d => { d.SetActive(false); });
                
                for (int i = 0; i < MAX_UI_LEVEL; ++i)
                    stageTxt[i].SetText($"{i + 1}");
                
                stageIcon[0].SetActive(true);
            }
            else if (selectStage.ChapterID == clearStage.ChapterID)
            {
                if (selectStage.StageLevel <= MAX_UI_LEVEL && MAX_UI_LEVEL< clearStage.StageLevel)
                {
                    stageFill.ForEach(d => { d.SetActive(true); });

                    for (int i = 0; i < MAX_UI_LEVEL; ++i)
                        stageTxt[i].SetText($"{i + 1}");

                    stageIcon[selectStage.StageLevel - 1].SetActive(true);
                }
                else if (MAX_UI_LEVEL < selectStage.StageLevel && MAX_UI_LEVEL < clearStage.StageLevel)
                {
                    for (int i = 0; i < clearStage.StageLevel - MAX_UI_LEVEL; ++i)
                        stageFill[i].SetActive(true);
                    
                    for (int i = 0; i < MAX_UI_LEVEL; ++i)
                        stageTxt[i].SetText($"{i + MAX_UI_LEVEL + 1}");
                
                    stageIcon[selectStage.StageLevel - MAX_UI_LEVEL - 1].SetActive(true);
                }
                else if (selectStage.StageLevel <= MAX_UI_LEVEL && clearStage.StageLevel <= MAX_UI_LEVEL)
                {
                    for (int i = 0; i < clearStage.StageLevel; ++i)
                        stageFill[i].SetActive(true);
                    
                    for (int i = 0; i < MAX_UI_LEVEL; ++i)
                        stageTxt[i].SetText($"{i + 1}");

                    stageIcon[selectStage.StageLevel - 1].SetActive(true);
                }
                else if (MAX_UI_LEVEL < selectStage.StageLevel && clearStage.StageLevel <= MAX_UI_LEVEL)
                {
                    stageFill.ForEach(d => { d.SetActive(false); });
                
                    for (int i = 0; i < MAX_UI_LEVEL; ++i)
                        stageTxt[i].SetText($"{i + MAX_UI_LEVEL + 1}");
                
                    stageIcon[0].SetActive(true);
                }
            }

            if (selectStage.Index == clearStage.Index)
            {
                if (clearStage.NextStage <= 0)
                    goMaxStageBtn.SetActive(false);
                else
                    goMaxStageBtn.SetActive(true);
            }
            else
            {
                goMaxStageBtn.SetActive(selectStage.Index != clearStage.NextStage);
            }
        }

        private void ShowStageSelectPopup()
        {
            var popup = UIManager.Instance.Get(PopupName.AutoBattle_ChapterSelect) as StageSelectPopup;
            if (popup)
            {
                var chapterID = TableDataManager.Instance.data.Stage
                    .Single(t => t.Index == UserDataManager.Instance.stageInfo.PlayStage).ChapterID;

                popup.SetData(chapterID);
                popup.Show();
            }
        }

        private void GoMaxStage()
        {
            List<Stage> stageList = TableDataManager.Instance.data.Stage.Where(t => t.StageType == 1).ToList();
            Stage clearStage = stageList.SingleOrDefault(t => 
                t.ChapterID == UserDataManager.Instance.stageInfo.StageData.chapter_id_cap
                && t.StageLevel == UserDataManager.Instance.stageInfo.StageData.level_cap);

            if (clearStage != null)
            {
                UserDataManager.Instance.stageInfo.PlayStage =
                    clearStage.NextStage <= 0 ? clearStage.Index : clearStage.NextStage;
            }
            else
            {
                UserDataManager.Instance.stageInfo.PlayStage = 1;
            }
            
            SetData();
        }
    }
}

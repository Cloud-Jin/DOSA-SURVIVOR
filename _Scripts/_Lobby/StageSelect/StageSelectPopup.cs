using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace ProjectM
{
    public class StageSelectPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.AutoBattle_ChapterSelect;

        public UIButton BackButton;
        public UIButton nextBtn;
        public UIButton prevBtn;
        public UIButton viewDetailBtn;
        [Space(20)] 
        public TMP_Text chapterTxt;
        public TMP_Text ChaterName;
        [Space(20)]
        public Image ChaterIconBg;
        [Space(20)]
        public StageSelectScrollView stageSelectScrollView;

        private int _Current, ChapterCount;
        
        protected override void Init()
        {
            BackButton.AddEvent(() => Hide());

            nextBtn.AddEvent(NextChater);
            prevBtn.AddEvent(PrevChater);
            viewDetailBtn.AddEvent(OpenUrlViewDetails);
            
            uiPopup.OnShowCallback.Event.AddListener(ReloadData);
            
            ChapterCount =TableDataManager.Instance.data.ChapterType.Length;
            
            stageSelectScrollView.SetActive(false);
        }

        public void SetData(int chapterID)
        {
            _Current = chapterID;
            
            List<Stage> stages = TableDataManager.Instance.data.Stage
                .Where(t => t.StageType == 1 && t.ChapterID == _Current).ToList();
            
            stageSelectScrollView.SetData(stages);
        }

        public UIButton GetTutorialCellViewBtn()
        {
            return stageSelectScrollView.GetTutorialCellViewBtn();
        }

        private void ReloadData()
        {
            ChapterType chapterType = TableDataManager.Instance.data.ChapterType.Single(t => t.Index == _Current);
            chapterTxt.SetText(LocaleManager.GetLocale("Chapter_Number_Txt", chapterType.Index));
            ChaterName.SetText(LocaleManager.GetLocale(chapterType.ChapterName));
            ChaterIconBg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.AutoBattle).GetSprite(chapterType.ChapterBgResource);
            
            List<Stage> stages = TableDataManager.Instance.data.Stage.Where(t => t.ChapterID == _Current).ToList();
            
            stageSelectScrollView.SetActive(true);
            stageSelectScrollView.ReloadScrollView();

            int minChapter = TableDataManager.Instance.data.Stage.Select(t => t.ChapterID)
                .Where(t => t > 0).Distinct().Min();
            int maxChapter = TableDataManager.Instance.data.Stage.Select(t => t.ChapterID)
                .Where(t => t > 0).Distinct().Max();

            prevBtn.SetActive(true);
            nextBtn.SetActive(true);

            if (_Current <= minChapter)
            {
                prevBtn.SetActive(false);
            }
            else if (_Current >= maxChapter)
            {
                nextBtn.SetActive(false);
            }
            else
            {
                prevBtn.SetActive(true);
                nextBtn.SetActive(true);
            }
        }

        private void OpenUrlViewDetails()
        {
            var url = TableDataManager.Instance.data.Config.Single(t => t.Index == 36).Text;
            Application.OpenURL(url);
        }

        void NextChater()
        {
            if (_Current + 1 <= ChapterCount)
            {
                SetData(_Current + 1);
                ReloadData();
            }
        }

        void PrevChater()
        {
            if (_Current - 1 > 0)
            {
                SetData(_Current - 1);
                ReloadData();
            }
        }
    }
}
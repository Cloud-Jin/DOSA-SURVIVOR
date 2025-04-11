using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using EnhancedUI.EnhancedScroller;
using InfiniteValue;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class StageSelectScrollCellView : EnhancedScrollerCellView
    {
        private Stage _data;

        public UIButton[] rewardList;
        [Space(20)]
        public UIButton StageSelect;
        [Space(20)] 
        public Image rewardIcon;
        [Space(20)]
        public TMP_Text rewardQuantity;
        public TMP_Text StageLabel;
        public TMP_Text BaseRewardGold;
        public TMP_Text BaseRewardExp;
        [Space(20)]
        public Transform complete;
        public Transform enableStageSelect;
        public Transform disableStageSelect;

        private UIButton _tutorialBtn;
        
        private void Awake()
        {
            StageSelect.AddEvent(OnClickStage);
        }

        public void SetData(Stage data)
        {
            if (data.Index == 1)
                _tutorialBtn = StageSelect;
            
            _data = data;
            InfVal gold = new InfVal($"{data.RewardGold}");
            InfVal exp = data.RewardExp;
            
            StageLabel.SetText(LocaleManager.GetLocale(data.Name));
            BaseRewardGold.SetText($"{gold.ToGoldString()}");
            BaseRewardExp.SetText($"{exp.ToGoldString()}");
            
            rewardList.ForEach(d =>
            {
                d.ClearEvent();
                int childCount = d.transform.childCount;
                for (int i = 0; i < childCount; ++i)
                    d.transform.GetChild(i).SetActive(false);
                
                d.SetActive(false);
            });
            
            int rewardListIndex = 0;
            List<RewardGroup> goodsRewardGroups = TableDataManager.Instance.data.RewardGroup
                .Where(t => t.GroupID == data.GoodsRewardGroupID).OrderBy(t => t.Order).ToList();

            for (int i = 0; i < goodsRewardGroups.Count; ++i)
            {
                GoodsType goodsType = TableDataManager.Instance.data.GoodsType
                    .SingleOrDefault(t => t.TypeID == goodsRewardGroups[i].RewardID);
                
                if (goodsType == null) continue;
                
                rewardList[rewardListIndex].SetActive(true);
                
                Transform boxTransform;

                switch (goodsType.TypeID)
                {
                    case 2: 
                    {
                        boxTransform = rewardList[rewardListIndex].transform.Find("Dia");
                        break;
                    }
                    case 3:
                    {
                        boxTransform = rewardList[rewardListIndex].transform.Find("Obsidian");
                        break;
                    }
                    case 11:
                    {
                        boxTransform = rewardList[rewardListIndex].transform.Find("Gold");
                        break;
                    }
                    default:
                        continue;
                }

                if (boxTransform == false) continue;
                
                boxTransform.SetActive(true);
                string strRewardCnt;
                InfVal maxCnt = goodsRewardGroups[i].RewardMaxCnt;
                InfVal minCnt = goodsRewardGroups[i].RewardMinCnt;
                if (maxCnt == minCnt)
                    strRewardCnt = $"x{maxCnt.ToGoodsString()}";
                else
                    strRewardCnt = $"{minCnt.ToGoodsString()}~{maxCnt.ToGoodsString()}";
                
                boxTransform.Find("Quantity").GetComponent<TMP_Text>().SetText(strRewardCnt);
                boxTransform.Find("Icon").GetComponent<Image>().sprite 
                    = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite(goodsType.Icon);

                rewardListIndex++;
            }
            
            List<StageRewardGroup> stageRewardGroups = TableDataManager.Instance.data.StageRewardGroup
                .Where(t => t.GroupID == data.StageRewardIconGroupID).OrderBy(t => t.Index).ToList();
            
            for (int i = 0; i < stageRewardGroups.Count; ++i)
            {
                StageReward stageReward = TableDataManager.Instance.data.StageReward
                    .SingleOrDefault(t => t.Index == stageRewardGroups[i].Index);
                
                if (stageReward == null) continue;
                
                rewardList[rewardListIndex].SetActive(true);
                rewardList[rewardListIndex].AddEvent(() => OnClickBox(stageReward));
                
                Transform boxTransform = rewardList[rewardListIndex++].transform.Find("Box");
                boxTransform.SetActive(true);
                Sprite iconSpirte = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite(stageReward.StageRewardIcon);
                boxTransform.Find("Icon").GetComponent<Image>().sprite = iconSpirte;
                boxTransform.Find("Quantity").SetActive(false);

                ColorUtility.TryParseHtmlString(stageReward.StageRewardRarityColor, out var color);
                boxTransform.GetComponent<Image>().color = color;
            }
            
            List<RewardGroup> rewardGroups = TableDataManager.Instance.data.RewardGroup
                .Where(t => t.GroupID == data.FirstClearRewardGroupID).ToList();

            // 최초 클리어 보상은 고정 1개
            if (rewardGroups.Count > 0)
            {
                GoodsType goodsType = TableDataManager.Instance.data.GoodsType
                    .SingleOrDefault(t => t.TypeID == rewardGroups[0].RewardID);

                if (goodsType != null)
                    rewardIcon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite(goodsType.Icon);
             
                // 개수는 MAX 값
                rewardQuantity.SetText($"x{rewardGroups[0].RewardMaxCnt}");
            }

            List<Stage> stageList = TableDataManager.Instance.data.Stage.Where(t => t.StageType == 1).ToList();
            var stageData = UserDataManager.Instance.stageInfo.StageData;
            
            Stage clearStage = stageList.SingleOrDefault(t => t.ChapterID == stageData.chapter_id_cap 
                                                              && t.StageLevel == stageData.level_cap);
            Stage rewardStage = stageList.SingleOrDefault(t => t.ChapterID == stageData.chapter_id_reward 
                                                               && t.StageLevel == stageData.level_reward);
            
            if (clearStage == null) return;

            if (clearStage.NextStage <= 0)
            {
                StageSelect.enabled = true;
                enableStageSelect.SetActive(true);
                disableStageSelect.SetActive(false);
            }
            else
            {
                StageSelect.enabled = data.Index <= clearStage.NextStage;
                enableStageSelect.SetActive(StageSelect.enabled);
                disableStageSelect.SetActive(!StageSelect.enabled);
            }

            if (data.Index <= rewardStage.Index)
                complete.SetActive(true);
            else
                complete.SetActive(false);
        }

        public UIButton GetTutorialBtn()
        {
            return _tutorialBtn;
        }

        private void OnClickStage()
        {
            UserDataManager.Instance.stageInfo.PlayStage = _data.Index;
            UIManager.Instance.GetPopup(PopupName.AutoBattle_ChapterSelect).Hide();
        }

        private void OnClickBox(StageReward stageReward)
        {
            ColorUtility.TryParseHtmlString(stageReward.StageRewardRarityColor, out var color);
            
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.slotObj.SetActive(true);
                popup.iconImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite(stageReward.StageRewardIcon);
                popup.bgImg.color = color;
                
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale(stageReward.StageRewardName))
                    .SetMessage(LocaleManager.GetLocale(stageReward.StageRewardDescription))
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("Common_Ok_Btn"))
                    .Build();
            }
        }
    }
}
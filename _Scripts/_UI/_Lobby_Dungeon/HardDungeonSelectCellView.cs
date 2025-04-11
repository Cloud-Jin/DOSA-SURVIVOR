using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace ProjectM
{
    public class HardDungeonSelectCellView : EnhancedScrollerCellView
    {
        public TMP_Text stageTxt;
        [Space(20)]
        public List<ItemSlot> rewardItemList;
        [Space(20)]
        public Transform clearBg;
        public Transform notClearBg;
        
        public void SetData(Stage stage, Stage nextStage)
        {
            clearBg.SetActive(false);
            notClearBg.SetActive(false);

            if (nextStage != null)
            {
                if (nextStage.Index <= 0)
                    clearBg.SetActive(true);
                if (stage.Index < nextStage.Index)
                    clearBg.SetActive(true);
                else if (nextStage.Index < stage.Index)
                    notClearBg.SetActive(true);
            }
            else
            {
                clearBg.SetActive(true);
            }

            stageTxt.SetText(LocaleManager.GetLocale(stage.Name));
            List<RewardGroup> rewardList = TableDataManager.Instance.data.RewardGroup
                .Where(t => t.GroupID == stage.FirstClearRewardGroupID).ToList();

            for (int i = 0; i < rewardList.Count; ++i)
            {
                RewardData rewardData = new RewardData();
                rewardData.t = rewardList[i].Type;
                rewardData.i = rewardList[i].RewardID;
                rewardData.c = rewardList[i].RewardMaxCnt.ToString();

                List<ScrollDataModel> scrollDataModel = APIRepository.ConvertReward(new() { rewardData });
                rewardItemList[i].SetDataScrollDataModel(scrollDataModel[0]);
            }
        }
    }
}
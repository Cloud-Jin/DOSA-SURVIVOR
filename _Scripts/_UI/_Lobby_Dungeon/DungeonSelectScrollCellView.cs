using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using EnhancedUI.EnhancedScroller;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

// 던전 스테이지 슬롯
namespace ProjectM
{
    public class DungeonSelectScrollCellView : EnhancedScrollerCellView
    {
        public List<ItemSlot> ItemSlots;
        public UIButton viewBtn;
        public TMP_Text step;
        public GameObject goSelect;
        public GameObject goClear,goLock;

        private int _idx;
        private bool isEnter, isReward;
        private void Awake()
        {
            viewBtn.AddEvent(OnClick);
            UserDataManager.Instance.stageInfo.SelectDungeonGold.Subscribe(t=> OnSelect()).AddTo(this);
        }

        public void SetData(List<List<ScrollDataModel>> list, int i)
        {
            _idx = i;
            foreach(var item in list[i].Select((value, index) => (value, index)))
            {
                var data = item.value;
                var index = item.index;
                ItemSlots[index].SetDataScrollDataModel(data);
            }

            OnSelect();
            isEnter = UserDataManager.Instance.stageInfo.GoldDungeonData.level_cap >= _idx;
            isReward = UserDataManager.Instance.stageInfo.GoldDungeonData.level_reward > _idx;
            goClear.SetActive(isReward);
            goLock.SetActive(!isEnter);
            // UserDataManager.Instance.stageInfo
            // Stage clearStage = TableDataManager.Instance.data.Stage
            //     .SingleOrDefault(t => t.ChapterID == stageData.chapter_id_cap && t.StageLevel == stageData.level_cap);
            step.SetText("Gold_Dungeon_Stage".Locale(i+1));
        }

        void OnClick()
        {
            if (!isEnter) return;
            
            var popup = UIManager.Instance.GetPopup(PopupName.Dungeon_Select) as PopupDungeonSelect;
            popup.JumpToIndex(dataIndex);
        }

        void OnSelect()
        {
            // Debug.Log($"{ UserDataManager.Instance.stageInfo.PlayDungeonGold} / {_idx}");
            bool isSelect =  UserDataManager.Instance.stageInfo.PlayDungeonGold == _idx;
            goSelect.SetActive(isSelect);
        }

    }
}
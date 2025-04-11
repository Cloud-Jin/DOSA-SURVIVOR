using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class DungeonSlot : EnhancedScrollerCellView
    {
        public UIButton titleBtn, bgBtn;
        public Image bgImage;
        public Image goldIconImg;
        public TMP_Text title, enterCount, sweepCount, adEnterCount;
        public TMP_Text needOpenTxt;
        public Transform redDot;
        public Transform needOpen;
        public Transform enter;
        public Transform clear;
        public Transform addEnter;
        
        private Dungeon _data;
        private bool isLock;                    // 컨텐츠 잠금
        private void Awake()
        {
            UserDataManager.Instance.clientInfo.DataUpdate.Where(t => isLock).Subscribe(t =>
            {
                if(_data != null)
                    SetData(_data);
            }).AddTo(this);

            LocaleManager.Instance.onUpdate.Subscribe(t =>
            {
                if(_data != null)
                    SetData(_data);
            }).AddTo(this);
        }

        public void SetData(Dungeon data)
        {
            _data = data;

            LocaleManager.Instance.onUpdate.Subscribe(i =>
            {
                title.SetText(data.Name.Locale());
            }).AddTo(this);
            
            bgImage.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.AutoBattle).GetSprite(data.Image);
            
            if (_data.Index == 1)
            {
                SetGoldDungeonUI();
            }
            else if (_data.Index == 2)
            {
                DungeonConfig dungeonConfig = TableDataManager.Instance.data.DungeonConfig
                    .Single(t => t.Index == 101);
                
                UnlockType unlockType = TableDataManager.Instance.data.UnlockType
                    .Single(t => t.Type == dungeonConfig.Value);
                UnlockCondition unlockCondition = TableDataManager.Instance.data.UnlockCondition
                    .Single(t => t.Index == unlockType.UnlockConditionID);
                
                isLock = !UserDataManager.Instance.clientInfo.GetUnlockData(dungeonConfig.Value);   // 던전클리어 체크 
                if (unlockCondition.ConditionType == 1)
                {
                    Stage unLockStage = TableDataManager.Instance.data.Stage
                        .Single(t => t.Index == unlockCondition.ConditionValue);
                    StageData stageData = UserDataManager.Instance.stageInfo.Data
                        .SingleOrDefault(u => u.type == 1);
                    List<Stage> stageList = TableDataManager.Instance.data.Stage
                        .Where(t => t.StageType == 1).ToList();
                    
                    // Stage clearStage;
                    
                    // if (stageData == null)
                    // {
                    //     clearStage = stageList.Single(t => t.ChapterID == 1 && t.StageLevel == 1);
                    //     
                    //     needOpen.SetActive(true);
                    // }
                    // else
                    // {
                    //     clearStage = stageList.Single(t => t.ChapterID == stageData.chapter_id_cap && t.StageLevel == stageData.level_cap);
                    //     needOpen.SetActive(false);
                    // }
                    
                    if (!isLock)
                    {
                        needOpen.SetActive(false);
                        
                        titleBtn.SetActive(true);
                        titleBtn.ClearEvent();
                        titleBtn.AddEvent(OnClick);
            
                        bgBtn.SetActive(true);
                        bgBtn.ClearEvent();
                        bgBtn.AddEvent(OnClick);
                    }
                    else
                    {
                        needOpen.SetActive(true);
                        needOpenTxt.SetText(LocaleManager.GetLocale(unlockCondition.ConditionDescription, unLockStage.Name.Locale()));
                        titleBtn.SetActive(false);
                        bgBtn.SetActive(true);
                    }
                    
                    enter.SetActive(false);
                    clear.SetActive(false);
                    addEnter.SetActive(false);
                    goldIconImg.SetActive(false);
                    
                    redDot.SetActive(false);
                }
            }
            else
            {
                // 심연
                enterCount.SetText("Enter_Count".Locale(UserDataManager.Instance.stageInfo.GoldDungeonData.enter_count, data.EnterCount));
                sweepCount.SetText("Sweep_Count".Locale(UserDataManager.Instance.stageInfo.GoldDungeonData.ffwd_ad_count, data.SweepCount));
                adEnterCount.SetText("Add_Enter_Count".Locale(UserDataManager.Instance.stageInfo.GoldDungeonData.enter_extra_count, data.AddEnterCount));
            }
        }

        void OnClick()
        {
            if (_data.Index == 1)
            {
                // data에 따라 던전 팝업수정
                var popup = UIManager.Instance.Get(PopupName.Dungeon_Select) as PopupDungeonSelect;
                popup.SetDungen(_data.Index);
                popup.Show();
            }
            else if (_data.Index == 2)
            {
                var popup = UIManager.Instance.Get(PopupName.HardDungeonSelect) as PopupHardDungeonSelect;
                popup.Show();
            }
        }

        void SetGoldDungeonUI()
        {
            // 골드던전
            enter.SetActive(true);
            clear.SetActive(false);
            addEnter.SetActive(false);
            goldIconImg.SetActive(true);
            
            LocaleManager.Instance.onUpdate.Subscribe(i =>
            {
                // 입장횟수 , 입장가능횟수
                var goldDungeonData = UserDataManager.Instance.stageInfo.GoldDungeonData;
                var count = (_data.EnterCount * (goldDungeonData.enter_extra_count + 1));
                var eCount = goldDungeonData.enter_count + goldDungeonData.ffwd_count;
                enterCount.SetText("Enter_Count".Locale(eCount, count));
                // 황금 던전 입장 남은 횟수, 광고 시청(입장 초기화)가 있을 경우 표시
                // bool isRedDot = count > 0 || goldDungeonData.enter_extra_count < _data.AddEnterCount;
                
                bool isEnter = (count) - eCount > 0;
                bool isRedDot =  isEnter || (goldDungeonData.enter_extra_count < _data.AddEnterCount);

                
                redDot.SetActive(isRedDot);
                
                // sweepCount.SetText("Sweep_Count".Locale(UserDataManager.Instance.stageInfo.GoldDungeonData.ffwd_ad_count, _data.SweepCount));
                // adEnterCount.SetText("Add_Enter_Count".Locale(UserDataManager.Instance.stageInfo.GoldDungeonData.enter_extra_count, _data.AddEnterCount));
            }).AddTo(this);
            
            
            titleBtn.ClearEvent();
            titleBtn.AddEvent(OnClick);
            
            bgBtn.ClearEvent();
            bgBtn.AddEvent(OnClick);
            
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Newtonsoft.Json.Linq;
using TimerTool;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class PopupDungeonClear : Popup
    {
        public override PopupName ID { get; set; } = PopupName.DungeonClear;
        public Image dungeonBg;
        public TMP_Text dungeonTitle, clearStep, ffwdCount, sweepTxt, adDisableRemainTimeTxt, sweepCountTxt, energyTxt;
        public UIButton adClearBtn;
        public UIButton clearBtn;
        public UIButton plusBtn;
        public UIButton minusBtn;
        public UIButton closeBtn;
        public ItemSlot ItemSlot;
        public Transform adDisable;

        private int _index;
        private int sweepCount;
        private int count;
        private int sweepConsumeCount;
        private IDisposable _adDisableDisposable;
        
        protected override void Init()
        {
            // adClearBtn.AddEvent(OnClickAdClear);
            clearBtn.AddEvent(OnClickClear);
            closeBtn.AddEvent(Hide);
            plusBtn.AddEvent(OnPlus);
            minusBtn.AddEvent(OnMinus);

            uiPopup.OnShowCallback.Event.AddListener(() =>
            {
                // itemSlot
                var level = UserDataManager.Instance.stageInfo.GoldDungeonData.level_cap;
                var d = TableDataManager.Instance.data.DungeonLevel.Single(t => t.Level == level);
                List<RewardData> _rewardDatas = new List<RewardData>()
                {
                    new RewardData()
                    {
                        t = 1,
                        i = d.RewardType,
                        c = d.Reward.ToString()        
                    }
                };
                List<ScrollDataModel> convertReward = APIRepository.ConvertReward(_rewardDatas);
                ItemSlot.SetDataScrollDataModel(convertReward[0]);
            });
        }

        public void SetDungen(int index)
        {
            _index = index;
            var data = TableDataManager.Instance.data.Dungeon.Single(t => t.Index == index);
            var goldDungeonData = UserDataManager.Instance.stageInfo.GoldDungeonData;
            var tbStage = TableDataManager.Instance.data.Stage.Single(t => t.DungeonIndex == index);
            dungeonTitle.SetText(tbStage.Name.Locale());
            dungeonBg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.AutoBattle).GetSprite(data.Image);
            sweepConsumeCount = data.SweepConsumeCount;
            // enterCount = (tbDungeon.EnterCount * (goldDungeonData.enter_extra_count + 1 )) - (goldDungeonData.enter_count + goldDungeonData.ffwd_count);
            sweepCount = (data.EnterCount * (goldDungeonData.enter_extra_count + 1 )) - (goldDungeonData.enter_count + goldDungeonData.ffwd_count);
            count = sweepCount;
            // var sweepCount = data.SweepCount - goldDungeonData.ffwd_ad_count; 
            // clearStep.SetText("Gold_Dungeon_Stage".$"L {goldDungeonData.level_cap}단계");
            clearStep.SetText("Gold_Dungeon_Stage".Locale(goldDungeonData.level_cap));
            ffwdCount.SetText("Dungeon_Count".Locale(sweepCount));
            clearBtn.interactable = sweepCount > 0;
            sweepCountTxt.SetText($"{count}");
            energyTxt.SetText($"x {sweepConsumeCount*count}");

            SetData();
        }
        
        public void SetData()
        {
            if (TimerManager.Instance.GetAdTimer(TimerManager.AdTimerType.FAST_DUNGEON_CLEAR) != null)
            {
                adClearBtn.interactable = false;
                
                sweepTxt.SetActive(false);
                adDisable.SetActive(true);
                adDisableRemainTimeTxt.SetActive(true);
                
                if (_adDisableDisposable == null)
                    _adDisableDisposable = this.UpdateAsObservable().Subscribe(_ => UpdateAdRemainTime()).AddTo(this);
            }
            else
            {
                var data = TableDataManager.Instance.data.Dungeon.Single(t => t.Index == _index);
                var goldDungeonData = UserDataManager.Instance.stageInfo.GoldDungeonData;
                var sweepCount = data.SweepCount - goldDungeonData.ffwd_ad_count; 
                adClearBtn.interactable = sweepCount > 0;

                sweepTxt.SetActive(true);
                adDisable.SetActive(false);
                adDisableRemainTimeTxt.SetActive(false);
            }
        }
        
        private void OnPlus()
        {
            ++count;
            
            if (count >= sweepCount)
                count = sweepCount;

            ReloadData();
        }
        
        private void OnMinus()
        {
            --count;
            
            if (count <= 0)
                count = (sweepCount == 0) ? 0 : 1;

            ReloadData();
        }

        public void ReloadData()
        {
            sweepCountTxt.SetText($"{count}");
            energyTxt.SetText($"x {sweepConsumeCount*count}");
        }
        
        private void UpdateAdRemainTime()
        {
            Timer adTimer = TimerManager.Instance.GetAdTimer(TimerManager.AdTimerType.FAST_DUNGEON_CLEAR);
            if (adTimer == null) return;

            TimeSpan remainTime = TimeSpan.FromSeconds(adTimer.GetTimeRemaining());
            if (remainTime.TotalSeconds <= 0)
            {
                _adDisableDisposable?.Dispose();
                _adDisableDisposable = null;
                
                TimerManager.Instance.RemoveAdTimer(TimerManager.AdTimerType.FAST_DUNGEON_CLEAR);
                return;
            }
            
            adDisableRemainTimeTxt.SetText($"{remainTime.Minutes:D2}"+":"+$"{remainTime.Seconds:D2}");
        }

        void OnClickAdClear()
        {
            AdMobManager.Instance.ShowAD(() =>
            {
                SetData();
                
                var payload = new Dictionary<string, object>
                {
                    { "type", 2 }, { "extra", 1 }
                };

                APIRepository.RequestFastAutoBattleReward(payload, data =>
                {
                    OnResponseSucces(data);
                }, reply =>
                {
                    var key = reply.Response.Unbox<JObject>()["reason"]?.ToString();
                    Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    alarm.InitBuilder()
                        .SetMessage(key.Locale())
                        .Build();
                });
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
            }, TimerManager.AdTimerType.FAST_DUNGEON_CLEAR);
        }

        void OnClickClear()
        {
            var payload = new Dictionary<string, object>
            {
                { "type", 2 }, { "count", count }
            };

            APIRepository.RequestFastAutoBattleReward(payload, data =>
            {
                OnResponseSucces(data);
            }, reply =>
            {
                var key = reply.Response.Unbox<JObject>()["reason"]?.ToString();
                Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(key.Locale())
                    .Build();
            });
        }
        
        void OnResponseSucces(FastBattleRewardData data)
        {
            ShowPendingPopup = false;
            
            Hide();
            // SetCount();
            var rewardList = APIRepository.ConvertReward(data.rewards);
            // rewardList.AddRange(APIRepository.ConvertReward(data.currency_box_rewards));
            // rewardList.AddRange(APIRepository.ConvertReward(data.gear_box_rewards));
            
            var popup = UIManager.Instance.Get(PopupName.Common_Reward) as CommonRewardPopup;
            popup.AddShowCallback(UIManager.Instance.HideLoadingPopup);
            popup.SetData(rewardList);
            popup.Show();

            if (UILobby.Instance)
            {
                UILobby.Instance.Contents[4].GetComponent<ContainerDungeon>().RedDotCheck();
                ((PopupDungeonSelect)UIManager.Instance.GetPopup(PopupName.Dungeon_Select)).RedDotCheck();
                var popupDungeonSelect = UIManager.Instance.GetPopup(PopupName.Dungeon_Select) as PopupDungeonSelect;
                popupDungeonSelect?.SetData();
            }
        }
    }

}
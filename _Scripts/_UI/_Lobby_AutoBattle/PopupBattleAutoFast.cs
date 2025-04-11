using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Newtonsoft.Json.Linq;
using TimerTool;
using TMPro;
using UniRx;
using UniRx.Triggers;

namespace ProjectM
{
    public class PopupBattleAutoFast : Popup
    {
        public override PopupName ID { get; set; } = PopupName.AutoBattle_BattleAutoFast;
        public UIButton AdRewardBtn, RewardBtn, CloseBtn;
        public TMP_Text timeLabel, adCountLabel, countLabel, energyLabel, adDisableRemainTimeTxt;
        private int basicEnterCount, basicEnergy;
        
        private IDisposable _adDisableDisposable;
        
        protected override void Init()
        {
            CloseBtn.AddEvent(Close);
            AdRewardBtn.AddEvent(OnClickAdReward);
            RewardBtn.AddEvent(OnClickReward);
            
            var data = TableDataManager.Instance.data.AutoBattleConfig;
            var _minutes = data.Single(t => t.Index == 2).Value + UserDataManager.Instance.payInfo.IncreaseQuickBattleRewardTime;
            var _time =  TimeSpan.FromMinutes(_minutes).TotalSeconds;
            int _hour = (int)(_time / 3600);
            int _min = (int)(_time % 3600 / 60);
            int _sec = (int)(_time % 3600 % 60);
            
            timeLabel.SetText(LocaleManager.GetLocale("Fast_Battle_Reward_Desc",
                String.Format("{0:D2} : {1:D2} : {2:D2}",_hour,_min,_sec)));

            basicEnterCount = data.Single(t => t.Index == 3).Value;
            basicEnergy = data.Single(t => t.Index == 7).Value;
            
            SetCount();
        }

        void SetCount()
        {
            var data = TableDataManager.Instance.data.AutoBattleConfig;
            // var _count = data.Single(t => t.Index == 3).Value
            //              + UserDataManager.Instance.payInfo.IncraseQuickBattleRewardTime
            //              - UserDataManager.Instance.stageInfo.StageData.ffwd_count;

            var _count = UserDataManager.Instance.stageInfo.StageData.ffwd_count;
            if (_count < 0) _count = 0;
            var _adMaxCount = data.Single(t => t.Index == 4).Value;
            var _adCount = _adMaxCount - UserDataManager.Instance.stageInfo.StageData.ffwd_ad_count;
            var energyNum = _count >= basicEnterCount ? basicEnergy+(_count-basicEnterCount+1) : basicEnergy;
            
            countLabel.SetText(LocaleManager.GetLocale("Fast_Battle_Reward_Get_Count", _count));
            adCountLabel.SetText(string.Format(LocaleManager.GetLocale("Fast_Battle_Watch_Ad_Btn",_adCount,_adMaxCount)));
            energyLabel.SetText($"x {energyNum}");
            AdRewardBtn.interactable = _adCount > 0;
            RewardBtn.interactable = true; //_count > 0;
            
            SetData();
        }

        void Close()
        {
            Hide();
        }

        public void SetData()
        {
            if (TimerManager.Instance.GetAdTimer(TimerManager.AdTimerType.FAST_BATTLE) != null)
            {
                AdRewardBtn.interactable = false;
                
                adCountLabel.SetActive(false);
                adDisableRemainTimeTxt.SetActive(true);
                
                if (_adDisableDisposable == null)
                    _adDisableDisposable = this.UpdateAsObservable().Subscribe(_ => UpdateAdRemainTime()).AddTo(this);
            }
            else
            {
                var data = TableDataManager.Instance.data.AutoBattleConfig;
                var _adMaxCount = data.Single(t => t.Index == 4).Value;
                var _adCount = _adMaxCount - UserDataManager.Instance.stageInfo.StageData.ffwd_ad_count;
                AdRewardBtn.interactable = _adCount > 0;
                
                adCountLabel.SetActive(true);
                adDisableRemainTimeTxt.SetActive(false);
            }
        }

        private void UpdateAdRemainTime()
        {
            Timer adTimer = TimerManager.Instance.GetAdTimer(TimerManager.AdTimerType.FAST_BATTLE);
            if (adTimer == null) return;

            TimeSpan remainTime = TimeSpan.FromSeconds(adTimer.GetTimeRemaining());
            if (remainTime.TotalSeconds <= 0)
            {
                _adDisableDisposable?.Dispose();
                _adDisableDisposable = null;
                
                TimerManager.Instance.RemoveAdTimer(TimerManager.AdTimerType.FAST_BATTLE);
                return;
            }
            
            adDisableRemainTimeTxt.SetText($"{remainTime.Minutes:D2}"+":"+$"{remainTime.Seconds:D2}");
        }
        
        void OnClickAdReward()
        {
            AdMobManager.Instance.ShowAD(() =>
            {
                SetData();
                
                var payload = new Dictionary<string, object>
                {
                    { "type", 1 }, { "ad", 1 }
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
            }, TimerManager.AdTimerType.FAST_BATTLE);
        }

        void OnClickReward()
        {
            //6,7
            var data = TableDataManager.Instance.data.AutoBattleConfig;
            var _idx = data.Single(t => t.Index == 6).Value;
            // var _count = data.Single(t => t.Index == 7).Value;
            var _count = UserDataManager.Instance.stageInfo.StageData.ffwd_count;
            // 에너지 재화 체크
            var energyNum = _count >= basicEnterCount ? basicEnergy+(_count-basicEnterCount+1) : basicEnergy;
            bool enough = UserDataManager.Instance.currencyInfo.ValidGoods((CurrencyType)_idx, energyNum);
            if (!enough)
            {
                var alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale("Not_Enough_Energy_Msg"))
                    .Build();
                return;
            }
            
            var payload = new Dictionary<string, object>
            {
                { "type", 1}, { "ad", 0 }
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
            
            // Hide();
            SetCount();
            var rewardList = APIRepository.ConvertReward(data.default_rewards);
            rewardList.AddRange(APIRepository.ConvertReward(data.currency_box_rewards));
            rewardList.AddRange(APIRepository.ConvertReward(data.gear_box_rewards));

            var popup = UIManager.Instance.Get(PopupName.Common_Reward) as CommonRewardPopup;
            popup.AddShowCallback(UIManager.Instance.HideLoadingPopup);
            popup.SetData(rewardList);
            popup.Show();
                
            // 레벨업 보상
            int prevLv = UserDataManager.Instance.userInfo.PlayerData.player.level;
            UserDataManager.Instance.userInfo.SetPlayerData(data.player);
            if (prevLv < data.player.level)
            {
                Action ShowLevelUp = () =>
                {
                    var levelUpRewardList = APIRepository.ConvertReward(data.level_up_rewards);
                        
                    var levelUp = UIManager.Instance.Get(PopupName.Lobby_AccountLevelUp) as PopupAccountLevelUp;
                    levelUp.SetLevelData(prevLv,data.player.level);
                    levelUp.SetRewardData(levelUpRewardList);
                    levelUp.Show();
                        
                    UserDataManager.Instance.currencyInfo.SetItem(data.level_reward_currencies);
                    
                    if (LobbyMain.Instance)
                        LobbyMain.Instance.ReloadLobbyRedDot();
                };
                popup.HideCallback(ShowLevelUp);
            }
        }

        public void ReloadAutoBattleMaxCount()
        {
            var data = TableDataManager.Instance.data.AutoBattleConfig
                .Single(t => t.Index == 3);
            int count = data.Value + UserDataManager.Instance.payInfo.IncreaseQuickBattleRewardTime
                         - UserDataManager.Instance.stageInfo.StageData.ffwd_count;

            if (count < 0)
                count = 0;
            
            countLabel.SetText(LocaleManager.GetLocale("Fast_Battle_Reward_Get_Count", count));
        }
    }
}
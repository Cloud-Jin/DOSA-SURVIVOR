using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using TMPro;
using UniRx;
using UnityEngine;

namespace ProjectM
{
    public class PopupBattleAuto : Popup
    {
        public override PopupName ID { get; set; } = PopupName.AutoBattle_BattleAuto;
        public PopupBattleAutoScrollView popupBattleAutoScrollView;
        public UIButton AdRewardBtn, RewardBtn, closeRewardBtn;
        public TMP_Text accumulateTime, gold, exp, goods, gear;
        public TMP_Text adRewardRatioTxt;
        protected override void Init()
        {
            NetworkManager.Instance.onConnect.Subscribe(t =>
            {
                Debug.Log("우편으로 자동전투 보상받음");
                Hide();
            }).AddTo(this);
            
            uiPopup.OnShowCallback.Event.AddListener(()=>
            {
                popupBattleAutoScrollView.SetActive(true);
                popupBattleAutoScrollView.ReloadScrollView();
            });
            
            // uiPopup.OnHideCallback.Event.AddListener(() =>
            // {
            //     SetShowPendingPopup(true);
            // });
            
            AdRewardBtn.AddEvent(OnClickAdReward);
            RewardBtn.AddEvent(OnClickReward);
            closeRewardBtn.AddEvent(OnClickReward);
            
            var baseLocale = LocaleManager.GetLocale("Auto_Battle_Reward_Get_Count");
            var autoBattleConfig = TableDataManager.Instance.data.AutoBattleConfig;
            var stageInfo = UserDataManager.Instance.stageInfo.StageData;

            // 스테이지
            var clearStageData = TableDataManager.Instance.GetStageData(stageInfo.chapter_id_cap, stageInfo.level_cap, 1);
            Stage tbStage = null;
            if (clearStageData.NextStage > 0)
            {
                tbStage = TableDataManager.Instance.GetStageData(clearStageData.NextStage);
            }
            else
                tbStage = TableDataManager.Instance.GetStageData(clearStageData.Index);

            // 타임
            var maxMin = autoBattleConfig.Single(t => t.Index == 1).Value; // 1440
            var timeSpan = DateTime.UtcNow - UserDataManager.Instance.stageInfo.StageData.afk_reward_at;
            var maxTimeSpan = TimeSpan.FromMinutes(maxMin);
            var _time = (timeSpan.TotalMinutes > maxMin) ? maxTimeSpan.TotalSeconds : timeSpan.TotalSeconds;
            
            int _hour = (int)(_time / 3600);
            int _min = (int)(_time % 3600 / 60);
            int _sec = (int)(_time % 3600 % 60);
            
            string saveTime = String.Format("{0:D2} : {1:D2} : {2:D2}",_hour,_min,_sec);

            accumulateTime.SetText(LocaleManager.GetLocale("Auto_Battle_Accumulate_Time", saveTime, maxMin / 60));
            InfVal _gold = tbStage.RewardGold * (autoBattleConfig.Single(t => t.Index == 9).Value / (decimal)10000f);
            InfVal _exp = tbStage.RewardExp * (autoBattleConfig.Single(t => t.Index == 10).Value / (decimal)10000f);

            gold.SetText(String.Format("+" + baseLocale, _gold.ToGoldString(), autoBattleConfig.Single(t => t.Index == 15).Value));
            exp.SetText(String.Format("+" + baseLocale, _exp.ToGoldString(), autoBattleConfig.Single(t => t.Index == 16).Value));
            goods.SetText(String.Format(baseLocale, 1, autoBattleConfig.Single(t => t.Index == 11).Value));
            gear.SetText(String.Format(baseLocale, 1, autoBattleConfig.Single(t => t.Index == 12).Value));

            AutoBattleConfig adRewardRatio = TableDataManager.Instance.data.AutoBattleConfig
                .SingleOrDefault(t => t.Index == 8);
            if (adRewardRatio != null)
            {
                adRewardRatioTxt.SetText(LocaleManager.GetLocale("Auto_Battle_Reward_Ad_Get_Btn",
                    adRewardRatio.Value / 10000f));
            }
        }
        public void SetRewardData(List<ScrollDataModel> _data)
        {
            popupBattleAutoScrollView.SetData(_data);
        }
        
        void OnClickAdReward()
        {
            AdMobManager.Instance.ShowAD(() =>
            {
                var payload = new Dictionary<string, object>
                {
                    { "type", 1 }, { "ad", 1 }
                };

                APIRepository.RequestAutoBattleReward(payload, data => { OnResponseSucces(data); });
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
            });
        }

        void OnClickReward()
        {
            ShowPendingPopup = false;
            var payload = new Dictionary<string, object>
            {
                { "type", 1}, { "ad", 0 }
            };
            
            APIRepository.RequestAutoBattleReward(payload, data =>
            {
                OnResponseSucces(data);
            });
        }


        void OnResponseSucces(AutoBattleRewardData data)
        {
            ShowPendingPopup = false;
            Hide();
            
            var rewardList = APIRepository.ConvertReward(data.default_rewards);
            rewardList.AddRange(APIRepository.ConvertReward(data.currency_box_rewards));
            rewardList.AddRange(APIRepository.ConvertReward(data.gear_box_rewards));
                
            var popup = UIManager.Instance.Get(PopupName.Common_Reward) as CommonRewardPopup;
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
                
                popup.AddHideCallback(ShowLevelUp);
            }
        }
    }
}
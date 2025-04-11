using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Phoenix;

namespace ProjectM
{
    public partial class APIRepository
    {
        // 자동전투 보상
        public static void RequestAutoBattleRoll(Dictionary<string, object> payload, Action<AutoBattleRollData> onResponse)
        {
            // type 1
            Action<AutoBattleRollData> resp = data =>
            {
            };
            resp += onResponse;

            Net.PushUserChannel("stage_afk_reward_roll", payload, resp, true);
        }
        
        // 자동전투 보상수락
        public static void RequestAutoBattleReward(Dictionary<string, object> payload, Action<AutoBattleRewardData> onResponse)
        {
            // type 1 ad 0
            Action<AutoBattleRewardData> resp = data =>
            {
                switch (data.stage.type)
                {
                    case 1:
                        UserDataManager.Instance.stageInfo.StageData = data.stage;
                        NotificationsManager.Instance.SetNotificationType(1);
                        NotificationsManager.Instance.SetNotificationType(2);
                        break;
                    case 2:
                        UserDataManager.Instance.stageInfo.GoldDungeonData = data.stage;
                        break;
                }
                
                UserDataManager.Instance.currencyInfo.SetItem(data.currencies);
                if (data.trait_point_max > 0)
                    UserDataManager.Instance.userTraitInfo.apMax.Value = data.trait_point_max;
                data.gears?.ForEach(gear => 
                {
                    UserDataManager.Instance.gearInfo.AddGear(gear); 
                });
            };
            resp += onResponse;

            Net.PushUserChannel("stage_afk_reward", payload, resp, true);
        }
        
        public static void RequestFastAutoBattleReward(Dictionary<string, object> payload, Action<FastBattleRewardData> onResponse, Action<Reply> onError)
        {
            Action<FastBattleRewardData> resp = data =>
            {
                switch (data.stage.type)
                {
                    case 1:
                        UserDataManager.Instance.stageInfo.StageData = data.stage;
                        break;
                    case 2:
                        UserDataManager.Instance.stageInfo.GoldDungeonData = data.stage;
                        break;
                }
                UserDataManager.Instance.currencyInfo.SetItem(data.currencies);
                if (data.trait_point_max > 0)
                    UserDataManager.Instance.userTraitInfo.apMax.Value = data.trait_point_max;
                data.gears?.ForEach(gear => 
                {
                    UserDataManager.Instance.gearInfo.AddGear(gear); 
                });
            };
            resp += onResponse;

            Net.PushUserChannel("stage_ffwd", payload, resp, true, onError);
        }
        
        public static void RequestBattleExtra(Dictionary<string, object> payload, Action<AutoBattleRewardData> onResponse, Action<Reply> onError)
        {
            Action<AutoBattleRewardData> resp = data =>
            {
                switch (data.stage.type)
                {
                    case 1:
                        UserDataManager.Instance.stageInfo.StageData = data.stage;
                        break;
                    case 2:
                        UserDataManager.Instance.stageInfo.GoldDungeonData = data.stage;
                        break;
                }
                // UserDataManager.Instance.currencyInfo.SetItem(data.currencies);
                // if (data.trait_point_max > 0)
                //     UserDataManager.Instance.userTraitInfo.apMax.Value = data.trait_point_max;
                // data.gears?.ForEach(gear => 
                // {
                //     UserDataManager.Instance.gearInfo.AddGear(gear); 
                // });
            };
            resp += onResponse;

            Net.PushUserChannel("stage_extra", payload, resp, true, onError);
        }
    }
}
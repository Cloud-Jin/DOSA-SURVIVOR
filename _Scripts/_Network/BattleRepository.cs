using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Phoenix;
using UnityEngine;
using ProjectM.Battle;

namespace ProjectM
{
    public partial class APIRepository
    {
        public static void RequestStageStart(Dictionary<string, object> payload, Action<StageStartData> onResponse)
        {
            Action<StageStartData> resp = data =>
            { 
                // 에너지 차감
                if(data.currencies != null)
                    UserDataManager.Instance.currencyInfo.SetItem(data.currencies);
                
                // Debug.Log(resp);
            };
            resp += onResponse;

            Net.PushUserChannel("stage_start",payload, resp, true);
        }
        
        public static void RequestStageEnd(Dictionary<string, object> payload, Action<StageEndData> onResponse, Action<string> onFail)
        {
            Action<StageEndData> resp = data =>
            {
                // UserDataManager.Instance.stageInfo.Data = data.stage; // 결과창 사후체크.
                if (data.currencies != null)
                {
                    UserDataManager.Instance.currencyInfo.SetItem(data.currencies);
                }

                data.gears?.ForEach(gear => 
                {
                    UserDataManager.Instance.gearInfo.AddGear(gear); 
                });
                
                if (data.trait_point_max > 0)
                    UserDataManager.Instance.userTraitInfo.apMax.Value = data.trait_point_max;
            };
            resp += onResponse;

            Net.PushUserChannel("stage_end", payload, resp, onFail, false);
        }
        
        public static void RequestStageCrack(Dictionary<string, object> payload, Action<JObject> onResponse)
        {
            Action<JObject> resp = data =>
            {
                var bb = BlackBoard.Instance.data;
                bb.RewardDatas = data["breach_rewards"]?.ToObject<List<RewardData>>();
                if(bb.benefitEffect > 0)
                    bb.BenefitRewardDatas = data["benefit_breach_rewards"]?.ToObject<List<RewardData>>();
                
                Net.DisposeEvent("stage_crack");
            };
            resp += onResponse;

            Net.PushUserChannel("stage_crack", payload, resp, false);
        }
        
        public static void RequestStageCrackRoll(Dictionary<string, object> payload, Action<JObject> onResponse)
        {
            Action<JObject> resp = data =>
            {
                var bb = BlackBoard.Instance.data;
                bb.RewardDatas = data["breach_rewards"].ToObject<List<RewardData>>();
                if(bb.benefitEffect > 0)
                    bb.BenefitRewardDatas = data["benefit_breach_rewards"].ToObject<List<RewardData>>();
                
                Net.DisposeEvent("stage_crack_roll");
            };
            resp += onResponse;

            Net.PushUserChannel("stage_crack_roll", payload, resp, true);
        }

        public static void RequestStageRevive(Action<JObject> onResponse)
        {
            Action<JObject> resp = resp =>
            {
                
            };
            resp += onResponse;
            
            Net.PushUserChannel("stage_revive", resp, true);
        }
    }
}
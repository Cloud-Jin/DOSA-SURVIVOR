using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using Gbros.UniRx;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;

// 제약조건에따라 생성
// 1번
// 2번
// 3번 

namespace ProjectM.Battle
{
    public class ChallengeBattleManager : BattleManager
    {
        private Dictionary<int, PenaltyData> penalty = new();            // 적용중 패널티
        public List<PenaltyData> penaltyList = new();                   // 적용중 패널티 List
        
        protected override void Init()
        {
            base.Init();
            
            SetPenaltyData();
            BlackBoard.Instance.LobbyTap = LobbyTap.Dungeon;
        }

        void SetPenaltyData()
        {
            // 패널티 세팅
            List<ChallengeEffectGroup> challengeEffectGroupList = TableDataManager.Instance.data
                .ChallengeEffectGroup
                .Where(t => t.GroupID == tbStage.ChallengeEffectGroupID).ToList();
            
            foreach (var t1 in challengeEffectGroupList)
            {
                ChallengePenaltyType t2 = TableDataManager.Instance.data.ChallengePenaltyType
                    .Single(t => t.Type == t1.TypeID);

                PenaltyData data = new PenaltyData()
                {
                    EffectGroup = t1,
                    PenaltyType = t2
                };
                penalty.Add(t1.TypeID, data);
                penaltyList.Add(data);
            }
        }

        protected override void GameStart()
        {
            base.GameStart();
            var reviveValue = GetPenaltyValue(ChallengePenalty.Revive);
            if (reviveValue > 0)
            {
                BlackBoard.Instance.data.reviveCount = 0;
                reviveCount = 0;
            }
            
            var timeOutValue = GetPenaltyValue(ChallengePenalty.TimeOut);
            if (timeOutValue > 0)
            {
                
            }
            
            var viewRangeValue = GetPenaltyValue(ChallengePenalty.ViewRange);
            if (viewRangeValue > 0)
            {
                var _value = MyMath.Increase(1, viewRangeValue / 100f);
                gameObjectRef.screenViewRange.SetActive(true);
                gameObjectRef.screenViewRange.transform.localScale = Vector3.one * _value;
            }
            PlayerManager.Instance.player.SetActive(true);
            SetCameraZoom(zoomIndex);
            TimerStart(); 
            
            var popup = UIManager.Instance.Get(PopupName.HardModeInfo);
            popup.Show();
            popup.HideCallback(() =>
            {
                ((ChallengeSpawner)spawner).InitBoss();
            });
            // 닫히면

            if(level.Value == 1)
                SpawnEXP(); // 초기 경험치 조각 드랍
            
            Debug.Log("Challenge Battle Manager");
        }

        public int GetPenaltyValue(ChallengePenalty typeKey)
        {
            if(penalty.TryGetValue((int)typeKey, out var value))
            {
                return value.EffectGroup.Value;
                //return value;
            }
            else
            {
                return 0;
            }
        }
        
        public override void GetExp(int addExp)
        {
            var calcExp = MyMath.Increase(addExp, SkillSystem.Instance.incValue.Exp);
            calcExp = MyMath.Decrease(calcExp, GetPenaltyValue(ChallengePenalty.Exp) / 100f);
            // 패널티 감소
            
            exp += calcExp;
            ExpAdd.OnNext(calcExp);
            
            SoundManager.Instance.PlayFX("Get_Exp");
        }
        
        protected override void Victory()
        {
            UIManager.Instance.PendingPopupClear();
            BattleState.Value = Battle.BattleState.Victory;
            
            Debug.Log("생존완료!");
            var tbStage = TableDataManager.Instance.data.Stage.Single(t => t.Index == BlackBoard.Instance.data.mapIndex);
            
            JObject data = new JObject();
            data.Add("type", 3);
            data.Add("t", gameTime.Value);
            data.Add("ge", GoldToadKill.Value);
            // 완료시만
            data.Add("c", 1);
            data.Add("k", kill.Value);
            data.Add("skill", JToken.FromObject(SkillSystem.Instance.ConvertData()));
            
            var payload = new Dictionary<string, object> { { "type", 3 }, { "chapter_id", tbStage.ChapterID }, { "level", tbStage.StageLevel },
                { "data", data } };
            
            BlackBoard.Instance.ResetData();
            APIRepository.RequestStageEnd(payload, data =>
            {
                default_rewards.Clear();
                add_rewards.Clear();

                // 기본 보상
                var rewardList = APIRepository.ConvertReward(data.default_rewards);
                rewardList.AddRange(APIRepository.ConvertReward(data.first_clear_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.clear_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.benefit_default_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.benefit_clear_rewards));
                default_rewards = rewardList;
                
                // 추가보상
                var addRewardList = APIRepository.ConvertReward(data.vip_rewards);
                addRewardList.AddRange(APIRepository.ConvertReward(data.gold_elite_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.breach_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.benefit_breach_rewards));
                add_rewards = addRewardList;

                var popup = UIManager.Instance.Get(PopupName.Battle_ResultVictory) as PopupResultVictory;
                popup.SetRewardData(default_rewards);
                popup.SetAddRewardData(add_rewards);
                popup.Show();
                popup.SetTitle(0);
                popup.SetHardMode();
                
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
                    
                    UIManager.Instance.ReservePopup(ShowLevelUp);
                }
                
                // 연출
                UserDataManager.Instance.stageInfo.HardDungeonData = data.stage; // 결과창 사후체크.
                
                // QuickUI
                UIManager.Instance.ReservePopup(() =>
                {
                    var popup = UIManager.Instance.Get(PopupName.HardDungeonSelect) as PopupHardDungeonSelect;
                    popup.Show();
                });
                
                //
            }, (key) =>
            {
                var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage(key.Locale())
                    .SetYesButton(()=>
                    {
                        SceneLoadManager.Instance.LoadScene("Lobby");
                    }, "UI_Key_011".Locale())
                    .Build();
                Debug.Log(key);
            });
        }
        
        public override void Lose()
        {
            UIManager.Instance.PendingPopupClear();
            BattleState.Value = Battle.BattleState.Lose;
            var tbStage = TableDataManager.Instance.data.Stage.Single(t => t.Index == BlackBoard.Instance.data.mapIndex);
            
            JObject data = new JObject();
            data.Add("type", 3);
            data.Add("t", gameTime.Value);
            data.Add("ge", GoldToadKill.Value);
            
            var payload = new Dictionary<string, object> { { "type", 3 }, { "chapter_id", tbStage.ChapterID }, { "level", tbStage.StageLevel },
                { "data", data } };

            BlackBoard.Instance.ResetData();
            APIRepository.RequestStageEnd(payload, data =>
            {
                default_rewards.Clear();
                add_rewards.Clear();
                
                // 기본 보상
                var rewardList = APIRepository.ConvertReward(data.default_rewards);
                rewardList.AddRange(APIRepository.ConvertReward(data.clear_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.benefit_default_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.benefit_clear_rewards));
                default_rewards = rewardList;
                
                // 추가보상
                var addRewardList = APIRepository.ConvertReward(data.vip_rewards); 
                addRewardList.AddRange(APIRepository.ConvertReward(data.gold_elite_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.breach_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.benefit_breach_rewards));
                add_rewards = addRewardList;

                var popup = UIManager.Instance.Get(PopupName.Battle_ResultVictory) as PopupResultVictory;
                popup.SetRewardData(default_rewards);
                popup.SetAddRewardData(add_rewards);
                popup.Show();
                popup.SetTitle(1);
                popup.SetHardMode();

                // 연출
                
                // QuickUI
                UIManager.Instance.ReservePopup(() =>
                {
                    var popup = UIManager.Instance.Get(PopupName.HardDungeonSelect) as PopupHardDungeonSelect;
                    popup.Show();
                });
            }, (key) =>
            {
                var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage(key.Locale())
                    .SetYesButton(()=>
                    {
                        SceneLoadManager.Instance.LoadScene("Lobby");
                    }, "UI_Key_011".Locale())
                    .Build();
                Debug.Log(key);
            });
            
         
        }
    }
}
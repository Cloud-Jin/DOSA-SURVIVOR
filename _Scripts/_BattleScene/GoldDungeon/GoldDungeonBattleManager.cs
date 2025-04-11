using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using Gbros.UniRx;
using Newtonsoft.Json.Linq;
using ProjectM.Battle.Dungeon;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
// 60킬 승리.
// 타임 아웃.
namespace ProjectM.Battle
{
    public class GoldDungeonBattleManager : BattleManager
    {
        public ReactiveProperty<bool> OnAutoSkillUp;
        protected override void Init()
        {
            base.Init();
            OnAutoSkillUp = new ReactiveProperty<bool>(PlayerPrefs.GetInt(MyPlayerPrefsKey.DungeonGoldAutoSkill, 0) == 1);
            // 다 잡으면 승리.
            var killCount = TableDataManager.Instance.GetDungeonConfig(1).Value;
           
            
            kill.Where(k => k == killCount).Subscribe(t =>
            {
                if (gameTime.Value <= 0) return;
                
                Debug.Log("승리");
                battleTimerPause.SetValueAndForceNotify(true);
                Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => Victory()).AddTo(this);
            }).AddTo(this);
            
           
            
            BlackBoard.Instance.LobbyTap = LobbyTap.Dungeon;
        }

        protected override void GameStart()
        {
            base.GameStart();
            PlayerManager.Instance.player.SetActive(true);
            // BattleState.Value = Battle.BattleState.Run;
            
            // SetCameraZoom(zoomIndex);
            // TimerStart();
            var zoomValue = TableDataManager.Instance.GetDungeonConfig(11).Value / 100f;
            SetCameraZoom(zoomValue);
            
            SkillSystem.Instance.InitReady.Where(t=> t).Subscribe(_ =>
            {
                FirstLevelUp();
                AutoSkillUp();
            });
            
            Debug.Log("Gold Battle Manager");
        }
        
        private void FirstLevelUp()
        {
            var levelUpCount = TableDataManager.Instance.GetDungeonConfig(2).Value;
            // levelUpCount = 1;
            // Set Event
            IDisposable levelUp = null;
            int _lv = 0;
            levelUp = OnLevelUp.Subscribe(t =>
            {
                _lv++;
                // Config 래벨업까지
                if (_lv >= levelUpCount)
                {
                    var popup = UIManager.Instance.GetPopup(PopupName.Battle_LevelUp);
                    popup.HideCallback(() =>
                    {
                        var view = UIManager.Instance.GetView(ViewName.Battle_Top) as UIBattleTop;
                        view.SetGoldBattleUI(TimerStart);
                        
                        var popupPause = UIManager.Instance.Get(PopupName.Battle_Pause);
                        popupPause.Show();
                    });
                    levelUp.Dispose();
                    
                    var autoSkill = UIManager.Instance.GetPopup(PopupName.Battle_SkillSellect);
                    autoSkill.Hide();
                }
            }).AddTo(this);
            
            
            if (level.Value == 1)
            {
                for (int i = 0; i < levelUpCount; i++)
                {
                    LevelUp();
                }
            }
        }

        private void AutoSkillUp()
        {
            var popup = UIManager.Instance.Get(PopupName.Battle_SkillSellect);
            popup.Show();
        }

        protected override void Victory()
        {
            UIManager.Instance.PendingPopupClear();
            BattleState.Value = Battle.BattleState.Victory;
            
            Debug.Log("생존완료!");

            JObject data = new JObject();
            data.Add("type", 2);
            data.Add("t", gameTime.Value);
            
            // 완료시만
            data.Add("c", 1);
            data.Add("k", kill.Value);
            data.Add("skill", JToken.FromObject(SkillSystem.Instance.ConvertData()));
            // _level 던전 레벨
            var _level = BlackBoard.Instance.data.dungeonLevel;
            var payload = new Dictionary<string, object> { { "type", 2 }, { "level", _level }, { "data", data } };

            BlackBoard.Instance.ResetData();
            APIRepository.RequestStageEnd(payload, data =>
            {
                {
                    default_rewards.Clear();
                    add_rewards.Clear();

                    // 기본 보상
                    var rewardList = APIRepository.ConvertReward(data.clear_rewards);
                    rewardList.AddRange(APIRepository.ConvertReward(data.first_clear_rewards));
                    // rewardList.AddRange(APIRepository.ConvertReward(data.benefit_default_rewards));
                    // rewardList.AddRange(APIRepository.ConvertReward(data.benefit_clear_rewards));
                    default_rewards = rewardList;

                    var popup = UIManager.Instance.Get(PopupName.Battle_ResultDungeon) as PopupResultDungeon;
                    popup.SetRewardData(default_rewards);
                    popup.Show();
                    popup.SetTitle(0);

                    // 연출
                    int i = UserDataManager.Instance.stageInfo.StageGoldDungeon(data.stage);
                    UserDataManager.Instance.stageInfo.GoldDungeonData = data.stage; // 결과창 사후체크.

                    var maxLevel = TableDataManager.Instance.GetDungeonConfig(9).Value;
                    if (i > 0 && i < maxLevel) // 최초 클리어.
                    {
                        UserDataManager.Instance.stageInfo.PlayDungeonGold = i;
                    }
                }
                // 던전
                UIManager.Instance.ReservePopup(() =>
                {
                    var popup = UIManager.Instance.Get(PopupName.Dungeon_Select) as PopupDungeonSelect;
                    popup.SetDungen(1);
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
                
                UIManager.Instance.ReservePopup(() =>
                {
                    var popup = UIManager.Instance.Get(PopupName.Dungeon_Select) as PopupDungeonSelect;
                    popup.SetDungen(1);
                    popup.Show();    
                });
                Debug.Log(key);
            });
        }

        public override void Lose()
        {
            UIManager.Instance.PendingPopupClear();
            BattleState.Value = Battle.BattleState.Lose;
            
            JObject data = new JObject();
            data.Add("type", 2);
            data.Add("t", gameTime.Value);
            
            // // 완료시만
            // data.Add("c", 1);
            // data.Add("k", kill.Value);
            
            // _level 던전 레벨
            var _level = BlackBoard.Instance.data.dungeonLevel;
            var payload = new Dictionary<string, object> { { "type", 2 }, { "level", _level }, { "data", data } };

            BlackBoard.Instance.ResetData();
            APIRepository.RequestStageEnd(payload, data =>
            {
                default_rewards.Clear();
                add_rewards.Clear();
                
                /*// 기본 보상
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
                add_rewards = addRewardList;*/

                
                var popup = UIManager.Instance.Get(PopupName.Battle_ResultDungeon) as PopupResultDungeon;
                popup.Show();
                popup.SetTitle(1);

                // 연출
                UIManager.Instance.ReservePopup(() =>
                {
                    var popup = UIManager.Instance.Get(PopupName.Dungeon_Select) as PopupDungeonSelect;
                    popup.SetDungen(1);
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
                
                UIManager.Instance.ReservePopup(() =>
                {
                    var popup = UIManager.Instance.Get(PopupName.Dungeon_Select) as PopupDungeonSelect;
                    popup.SetDungen(1);
                    popup.Show();    
                });
                Debug.Log(key);
            });
        }
    }
}
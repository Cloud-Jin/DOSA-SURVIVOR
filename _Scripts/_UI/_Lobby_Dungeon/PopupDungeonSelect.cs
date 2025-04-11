using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using ProjectM.Battle;
using Sirenix.OdinInspector;
using UniRx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 던전 선택팝업
namespace ProjectM
{
    public class PopupDungeonSelect : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Dungeon_Select;

        public DungeonSelectScrollView scrollView;
        // 스크롤러
        public UIButton dungeonStart,dungeonStartBooster, dungeonResetAd, sweepBtn, closeBtn, questionBtn,questionBoosterBtn;
        public TMP_Text title, dungeonDescription, battleCount, addBattleCount, enterEnergy,enterEnergyBoost;
        public UIToggle boosterToggle;
        public Image selectBG;
        public GameObject dungeonBtn;
        public Transform redDot;
        public UIToggle autoSkillUp;
        public LockContent LockContents;
        private Stage tbStage;
        private Dungeon tbDungeon;
        private int enterCount, addEnterCount;
        private int dungeonIndex;
        private bool isEnter;
        
        protected override void Init()
        {
            uiPopup.OnShowCallback.Event.AddListener(() =>
            {
                scrollView.ReloadScrollView();
                scrollView.enhancedScroller.JumpToDataIndex(UserDataManager.Instance.stageInfo.PlayDungeonGold, 0.31f);
            });

            questionBtn.AddEvent(OnDesc);
            questionBoosterBtn.AddEvent(OnDescBooster);
            dungeonStart.AddEvent(()=>StartBattle(0, 0));
            dungeonStartBooster.AddEvent(()=>StartBattle(0, 1));
            dungeonResetAd.AddEvent(()=> AddStageExtra());
            sweepBtn.AddEvent(SweepBattle);
            closeBtn.AddEvent(Hide);
            LockContents.SetLockEventUIButton(9);
            
            autoSkillUp.isOn = (PlayerPrefs.GetInt(MyPlayerPrefsKey.DungeonGoldAutoSkill, 0) == 1);
            autoSkillUp.ObserveEveryValueChanged(x => x.isOn).Subscribe(t =>
            {
                PlayerPrefs.SetInt(MyPlayerPrefsKey.DungeonGoldAutoSkill, t ? 1 : 0);
                PlayerPrefs.Save();
            }).AddTo(this);
            
            var boosterValue = PlayerPrefs.GetInt(MyPlayerPrefsKey.GoldBattleBooster, 0) == 1;
            boosterToggle.isOn = boosterValue;
            boosterToggle.ObserveEveryValueChanged(x => x.isOn).Subscribe(t =>
            {
                UISelectionState state = isEnter ? UISelectionState.Normal : UISelectionState.Disabled;
                dungeonStart.SetActive(!t);
                dungeonStart.SetState(state);
                dungeonStartBooster.SetActive(t);
                dungeonStartBooster.SetState(state);
                
                PlayerPrefs.SetInt(MyPlayerPrefsKey.GoldBattleBooster, t ? 1 : 0);
                PlayerPrefs.Save();
            }).AddTo(this);
        }
        
        public void AddStageExtra()
        {
            AdMobManager.Instance.ShowAD(() =>
                {
                    var payload = new Dictionary<string, object>
                    {
                        { "type", 2 }
                    };
            
            
                    APIRepository.RequestBattleExtra(payload, data =>
                    {
                        SetData();
                        UILobby.Instance.Contents[4].GetComponent<ContainerDungeon>().RedDotCheck();
                    }, reply => { });
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

        public void SetDungen(int index)
        {
            dungeonIndex = index;
            SetData();
            RedDotCheck();
            
            // List in List
            List<List<ScrollDataModel>> data = new ();// APIRepository.ConvertReward(rewardDataList));
            var maxLevel = TableDataManager.Instance.GetDungeonConfig(9).Value; 
            for (int i = 0; i < maxLevel; i++)
            {
                var d = TableDataManager.Instance.data.DungeonLevel.Single(t => t.Level == i + 1);
                // table
                List<RewardData> _rewardDatas = new List<RewardData>()
                {
                    new RewardData()
                    {
                        t = 1,
                        i = d.RewardType,
                        c = d.Reward.ToString()        
                    },
                    new RewardData()
                    {
                        t = 1,
                        i = d.FirstClearRewardType,
                        c = d.FirstClearReward.ToString()
                    }
                };
                List<ScrollDataModel> convertReward = APIRepository.ConvertReward(_rewardDatas);
                data.Add(convertReward);
            }
            scrollView.SetData(data);
        }

        public void SetData()
        {
            tbStage = TableDataManager.Instance.data.Stage.Single(t => t.DungeonIndex == dungeonIndex);
            tbDungeon = TableDataManager.Instance.data.Dungeon.Single(t => t.Index == dungeonIndex);
            title.SetText(tbStage.Name.Locale());
            enterEnergy.SetText($"x {tbStage.ConsumeCount}");
            enterEnergyBoost.SetText($"x {tbStage.ConsumeCount}");
            dungeonDescription.SetText(tbDungeon.DungeonDescription.Locale());
            selectBG.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.AutoBattle).GetSprite(tbDungeon.Image);
            // battleCount
            var goldDungeonData = UserDataManager.Instance.stageInfo.GoldDungeonData;
            enterCount = (tbDungeon.EnterCount * (goldDungeonData.enter_extra_count + 1 )) - (goldDungeonData.enter_count + goldDungeonData.ffwd_count);
            battleCount.SetText("Dungeon_Count".Locale(enterCount));

            addEnterCount = UserDataManager.Instance.stageInfo.GoldDungeonData.enter_extra_count;
            addBattleCount.SetText($"{addEnterCount}/{tbDungeon.AddEnterCount}");
            
            // dungeonStart.SetActive(enterCount > 0);
            isEnter = enterCount > 0;
            UISelectionState state = isEnter ? UISelectionState.Normal : UISelectionState.Disabled;
            dungeonStart.SetActive(true);
            dungeonStart.interactable = isEnter;
            dungeonStart.SetState(state);
            dungeonStartBooster.interactable = isEnter;
            dungeonStartBooster.SetState(state);
            dungeonResetAd.SetActive(enterCount <= 0 && addEnterCount < tbDungeon.AddEnterCount);
            dungeonResetAd.interactable = addEnterCount < tbDungeon.AddEnterCount;
            
            dungeonBtn.SetActive(enterCount > 0 || addEnterCount == tbDungeon.AddEnterCount);
        }

        void StartBattle(int extra, int booster)
        {
            var stageIdx = UserDataManager.Instance.stageInfo.PlayDungeonGold;
            stageIdx = 10001;
            var tbStage = TableDataManager.Instance.data.Stage.Single(t => t.Index == stageIdx);
            var level = UserDataManager.Instance.stageInfo.PlayDungeonGold + 1;
            // extra
            var payload = new Dictionary<string, object> { { "type", 2 }, { "level", level }, {"extra", extra}, {"boost", booster}};

            if (extra == 0)
            {
                // 에너지 재화 체크
                bool enough = UserDataManager.Instance.currencyInfo.ValidGoods((CurrencyType)tbStage.ConsumeType, tbStage.ConsumeCount);
                if (!enough)
                {
                    var alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    alarm.InitBuilder()
                        .SetMessage(LocaleManager.GetLocale("Not_Enough_Energy_Msg"))
                        .Build();
                    return;
                }
            }
            else if (extra == 1)
            {
                // 추가입장제한 체크
                bool enough = UserDataManager.Instance.stageInfo.GoldDungeonData.enter_extra_count < tbDungeon.AddEnterCount;
                if (!enough)
                {
                    // var alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    // alarm.InitBuilder()
                    //     .SetMessage(LocaleManager.GetLocale("Not_Enough_Energy_Msg"))
                    //     .Build();
                    return;
                }
            }

            Action stageAPI = () =>
            {
                APIRepository.RequestStageStart(payload, data =>
                {
                    UserDataManager.Instance.stageInfo.GoldDungeonData = data.stage;
                    // Debug.Log(data);
                    var bb = BlackBoard.Instance;
                    bb.ClearData();
               
                    // 황금던전 부스터 시간 제한 stageConfig 60번
                    bb.data.gameTime = (booster == 0) ? tbStage.StageTime : TableDataManager.Instance.GetStageConfig(60).Value;
                    bb.data.mapIndex = tbStage.Index;
                    bb.data.attack = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Attack}";
                    bb.data.hp = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Hp}";
                    bb.data.maxHp = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Hp}";
                    bb.data.weaponInfo = UserDataManager.Instance.gearInfo.GetEquipWeaponInfo();
                    bb.data.costumeInfo = UserDataManager.Instance.gearInfo.GetEquipCostumeInfo();
                    bb.data.dungeonLevel = level;
                
                    bb.data.reviveCount = 0;
                    bb.data.RewardDatas = data.breach_rewards;
                    bb.data.BenefitRewardDatas = data.benefit_breach_rewards;
                    bb.data.benefit_ids = data.benefit_ids;
                    bb.data.benefitEffect = UserDataManager.Instance.payInfo.IncreaseStageClearBaseReward;
                    bb.data.booster = booster == 1;
                    var tbSet = bb.GetSkillSet();
                    bb.SetSkillData("player",tbSet.ChangeSkillID,1);
                
                    bb.Save();
                    // data.stage.etc
                    // send
                    // AppsFlyerManager.Instance.SendLevel(stageIdx);
                    SceneLoadManager.Instance.LoadScene("BattleBase");
                });
            };
            
            if(booster == 0)
                stageAPI.Invoke();
            else
            {
                AdMobManager.Instance.ShowAD(() =>
                    {
                        stageAPI.Invoke();
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
            
        }

        void SweepBattle()
        {
            if (UserDataManager.Instance.stageInfo.GoldDungeonData.level_cap == 0)
            {
                var alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage("Sweep_Alarm_Msg".Locale())
                    .SetDuration(3f)
                    .Build();
                return;
            }
            // Debug.Log("소탕팝업");
            var popup = UIManager.Instance.Get(PopupName.DungeonClear) as PopupDungeonClear;
            popup.SetDungen(dungeonIndex);
            popup.Show();
        }

        public void RedDotCheck()
        {
            // var sweepCount = tbDungeon.SweepCount - UserDataManager.Instance.stageInfo.GoldDungeonData.ffwd_ad_count;
            // redDot.SetActive(sweepCount > 0);
        }

        void OnDesc()
        {
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale("Help"))
                    .SetMessage("Skill_Select_Auto_Desc".Locale())
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("Common_Ok_Btn"))
                    .Build();
            }
        }
        
        void OnDescBooster()
        {
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale("Help"))
                    .SetMessage("Booster_Kill_GoldDungeon_Desc".Locale())
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("Common_Ok_Btn"))
                    .Build();
            }
        }

        [Button]
        public void JumpToIndex(int i)
        {
            scrollView.enhancedScroller.JumpToDataIndex(i, 0.31f);
            UserDataManager.Instance.stageInfo.PlayDungeonGold = i;
        }
    }
}
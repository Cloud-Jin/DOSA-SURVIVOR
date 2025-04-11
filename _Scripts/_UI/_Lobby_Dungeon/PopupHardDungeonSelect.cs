using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using ProjectM.Battle;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class PopupHardDungeonSelect : Popup
    {
        public override PopupName ID { get; set; } = PopupName.HardDungeonSelect;

        public TMP_Text stageTxt;
        public TMP_Text stageCostTxt;
        [Space(20)]
        public HardDungeonSelectScrollView scrollView;
        [Space(20)]
        public List<TMP_Text> penaltyList;
        public List<Image> penaltyIconList;
        [Space(20)]
        public UIButton backBtn;
        public UIButton startBtn;
        public UIButton startBoosterBtn;
        public UIToggle boosterToggle;
        public UIButton helpBtn;
        public UIButton helpBoosterBtn;
        public LockContent LockContents;
        
        private Stage _stage;

        private bool _sameEquipWeaponPenalty;
        private bool _isEquipWeaponPenalty;
        private string _weaponPenaltyToastTxt;
        
        protected override void Init()
        {
        }

        private void Start()
        {   
            AddShowCallback(SetData);
            LockContents.SetLockEventUIButton(9);
            backBtn.AddEvent(OnHide);
            startBtn.AddEvent(() => OnBattleStart());
            startBoosterBtn.AddEvent(() => OnBattleStart(1));
            helpBtn.AddEvent(OnHelp);
            helpBoosterBtn.AddEvent(OnHelpBooster);
            var boosterValue = PlayerPrefs.GetInt(MyPlayerPrefsKey.ChallengeBattleBooster, 0) == 1;
            boosterToggle.isOn = boosterValue;
            boosterToggle.ObserveEveryValueChanged(x => x.isOn).Subscribe(t =>
            {
                startBtn.SetActive(!t);
                startBoosterBtn.SetActive(t);
                PlayerPrefs.SetInt(MyPlayerPrefsKey.ChallengeBattleBooster, t ? 1 : 0);
                PlayerPrefs.Save();
            }).AddTo(this);
        }

        public void SetData()
        {
            _isEquipWeaponPenalty = false;
            _sameEquipWeaponPenalty = false;

            List<Stage> stageList = TableDataManager.Instance.data.Stage.Where(t => t.StageType == 3).ToList();
            StageData stageData = UserDataManager.Instance.stageInfo.Data.SingleOrDefault(u => u.type == 3);

            if (stageData == null)
            {
                _stage = stageList.Single(s => s.ChapterID == 1 && s.StageLevel == 1);
            }
            else
            {
                _stage = stageList.SingleOrDefault(s => s.ChapterID == stageData.chapter_id_cap
                                                        && s.StageLevel == stageData.level_cap);
                
                if (_stage == null)
                {
                    _stage = stageList.Single(s => s.ChapterID == 1 && s.StageLevel == 1);
                }
                else
                {
                    if (0 < _stage.NextStage)
                        _stage = stageList.Single(s => s.Index == _stage.NextStage);
                    else
                        startBtn.SetActive(false);
                }
            }
            
            stageTxt.SetText(LocaleManager.GetLocale(_stage.Name));
            stageCostTxt.SetText($"x {_stage.ConsumeCount}");
            
            List<ChallengeEffectGroup> challengeEffectGroupList = TableDataManager.Instance.data
                .ChallengeEffectGroup
                .Where(t => t.GroupID == _stage.ChallengeEffectGroupID).ToList();

            int penaltyIndex = 0;

            penaltyList.ForEach(p => p.SetActive(false));
            penaltyIconList.ForEach(p => p.SetActive(false));
            
            for (int i = 0; i < challengeEffectGroupList.Count; ++i)
            {
                ChallengePenaltyType challengePenaltyType = TableDataManager.Instance.data.ChallengePenaltyType
                    .Single(t => t.Type == challengeEffectGroupList[i].TypeID);

                SetPenalty(challengeEffectGroupList[i], challengePenaltyType, penaltyIndex++);
            }

            int index = stageList.IndexOf(_stage);
            
            scrollView.SetActive(true);
            scrollView.SetData();
            scrollView.enhancedScroller.JumpToDataIndex(index, 0.35f);
        }

        private void SetPenalty(ChallengeEffectGroup challengeEffectGroup, ChallengePenaltyType challengePenaltyType, int index)
        {
            switch (challengePenaltyType.Type)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 9:
                case 17:
                {
                    string penaltyTxt = LocaleManager.GetLocale(
                        challengePenaltyType.Description, challengeEffectGroup.Value / 10000f * 100f);
                    penaltyList[index].SetText(penaltyTxt);
                    penaltyList[index].SetActive(true);
                    penaltyIconList[index].SetActive(true);
                    break;
                }
                case 6:
                case 7:
                case 8:
                case 10: 
                case 12:
                case 13:
                case 16:
                {
                    string penaltyTxt = LocaleManager.GetLocale(challengePenaltyType.Description);
                    penaltyList[index].SetText(penaltyTxt);
                    penaltyList[index].SetActive(true);
                    penaltyIconList[index].SetActive(true);
                    break;
                }
                case 11:
                {
                    Monster monster = TableDataManager.Instance.data.Monster
                        .Single(t => t.Index == challengeEffectGroup.Value);
                    string monsterName = LocaleManager.GetLocale(monster.Name);
                    string penaltyTxt = LocaleManager.GetLocale(challengePenaltyType.Description, monsterName);
                    penaltyList[index].SetText(penaltyTxt);
                    penaltyList[index].SetActive(true);
                    penaltyIconList[index].SetActive(true);
                    break;
                }
                case 14:
                {
                    _isEquipWeaponPenalty = true;
                    
                    EquipType equipType = TableDataManager.Instance.data.EquipType
                        .Single(t => t.Index == challengeEffectGroup.Value);

                    if (UserDataManager.Instance.EquipGears.TryGetValue(1, out var equipGear))
                        _sameEquipWeaponPenalty = equipGear.GetEquipment().EquipType == equipType.Index;

                    string equipTypeName = LocaleManager.GetLocale(equipType.Name);
                    _weaponPenaltyToastTxt = LocaleManager.GetLocale("Penalty_Type_Weapon_Type", equipTypeName);
                    penaltyList[index].SetText(LocaleManager.GetLocale(challengePenaltyType.Description, equipTypeName));
                    penaltyList[index].SetActive(true);
                    penaltyIconList[index].SetActive(true);
                    break;
                }
                case 15:
                {
                    string penaltyTxt = LocaleManager.GetLocale(challengePenaltyType.Description, challengeEffectGroup.Value);
                    penaltyList[index].SetText(penaltyTxt);
                    penaltyList[index].SetActive(true);
                    penaltyIconList[index].SetActive(true);
                    break;
                }
            }   
        }

        private void OnHide()
        {
            Hide();
        }

        private void OnBattleStart(int booster = 0)
        {
            // 에너지 재화 체크
            bool enough = UserDataManager.Instance.currencyInfo.ValidGoods((CurrencyType)_stage.ConsumeType, _stage.ConsumeCount);
            if (!enough)
            {
                var alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale("Not_Enough_Energy_Msg"))
                    .Build();
                return;
            }

            if (_isEquipWeaponPenalty)
            {
                if (_sameEquipWeaponPenalty == false)
                {
                    Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    alarm.InitBuilder().SetMessage(_weaponPenaltyToastTxt).Build();
                    return;
                }
            }

            var payload = new Dictionary<string, object>
            {
                { "type", _stage.StageType }, { "chapter_id", _stage.ChapterID }, { "level", _stage.StageLevel }, { "boost", booster }
            };

            Action stageAPI = () =>
            {
                APIRepository.RequestStageStart(payload, data =>
                {
                    var bb = BlackBoard.Instance;
                    bb.ClearData();
              
                    bb.data.gameTime = _stage.StageTime;
                    bb.data.mapIndex = _stage.Index;
                    bb.data.attack = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Attack}";
                    bb.data.hp = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Hp}";
                    bb.data.maxHp = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Hp}";
                    bb.data.weaponInfo = UserDataManager.Instance.gearInfo.GetEquipWeaponInfo();
                    bb.data.costumeInfo = UserDataManager.Instance.gearInfo.GetEquipCostumeInfo();
                    // bb.data.dungeonLevel = _stage.StageLevel;
                
                    bb.data.RewardDatas = data.breach_rewards;
                    bb.data.BenefitRewardDatas = data.benefit_breach_rewards;
                    bb.data.benefit_ids = data.benefit_ids;
                    bb.data.benefitEffect = UserDataManager.Instance.payInfo.IncreaseStageClearBaseReward;
                    bb.data.booster = booster == 1;
                    
                    var tbSet = bb.GetSkillSet();
                    bb.SetSkillData("player", tbSet.ChangeSkillID, 1);
                    bb.Save();
                
                    AppsFlyerManager.Instance.SendLevel(_stage.Index);
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

        private void OnHelp()
        {
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale("Help"))
                    .SetMessage(LocaleManager.GetLocale("ChallengeMode_Desc"))
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("Common_Ok_Btn"))
                    .Build();
            }
        }
        
        private void OnHelpBooster()
        {
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale("Help"))
                    .SetMessage("Booster_Kill_Desc".Locale())
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("Common_Ok_Btn"))
                    .Build();
            }
        }
    }
}
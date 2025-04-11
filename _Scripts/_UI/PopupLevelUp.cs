using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using ProjectM.Battle;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;

namespace ProjectM
{
    public class PopupLevelUp : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_LevelUp;
        public SlotLevelUp[] slots;
        public LevelUpCombination[] slotCombinations;
        public SkillViewer[] activeSlots;
        public SkillViewer[] passiveSlots;
        public UIButton reRollBtn;
        public TMP_Text remainLevel, reRollCount;
        public Transform adReRoll;
        private CompositeDisposable disposables = new CompositeDisposable();
        private int targetIdx;
        protected override void Init()
        {
            slots[0].AddComponent<TouchTypeObject>().TouchType = 19;
            var slotButtons = slots.Select(t => t.levelUpBtn).ToList();
            uiPopup.Buttons.AddRange(slotButtons);
            //SkillSystem.Instance 스킬 목록, 확률 Get
            // Debug.Log("레벨업 팝업");
            SoundManager.Instance.PlayFX("LevelUp");
            ParticleImageUnscaled();
            activeSlots.ForEach(t => t.Hide());
            passiveSlots.ForEach(t => t.Hide());
            
            foreach (var item in SkillSystem.Instance.ActiveSlots.Select((value, index) => (value,index)))
            {
                activeSlots[item.index].SetData(item.value);
            }
            
            foreach (var item in SkillSystem.Instance.PassiveSlots.Select((value, index) => (value,index)))
            {
                passiveSlots[item.index].SetData(item.value);
            }

            RollSkill();
            reRollBtn.AddEvent(OnClickReRoll);
            SetReRollBtn();
            // reRollBtn.SetActive(false);
        }

        void RollSkill()
        {
            var levelUpSkill = SkillSystem.Instance.RandomLevelUpSkill();
            var bonusSkill = SkillSystem.Instance.BonusSkillUp(levelUpSkill);
            var levelUpData = new List<LevelUpData>();
            var tbSkills = new List<SkillAI>();
            
            for (int i = 0; i < levelUpSkill.Count; i++)
            {
                int idx = levelUpSkill[i];
                int level = 0;
                    
                if (idx == 401)
                {
                    level = SkillSystem.Instance.heroLevel.Value;
                }
                else
                {
                    level = SkillSystem.Instance.GetSkillInfo(idx)?.level ?? 0;
                }
                
                var tbSkill = TableDataManager.Instance.GetSkillAiData(idx, (level == 0) ? bonusSkill[i] : level + bonusSkill[i]);
                tbSkills.Add(tbSkill);
                LevelUpData data = new LevelUpData()
                {
                    idx = idx,
                    level = level,
                    UpLevel = bonusSkill[i],
                    SkillGroup = tbSkill.SkillGroup,
                    name = LocaleManager.GetLocale(tbSkill.Name),
                    desc = LocaleManager.GetLocale(tbSkill.Description, tbSkill.DamageRatio),
                    Icon = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(tbSkill.Icon)
                };
                levelUpData.Add(data);
            }
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (levelUpSkill.Count > i)
                {
                    slots[i].SetUI(levelUpData[i]);
                    slotCombinations[i].SetUI(levelUpData[i].idx);
                }
                else
                {
                    slots[i].transform.parent.SetActive(false);
                    slotCombinations[i].HideUI();
                }
            }
            
            // 던전에서만 표기
            var bb = BlackBoard.Instance.data;
            if (bb.dungeonLevel > 0)
            {
                // 자동스킬업
                var findIndex = tbSkills.FindIndex(t => t.SkillGroup == 6);
                targetIdx = findIndex;

                if (targetIdx == -1)
                {
                    var bonusIndex = bonusSkill.FindIndex(t => t == 2);
                    targetIdx = bonusIndex;
                }

                if (targetIdx == -1) targetIdx = 0;

                var bm = GoldDungeonBattleManager.Instance as GoldDungeonBattleManager;
                bm.levelUpCount.Subscribe(x =>
                {
                    remainLevel.SetActive(x > 0);
                    remainLevel.SetText("Dungeon_Level_Up_Count".Locale(x - 1));
                }).AddTo(disposables);
                
                uiPopup.OnShowCallback.Event.AddListener(() =>
                {
                    bm.OnAutoSkillUp.Subscribe(isOn =>
                    {
                        if (isOn)
                        {
                            // 자동선택 옵션
                            Observable.Timer(TimeSpan.FromSeconds(0.4f), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                            {
                                uiPopup.Buttons[targetIdx].onClickEvent.Invoke();
                            }).AddTo(disposables);
                        }
                        else
                            DisposeRX();
                    }).AddTo(this);    
                });
            }
            else
            {
                remainLevel.SetActive(false);
            }
        }

        // 지정된 데이터 반영
        public void SetRoll(StatedData statedData)
        {
            var levelUpSkill = SkillSystem.Instance.RandomLevelUpSkill(statedData.SkillIdx);
            var bonusSkill = statedData.Bonus;
            var levelUpData = new List<LevelUpData>();
            var tbSkills = new List<SkillAI>();
            
            for (int i = 0; i < levelUpSkill.Count; i++)
            {
                int idx = levelUpSkill[i];
                int level = 0;
                    
                if (idx == 401)
                {
                    level = SkillSystem.Instance.heroLevel.Value;
                }
                else
                {
                    level = SkillSystem.Instance.GetSkillInfo(idx)?.level ?? 0;
                }
                
                var tbSkill = TableDataManager.Instance.GetSkillAiData(idx, (level == 0) ? bonusSkill[i] : level + bonusSkill[i]);
                tbSkills.Add(tbSkill);
                LevelUpData data = new LevelUpData()
                {
                    idx = idx,
                    level = level,
                    UpLevel = bonusSkill[i],
                    SkillGroup = tbSkill.SkillGroup,
                    name = LocaleManager.GetLocale(tbSkill.Name),
                    desc = LocaleManager.GetLocale(tbSkill.Description, tbSkill.DamageRatio),
                    Icon = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(tbSkill.Icon)
                };
                levelUpData.Add(data);
            }
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (levelUpSkill.Count > i)
                {
                    slots[i].SetUI(levelUpData[i]);
                    slotCombinations[i].SetUI(levelUpData[i].idx);
                }
                else
                {
                    slots[i].transform.parent.SetActive(false);
                    slotCombinations[i].HideUI();
                }
            }
        }

        public void DisposeRX()
        {
            disposables.Clear();
        }

        void OnClickReRoll()
        {
            Debug.Log("리롤");
            // count Server API
            // case AD
            // case trait
            var bb = BlackBoard.Instance.data;
            if (bb.traitReRoll > 0)
            {
                bb.traitReRoll--;
                SetReRollBtn();
                DisposeRX();
                RollSkill();
                
            }
            else if (bb.adReRoll > 0)
            {
                AdReRoll();
            }
        }

        void SetReRollBtn()
        {
            var bb = BlackBoard.Instance.data;
            reRollBtn.SetActive(bb.traitReRoll + bb.adReRoll > 0);
            if (bb.traitReRoll > 0)
            {
                reRollCount.SetText($"Remain_Count".Locale(bb.traitReRoll));
                adReRoll.SetActive(false);
            }
            else if (bb.adReRoll > 0)
            {
                reRollCount.SetText($"Remain_Count".Locale(bb.adReRoll));
                adReRoll.SetActive(true);
            }
        }

        void AdReRoll()
        {
            AdMobManager.Instance.ShowAD(() =>
            {
                APIRepository.RequestStageRevive(t =>
                {
                    BlackBoard.Instance.data.adReRoll--;
                    SetReRollBtn();
                    DisposeRX();
                    RollSkill();
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
            });
        }
        
        public class LevelUpData
        {
            public int idx;
            public int level;       // 현재 레벨
            public int UpLevel;     // 올려야 할 레벨
            public string name;
            public string desc;
            public Sprite Icon;
            public int SkillGroup;
        }

        public class StatedData
        {
            public List<int> SkillIdx;
            public List<int> Bonus;
        }
    }
}
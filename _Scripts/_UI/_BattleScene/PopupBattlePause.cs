using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM.Battle
{
    public class PopupBattlePause : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_Pause;
        public TMP_Text TitleStage;
        public UIButton GiveUpBtn, ContinueBtn, damageBtn;
        public SkillViewer[] activeSlots;
        public SkillViewer[] passiveSlots;
        public HeroSlotPanel[] HeroSlotPanels;
        public GameObject[] heroLevel;
        public GameObject heroSkillIcon;
        public UIButton vibrateOnBtn, vibrateOffBtn;
        public UIButton soundOnBtn, soundOffBtn;

        [Header("# 챌린지")]
        public GameObject hardMode;
        public List<TMP_Text> penaltyList;
        public List<Image> penaltyIconList;


        protected override void Init()
        {
            var battleManager = BattleManager.Instance;
            TitleStage.SetText(LocaleManager.GetLocale(battleManager.tbStage.Name));
            GiveUpBtn.AddEvent(OnClickGiveUp);
            ContinueBtn.AddEvent(OnClickContinue);
            damageBtn.AddEvent(OnClickDamage);
            
            activeSlots.ForEach(t => t.Hide());
            passiveSlots.ForEach(t => t.Hide());
            HeroSlotPanels.ForEach(t => t.Hide());

            if (TutorialManager.Instance.IsTutorialBattle)
                GiveUpBtn.interactable = false;
            
            foreach (var item in SkillSystem.Instance.ActiveSlots.Select((value, index) => (value,index)))
            {
                activeSlots[item.index].SetData(item.value);
            }
            
            foreach (var item in SkillSystem.Instance.PassiveSlots.Select((value, index) => (value,index)))
            {
                passiveSlots[item.index].SetData(item.value);
            }
            
            foreach (var item in PlayerManager.Instance.GetHeroList().Select((value, index) => (value,index)))
            {
                if(item.index < HeroSlotPanels.Length)
                    HeroSlotPanels[item.index].SetHero(item.value);
            }
            //
            
            var _heroLevel = SkillSystem.Instance.heroLevel.Value;
            if (PlayerManager.Instance.GetHeroList().Count == 0)
            {
                _heroLevel = 0;
                heroSkillIcon.SetActive(false);
            }

            for (int i = 0; i < heroLevel.Length; i++)
            {
                heroLevel[i].SetActive(_heroLevel>i);
            }

            vibrateOnBtn.AddEvent(() => OnClickVibrate(0));
            vibrateOffBtn.AddEvent(() => OnClickVibrate(1));
            soundOnBtn.AddEvent(() => OnClickSoundState(1));
            soundOffBtn.AddEvent(() => OnClickSoundState(0));

            if ((OptionSettingManager.Instance.GetEffectMute() && OptionSettingManager.Instance.GetBgmMute()) ||
                (OptionSettingManager.Instance.GetBgmVolume() <= 0.0f && OptionSettingManager.Instance.GetEffectVolume() <= 0.0f))
            {
                soundOffBtn.SetActive(true);    
                soundOnBtn.SetActive(false);
            }
            else
            {
                soundOffBtn.SetActive(false);
                soundOnBtn.SetActive(true);    
            }
                
            vibrateOnBtn.SetActive(OptionSettingManager.Instance.GetVibration());
            vibrateOffBtn.SetActive(!OptionSettingManager.Instance.GetVibration());

            SetHardModeUI();
        }

        void OnClickGiveUp()
        {
            var popup = UIManager.Instance.Get(PopupName.Battle_Giveup);
            popup.Show();
        }

        void OnClickContinue()
        {
            Hide();
        }

        void OnClickDamage()
        {
            var popup = UIManager.Instance.Get(PopupName.Battle_Damage);
            popup.Show();
        }
        
        void OnClickVibrate(int state)
        {
            if (state == 1)
            {
                vibrateOnBtn.SetActive(true);
                vibrateOffBtn.SetActive(false);
            }
            else
            {
                vibrateOnBtn.SetActive(false);
                vibrateOffBtn.SetActive(true);
            }
            
            OptionSettingManager.Instance.SetVibration(state);
        }

        void OnClickSoundState(int state)
        {
            if (state == 0)
            {
                soundOnBtn.SetActive(true);
                soundOffBtn.SetActive(false);
            }
            else
            {
                soundOnBtn.SetActive(false);
                soundOffBtn.SetActive(true);
            }

            OptionSettingManager.Instance.SetSoundMute(state);
        }

        void SetHardModeUI()
        {
            var challengeBattleManager = ChallengeBattleManager.Instance as ChallengeBattleManager;
            if (challengeBattleManager == null)
            {
                hardMode.SetActive(false);
                return;
            }
            
            hardMode.SetActive(true);
            penaltyList.ForEach(p => p.transform.parent.SetActive(false));

            for (int i = 0; i < challengeBattleManager.penaltyList.Count; i++)
            {
                var data = challengeBattleManager.penaltyList[i];
                SetPenalty(data.EffectGroup, data.PenaltyType, i);
            }
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
                    EquipType equipType = TableDataManager.Instance.data.EquipType
                        .Single(t => t.Index == challengeEffectGroup.Value);
                    
                    string equipTypeName = LocaleManager.GetLocale(equipType.Name);
                    string penaltyTxt = LocaleManager.GetLocale(challengePenaltyType.Description, equipTypeName);
                    penaltyList[index].SetText(penaltyTxt);
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
            penaltyList[index].transform.parent.SetActive(true);
        }
    }
}
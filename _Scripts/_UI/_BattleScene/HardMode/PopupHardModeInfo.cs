using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM.Battle
{
    public class PopupHardModeInfo : Popup
    {
        public override PopupName ID { get; set; } = PopupName.HardModeInfo;
        public GameObject hardMode;
        public List<TMP_Text> penaltyList;
        public List<Image> penaltyIconList;
        protected override void Init()
        {
            SetHardModeUI();
            ParticleImageUnscaled();
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
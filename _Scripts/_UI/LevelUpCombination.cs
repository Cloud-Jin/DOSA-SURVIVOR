using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectM.Battle
{
    public class LevelUpCombination : MonoBehaviour
    {
        public List<SlotLevelUpCombination> slots;

        public void SetUI(int GroupID)
        {
            var tbCombination = TableDataManager.Instance.data.MasterSkillCombination;
            var CombinationList = tbCombination.Where(t => t.EvolutionSkillGroupID == GroupID || t.TargetCombinationSkillGroupID == GroupID).ToList();

            for (int i = 0; i < slots.Count; i++)
            {
                var tbData = i < CombinationList.Count ? CombinationList[i] : null;
                slots[i].SetUI(GroupID, tbData);
            }

            bool isActive = false;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].gameObject.activeSelf)
                {
                    isActive = true;
                    break;
                }
            }
            
            gameObject.SetActive(isActive);
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
        }
    }

}
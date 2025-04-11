using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


// 조합스킬 찾아서 세팅.

namespace ProjectM.Battle
{
    public class SlotLevelUpCombination : MonoBehaviour
    {
        public Image Icon;
        public GameObject[] stars;

        private int _targetID, _targetLv;
        public void SetUI(int idx, MasterSkillCombination data)
        {
            // 작업중 스킬 제외하기
            if (data == null)// || data.EvolutionSkillGroupID == 108)
            {
                HideUI();
                return;
            }
            gameObject.SetActive(true);
            if (data.EvolutionSkillGroupID == idx)
            {
                _targetID = data.TargetCombinationSkillGroupID;
                _targetLv = data.TargetCombinationSkillLevel;
            }
            else
            {
                // 장착무기체크.
                var tbSet = BlackBoard.Instance.GetSkillSet();
                var id = TableDataManager.Instance.data.SkillAI.Where(t => t.SkillGroup == 5 || t.SkillGroup == 99).Select(t=> t.GroupID).Distinct().ToList();
                if (id.Contains(data.EvolutionSkillGroupID) && tbSet.ChangeSkillID != data.EvolutionSkillGroupID)
                {
                    HideUI();
                    return;
                }
                
                _targetID = data.EvolutionSkillGroupID;
                _targetLv = data.EvolutionSkillLevel;
            }

            var tbLinkSkill = TableDataManager.Instance.GetSkillAiData(_targetID, 1);
            Icon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(tbLinkSkill.Icon);
            for (int i = 1; i < stars.Length; i++)
            {
                stars[i].SetActive(_targetLv > i);
            }
        }
        
        public void HideUI()
        {
            gameObject.SetActive(false);
        }
    }

}
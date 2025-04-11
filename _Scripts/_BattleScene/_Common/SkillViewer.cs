using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// 스킬 아이콘, 레벨 표시


namespace ProjectM.Battle
{
    public class SkillViewer : MonoBehaviour
    {
        public Image Icon;
        public GameObject Panel;
        public GameObject[] Star;
        public Sprite[] StarColor;
        
        public void SetData(SkillBase skillBase)
        {
            Panel.SetActive(true);
            var tbSkill = TableDataManager.Instance.GetSkillAiData(skillBase.idx, skillBase.level);
            Icon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(tbSkill.Icon);
            
            if(Star == null) return;
            if (tbSkill.SkillGroup == 6) // 마스터 스킬
            {
                for (int i = 0; i < Star.Length; i++)
                {
                    if (i == 2)
                    {
                        Star[i].SetActive(true);
                        Star[i].GetComponent<Image>().sprite = StarColor[1];
                    }
                    else
                        Star[i].SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < Star.Length; i++)
                {
                    if (skillBase.level > i)
                    {
                        Star[i].SetActive(true);
                    }
                    else
                        Star[i].SetActive(false);
                }
            }
        }
        
        public void SetData(int idx)
        {
            Panel.SetActive(true);
            int level = SkillSystem.Instance.heroLevel.Value;
            
            var tbSkill = TableDataManager.Instance.GetSkillAiData(idx, level);
            Icon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(tbSkill.Icon);
            
            if(Star == null) return;
            
            for (int i = 0; i < Star.Length; i++)
            {
                if (level > i)
                {
                    Star[i].SetActive(true);
                }
                else 
                    Star[i].SetActive(false);
            }
        }

        public void Hide()
        {
            Panel.SetActive(false);
        }
    }

}
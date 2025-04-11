using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM.Battle
{
    public class DamageViewer : MonoBehaviour
    {
        public Image icon;
        public TMP_Text nameLabel;
        public TMP_Text damageLabel;
        public TMP_Text damagePercentageLabel;
        public Image percentage;
        public Image[] stars;
        
        public void SetData(DamageInfo info)
        {
            icon.sprite = info.iconSprite;
            nameLabel.SetText(info.nameStr);
            damageLabel.SetText(info.damageStr);
            damagePercentageLabel.SetText($"{info.damagePer:P}");
            percentage.fillAmount = info.damagePer;
            
            if (info.idx == 0) return;
            var tbSkill = TableDataManager.Instance.GetSkillAiData(info.idx, info.level);

            
            if (tbSkill.SkillGroup == 6) // 마스터 스킬
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (i == 2)
                    {
                        stars[i].SetActive(true);
                        stars[i].GetComponent<Image>().sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill)
                            .GetSprite("UI_Icon_GradeStar_01");
                    }
                    else
                        stars[i].SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < stars.Length; i++)
                {
                    if (info.level > i)
                    {
                        stars[i].SetActive(true);
                    }
                    else
                        stars[i].SetActive(false);
                }
            }

        }
    }

    public class DamageInfo
    {
        public Sprite iconSprite;
        public string nameStr;
        public string damageStr;
        public float damagePer;
        public int level;
        public int idx;
    }
}

using System;
using System.Collections.Generic;
using DG.Tweening;
using Doozy.Runtime.UIManager.Components;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// bonus 스킬업
// 마스터 스킬업
// 조합UI
namespace ProjectM.Battle
{
    public class SlotLevelUp : MonoBehaviour
    {
        public UIButton levelUpBtn;
        public TMP_Text localeName, localeDesc;
        public HeroSkillUpViewer heroSkillUpViewer;
        public Sprite[] starBg;
        public Image icon;
        public Image[] Level;
        public GameObject iconNew, iconBonus;
        public GameObject[] panel;
        private PopupLevelUp.LevelUpData _data;

        private void Awake()
        {
            levelUpBtn.AddEvent(OnClickSlot);
        }

        public void SetUI(PopupLevelUp.LevelUpData data)
        {
            this._data = data;
            gameObject.SetActive(true);
            
            localeName.SetText(data.name);
            localeDesc.SetText(data.desc);
            panel.ForEach(t => t.SetActive(false));
            int spriteIdx = data.SkillGroup ==  6 ? 2 : data.SkillGroup == 4 || data.SkillGroup == 11 ? 1 : 0;
            panel[spriteIdx].SetActive(true);
            
            if (data.SkillGroup == 6) // 마스터 스킬
            {
                for (int i = 0; i < Level.Length; i++)
                {
                    if (i == 0)
                    {
                        Level[i].sprite = starBg[1];    
                    }
                    else
                    {
                        Level[i].gameObject.SetActive(false);
                    }
                }
            }
            else if (data.SkillGroup == 8) // HP 회복
            {
                for (int i = 0; i < Level.Length; i++)
                {
                    Level[i].SetActive(false);
                }
            }
            else // 일반 스킬
            {
                for (int i = 0; i < Level.Length; i++)
                {
                    Level[i].sprite = starBg[i < data.level ? 0 : 2];   // 현재 레벨 표시
                    Level[i].color = Color.white;
                    DOTween.Kill(Level[i]);
                    
                    if (data.level <=  i  && i < data.level + data.UpLevel) // 레벨업할 스킬레벨 표시
                    {
                        Level[i].sprite = starBg[0];
                        Level[i].DOFade(0, 0.7f).SetEase(Ease.OutSine)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetUpdate(true);
                    }
                }
            }

            // iconNew.SetActive(data.level == 0 && data.UpLevel == 1);
            iconBonus.SetActive(data.UpLevel > 1);
            switch (_data.idx)
            {
                case 401:
                    icon.SetActive(false);
                    icon.sprite = null;
                    heroSkillUpViewer.SetData(PlayerManager.Instance.GetHeroList());
                    break;
                default:
                    icon.SetActive(true);
                    icon.sprite = data.Icon;
                    heroSkillUpViewer.SetData(new List<Hero>());
                    break;
            }
            
        }

        void OnClickSlot()
        {
            var popup = UIManager.Instance.GetPopup(PopupName.Battle_LevelUp) as PopupLevelUp;
            popup.DisposeRX();
            BattleManager.Instance.levelUpCount.Value--;
            switch (_data.idx)
            {
                case 401:// 영웅 스킬레벨업
                    SkillSystem.Instance.HeroSkillUp(_data.UpLevel);
                    break;
                case 402:// 체력
                    var rate = TableDataManager.Instance.GetSkillAiData(402, 1).DamageRatio / 100f;
                    PlayerManager.Instance.OnHealing(rate);
                    break;
                default:// 플레이어 스킬업
                    SkillSystem.Instance.LevelUp(_data.idx, _data.level + _data.UpLevel);
                    break;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < Level.Length; i++)
            {
                DOTween.Kill(Level[i]);
            }
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
        }
    }
}

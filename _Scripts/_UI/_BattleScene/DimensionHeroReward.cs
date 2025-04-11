using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 균열보상 영웅소환 타입
namespace ProjectM.Battle
{
    public class DimensionHeroReward : MonoBehaviour
    {
        public TMP_Text heroName;
        public Transform heroIcon;
        public Image HeroNameBg, HeroBg;  

        public List<GameObject> Slots;

        public List<Image> SkillIcons;
        public List<TMP_Text> SkillNames, SKillDesc;

        private List<SkillAI> heroSkills;

        public TMP_Text HeroStat;
        
        public void SetData(PopupDimension.RewardData data)
        {
            heroSkills = new List<SkillAI>();
            var resourcesManager = ResourcesManager.Instance;
            
            var hero = TableDataManager.Instance.data.Monster.Single(t => t.Index == data.heroIdx);
            var artifact = TableDataManager.Instance.GetCard(data.heroIdx);
            var rarity = TableDataManager.Instance.data.CardRarityType.Single(t => t.Index == artifact.CardRarity); 
            heroName.SetText(LocaleManager.GetLocale(hero.Name));

            var icon = resourcesManager.Instantiate(artifact.HeroIdleAnim, heroIcon);
            icon.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
            // HeroNameBg.sprite = resourcesManager.GetAtlas(MyAtlas.Equipment_Frame).GetSprite($"{rarity.RarityColor}_Title");
            HeroNameBg.sprite = resourcesManager.GetResources<Sprite>($"{rarity.RarityColor}_Title");
            HeroBg.sprite = resourcesManager.GetResources<Sprite>($"{rarity.RarityColor}_Icon");
            // HeroBg.sprite = resourcesManager.GetAtlas(MyAtlas.Equipment_Frame).GetSprite($"{rarity.RarityColor}_Icon");

            var patterns = TableDataManager.Instance.GetSkillPattern(hero.PatternGroupID);
            foreach (var t in patterns)
            {
                var skillAi = TableDataManager.Instance.GetSkillAiData(t.SkillGroupID, 1);
                if (skillAi.SkillGroup != 9)
                    heroSkills.Add(skillAi);
            }

            // 슬롯 1~3 스킬 표시
            for (int i = 0; i < 3; i++)
            {
                if (heroSkills.Count > i)
                {
                    SkillIcons[i].sprite = resourcesManager.GetAtlas(MyAtlas.BattleSkill).GetSprite(heroSkills[i].Icon);
                    SkillNames[i].SetText(LocaleManager.GetLocale(heroSkills[i].Name));
                    SKillDesc[i].SetText(LocaleManager.GetLocale(heroSkills[i].Description));
                }
                else
                {
                    Slots[i].SetActive(false);
                }
            }
            
            Slots[3].SetActive(true);  // 능력치
            string AbilityText = String.Format($"+Atk   {artifact.BasicAtk*0.01f}%\n+Hp     {(artifact.BasicHP*0.01f)}%");
            HeroStat.SetText(AbilityText);
            
            // Player Base
            
            // var factor = TableDataManager.Instance.data.ArtifactHeroAbilityFactor.Single(t => t.Rarity == artifact.ArtifactRarity);
            // int level = 1;
            // var rarityFactor = factor.AbilityFactor / 10000f;
            // var LevelFactor = (factor.LevelFactor / 10000f) * level;
            // var resultFactor = rarityFactor + LevelFactor;
            // // player * resultFactor // 계산위치 playermanager?
            // var player = PlayerManager.Instance.player;
            // string AbilityText = String.Format($"Atk {(player.baseAttack * resultFactor).ToParseString()}\nHp {(player.baseHealth * resultFactor).ToParseString()}");
            
            
            Slots[4].SetActive(false);   // 보유스킬???
            Slots[5].SetActive(false); // 광고
        }
    }
}
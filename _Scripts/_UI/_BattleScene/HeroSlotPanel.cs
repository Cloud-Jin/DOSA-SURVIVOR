using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectM.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM.Battle
{
    public class HeroSlotPanel : MonoBehaviour
    {
        public List<SkillViewer> HeroSkillViewers;
        public Image heroIcon;

        public void SetHero(Hero hero)
        {
            var artifact = TableDataManager.Instance.data.Card.Single(t => t.HeroCharacterID == hero.unitID);
            heroIcon.SetActive(true);
            heroIcon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(artifact.HeroCharacterBigIcon);

            // var heroSkill = hero.StateMachine.SkillStates.Where(t => t.data.SkillGroup != 9).ToList();
            // for (int i = 0; i < HeroSkillViewers.Count; i++)
            // {
            //     if (heroSkill.Count > i)
            //         HeroSkillViewers[i].SetData(heroSkill[i].data.GroupID);
            //     else
            //     {
            //         HeroSkillViewers[i].Hide();
            //     }
            // }
        }

        public void Hide()
        {
            heroIcon.SetActive(false);
            HeroSkillViewers.ForEach(t=> t.Hide());
        }
    }
}

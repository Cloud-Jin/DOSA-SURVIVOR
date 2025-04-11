using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM.Battle
{
    public class PopupGetHero : Popup
    {
        Transform heroIcon;
        public TMP_Text HeroName, JoinMessage, hpTmp, atkTmp;
        public List<GameObject> Slots;
        public List<Transform> RarityIcon; 
        public override PopupName ID { get; set; } = PopupName.Battle_GetHero;
        protected override void Init()
        {
            ParticleImageUnscaled();
            SoundManager.Instance.PlayFX("RewardGetPopup");
        }

        public void SetHero(int idx)
        {
            var artifact = TableDataManager.Instance.data.Card.Single(t => t.HeroCharacterID == idx);
            var monster = TableDataManager.Instance.data.Monster.Single(t => t.Index == idx);
            var rarity = TableDataManager.Instance.data.CardRarityType.Single(t => t.Index == artifact.CardRarity); 
            // BigIcon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Icon).GetSprite(artifact.HeroCharacterBigIcon);
            Slots.ForEach(t=> t.SetActive(false));
            
            for (int i = 0; i < Slots.Count; i++)
            {
                if (i + 1 == rarity.Index)
                {
                    Slots[i].SetActive(true);
                    heroIcon = RarityIcon[i];
                }
            }
            var icon = ResourcesManager.Instance.Instantiate(artifact.HeroIdleAnim, heroIcon);
            icon.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;

            string heroName = LocaleManager.GetLocale(monster.Name);
            HeroName.SetText(heroName);
            JoinMessage.SetText(LocaleManager.GetLocale("Dimension_Reward_Confirm_01", heroName));
            hpTmp.SetText($"+{artifact.BasicHP*0.01f}%");
            atkTmp.SetText($"+{artifact.BasicAtk*0.01f}%");
        }
    }

}
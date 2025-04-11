using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM.Battle
{
    public class PopupBattleDamage : Popup
    {
        public override PopupName ID { get; set; }
        public List<DamageViewer> PlayerDamageViewers;
        public List<DamageViewer> heroDamageViewers;
        
        protected override void Init()
        {
            ShowPendingPopup = false;
            InfVal _total = 0;
            var _activeSlots = SkillSystem.Instance.ActiveSlots.Select(t => t as ActiveSkill).ToList();

            _activeSlots.ForEach(t => _total += t.accDamage);
            var _herolist = PlayerManager.Instance.GetHeroList();
            _herolist.ForEach(t => _total += t.accDamage);
            
            foreach (var viewer in PlayerDamageViewers.Select((value, index) => (value,index)))
            {
                if (_activeSlots.Count > viewer.index)
                {
                    var active = _activeSlots[viewer.index];
                    var tbSkill = TableDataManager.Instance.GetSkillAiData(active.idx, active.level);
                    
                    InfVal per = 0;
                    if(_total > 0)
                        per = active.accDamage / _total;
                    
                    DamageInfo info = new DamageInfo()
                    {
                        iconSprite    = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(tbSkill.Icon),
                        damageStr = active.accDamage.ToParseString(),
                        nameStr = LocaleManager.GetLocale(tbSkill.Name),
                        damagePer = (float)per,
                        level = active.level,
                        idx = active.idx
                    };
                    viewer.value.SetData(info);
                    // Debug.Log("스킬 있음");
                }
                else
                {
                    // Debug.Log("스킬 없음");
                    viewer.value.SetActive(false);
                }
                
                // hero

                foreach (var heroViewer in heroDamageViewers.Select((value, index) => (value, index)))
                {
                    if (_herolist.Count > heroViewer.index)
                    {
                        var hero = _herolist[heroViewer.index];
               
                        var artifact = TableDataManager.Instance.data.Card.Single(t => t.HeroCharacterID == hero.unitID);
                        var tbHero = TableDataManager.Instance.data.Monster.Single(t => t.Index == hero.unitID);

                        InfVal per = 0;
                        if (_total > 0)
                            per = hero.accDamage / _total;
                
                        DamageInfo info = new DamageInfo()
                        {
                            iconSprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(artifact.HeroCharacterBigIcon),
                            damageStr = hero.accDamage.ToParseString(),
                            nameStr = LocaleManager.GetLocale(tbHero.Name),
                            damagePer = (float)per
                        };
                        heroViewer.value.SetData(info);
                    }
                    else
                    {
                        heroViewer.value.SetActive(false);
                    }
                }
            }
        }
    }
}

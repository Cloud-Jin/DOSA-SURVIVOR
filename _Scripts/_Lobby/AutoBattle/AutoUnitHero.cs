using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectM.Battle;
using UnityEngine;

namespace ProjectM.AutoBattle
{
    public class AutoUnitHero : AutoUnit
    {
        public override void Init(Monster stat)
        {
            base.Init(stat);
            var artifact = TableDataManager.Instance.data.Card.Single(t => t.HeroCharacterID == stat.Index);
            // var factor = TableDataManager.Instance.data.ArtifactHeroAbilityFactor.Single(t => t.Rarity == artifact.ArtifactRarity);
            // int level = 1;
            // var rarityFactor = factor.AbilityFactor / 10000f;
            // var LevelFactor = (factor.LevelFactor / 10000f) * level;
            // var resultFactor = rarityFactor + LevelFactor;
            // player * resultFactor
            unitId = stat.Index;
            attack = UserDataManager.Instance.gearInfo.GetEquipGearsPower().Attack * 1;
            moveSpeed = TableDataManager.Instance.data.AutoBattleConfig.Single(t => t.Index == 18).Value;
        }
    }
}
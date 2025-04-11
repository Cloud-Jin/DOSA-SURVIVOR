using System;
using System.Collections.Generic;
using InfiniteValue;
using UnityEngine.Serialization;

// 플레이어 레벨, 경험치, HP, 스킬
// 영웅 HP 스킬
// 스킬 groupID, Level

namespace ProjectM.Battle
{
    [Serializable]
    public class BattleData
    {
        public int mapIndex;
        public int gameTime;     // 남은 시간
        public int killScore, crackScore, bossScore;
        public int reviveCount;
        public int bossOrder;
        public int crackOrder;
        public int level;
        public float exp;
        public string attack,hp, maxHp;
        public Equipment weaponInfo;
        public Costume costumeInfo;
        public int heroLv;
        public WaveType WaveType;
        public int zoom;
        public int adReRoll;                // 광고 스킬재굴림
        public int traitReRoll;             // 특성 스킬재굴림
        public Dictionary<string, List<SkillData>> SkillDatas = new Dictionary<string, List<SkillData>>();
        public List<SkillData> HeroDatas = new List<SkillData>();
        public List<RewardData> RewardDatas;
        public List<RewardData> BenefitRewardDatas = new List<RewardData>();
        public List<int> benefit_ids = new List<int>();
        public int benefitEffect;
        public int dungeonLevel;
        public bool booster;                // 스테이지 부스터
    }
    
    [Serializable]
    public class SkillData
    {
        public int GroupID;
        public int Lv;
        public string accDamage;
    }
}

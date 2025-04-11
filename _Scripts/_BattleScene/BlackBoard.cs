using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InfiniteValue;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

/*
남은시간
전체 몬스터 킬수
균열 선택지 정보 및 보상 획득 정보
캐릭터 레벨 및 경험치 남은 HP
스킬 ( 플레이어, 영웅 )
보스 처치정보(몇번째 보스까지 처치했는지)
영웅 List  ( 소환한 영웅 ) 남은 HP
*/


namespace ProjectM.Battle
{
    /// <summary>
    /// 게임정보 데이터 저장 / 로드
    /// </summary>
    
    
    public class BlackBoard : Singleton<BlackBoard>
    {
        public BattleData data;
        public LobbyTap LobbyTap;
        
        protected override void Init()
        {
            // for Editor
            data = new BattleData();
            LobbyTap = LobbyTap.Lobby;
            ClearData();
            data.mapIndex = 74;
            data.gameTime = 600;
            data.attack = new InfVal(2000).ToString();
            data.hp = data.maxHp = 10000000000000.ToString();
            data.traitReRoll = 99;
            data.booster = true;
            data.weaponInfo = TableDataManager.Instance.data.Equipment.Single(t => t.Index == 101);
            SetSkillData("player",1,1);
        }


        public void ClearData()
        {
            var dataManager = TableDataManager.Instance;
            data = new BattleData();
            data.level = 1;
            data.reviveCount = 1;
            data.heroLv = 1;
            data.WaveType = WaveType.Normal;
            data.crackOrder = 1;
            data.adReRoll = dataManager.GetStageConfig(18).Value;
            data.traitReRoll = GetRollChance();
            data.booster = false;
        }
        public void SetSkillData(string key, int GroupID, int Lv)
        {
            if (!data.SkillDatas.ContainsKey(key))
            {
                data.SkillDatas.Add(key, new List<SkillData>());
            }
           
            var m = data.SkillDatas[key].Where(p => p.GroupID == GroupID).SingleOrDefault();
            if (m != null)
            {
                m.Lv = Lv; // 갱신
            }
            else
            {
                data.SkillDatas[key].Add(new SkillData(){GroupID = GroupID, Lv = Lv, accDamage = ""});
            }
        }

        public void AddHeroData(int index)
        {
            if (data.HeroDatas.Exists(t => t.GroupID == index))
                return;
            
            data.HeroDatas.Add(new SkillData(){GroupID = index});
        }

        public void ResetData()
        {
            PlayerPrefs.SetString(MyPlayerPrefsKey.BlackBoard, string.Empty);
            PlayerPrefs.Save();
        }

        [Button]
        public void Save()
        {
            // ToJson
            string jsonData = ObjectToJson(data);
            Debug.Log(jsonData);
            PlayerPrefs.SetString(MyPlayerPrefsKey.BlackBoard, jsonData);
            PlayerPrefs.Save();
        }
        
        [Button]
        public void SaveBattleData()
        {
            if(data.mapIndex == 0) return;
            if(data.mapIndex == 10001) return;
            
            var bm = BattleManager.Instance;
            data.gameTime = bm.gameTime.Value;
            data.killScore = bm.kill.Value;
            data.level = bm.level.Value;
            data.exp = bm.exp;
            data.crackScore = bm.CrackKill.Value;
            data.reviveCount = bm.reviveCount;
            data.bossOrder = bm.bossOrder;
            data.crackOrder = bm.crackOrder;
            data.heroLv = SkillSystem.Instance.heroLevel.Value;
            data.WaveType = bm.spawner.waveType;
            data.zoom = bm.zoomIndex;
            data.hp = PlayerManager.Instance.player.health.ToString();
            
            foreach (var slot in SkillSystem.Instance.ActiveSlots.Select(t => (ActiveSkill)t))
            {
                var skill = data.SkillDatas["player"].Single(t => t.GroupID == slot.idx);
                skill.accDamage = slot.accDamage.ToString();
                skill.Lv = slot.level;
            }
            
            foreach (var slot in SkillSystem.Instance.PassiveSlots.Select(t => (PassiveSkill)t))
            {
                var skill = data.SkillDatas["player"].Single(t => t.GroupID == slot.idx);
                skill.Lv = slot.level;
            }
            
            foreach (var hero in PlayerManager.Instance.GetHeroList())
            {
                data.HeroDatas.Single(t => t.GroupID == hero.unitID).accDamage = hero.accDamage.ToString();
            }
            
            
            // 영웅들 누적딜
            
            
            Save();
        }

        [Button]
        public bool Load()
        {
            var jsonData = PlayerPrefs.GetString(MyPlayerPrefsKey.BlackBoard, "");
            if (string.IsNullOrEmpty(jsonData))
                return false;
            
            var loadData = JsonToObject<BattleData>(jsonData);
            data = loadData;
            return true;
        }

        string ObjectToJson(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        T JsonToObject<T>(string jsonData)
        {
            JObject a = JObject.Parse(jsonData);
            var ret = a.ToObject<T>();
            Debug.Log(ret);
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public Costume GetCostume()
        {
            if (data.costumeInfo == null)
            {
                return TableDataManager.Instance.data.Costume.Single(t => t.Index == 1);
            }
            
            return data.costumeInfo;
        }
        public SkillSet GetSkillSet()
        {
            var weapon = data.weaponInfo;
            var costume = GetCostume();
            List<SkillSet> skillSets = new List<SkillSet>();
            
            if (costume != null && costume.TargetSkillSetID > 0)
            {
                var costumeSet = TableDataManager.Instance.data.SkillSet.Single(t => t.index == costume.TargetSkillSetID);
                skillSets.Add(costumeSet);
            }
            
            if (weapon != null)
            {
                var weaponSet = TableDataManager.Instance.data.SkillSet.Single(t => t.index == weapon.SkillID);
                skillSets.Add(weaponSet);
                var data = skillSets.Where(t => t.MainGroupID == weaponSet.MainGroupID).OrderBy(t => t.Order).First();
                return data;
            }

            return null;
        }

        private int GetRollChance()
        {
            var rollType = TableDataManager.Instance.data.Trait.Where(t => t.Type == 201).ToList();
            int count = 0;
            rollType.ForEach(t =>
            {
                count += UserDataManager.Instance.userTraitInfo.GetTraitValue(t.Index);
            });
            
            return count;
        }
    }
}

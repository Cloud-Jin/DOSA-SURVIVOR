using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProjectM
{
    public class TableDataManager : Singleton<TableDataManager>
    {
        public DataModel data;
        public List<string> _includeList { get; private set; }
        public List<string> _matchList { get; private set; }
        public string hash;
        protected override void Init()
        {
            var dataDB = ResourcesManager.Instance.GetResources<TextAsset>("data");
            data = JsonUtility.FromJson<DataModel>(dataDB.text);
        }

        public IEnumerator InitTable()
        {
            var dataDB = ResourcesManager.Instance.GetResources<TextAsset>("data");
            
            hash = HashData.SHA256Hash(dataDB.text); 
            Debug.Log($"hash table {hash}");
            data = JsonUtility.FromJson<DataModel>(dataDB.text);
            
            
            var badListInclude = ResourcesManager.Instance.GetResources<TextAsset>("badList_Include");
            _includeList = new List<string>(badListInclude.text.Replace(",,,,,", String.Empty).Split("\r\n"));
            
            var badListMatch = ResourcesManager.Instance.GetResources<TextAsset>("badList_Match");
            _matchList = new List<string>(badListMatch.text.Replace(",,,,,", String.Empty).Split("\r\n"));
            
            initComplete = true;
            Debug.Log("TableDataManager InitComplete");
            yield break;
        }

        public int[] LevelData()
        {
            var retval = data.StageCharacterLevel.Select(t => t.Exp).ToArray();

            return retval;
        }
        
        public Config GetConfig(int index)
        {
            return data.Config.Single(t => t.Index == index);
        }

        public StageConfig GetStageConfig(int index)
        {
            return data.StageConfig.Single(t => t.Index == index);
        }
        
        public DungeonConfig GetDungeonConfig(int index)
        {
            return data.DungeonConfig.Single(t => t.Index == index);
        }

        public Stage GetStageData(int stageIndex)
        {
            return data.Stage.Single(t => t.Index == stageIndex);
        }
        
        public Stage GetStageData(int chapterID, int stageLv, int stageType)
        {
            List<Stage> stageList = data.Stage.Where(d => d.StageType == stageType).ToList();
            return stageList.SingleOrDefault(t => t.ChapterID == chapterID && t.StageLevel == stageLv);
        }

        public Monster[] GetNormalMonster(int groupID)
        {
            var MonsterGroup = data.NormalMonsterGroup
                .Where(t => t.GroupID == groupID)
                .Select(t => t.MonsterID)
                .ToArray();

            return Enumerable.Range(0, MonsterGroup.Length)
                .Select(i => MonsterGroup[i])
                .Select(Index => data.Monster.Single(x => x.Index == Index))
                .ToArray();
        }

        public NormalMonsterGroup[] GetNormalMonsterGroup(int groupID)
        {
            return data.NormalMonsterGroup
                .Where(t => t.GroupID == groupID)
                .ToArray();
        }

        public Monster[] GetBossMonstersGroup(int groupID)
        {
            var BossMonsterGroup = data.BossMonsterGroup
                .Where(t => t.GroupID == groupID)
                .Select(t => t.MonsterID)
                .ToArray();

            return Enumerable.Range(0, BossMonsterGroup.Length)
                .Select(i => BossMonsterGroup[i])
                .Select(Index => data.Monster.Single(x => x.Index == Index))
                .ToArray();
        }

        public BossMonsterGroup[] GetBossGroup(int groupID)
        {
            var BossMonsterGroup = data.BossMonsterGroup
                .Where(t => t.GroupID == groupID)
                .ToArray();

            return BossMonsterGroup;
        }

        public List<List<BossMonsterGroup>> GetBossMonsterGroups(int groupID)
        {
            var groups = GetBossGroup(groupID);
            var groupList = new List<List<BossMonsterGroup>>();
            groupList.Add(groups.Where(t=> t.Order == 1).ToList());
            groupList.Add(groups.Where(t=> t.Order == 2).ToList());
            
            return groupList;
        }

        public StageSpawnList[] GetstageSpawnLists(int groupID)
        {
            var stageSpawnLists = data.StageSpawnList.Where(t => t.GroupID == groupID).ToArray();
            return stageSpawnLists;
        }

        public StageMonsterExpPieceGroup[] GetMonsterExpPriceGroup(int ExpPieceGroupID)
        {
            return data.StageMonsterExpPieceGroup.Where(t => t.GroupID == ExpPieceGroupID).ToArray();
        }

        // public Skill GetSkillData(int groupID, int Level)
        // {
        //     return data.Skill.Single(t => t.GroupID == groupID && t.Level == Level);
        // }

        public SkillAI GetSkillAiData(int index)
        {
            return data.SkillAI.Single(t => t.Index == index);
        }

        public SkillAI GetSkillAiData(int groupID, int level)
        {
            return data.SkillAI.Single(t => t.GroupID == groupID && t.Level == level);
        }

        public SkillAI[] GetSkillAiDatas(int groupID)
        {
            return data.SkillAI.Where(t => t.GroupID == groupID).ToArray();
        }

        public PatternGroup[] GetSkillPattern(int groupID)
        {
            return data.PatternGroup.Where(t => t.GroupID == groupID).ToArray();
        }

        public Card GetCard(int heroID)
        {
            return data.Card.Single(t => t.HeroCharacterID == heroID);
        }

        public int GetTraitMaxLevel(int levelGroupID)
        {
            return data.TraitLevel.Count(t => t.GroupID == levelGroupID);
        }

        public UnlockCondition GetUnlockCondition(int ConditionID)
        {
            return data.UnlockCondition.Single(t => t.Index == ConditionID);
        }
    }
}
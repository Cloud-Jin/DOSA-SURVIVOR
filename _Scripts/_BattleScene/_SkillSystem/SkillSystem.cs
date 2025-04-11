using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Doozy.Runtime.Common.Extensions;
using InfiniteValue;
using Sirenix.OdinInspector;
using UniRx;
using Random = UnityEngine.Random;

// 배울수 있는 스킬
// 배운 스킬 정보
// GroupID
namespace ProjectM.Battle
{
    public class SkillSystem : Singleton<SkillSystem>
    {
        [HideInInspector] public List<SkillBase> Slots;
        public List<SkillBase> ActiveSlots, PassiveSlots;
        public IncValue incValue;
        public ReactiveProperty<int> heroLevel; // 영웅스킬 레벨
        int bonusRate;
        public float calcCoolTime;  // 최종쿨타임 감소수치
        public float calcDmgReduce; // 최종피해감소 감소수치
        public int calcMiss;        // 피해무시
        public float common;        // 공용스킬 피해량 증가

        Player player;
        Transform transformSkill;
        private List<int> activeSkillList;
        private List<int> passiveSkillList;
        private List<int> removeSkillList;
        private List<int> unlockActiveSkillList;          // 컨텐츠 언락
        private List<int> unlockPassiveSkillList;          // 컨텐츠 언락
        
        [HideInInspector] public ReactiveProperty<bool> InitReady;                         // 초기화 이벤트
        protected override void Init()
        {
            // base 무기 스킬 Lv 1
            removeSkillList = new List<int>();
            heroLevel = new ReactiveProperty<int>(1);
            InitReady = new ReactiveProperty<bool>();
            
            var skilltb = TableDataManager.Instance.data.SkillAI;
            activeSkillList = new List<int>() { 101, 102, 103, 104, 105, 106, 107, 108 };
            passiveSkillList = skilltb.Where(t => t.SkillGroup == 4).Select(t => t.GroupID).Distinct().ToList();
            InitUnlockSkill();
            CalcCoolTime();
            CalcDmgReduce();
            CalcMiss();
            CalcCommon();
        }
        
        public List<BattleSkillData> ConvertData()
        {
            var data = Slots;
            
            var convertdata = data.Select(t => new BattleSkillData() { i = t.idx, l = t.level }).ToList();
            if (heroLevel.Value > 0 && PlayerManager.Instance.GetHeroList().Count > 0)
            {
                convertdata.Add(new BattleSkillData() { i = 401, l = heroLevel.Value });
            }
            return convertdata;
        }

        private void Start()
        {
            PlayerManager.Instance.InitReady.Where(t=> t).Subscribe(_ => StartSet()).AddTo(this);
        }

        private void StartSet()
        {
            // BlackBoard에서 스킬 데이터 가져온다.
            
            player = PlayerManager.Instance.player;
            transformSkill = PlayerManager.Instance.playerble.slot;
            var bb = BlackBoard.Instance.data;
            var skillData = bb.SkillDatas["player"];
            foreach (var skill in skillData)
            { 
                LevelUp(skill.GroupID, skill.Lv);
            }
            
            foreach (var slot in ActiveSlots.Select(t => (ActiveSkill)t))
            {
                var sk = skillData.Single(t => t.GroupID == slot.idx);
                slot.accDamage = new InfVal(sk.accDamage);
            }

            
            // 영웅스킬레벨
            heroLevel.Value = bb.heroLv;
            bonusRate = BattleManager.Instance.GetBonusRate();
            InitReady.Value = true;

            player.SetMiss(calcMiss);
        }

        public SkillBase GetSkillInfo(int idx)
        {
            var skill = Slots.FirstOrDefault(t => t.idx == idx);
            return skill;
        }

        [Button]
        public void SkillUp(SkillType type, int level = 1)
        {
            LevelUp((int)type, level);
        }
        
        public void LevelUp(int groupID, int level)
        {
            // 스킬 인덱스로 스킬 판별
            // 레벨체크
            // Skill Add or Up
            var target = Slots.Find(x => x.idx == groupID);
            if (target)
            {
                target.SetLevel(level);
                return;
            }

            GameObject newSkill = new GameObject();
            newSkill.transform.localScale = Vector3.one;
          
            switch ((SkillType)groupID)
            {
                case SkillType.Flame_Charm:
                    newSkill.AddComponent<FlameCharm>().Init(player, groupID);
                    break;
                case SkillType.Sword_Strength:
                    newSkill.AddComponent<SwordStrength>().Init(player, groupID);
                    break;
                case SkillType.Fanning:
                    newSkill.AddComponent<Fanning>().Init(player, groupID);
                    break;
                case SkillType.Multiple_Shot:
                    newSkill.AddComponent<MultipleShot>().Init(player, groupID);
                    break;
                case SkillType.Flash_Stab:
                    newSkill.AddComponent<FlashStab>().Init(player, groupID);
                    break;
                case SkillType.Water_Wave:
                    newSkill.AddComponent<WaterWave>().Init(player, groupID);
                    break;
                case SkillType.Wisp_Flame:
                    newSkill.AddComponent<WispFlame>().Init(player, groupID);
                    break;
                case SkillType.Wisp_Ice:
                    newSkill.AddComponent<WispIce>().Init(player, groupID);
                    break;
                case SkillType.Rock_Guard:
                    newSkill.AddComponent<RockGuard>().Init(player, groupID);
                    break;
                case SkillType.Lightning_Ball:
                    newSkill.AddComponent<LightningBall>().Init(player, groupID);
                    break;
                case SkillType.Wind_Swallow:
                    newSkill.AddComponent<WindSwallow>().Init(player, groupID);
                    break;
                case SkillType.Lightning_Cloud:
                    newSkill.AddComponent<LightningCloud>().Init(player, groupID);
                    break;
                case SkillType.ThornVine:
                    newSkill.AddComponent<ThornVine>().Init(player, groupID);
                    break;
                
                case SkillType.IgnisFatuus:
                    newSkill.AddComponent<IgnisFatuus>().Init(player, groupID);
                    break;
                
                case SkillType.LeafBlow:
                    newSkill.AddComponent<LeafBlow>().Init(player, groupID);
                    break;
                case SkillType.IncHp:
                    newSkill.AddComponent<HpPassive>().Init(player, groupID);
                    break;
                case SkillType.IncMoveSpeed:
                    newSkill.AddComponent<MovePassive>().Init(player, groupID);
                    break;
                case SkillType.IncBulletSpeed:
                    newSkill.AddComponent<ProjectileSpeedPassive>().Init(player, groupID);
                    break;
                case SkillType.IncExp:
                    newSkill.AddComponent<ExpPassive>().Init(player, groupID);
                    break;
                case SkillType.IncDamage:
                    newSkill.AddComponent<DamagePassive>().Init(player, groupID);
                    break;
                case SkillType.IncPickUp:
                    newSkill.AddComponent<PickUpRangePassive>().Init(player, groupID);
                    break;
                case SkillType.IncHpRecovery:
                    newSkill.AddComponent<HpRecoveryPassive>().Init(player, groupID);
                    break;
                // case SkillType.IncCooltime:
                //     newSkill.AddComponent<CooltimePassive>().Init(player, groupID);
                //     break;
                case SkillType.IncScale:
                    newSkill.AddComponent<ScalePassive>().Init(player, groupID);
                    break;
                // case SkillType.IncBossDamage:
                //     newSkill.AddComponent<BossDamagePassive>().Init(player, groupID);
                //     break;
                case SkillType.IncDmgReduce:
                    newSkill.AddComponent<DamageReducePassive>().Init(player, groupID);
                    break;
                case SkillType.IncCriticalRate:
                    newSkill.AddComponent<CriticalRatePassive>().Init(player, groupID);
                    break;
                case SkillType.Charm_Roll_Master:
                    newSkill.AddComponent<CharmRollMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.SwordStrength_Master:
                    newSkill.AddComponent<SwordStrengthMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Fanning_Master:
                    newSkill.AddComponent<FanningMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Multiple_Shot_Master:
                    newSkill.AddComponent<MultipleShotMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Flash_Stab_Master:
                    newSkill.AddComponent<FlashStabMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Water_Wave_Master:
                    newSkill.AddComponent<WaterWaveMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Wisp_Master:
                    newSkill.AddComponent<WispMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Rock_Guard_Master:
                    newSkill.AddComponent<RockGuardMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Lightning_Ball_Master:
                    newSkill.AddComponent<LightningBallMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Wind_Swallow_Master:
                    newSkill.AddComponent<WindSwallowMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Lightning_Cloud_Master:
                    newSkill.AddComponent<LightningCloudMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Thorn_Vine_Master:
                    newSkill.AddComponent<ThornVineMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Ignis_Fatuus_Master:
                    newSkill.AddComponent<IgnisFatuusMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.Leaf_Blow_Master:
                    newSkill.AddComponent<LeafBlowMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.SkullDonuts:
                    newSkill.AddComponent<SkullDonuts>().Init(player, groupID);
                    break;
                case SkillType.Judgment:
                    newSkill.AddComponent<Judgment>().Init(player, groupID);
                    break;
                case SkillType.FiveStars:
                    newSkill.AddComponent<FiveStars>().Init(player, groupID);
                    break;
                case SkillType.Shuriken:
                    newSkill.AddComponent<Shuriken>().Init(player, groupID);
                    break;
                case SkillType.LightningStep:
                    newSkill.AddComponent<LightningStep>().Init(player, groupID);
                    break;
                case SkillType.SkullDonutsMaster:
                    newSkill.AddComponent<SkullDonutsMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.JudgmentMaster:
                    newSkill.AddComponent<JudgmentMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.FiveStarsMaster:
                    newSkill.AddComponent<FiveStarsMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.ShurikenMaster:
                    newSkill.AddComponent<ShurikenMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                case SkillType.LightningStepMaster:
                    newSkill.AddComponent<LightningStepMaster>().Init(player, groupID);
                    CombinationSkillInit(groupID, newSkill.GetComponent<ActiveSkill>());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(groupID), groupID, null);
            }

            var skillbase = newSkill.GetComponent<SkillBase>();
            skillbase.SetLevel(level);
            Slots.Add(skillbase);
            BlackBoard.Instance.SetSkillData("player", skillbase.idx, skillbase.level);
            
            // TODO 타입으로 판별?
            var data = TableDataManager.Instance.GetSkillAiData(groupID, level);
            if (data.SkillGroup == 4 || data.SkillGroup == 11)
            {
                PassiveSlots.Add(newSkill.GetComponent<PassiveSkill>());
            }
            else
            {
                ActiveSlots.Add(newSkill.GetComponent<ActiveSkill>());
            }
        }

        public void PassiveUpdate()
        {
            var active = Slots.OfType<ActiveSkill>().ToList();
            
            foreach (var skill in active)
            {
                skill.DataUpdate();
            }
        }
        
        public List<int> RandomLevelUpSkill()
        {
            // 스킬 IDX 변경
            List<int> skillIdx = new List<int>();
            //var skilltb = TableDataManager.Instance.data.SkillAI;
            // activeSkillList = skilltb.Where(t => t.SkillGroup == 3 && t.Level > 0).Select(t => t.GroupID).Distinct().ToList();
            // passiveSkillList = skilltb.Where(t => t.SkillGroup == 4).Select(t => t.GroupID).Distinct().ToList();
            var weaponID = BlackBoard.Instance.GetSkillSet().ChangeSkillID;
            var masterIdx = new List<int>();            // 마스터 스킬 그룹
            // var traitActiveSkillList = TraitSkills(10);
            // var traitPassiveSkillList = TraitSkills(11);
            activeSkillList.AddRange(unlockActiveSkillList);
            passiveSkillList.AddRange(unlockPassiveSkillList);
            // 특성스킬 추가
            if (BattleManager.Instance.level.Value == 2)    // TODO 최초 레벨업에는 공격 스킬만 나온다 나중에 변경할 수 있음
            {
                skillIdx.AddRange(activeSkillList);         // 액티브
                skillIdx.Add(weaponID);                     // 무기스킬
            }
            else
            {
                skillIdx.AddRange(activeSkillList);         // 액티브
                skillIdx.AddRange(passiveSkillList);        // 패시브
                skillIdx.Add(weaponID);                     // 무기스킬
                if(PlayerManager.Instance.GetHeroList().Count>0)
                    skillIdx.Add(401);                  // 영웅 스킬 레벨업 401
                
                // 마스터 스킬 조건체크 로직
                foreach (var active in ActiveSlots)
                {
                    masterIdx.AddRange(CombinationSkill(active));
                }
                
                skillIdx.AddRange(masterIdx);
            }
            
            // 마스터 무기 스킬 제외로직
            skillIdx = skillIdx.Except(removeSkillList).ToList();
            
            // TODO 미작업 스킬 제외로직
            var UnworkedSkill = new List<int>() { 208}; //, 206, 207, 208, 209, 210 };
            skillIdx = skillIdx.Except(UnworkedSkill).ToList();
            

            // 이미배운 스킬레벨 5 제외
            var skillLvMax = Slots.Where(t => t.level == 5).Select(t => t.idx).ToList();
            skillIdx = skillIdx.Except(skillLvMax).ToList();
            
            if (heroLevel.Value == 5)
            {
                // 영웅스킬 5레벨 
                skillIdx.Remove(401);
            }
            
            if (ActiveSlots.Count >= 5) // 액티브 슬롯이 가득 찼다면 미획득 액티브 스킬들 제외
            {
                var maxCountActive = activeSkillList.Where(t => Slots.FindIndex(x=> x.idx == t) == -1).ToList();
                skillIdx = skillIdx.Except(maxCountActive).ToList();
            }
            
            if (PassiveSlots.Count >= 5) // 패시브 슬롯이 가득 찼다면 미획득 패시브 스킬들 제외
            {
                var maxCountPassive = passiveSkillList.Where(t => Slots.FindIndex(x=> x.idx == t) == -1).ToList();
                skillIdx = skillIdx.Except(maxCountPassive).ToList();
            }

            var numberOfRandomSelections = skillIdx.Count; // 남은 스킬 만큼   
            if (skillIdx.Count > 3) // 최대 3개 까지 뽑자
            {
                numberOfRandomSelections = 3;
            }
            else if(skillIdx.Count == 0)
            {
                numberOfRandomSelections = 1;
                skillIdx.Add(402); // HP 회복스킬
            }
            
            
            var retSkill = skillIdx
                .OrderBy(i => !masterIdx.Contains(i))
                .ThenBy(i => Random.value)
                .Take(numberOfRandomSelections)
                .ToList();

            return retSkill;
        }
        
        // 정해진 스킬리스트
        public List<int> RandomLevelUpSkill(List<int> stated)
        {
            // 스킬 IDX 변경
            List<int> skillIdx = new List<int>();
            var skilltb = TableDataManager.Instance.data.SkillAI;
            var activeSkillList = skilltb.Where(t => t.SkillGroup == 3 && t.Level > 0).Select(t => t.GroupID).Distinct().ToList();
            var passiveSkillList = skilltb.Where(t => t.SkillGroup == 4).Select(t => t.GroupID).Distinct().ToList();
            var weaponID = BlackBoard.Instance.GetSkillSet().ChangeSkillID;
            var masterIdx = new List<int>();            // 마스터 스킬 그룹
            
            activeSkillList.AddRange(unlockActiveSkillList);
            passiveSkillList.AddRange(unlockPassiveSkillList);
            // 특성스킬 추가
            {
                skillIdx.AddRange(activeSkillList);         // 액티브
                skillIdx.AddRange(passiveSkillList);        // 패시브
                skillIdx.Add(weaponID);                     // 무기스킬
                if(PlayerManager.Instance.GetHeroList().Count>0)
                    skillIdx.Add(401);                  // 영웅 스킬 레벨업 401
                
                // 마스터 스킬 조건체크 로직
                foreach (var active in ActiveSlots)
                {
                    masterIdx.AddRange(CombinationSkill(active));
                }
                
                skillIdx.AddRange(masterIdx);
            }
            
            // 마스터 무기 스킬 제외로직
            skillIdx = skillIdx.Except(removeSkillList).ToList();
            
            // TODO 미작업 스킬 제외로직
            var unworkedSkill = new List<int>() { 208}; //, 206, 207, 208, 209, 210 };
            unworkedSkill.AddRange(stated);                 // 확정적으로 나오는 명시된 스킬 제외
            skillIdx = skillIdx.Except(unworkedSkill).ToList();
            

            // 이미배운 스킬레벨 5 제외
            var skillLvMax = Slots.Where(t => t.level == 5).Select(t => t.idx).ToList();
            skillIdx = skillIdx.Except(skillLvMax).ToList();
            
            if (heroLevel.Value == 5)
            {
                // 영웅스킬 5레벨 s
                skillIdx.Remove(401);
            }
            
            if (ActiveSlots.Count >= 5) // 액티브 슬롯이 가득 찼다면 미획득 액티브 스킬들 제외
            {
                var maxCountActive = activeSkillList.Where(t => Slots.FindIndex(x=> x.idx == t) == -1).ToList();
                skillIdx = skillIdx.Except(maxCountActive).ToList();
            }
            
            if (PassiveSlots.Count >= 5) // 패시브 슬롯이 가득 찼다면 미획득 패시브 스킬들 제외
            {
                var maxCountPassive = passiveSkillList.Where(t => Slots.FindIndex(x=> x.idx == t) == -1).ToList();
                skillIdx = skillIdx.Except(maxCountPassive).ToList();
            }

            var numberOfRandomSelections = skillIdx.Count; // 남은 스킬 만큼
            var statedCount = stated.Count;
            if (skillIdx.Count > 3) // 최대 3개 까지 뽑자
            {
                numberOfRandomSelections = 3 - statedCount;
            }
            else if(skillIdx.Count == 0)
            {
                numberOfRandomSelections = 1;
                skillIdx.Add(402); // HP 회복스킬
            }
            
            
            var retSkill = skillIdx
                .OrderBy(i => !masterIdx.Contains(i))
                .ThenBy(i => Random.value)
                .Take(numberOfRandomSelections)
                .ToList();
            
            stated.AddRange(retSkill);
            return stated;
        }

        public List<int> BonusSkillUp(List<int> levelUpSkills)
        {
            List<int> newlv = new List<int>(){1,1,1};
            List<int> index = new List<int>();
            if (MyMath.RandomPer(10000, bonusRate))
            {
                // 스킬데이터 확인.
                // 타입, 레벨 확인.
                for (int i = 0; i < levelUpSkills.Count; i++)
                {
                    var skillInfo = TableDataManager.Instance.GetSkillAiData(levelUpSkills[i], 1);
                    if (skillInfo.SkillGroup == 6 || skillInfo.SkillGroup == 7 || skillInfo.SkillGroup == 8)
                    {
                        newlv[i] = 1;
                    }
                    else
                    {
                        var info = GetSkillInfo(levelUpSkills[i]);
                        if(info?.level > 3) 
                            newlv[i] = 1;
                        else
                        {
                            index.Add(i);
                        }
                    }
                }

                if(index.Count > 0)
                    newlv[index.GetRandomItem()] = 2;
            }
            
            return newlv;
        }
        
        List<int> TraitSkills(int skillGroup)
        {
            // 10 , 11
            var traitTypes = TableDataManager.Instance.data.TraitType.Where(t => t.OptionType == 18).ToList();
            var traits = TableDataManager.Instance.data.Trait.Where(t => traitTypes.Exists(x=> x.Type == t.Type)).ToList();
            var ret = new List<int>();
            
            foreach (var trait in traits)
            {
                var data = UserDataManager.Instance.userTraitInfo.GetTraitValue(trait.Index);
                if(data > 0)
                {
                    var skillAiData = TableDataManager.Instance.GetSkillAiData(data, 1);
                    if(skillAiData.SkillGroup == skillGroup)
                        ret.Add(data);
                }
            }

            return ret;
        }

        // 마스터스킬 재료스킬들은 레벨업시 등장하지 않음
        // 액티브 + 패시브 , 액티브 + 액티브
        private List<int> CombinationSkill(SkillBase active)
        {
            List<int> masterSkillIdx = new List<int>();
            //Slots
            var tb = TableDataManager.Instance.data.MasterSkillCombination;
            var combinations = tb.Where(t => 
                t.EvolutionSkillGroupID == active.idx && t.EvolutionSkillLevel <= active.level)
                .ToList();
            //1 조건

            foreach (var t1 in combinations)
            {
                bool target = Slots.Exists(t =>
                    t.idx == t1.TargetCombinationSkillGroupID &&
                    t.level >= t1.TargetCombinationSkillLevel);

                // 조건2
                if (target)
                {
                    masterSkillIdx.Add(t1.MasterSkillGroupID);
                }
            }


            return masterSkillIdx;
        }

        public void CombinationSkillInit(int groupID, ActiveSkill masterSkill)
        {
            var tb = TableDataManager.Instance.data.MasterSkillCombination;
            var combination = tb.Single(t => t.MasterSkillGroupID == groupID);
            InfVal _accDamage = 0;
            //액티브 체크
            int evoIndex = ActiveSlots.FindIndex(t => t.idx == combination.EvolutionSkillGroupID);
            if (evoIndex > -1)
            {
                Slots.Remove(ActiveSlots[evoIndex]);
                ActiveSlots.RemoveAt(evoIndex);
                removeSkillList.Add(combination.EvolutionSkillGroupID);

                
                var _active = transformSkill.Find($"Skill {combination.EvolutionSkillGroupID}").GetComponent<ActiveSkill>();
                _accDamage += _active.accDamage;
                _active.Dispose();
                Destroy(_active.gameObject);
            }
            int targetIndex = ActiveSlots.FindIndex(t => t.idx == combination.TargetCombinationSkillGroupID);
            if (targetIndex > -1)
            {
                Slots.Remove(ActiveSlots[targetIndex]);
                ActiveSlots.RemoveAt(targetIndex);
                removeSkillList.Add(combination.TargetCombinationSkillGroupID);
                
                var _active = transformSkill.Find($"Skill {combination.TargetCombinationSkillGroupID}").GetComponent<ActiveSkill>();
                _accDamage += _active.accDamage;
                _active.Dispose();
                Destroy(_active.gameObject);
            }
            
            // 제거된 슬롯 데미지누적 가져오기.
            masterSkill.accDamage = _accDamage;
        }

        [Button]
        public void HeroSkillUp(int level)
        {
            heroLevel.Value += level;
        }

        void InitUnlockSkill()
        {
            unlockActiveSkillList = new List<int>();
            unlockPassiveSkillList = new List<int>();
            
            if(UserDataManager.Instance.clientInfo.GetUnlockData(3))
            {
                unlockActiveSkillList.Add(111);
            }
            
            if(UserDataManager.Instance.clientInfo.GetUnlockData(4))
            {
                unlockActiveSkillList.Add(110);
            }
            
            if(UserDataManager.Instance.clientInfo.GetUnlockData(5))
            {
                unlockPassiveSkillList.Add(211);
            }
        }

        public void CalcCoolTime()
        {
            double _Cooltime = (100 - incValue.Cooltime) / 100f;

            double optipn = 1;
            // 옵션값 0 일때 예외처리 추가
            if (UserDataManager.Instance.gearInfo.GetEquipGearsPower().CharAttackSpeed > 0)
            {
                // optipn = (10000 - (double)UserDataManager.Instance.gearInfo.GetEquipGearsPower().CharAttackSpeed) / 10000f;
                optipn = UserDataManager.Instance.gearInfo.GetEquipGearsPower().CharAttackSpeed;
            }
            
            calcCoolTime = (float)(1 - (_Cooltime * optipn)) * 100f;
            // calcCoolTime = 100 - (_Cooltime * UserDataManager.Instance.gearInfo.GetEquipGearsPower().CharAttackSpeed) * 100f;
            // else
            // {
            //     calcCoolTime = 100 - (_Cooltime) * 100f;
            // }
        }
        
        public void CalcDmgReduce()
        {
            // var traitTypes = TableDataManager.Instance.data.TraitType.Where(t => t.OptionType == 16).ToList();
            // var traits = TableDataManager.Instance.data.Trait.Where(t => traitTypes.Exists(x=> x.Type == t.Type)).ToList();
            // List<float> val = new List<float>();
            //
            // foreach (var trait in traits)
            // {
            //     var data = UserDataManager.Instance.userTraitInfo.GetTraitValue(trait.Index);
            //     if(data > 0)
            //     {
            //         val.Add((10000 - data) / 10000f);
            //     }
            // }
            //
            var skillValue = incValue.DamageReduce * 100f;
            skillValue = skillValue > 0 ? (10000 - skillValue) / 10000f : 1;
            //
            // var traitValue = 1f;
            // val.ForEach(i => { traitValue *= i; });

            
            var decreaseDmg = UserDataManager.Instance.gearInfo.GetEquipGearsPower().DecreaseDmg;
            decreaseDmg = decreaseDmg > 0 ? decreaseDmg : 1;
            calcDmgReduce = (1 - (skillValue * decreaseDmg)) * 100f;
        }
        
        public void CalcMiss()
        {
            var traitTypes = TableDataManager.Instance.data.TraitType.Where(t => t.OptionType == 17).ToList();
            var traits = TableDataManager.Instance.data.Trait.Where(t => traitTypes.Exists(x=> x.Type == t.Type)).ToList();
            int val = 0;

            foreach (var trait in traits)
            {
                var data = UserDataManager.Instance.userTraitInfo.GetTraitValue(trait.Index);
                if(data > 0)
                {
                    val += data;
                }
            }

            calcMiss = val;
        }
        
        public void CalcCommon()
        {
            var traitTypes = TableDataManager.Instance.data.TraitType.Where(t => t.OptionType == 15).ToList();
            var traits = TableDataManager.Instance.data.Trait.Where(t => traitTypes.Exists(x=> x.Type == t.Type)).ToList();
            int val = 0;

            foreach (var trait in traits)
            {
                var data = UserDataManager.Instance.userTraitInfo.GetTraitValue(trait.Index);
                if(data > 0)
                {
                    val += data;
                }
            }

            common = val / 100f;
        }

        public float GetCoolTime(float cooltime)
        {
            return MyMath.Decrease(cooltime, calcCoolTime);
        }
    }

    [Serializable]
    public struct IncValue
    {
        public int Hp;
        public int MoveSpeed;
        public int BulletSpeed;
        public int Exp;
        public int damage;
        public int PickUpRange;
        public int HpRecovery;
        public int Cooltime;
        public int Scale;
        public int BossDamgage;
        public int CriticalRate;
        public int DamageReduce;



    }

}
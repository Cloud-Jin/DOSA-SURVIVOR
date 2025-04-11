using System;

[Serializable]
public class DataModel
{
    public AccountLevel[] AccountLevel;
    public ADBuffType[] ADBuffType;
    public ADBuffInfo[] ADBuffInfo;
    public AttendanceInfo[] AttendanceInfo;
    public AttendanceGroup[] AttendanceGroup;
    public AutoBattleConfig[] AutoBattleConfig;
    public HeroSummonSetting[] HeroSummonSetting;
    public TreasureBox[] TreasureBox;
    public StageRewardGroup[] StageRewardGroup;
    public StageReward[] StageReward;
    public AutoBattleSkillAI[] AutoBattleSkillAI;
    public BossMonsterGroup[] BossMonsterGroup;
    public BuffConfig[] BuffConfig;
    public Buff[] Buff;
    public CardConfig[] CardConfig;
    public CardReinforce[] CardReinforce;
    public CardRarityType[] CardRarityType;
    public Card[] Card;
    public CardType[] CardType;
    public CardOptionGroup[] CardOptionGroup;
    public CardOptionLevel[] CardOptionLevel;
    public CardAwakening[] CardAwakening;
    public ChallengeEffectGroup[] ChallengeEffectGroup;
    public ChallengePenaltyType[] ChallengePenaltyType;
    public Config[] Config;
    public GDPR[] GDPR;
    public Terms[] Terms;
    public CostumeRarityType[] CostumeRarityType;
    public Costume[] Costume;
    public DungeonConfig[] DungeonConfig;
    public Dungeon[] Dungeon;
    public DungeonLevel[] DungeonLevel;
    public EquipConfig[] EquipConfig;
    public RarityType[] RarityType;
    public EquipType[] EquipType;
    public Equipment[] Equipment;
    public OptionType[] OptionType;
    public OptionRatio[] OptionRatio;
    public OptionEffect[] OptionEffect;
    public Option[] Option;
    public ReinforceTotalLevel[] ReinforceTotalLevel;
    public Breakthrough[] Breakthrough;
    public ExploreConfig[] ExploreConfig;
    public CombatLevel[] CombatLevel;
    public ExploreStage[] ExploreStage;
    public ExploreChapter[] ExploreChapter;
    public ExploreMonster[] ExploreMonster;
    public ExploreBg[] ExploreBg;
    public GuardianConfig[] GuardianConfig;
    public OrbType[] OrbType;
    public GuardianType[] GuardianType;
    public ResearchLevel[] ResearchLevel;
    public GuardianLevel[] GuardianLevel;
    public Dialog[] Dialog;
    public GoodsType[] GoodsType;
    public CardItemType[] CardItemType;
    public BattleItemType[] BattleItemType;
    public Mission[] Mission;
    public Achievement[] Achievement;
    public MissionCondition[] MissionCondition;
    public MissionType[] MissionType;
    public Monster[] Monster;
    public PatternGroup[] PatternGroup;
    public BuffGroup[] BuffGroup;
    public PayTab[] PayTab;
    public PayInfo[] PayInfo;
    public SpecialPackage[] SpecialPackage;
    public PayMembershipReward[] PayMembershipReward;
    public PayMembershipBenefit[] PayMembershipBenefit;
    public Portrait[] Portrait;
    public ProfileInfo[] ProfileInfo;
    public Title[] Title;
    public PushInfo[] PushInfo;
    public RandomBoxConfig[] RandomBoxConfig;
    public RandomBoxType[] RandomBoxType;
    public RandomBoxInfo[] RandomBoxInfo;
    public RandomBoxPayType[] RandomBoxPayType;
    public RandomBoxRewardInfo[] RandomBoxRewardInfo;
    public RandomBoxReward[] RandomBoxReward;
    public RewardGroup[] RewardGroup;
    public RewardPack[] RewardPack;
    public SkillAI[] SkillAI;
    public SkillSet[] SkillSet;
    public MasterSkillCombination[] MasterSkillCombination;
    public SkillTypeGroup[] SkillTypeGroup;
    public StageConfig[] StageConfig;
    public ChapterType[] ChapterType;
    public MapType[] MapType;
    public Stage[] Stage;
    public NormalMonsterGroup[] NormalMonsterGroup;
    public StageSpawnList[] StageSpawnList;
    public StageMonsterExpRange[] StageMonsterExpRange;
    public StageMonsterExpPieceGroup[] StageMonsterExpPieceGroup;
    public StageCharacterLevel[] StageCharacterLevel;
    public CrackCardGroup[] CrackCardGroup;
    public BattleItem[] BattleItem;
    public TraitConfig[] TraitConfig;
    public Trait[] Trait;
    public TraitTree[] TraitTree;
    public TraitLevel[] TraitLevel;
    public TraitType[] TraitType;
    public TutorialType[] TutorialType;
    public Tutorial[] Tutorial;
    public UnlockType[] UnlockType;
    public UnlockCondition[] UnlockCondition;
}

[Serializable]
public class AccountLevel
{
    public int Level;
    public int Exp;
    public int RewardGroup;
}

[Serializable]
public class ADBuffType
{
    public int Index;
    public string ADBuffIcon;
    public string ADBuffName;
    public int ADBuffOrder;
    public int OptionType;
}

[Serializable]
public class ADBuffInfo
{
    public int Index;
    public int BuffID;
    public int BuffLevel;
    public int OptionValue;
    public int EffectTime;
    public int NeedLevelCount;
}

[Serializable]
public class AttendanceInfo
{
    public int Index;
    public int Position;
    public int Condition;
    public string Title;
    public string Image;
    public int MaxDays;
    public int AttendanceGroup;
    public int NextID;
    public string StartEventTime;
    public string EndEventTime;
}

[Serializable]
public class AttendanceGroup
{
    public int Index;
    public int GroupID;
    public int Day;
    public int RewardGroupID;
}

[Serializable]
public class AutoBattleConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class HeroSummonSetting
{
    public int ArtifactRarity;
    public int HeroSummonRatio;
}

[Serializable]
public class TreasureBox
{
    public int Index;
    public int RewardCollectTime;
    public string TreasureBoxAni;
}

[Serializable]
public class StageRewardGroup
{
    public int GroupID;
    public int Index;
}

[Serializable]
public class StageReward
{
    public int Index;
    public string StageRewardIcon;
    public int StageRewardRarity;
    public string StageRewardName;
    public string StageRewardDescription;
    public string StageRewardRarityColor;
}

[Serializable]
public class AutoBattleSkillAI
{
    public int Index;
    public int TypeID;
    public string SkillAnimPrefab;
    public string SkilResourcePrefab;
    public string Pivot;
    public int RotationAble;
    public int DamageRatio;
    public int Speed;
    public int Range;
}

[Serializable]
public class BossMonsterGroup
{
    public int GroupID;
    public int Order;
    public int MonsterID;
    public int SpawnType;
    public int SpawnKillCount;
    public int MonsterHPIncrease;
    public int MonsterAttackIncrease;
    public int RewardGroupID;
}

[Serializable]
public class BuffConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class Buff
{
    public int Idx;
    public string Name;
    public int Category;
    public int Type;
    public int SubType;
    public int Level;
    public int DurationTime;
    public int DamageTime;
    public int TargetMonsterType;
    public string Resource;
}

[Serializable]
public class CardConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class CardReinforce
{
    public int LevelSection;
    public int ReinforceMaterialCoefficient;
}

[Serializable]
public class CardRarityType
{
    public int Index;
    public string RarityColor;
    public string RankingRarityColor;
    public string CardBackImage;
    public string CardFrameImage;
    public int CardAwakeningGroupID;
}

[Serializable]
public class Card
{
    public int Index;
    public string CardName;
    public string CardIcon;
    public int CardRarity;
    public int CardType;
    public int BasicAtk;
    public int BasicHP;
    public int OptionGroupID;
    public int EquipSkillType;
    public int TargetSkillSetID;
    public int HeroCharacterID;
    public string HeroCharacterIcon;
    public string HeroCharacterBigIcon;
    public string HeroIdleAnim;
}

[Serializable]
public class CardType
{
    public int Index;
    public string CardTypeIcon;
    public int ReinforceMaterialD;
    public int ReinforceMaterialValue;
    public int ReinforcePriceID;
    public int ReinforcePriceValue;
    public int AwakeningMaterialID;
}

[Serializable]
public class CardOptionGroup
{
    public int GroupID;
    public int AddOptionType;
    public int AddOptionLevelGroupID;
}

[Serializable]
public class CardOptionLevel
{
    public int GroupID;
    public int Level;
    public int Value;
}

[Serializable]
public class CardAwakening
{
    public int GroupID;
    public int Level;
    public int CardConsumeCount;
    public int AwakeningEnhanceRatio;
    public int ReinforceMaxLevel;
}

[Serializable]
public class ChallengeEffectGroup
{
    public int GroupID;
    public int TypeID;
    public int Value;
}

[Serializable]
public class ChallengePenaltyType
{
    public int Type;
    public int ValueType;
    public string Description;
}

[Serializable]
public class Config
{
    public int Index;
    public int Value;
    public string Text;
}

[Serializable]
public class GDPR
{
    public int Idx;
    public string CountryCode;
}

[Serializable]
public class Terms
{
    public int Idx;
    public string Link;
    public int Version;
}

[Serializable]
public class CostumeRarityType
{
    public int Index;
    public int Grade;
    public string IconBackground;
}

[Serializable]
public class Costume
{
    public int Index;
    public int Type;
    public int Grade;
    public string Name;
    public string Resource;
    public string Icon;
    public int Obb;
    public int RetentionEffectID;
    public int TargetSkillSetID;
    public string Description;
    public int TargetEquipType;
    public string ChangeEquipIcon;
}

[Serializable]
public class DungeonConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class Dungeon
{
    public int Index;
    public int Order;
    public string Name;
    public string Image;
    public int CountResetType;
    public int EnterCount;
    public int AddEnterCount;
    public int AddEnterConsumeType;
    public int AddEnterConsumeCount;
    public int SweepCount;
    public int SweepConsumeType;
    public int SweepConsumeCount;
    public string DungeonDescription;
    public string SweepDescription;
}

[Serializable]
public class DungeonLevel
{
    public int DungeonIndex;
    public int Level;
    public int RewardType;
    public int Reward;
    public int RewardBox;
    public int FirstClearRewardType;
    public int FirstClearReward;
    public int FirstClearRewardBox;
    public string MonsterEnhanceRatio;
    public int ClearMinATK;
}

[Serializable]
public class EquipConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class RarityType
{
    public int Index;
    public int ReinforceMaxLevel;
    public int ChangeOptionCost;
    public string RarityColor;
    public string RarityTextColor;
    public string RarityTextKey;
}

[Serializable]
public class EquipType
{
    public int Index;
    public int Slot;
    public string Name;
}

[Serializable]
public class Equipment
{
    public int Index;
    public int EquipType;
    public int JobType;
    public string EquipName;
    public string EquipIcon;
    public int EquipRarity;
    public int EquipGrade;
    public int EquipAtk;
    public int EquipHP;
    public int SkillID;
    public int AddOptionCount;
    public int AddOptionEffectID;
    public int AddOptionRatioGroupID;
    public int MergeMaterialValue;
    public int ReinforceBaseCost;
    public int BreakthroughGroupID;
}

[Serializable]
public class OptionType
{
    public int AddOptionType;
    public string AddOptionName;
    public string AddOptionValueName;
    public int AddOptionValueType;
}

[Serializable]
public class OptionRatio
{
    public int GroupID;
    public int AddOptionGrade;
    public int AddOptionRatio;
    public int AddOptionGroupID;
    public string AddOptionGradeTextColor;
    public string AddOptionGradeName;
}

[Serializable]
public class OptionEffect
{
    public int Index;
    public int AddOptionType;
    public int AddOptionEffectValue;
}

[Serializable]
public class Option
{
    public int GroupID;
    public int AddOptionType;
    public int AddOptionValue;
}

[Serializable]
public class ReinforceTotalLevel
{
    public int Index;
    public int NeedReinforceLevel;
    public int IncValue;
}

[Serializable]
public class Breakthrough
{
    public int Index;
    public int BreakthroughMainGroupID;
    public int BreakthroughLevel;
    public int MaterialEquipID;
    public int MaterialEquipValue;
}

[Serializable]
public class ExploreConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class CombatLevel
{
    public int Level;
    public string NeedLevelCount;
    public int StageGroupID;
}

[Serializable]
public class ExploreStage
{
    public int GroupID;
    public int Stage;
    public int SkipStage;
}

[Serializable]
public class ExploreChapter
{
    public int Index;
    public int ChapterGroupID;
    public int RewardID;
    public int MonsterGroupID;
}

[Serializable]
public class ExploreMonster
{
    public int GroupID;
    public int Order;
    public int MonsterID;
    public int BossNoti;
}

[Serializable]
public class ExploreBg
{
    public int Index;
    public string BgResource;
}

[Serializable]
public class GuardianConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class OrbType
{
    public int Type;
    public string Name;
    public int OptionType;
    public int GuardianGroupID;
    public string OptionValueDesc;
}

[Serializable]
public class GuardianType
{
    public int GroupID;
    public int ResearchLevel;
    public string Icon;
    public int OptionRatio;
    public int FailDialogGroupID;
}

[Serializable]
public class ResearchLevel
{
    public int Index;
    public int MaxLevel;
    public int Time;
    public string ResearchCost;
}

[Serializable]
public class GuardianLevel
{
    public int Index;
    public int Rate;
    public int Cost;
}

[Serializable]
public class Dialog
{
    public int GroupID;
    public string ReinforceDialog;
}

[Serializable]
public class GoodsType
{
    public int TypeID;
    public string Name;
    public string Icon;
    public string IconBackground;
}

[Serializable]
public class CardItemType
{
    public int TypeID;
    public string Name;
}

[Serializable]
public class BattleItemType
{
    public int TypeID;
    public string Name;
}

[Serializable]
public class Mission
{
    public int Index;
    public int MissionType;
    public int Order;
    public int ConditionID;
    public int RewardGroupID;
}

[Serializable]
public class Achievement
{
    public int Index;
    public int Group;
    public int Level;
    public int ConditionID;
    public int RewardGroupID;
}

[Serializable]
public class MissionCondition
{
    public int Index;
    public int ConditionType;
    public int Parameter1;
    public int Parameter2;
    public int Parameter3;
    public string ConditionCount;
}

[Serializable]
public class MissionType
{
    public int ConditionType;
    public string Description;
    public int LinkType;
    public int LinkTargetID;
}

[Serializable]
public class Monster
{
    public int Index;
    public int Type;
    public int Grade;
    public string Name;
    public string Resource;
    public int Obb;
    public int HP;
    public int Attack;
    public int Speed;
    public int CriticalRatio;
    public int KnockBackResistance;
    public int PatternGroupID;
    public int BuffGroupID;
    public int Exp;
    public int RespawnCoolTime;
    public int EliteScale;
    public string BossSpawnTalk;
    public string BossDeadTalk;
}

[Serializable]
public class PatternGroup
{
    public int GroupID;
    public int SkillGroupID;
}

[Serializable]
public class BuffGroup
{
    public int GroupID;
    public int BuffID;
    public int BuffValue;
}

[Serializable]
public class PayTab
{
    public int Index;
    public string TabName;
    public int Order;
    public int DisplayType;
}

[Serializable]
public class PayInfo
{
    public int Index;
    public string ProductionName;
    public string ProductionID;
    public string Icon;
    public int TabType;
    public int SubType;
    public string ProductionBg;
    public int Order;
    public int PriceType;
    public int PriceID;
    public int Price;
    public int DiscountPrice;
    public int DiscountRate;
    public int ProductionMultiple;
    public int NewTag;
    public int RewardType;
    public int RewardGroupID;
    public int PurchaseLimitType;
    public int PurchaseLimitCount;
    public int PurchaseConditionType;
    public int DisplayConditionType;
    public int DisplayConditionValue;
    public int ViewDetail;
    public string PurchaseStartTime;
    public string PurchaseEndTime;
    public int FirstPurchaseGroupID;
    public string MailTitle;
}

[Serializable]
public class SpecialPackage
{
    public int Index;
    public string Title;
    public string Description;
    public string VideoName;
    public string SkillName;
}

[Serializable]
public class PayMembershipReward
{
    public int Index;
    public int RewardGroupID;
    public int DailyRewardGroupID;
    public int BenefitGroupID;
    public int BenefitPeriod;
    public string MailTitle;
    public string MailDescription;
}

[Serializable]
public class PayMembershipBenefit
{
    public int GroupID;
    public int Order;
    public int BenefitType;
    public int BenefitValue;
    public string BenefitDescription;
}

[Serializable]
public class Portrait
{
    public int Index;
    public string Name;
    public string Icon;
    public string Description;
}

[Serializable]
public class ProfileInfo
{
    public int Index;
    public string Name;
    public string BgColor;
    public int Notation;
}

[Serializable]
public class Title
{
    public int Index;
    public string Name;
    public string Description;
}

[Serializable]
public class PushInfo
{
    public int Index;
    public int Type;
    public int TypeValue;
    public string PushTitle;
    public string PushDescription;
}

[Serializable]
public class RandomBoxConfig
{
    public int Index;
    public string Value;
}

[Serializable]
public class RandomBoxType
{
    public int Index;
    public string BoxIconName;
    public string BoxName;
    public int BoxOrder;
    public string StartEventTime;
    public string EndEventTime;
    public int RewardPackID;
    public string RewardDesc;
    public string LVRewardMailTitle;
    public string BoxNameMissionTitle;
}

[Serializable]
public class RandomBoxInfo
{
    public int Index;
    public int BoxID;
    public int BoxLevel;
    public int PayTypeID;
    public int RewardGroupID;
    public int NeedLevelCount;
    public int LevelRewardGroupID;
}

[Serializable]
public class RandomBoxPayType
{
    public int Index;
    public int RepeatType;
    public int BoxCount;
    public int PriceType;
    public int PriceID;
    public int Price;
}

[Serializable]
public class RandomBoxRewardInfo
{
    public int GroupID;
    public int RewardPayType;
    public int RewardPackID;
    public int RewardRarity;
    public int RewardRatio;
    public int GradeRewardGroupID;
}

[Serializable]
public class RandomBoxReward
{
    public int GroupID;
    public int Grade;
    public int RewardRatio;
    public int RewardCount;
}

[Serializable]
public class RewardGroup
{
    public int Idx;
    public int GroupID;
    public int RewardPayType;
    public int Order;
    public int Type;
    public int RewardID;
    public int RewardMinCnt;
    public int RewardMaxCnt;
    public int RewardRatio;
}

[Serializable]
public class RewardPack
{
    public int RewardPackID;
    public int Type;
    public int ItemType;
}

[Serializable]
public class SkillAI
{
    public int Index;
    public string Name;
    public string Icon;
    public int GroupID;
    public int SkillGroup;
    public int Level;
    public string Description;
    public string Ani;
    public string ObjectResource;
    public string HitEffectResource;
    public string Pivot;
    public int ObjectValue;
    public int Type;
    public int TypeValue;
    public int Target;
    public int PriorityTarget;
    public int Count;
    public int CountTime;
    public int DamageRatio;
    public int DamegeTime;
    public int Range;
    public int Angle;
    public int CastingTime;
    public int Scale;
    public int Speed;
    public int DurationTime;
    public int BounceType;
    public int Bounce;
    public int Penetration;
    public int KnockBack;
    public int ProjectileBlockType;
    public int ProjectileBlockedType;
    public int AddExplosionDeBuffCheck;
    public int AddExplosionAble;
    public int AddExplosionTime;
    public int AddExplosionSkillId;
    public int BuffTarget;
    public int BuffID;
    public int BuffValue;
    public int BuffTime;
    public int BuffRatio;
    public int CoolTime;
    public int NextSkillId;
}

[Serializable]
public class SkillSet
{
    public int index;
    public int MainGroupID;
    public int TargetSkillID;
    public int ChangeSkillID;
    public int Order;
}

[Serializable]
public class MasterSkillCombination
{
    public int MasterSkillGroupID;
    public int EvolutionSkillGroupID;
    public int EvolutionSkillLevel;
    public int TargetCombinationSkillGroupID;
    public int TargetCombinationSkillLevel;
}

[Serializable]
public class SkillTypeGroup
{
    public int Type;
    public int SkillGroup;
}

[Serializable]
public class StageConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class ChapterType
{
    public int Index;
    public string ChapterName;
    public string BgResource;
    public string ChapterBgResource;
}

[Serializable]
public class MapType
{
    public int Index;
    public int Type;
    public int CameraZoom;
    public int WaveCameraZoom1st;
    public int WaveCameraZoom2nd;
    public int WaveCameraZoom3rd;
    public int MonsterLocationLoopRange;
    public int MonsterLocationLoopRange1st;
    public int MonsterLocationLoopRange2nd;
    public int MonsterLocationLoopRange3rd;
}

[Serializable]
public class Stage
{
    public int Index;
    public int StageType;
    public int ChapterID;
    public int StageLevel;
    public int NextStage;
    public string Name;
    public string Resource;
    public int ConsumeType;
    public int ConsumeCount;
    public int MapType;
    public int DayNightType;
    public int SpawnGroupID;
    public int StageTime;
    public int StageMinClearTime;
    public int BoosterStageMinClearTime;
    public int StageMinATK;
    public int NormalMonsterGroupID;
    public int BossMonsterGroupID;
    public string MonsterEnhanceRatio;
    public string DimensionHPRatio;
    public int RewardExp;
    public int RewardGold;
    public int FirstClearRewardGroupID;
    public int StageRewardIconGroupID;
    public int EquipRewardGroupID;
    public int EquipRewardCount;
    public int AddGoodsRewardGroupID;
    public int GoodsRewardGroupID;
    public int EliteMonsterRewardGroupID;
    public int CrackItemRewardGroupID;
    public int CrackCardGroupID;
    public int CrackCardLevel;
    public int DungeonIndex;
    public int ChallengeEffectGroupID;
}

[Serializable]
public class NormalMonsterGroup
{
    public int GroupID;
    public int MonsterID;
    public int WaveRow;
    public int WaveSpawnCount;
}

[Serializable]
public class StageSpawnList
{
    public int Index;
    public int GroupID;
    public int SpawnType;
    public int Order;
    public int SpawnKillCount;
    public int SpawnDelay;
    public int SpawnCount;
    public int MonsterHPIncrease;
    public int MonsterAttackIncrease;
    public int MonsterExpMultiple;
}

[Serializable]
public class StageMonsterExpRange
{
    public int Index;
    public int ExpMinCount;
    public int ExpMaxCount;
    public int ExpPieceGroupID;
}

[Serializable]
public class StageMonsterExpPieceGroup
{
    public int GroupID;
    public int ExpPieceType;
    public int RewardRatio;
}

[Serializable]
public class StageCharacterLevel
{
    public int Level;
    public int Exp;
}

[Serializable]
public class CrackCardGroup
{
    public int GroupID;
    public int CardIndex;
    public int Ratio;
}

[Serializable]
public class BattleItem
{
    public int Idx;
    public int TypeID;
    public int Value;
    public string Icon;
    public string Resource;
}

[Serializable]
public class TraitConfig
{
    public int Index;
    public int Value;
}

[Serializable]
public class Trait
{
    public int Index;
    public int Tab;
    public int Row;
    public int Order;
    public int Type;
    public int TraitLevelGroupID;
    public int TraitTreeGroupID;
    public int ConsumePoint;
    public string SlotBackground;
}

[Serializable]
public class TraitTree
{
    public int GroupID;
    public int PrevTraitIndex;
}

[Serializable]
public class TraitLevel
{
    public int GroupID;
    public int Level;
    public int Value;
}

[Serializable]
public class TraitType
{
    public int Type;
    public string Icon;
    public string Description;
    public int OptionType;
}

[Serializable]
public class TutorialType
{
    public int Index;
    public int Type;
    public int ConditionType;
    public int ConditionValue;
    public int TutorialGroupID;
}

[Serializable]
public class Tutorial
{
    public int Index;
    public int GroupID;
    public int Order;
    public int TutorialClearSave;
    public string BalloonText;
    public int BalloonTextStandard;
    public int HighlightType;
    public int TouchType;
    public int DimmedAble;
}

[Serializable]
public class UnlockType
{
    public int Index;
    public int Order;
    public int Type;
    public int UnlockConditionID;
    public int UnlockPopupDisplay;
    public string Title;
    public string SubTitle;
    public string Description;
    public string Image;
    public int TutorialGroupID;
}

[Serializable]
public class UnlockCondition
{
    public int Index;
    public int ConditionType;
    public int ConditionValue;
    public string ConditionDescription;
}


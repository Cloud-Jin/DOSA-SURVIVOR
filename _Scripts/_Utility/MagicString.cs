public static class MyLayer
{
    public static readonly string Background = "Background";
    public static readonly string Item = "Item";
    public static readonly string Between = "Between";
    public static readonly string Enemy = "Enemy";
    public static readonly string Forground = "Forground";
    public static readonly string Player = "Player";
    public static readonly string VisibleParticle = "VisibleParticle";
}

public static class MyAtlas
{
   public static readonly string Common_Equipment = "Common_Equipment";
   public static readonly string Icon = "Icon";
   public static readonly string BattleIcon = "Battle_Icon";
   public static readonly string BattleSkill = "Battle_Skill";
   public static readonly string Summon_Shop = "Summon_Shop";
   public static readonly string Common_UISlot = "Common_UISlot";
   public static readonly string AutoBattle = "AutoBattle";
   public static readonly string Equipment_Frame = "Equipment_Frame";
   public static readonly string Common_Goods = "Common_Goods";
   public static readonly string Guardians = "Guardians";
   public static readonly string Card = "Card";
}

public static class MyPlayerPrefsKey
{
   public static readonly string IdToken = "IdToken";
   public static readonly string LoginPlatform = "LoginPlatform";
   public static readonly string PlayStage = "PlayStage";
   public static readonly string PlayDungeonGold = "PlayDungeonGold";
   public static readonly string DungeonGoldAutoSkill = "DungeonGoldAutoSkill";
   public static readonly string Review = "Review";
   public static readonly string Terms = "Terms";
   public static readonly string BlackBoard = "BlackBoard";
   public static readonly string AlarmAccept = "AlarmAccept";
   public static readonly string BattleBooster = "BattleBooster";
   public static readonly string GoldBattleBooster = "GoldBattleBooster";
   public static readonly string ChallengeBattleBooster = "ChallengeBattleBooster";
}




// 컨텐츠_분류
public enum ViewName 
{
   Lobby_Main,
   Battle_Top,
   Battle_Cheat,     // 배틀용 치트UI
   Common_Loading,
   
   
   
   
   
   Title_Main,
}

public enum PopupName
{
   Battle_LevelUp,
   Battle_Pause,
   Battle_ResultVictory,
   Battle_ResultDefeat,
   Battle_DimensionRift,
   Battle_Giveup,
   Battle_AlarmBoss,
   Battle_AlarmEnemyWave,
   Battle_BossName,
   Battle_GetHero,
   Battle_GetItem,
   Battle_Revive,
   Battle_Damage,
   Battle_SkillSellect,
   Battle_ResultDungeon,
   Dungeon_Select,
   DungeonClear,
   
   AutoBattle_ChapterSelect,
   AutoBattle_BattleAuto,
   AutoBattle_BattleAutoFast,
   
   Gear_Select,
   Gear_LimitBreak,
   Gear_Compose,
   Gear_GradeProbability,
   Gear_OptionAutoChange,
   Gear_Equip_Alram,
   Gear_Auto_Equip_Alram,
   Gear_All_Compose_Alram,
   
   Summon_Result,
   Summon_Detail,
   
   Lobby_AccountLevelUp,
   Lobby_Profile,
   Lobby_ProfileNick,
   // Lobby_Beta_BetaSurvey,
   Lobby_Terms,
   Lobby_Trait,
   
   Common_Reward,
   Common_Alarm,
   Common_Confirm,
   Common_Account,
   Common_AccountDelete,
   Common_Information,
   Common_Loading,
   Common_NewContent,
   
   Post_Box,
   Post_Box_Detail,
   
   Shop_Purchase,
   Shop_Purchase_Receive,
   
   Option_Setting,
   Lang_Select,
   
   Mission,
   ChargingDia,
   ChargingEnergy,
   Attendance,
   Costume,
   Boost,
   SpecialShop,
   SpecialShopDetail,
   Ranking,
   RankingUserInfo,
   HardDungeonSelect,
   HardModeInfo,
   Guardian,
   GuardianAccelerate,
   ExplorationStage,
   Card,
   
   Tutorial_Intro,
   Tutorial_Popup_Finger,
   Tutorial_PopupDialog,
}

public enum PopupType
{
   Popup,
   Alarm
}

// Skill GroupID
public enum SkillType
{
   Flame_Charm = 1,
   Sword_Strength,
   Fanning,
   Multiple_Shot,
   Flash_Stab,
   Water_Wave = 101,
   Wisp_Flame,
   Wisp_Ice,
   Rock_Guard = 104,
   Lightning_Ball = 105,
   Wind_Swallow = 106,
   Lightning_Cloud,
   ThornVine,
   
   IgnisFatuus = 110,
   LeafBlow,
   
   IncHp = 201,
   IncMoveSpeed,
   IncBulletSpeed,
   IncExp,
   IncDamage,
   IncPickUp,
   IncHpRecovery,
   IncCooltime,
   IncScale,
   IncBossDamage,
   IncDmgReduce,
   IncCriticalRate,
   Charm_Roll_Master = 301,
   SwordStrength_Master,
   Fanning_Master,
   Multiple_Shot_Master,
   Flash_Stab_Master,
   Water_Wave_Master = 311,
   Wisp_Master,
   Rock_Guard_Master,
   Lightning_Ball_Master,
   Wind_Swallow_Master,
   Lightning_Cloud_Master,
   Thorn_Vine_Master,
   Ignis_Fatuus_Master,
   Leaf_Blow_Master,

   SkullDonuts = 10001,
   Judgment,
   FiveStars,
   Shuriken,
   LightningStep,
   SkullDonutsMaster = 11001,
   JudgmentMaster,
   FiveStarsMaster,
   ShurikenMaster,
   LightningStepMaster,
}

public enum UnitType
{
   None,
   Player,
   Hero,
   Enemy,
   EnemyElite = 5,
   Crack = 5,
   Boss,
}

public enum HitType
{
   None,          // UI 표기안함
   Normal,        // 일반
   Fatal,         // 치명타
   HyperFatal     // 하이퍼치명타
}

public enum WaveType
{
   NoSpawn,
   Normal,
   Wave,
}

public enum CurrencyType
{
   PayGem = -1, // 서버데이터 1번 유료잼 컨버팅 저장용
   Gem = 1,
   FreeGem = 2,
   Obsidian = 3,
   Gold = 11,
   Energy = 12, 
   Gacha_Ticket_Weapon = 13,
   Gacha_Ticket_Armor = 14,
   Guardian_Essence = 15,
   Exploration_Cloud = 16,
}

public enum LobbyTap
{
   Shop,
   Trait,// = 99,
   Character,// = 1,
   Lobby,
   Summon,
   Dungeon,
}

public enum StageItemType
{
   Recovery = 1,
   Magnet,
   Bomb,
   ExpLevel1 = 51,
   ExpLevel2,
   ExpLevel3,
}

public enum ChallengePenalty
{
   SpeedUp = 1,
   StatUp,
   AtkUp,
   HpUp,
   SizeUp,
   SuperArmour,
   TweenBoss,
   Hp,
   Exp,
   ViewRange,
   MapSkill,
   Crack,
   Hero,
   Weapon,
   TimeOut,
   Revive,
   SkillSize,
}
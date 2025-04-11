using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using ProjectM;
using Sirenix.Utilities;
using UnityEngine;
using TimerManager = ProjectM.TimerManager;

// 네트워크 공용 Data 
[Serializable]
public class UserAccount
{
    public PlayerData player;
    public FederationData federation_user;
    public bool is_free_change_name;
    public string id_token;
}

[Serializable]
public class FederationData
{
    public string type;
    public string email;
}

[Serializable]
public class CurrenciesData
{
    public List<Currency> Currencies { get; set; }
}


[Serializable]
public class CurrencyData
{
    public Currency currency { get; set; }
}

[Serializable]
public class Currency
{
    public CurrencyType type { get; set; }
    public string value { get; set; }
}

[Serializable]
public class PlayerData
{
    public string id;
    public string name;
    public int level;
    public int experience;
    public int portrait_id;
    public DateTime energy_at; // 에너지 충전
}

[Serializable]
public class StageStartData
{
    public List<RewardData> breach_rewards { get; set; }
    public List<RewardData> benefit_breach_rewards { get; set; }
    public List<Currency> currencies { get; set; }
    public List<int> benefit_ids { get; set; }
    public StageData stage { get; set; }
    public int gold_elite { get; set; }
}

[Serializable]
public class StageEndData
{
    public int trait_point_max { get; set; }
    public List<Currency> currencies { get; set; }
    public List<Currency> level_reward_currencies { get; set; }
    public StageData stage { get; set; }
    public PlayerData player { get; set; }
    public List<Gear> gears { get; set; }
    
    public List<RewardData> default_rewards { get; set; }
    public List<RewardData> clear_rewards { get; set; }
    public List<RewardData> benefit_breach_rewards { get; set; }
    public List<RewardData> benefit_clear_rewards { get; set; }
    public List<RewardData> benefit_default_rewards { get; set; }
    public List<RewardData> first_clear_rewards { get; set; }
    public List<RewardData> level_up_rewards { get; set; }
    public List<RewardData> vip_rewards { get; set; }
    
    public List<RewardData> gold_elite_rewards { get; set; }
    public List<RewardData> breach_rewards { get; set; }
}

[Serializable]
public class StageData
{
    public int type;                            // 스테이지 타입
    public int chapter_id;                      // 마지막진입 챕터
    public int level;                           // 마지막진입 스테이지
    public int level_cap { get; set; }          // 완료된 레벨
    public int level_reward { get; set; }       // 보상을 받은 레벨
    public DateTime afk_reward_at { get; set; } // 자동전투보상 시간
    public int chapter_id_cap { get; set; }     // 완료 챕터
    public int chapter_id_reward { get; set; }  // 보상을 받은 챕터
    public int ffwd_ad_count { get; set; }      // 자동전투 & 소탕
    public int ffwd_count { get; set; }         // 자동전투 카운트
    public int enter_count { get; set; }        // 입장 제한
    public int enter_extra_count { get; set; }  // 추가입장 제한
}

[Serializable]
public class RewardData
{
    // c = 갯수, t = 아이템 리워드 타입, i = 테이블 인덱스
    public int i { get; set; }
    public int t { get; set; }
    public string c { get; set; }
    public int o { get; set; }  // order 우편함
    public int b { get; set; }  // 추가 보상 아이콘 타입
}

[Serializable]
public class BattleSkillData
{
    public int i { get; set; }  // group id
    public int l { get; set; }  // level
}

[Serializable]
public class AutoBattleRollData
{
    public List<RewardData> default_rewards { get; set; }
    public List<RewardData> currency_box_rewards { get; set; }
    public List<RewardData> gear_box_rewards { get; set; }
}

[Serializable]
public class AutoBattleRewardData
{
    public StageData stage { get; set; }
    public PlayerData player { get; set; }
    public List<Currency> currencies { get; set; }
    public List<Gear> gears { get; set; }
    public List<Currency> level_reward_currencies { get; set; }
    public List<RewardData> level_up_rewards { get; set; }
    
    public List<RewardData> default_rewards { get; set; }
    public List<RewardData> currency_box_rewards { get; set; }
    public List<RewardData> gear_box_rewards { get; set; }
    public int trait_point_max { get; set; }
}

[Serializable]
public class FastBattleRewardData
{
    public StageData stage { get; set; }
    public PlayerData player { get; set; }
    public List<Currency> currencies { get; set; }
    public List<Currency> level_reward_currencies { get; set; }
    public List<RewardData> level_up_rewards { get; set; }
    public List<Gear> gears { get; set; }
    public List<RewardData> default_rewards { get; set; }
    public List<RewardData> rewards { get; set; }
    public List<RewardData> currency_box_rewards { get; set; }
    public List<RewardData> gear_box_rewards { get; set; }
    public int trait_point_max { get; set; }
}

[Serializable]
public class StageFirstClearRewardData
{
    public StageData stage { get; set; }
    public List<Currency> currencies { get; set; }
    public List<RewardData> total_rewards { get; set; }
}

[Serializable]
public class CheckEnegyData
{
    public DateTime energy_at { get; set; }             // player 
    public List<Currency> currencies { get; set; }
}

[Serializable]
public class GearOption
{
    public int item_id;
    public int option_index; // 추가 옵션 슬롯 번호
    public int option_type;
    public int option_grade;
    public bool is_keep;
}

[Serializable]
public class GearOptionData
{
    public GearOption gear_Option;
    public GearOption[] gear_Options { get; set; }
    public List<Currency> currencies { get; set; }
}

[Serializable]
public class GearReward
{
    public ScrollDataModel dataModel;
    public Equipment equipment;
}

[Serializable]
public class EquipGear
{
    public int pc_type;
    public int equip_type; // 장착 슬롯 번호
    public int item_id;

    public GearPowerInfo GetGearPowerInfo()
    {
        return UserDataManager.Instance.GearsPowerInfo
            .TryGetValue(item_id, out var gearPowerInfo) ? gearPowerInfo : null;
    }

    public Equipment GetEquipment()
    {
        return TableDataManager.Instance.data.Equipment.Single(d => d.Index == item_id);
    }

    public Gear GetGear()
    {
        UserDataManager.Instance.Gears.TryGetValue(item_id, out var gear);
        return gear;
    }
}

[Serializable]
public class Gear
{
    public int item_id;     // 아이템 인덱스
    public int count;       // 아이템 개수
    public int level;       // 아이템 레벨
    public int level_cap;   // 한계 돌파 체크용 레벨
    public bool isComposeRedDot;
    public bool isChangeOptionRedDot;
    public bool isLimitBreakRedDot;

    public bool IsShowRedDot()
    {
        return isComposeRedDot || isChangeOptionRedDot || isLimitBreakRedDot;
    }
    
    public GearPowerInfo GetGearPowerInfo()
    {
        return UserDataManager.Instance.GearsPowerInfo
            .TryGetValue(item_id, out var gearPowerInfo) ? gearPowerInfo : null;
    }

    public Dictionary<int, GearOption> GetGearOptions()
    {
        return UserDataManager.Instance.GearsOption
            .TryGetValue(item_id, out var gearOptions) ? gearOptions : null;
    }

    public EquipGear GetEquipGear()
    {
        return UserDataManager.Instance.EquipGears
            .SingleOrDefault(d => d.Value.item_id == item_id).Value;
    }
    
    public Equipment GetGearEquipment()
    {
        return TableDataManager.Instance.data.Equipment.Single(t => t.Index == item_id);
    }

    public int GetEquipType()
    {
        return GetGearEquipment().EquipType;
    }

    public int GetSlotType()
    {
        EquipType equipType = TableDataManager.Instance.data.EquipType
            .SingleOrDefault(t => t.Index == GetEquipType());

        return equipType?.Slot ?? 0;
    }
}

[Serializable]
public class RandomBox
{
    public int box_id;
    public int level;
    public int count;
    public int ad_count;
}

[Serializable]
public class PostBox
{
    public int id;
    public string subject;
    public string s_var;
    public string body;
    public string b_var;
    public bool is_confirm;
    public DateTime inserted_at;
    public DateTime expiry_at;
    public List<RewardData> items;
    public TimeSpan orderTime;
}

// 월정액
[Serializable]
public class Membership
{
    public int package_id { get; set; }
    public DateTime expiry_at { get; set; }
    public int timer_id { get; set; }
}

[Serializable]
public class OrderHistory
{
    public int package_id;
    public int count;
    public bool is_first;
    public DateTime reset_at;
}

[Serializable]
public class MissionData
{
    public int index;
    public DateTime reward_at;
    public string count;
    public int stateType;   // 1 = 보상 받기 전, 2 = 보상 받음
}

[Serializable]
public class AchievementInfo
{
    public int group;
    public int level;
    public int reward_level;
    public string count;
    public int stateType;
}

public class AchievementData
{
    public AchievementInfo achievement;
    public List<AchievementInfo> achievements;
}

[Serializable]
public class Attendance
{
    public int type;
    public int index;
    public int count;
    public bool is_check;
}

[Serializable]
public class AttendanceData
{
    public Attendance attendance;
    public List<Attendance> attendances;
    public List<RewardData> rewards;
    public PlayerData player;
    public List<RewardData> level_up_rewards;
    public List<Currency> level_reward_currencies;
    public List<Currency> currencies;
}

[Serializable]
public class CostumeData
{
    public int item_id;
}

[Serializable]
public class EquipCostume
{
    public int item_id;
}

[Serializable]
public class AdBuff
{
    public int type;
    public int level;
    public int count;
    public DateTime end_at;
    public int timer_id;
}

[Serializable]
public class AdBuffData
{
    public AdBuff ad_Buff;
    public List<AdBuff> ad_buffs;
}

public class TraitInfoData
{
    public List<TraitData> traits { get; set; }
    public TraitData trait { get; set; }
    public List<Currency> currencies { get; set; }
    public int trait_point_max { get; set; }
    public int trait_point_use { get; set; }
}

[Serializable]
public class TraitData
{
    public int level { get; set; }
    public int tab { get; set; }
    public int trait_id { get; set; }
}

[Serializable]
public class PlayerRankGear
{
    public int l;
    public int i;
}

[Serializable]
public class PlayerRankSikll
{
    public int i;
    public int l;
}

[Serializable]
public class PlayerRankStatus
{
    public int c;       // costume
    public int lv;      // level
    public string d;    // atk
    public string hp;   // hp
    public int p;       // player portrait
    public string n;    // name
    public List<int> h; // hero
    public List<PlayerRankGear> g;
    public List<PlayerRankSikll> s;
}

[Serializable]
public class PlayerRank
{
    public int r;               // rank
    public int i;               // stage index
    public int l;               // stage level
    public double t;            // clear time
    public PlayerRankStatus p;  // player rank status
}

[Serializable]
public class PlayerRankData
{
    public int type;
    public DateTime expiry_at;
    public PlayerRank player_rank;          // 내꺼
    public List<PlayerRank> player_ranks;   // 순위권
}

[Serializable]
public class PortraitInfo
{
    public int id;

    public PortraitInfo(int index)
    {
        id = index;
    }
}

[Serializable]
public class GuardianData
{
    public Guardian guardian;
    public List<Guardian> guardians;
    public GuardianResearch guardian_research;
    public List<Currency> currencies;
    public bool up;
    public int refund_count;

    public void SetData()
    {
        if (currencies.IsNullOrEmpty() == false) UserDataManager.Instance.currencyInfo.SetItem(currencies);
        if (guardian != null) UserDataManager.Instance.gearInfo.AddGuardian(guardian);
        if (guardians.IsNullOrEmpty() == false) UserDataManager.Instance.gearInfo.AddGuardian(guardians);
        if (guardian_research != null) UserDataManager.Instance.gearInfo.AddGuardianResearch(guardian_research);
    }
}

[Serializable]
public class Guardian
{
    public int type;
    public int level;
}

[Serializable]
public class GuardianResearch
{
    public int level;
    public int leveling;
    public DateTime end_at;
    public int timerID;
}

[Serializable]
public class ExplorationData
{
    public List<Currency> currencies;
    public List<Explore> explores;
    public Explore explore;
    public int stage_index;
    public List<RewardData> rewards;
    public int timerID;
    public string combat_score;

    public void SetData()
    {
        UserDataManager.Instance.gearInfo.AddExplorationData(this);
    }

    public void SetExplorationCombatScoreData()
    {
        UserDataManager.Instance.gearInfo.AddExplorationCombatScoreData(this);
    }
}

[Serializable]
public class Explore
{
    public int extra_count;
    public DateTime reset_at;
}

[Serializable]
public class EquipHeroCard
{
    public int item_id;
}

[Serializable]
public class HeroCard
{
    public int item_id;
    public int count;
    public int level;
    public int awake;
}

public class HeroCardData
{
    public List<EquipHeroCard> equip_hero_cards;
    public List<HeroCard> heroCards;
    public HeroCard heroCard;
}

[Serializable]
public class CommonApiResultData
{
    public PlayerData player;
    public OrderHistory order_history;
    public RandomBox random_box;
    public Membership membership;
    public MissionData mission;
    public Gear gear;
    public int trait_point_max;
    public PortraitInfo portrait;

    public List<EquipGear> equip_gears;
    public List<Membership> memberships;
    public List<Gear> gears;
    public List<Currency> currencies;
    public List<RandomBox> random_boxes;
    public List<OrderHistory> order_histories;
    public List<MissionData> missions;
    public List<CostumeData> costumes;
    public List<EquipCostume> equip_costumes;
    public List<RewardData> rewards;
    public List<RewardData> box_rewards;
    public List<Currency> level_reward_currencies;
    public List<RewardData> level_up_rewards;
    public List<PostBox> post_boxes;
    public List<PortraitInfo> portraits;

    public void SetData()
    {
        if (player != null) UserDataManager.Instance.userInfo.SetPlayerData(player);
        if (order_history != null) UserDataManager.Instance.payInfo.AddOrderHistory(order_history);
        if (random_box != null) UserDataManager.Instance.randomBoxInfo.AddRandomBox(random_box);
        if (membership != null) UserDataManager.Instance.payInfo.AddMembership(membership);
        if (mission != null) UserDataManager.Instance.missionInfo.AddMissionData(mission);
        
        if (gear != null)
        {
            UserDataManager.Instance.gearInfo.AddGear(gear);
            UserDataManager.Instance.gearInfo.SetGearEquipTypeRedDot(gear.GetEquipType());

            if (UILobby.Instance)
            {
                UILobby.Instance.SetRedDot(LobbyTap.Character,
                    UserDataManager.Instance.gearInfo.Gears
                        .Any(g => g.Value.IsShowRedDot()));
            }
        }
        
        if (portrait != null) UserDataManager.Instance.userProfileInfo.AddPortrait(portrait);
        
        if (equip_costumes != null) UserDataManager.Instance.gearInfo.AddEquipCostume(equip_costumes);
        if (equip_gears.IsNullOrEmpty() == false) UserDataManager.Instance.gearInfo.AddEquipGear(equip_gears);
        if (memberships.IsNullOrEmpty() == false) UserDataManager.Instance.payInfo.AddMembership(memberships);

        if (gears.IsNullOrEmpty() == false)
        {
            UserDataManager.Instance.gearInfo.AddGear(gears);
            
            gears.ForEach(g => 
            {
                UserDataManager.Instance.gearInfo.SetGearEquipTypeRedDot(g.GetEquipType());
            });
            
            if (UILobby.Instance)
            {
                UILobby.Instance.SetRedDot(LobbyTap.Character,
                    UserDataManager.Instance.gearInfo.Gears
                        .Any(g => g.Value.IsShowRedDot()));
            }
        }
        
        if (currencies.IsNullOrEmpty() == false) UserDataManager.Instance.currencyInfo.SetItem(currencies);
        if (random_boxes.IsNullOrEmpty() == false) UserDataManager.Instance.randomBoxInfo.AddRandomBox(random_boxes);
        if (order_histories.IsNullOrEmpty() == false) UserDataManager.Instance.payInfo.AddOrderHistory(order_histories);
        if (missions != null) UserDataManager.Instance.missionInfo.AddMissionData(missions);
        if (costumes != null) UserDataManager.Instance.gearInfo.AddCostume(costumes);
        if (trait_point_max > 0) UserDataManager.Instance.userTraitInfo.apMax.Value = trait_point_max;
        if (post_boxes.IsNullOrEmpty() == false)
        {
            UserDataManager.Instance.postBoxInfo.AddPostBox(post_boxes);
            UserDataManager.Instance.postBoxInfo.SortPostBoxes();
        }
        
        if (portraits.IsNullOrEmpty() == false)
            UserDataManager.Instance.userProfileInfo.AddPortrait(portraits);
        
        if (box_rewards.IsNullOrEmpty() == false)
        {
            box_rewards.ForEach(b =>
            {
                if (b.t == 7)
                {
                    Portrait portrait = TableDataManager.Instance.data.Portrait
                        .Single(t => t.Index == b.i);

                    if (portrait.Index == 1)
                        PlayerPrefs.SetInt(portrait.Icon, 2);
                    else
                        PlayerPrefs.SetInt(portrait.Icon, 1);
                                
                    PlayerPrefs.Save();
                }
            });
            
            if (UILobby.Instance)
                UILobby.Instance.LobbyGnb.SetPortaitRedDot();
            
            PopupProfile popup = UIManager.Instance.GetPopup(PopupName.Lobby_Profile) as PopupProfile;
            if (popup) popup.SetDataPortrait();
        }
    }
}

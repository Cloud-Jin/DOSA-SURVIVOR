// 유저 정보
// 다이아 1,2 
// 흑요석
// 골드
// 에너지

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using InfiniteValue;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Timer = TimerTool.Timer;

namespace ProjectM
{
    public partial class UserDataManager : Singleton<UserDataManager>
    {
        public UserAccountInfo userInfo;
        public CurrencyInfo currencyInfo;       // 재화
        public StageInfo stageInfo;
        public UserTraitInfo userTraitInfo;
        public PostBoxInfo postBoxInfo;
        public PayInfoData payInfo;
        public RandomBoxInfo randomBoxInfo;
        public MissionInfo missionInfo;
        public ClientInfo clientInfo;
        public PlayerRankInfo playerRankInfo;
        public UserProfileInfo userProfileInfo;
        public UserCardInfo userCardInfo;

        public List<UserCardData> UserCardDataList => userCardInfo.userCardDataList;

        public bool isShowAutoBattleReward;
        protected override void Init()
        {
            InitUserData();
            initComplete = true;
        }

        public void InitUserData()
        {
            userInfo = new UserAccountInfo();
            userTraitInfo = new UserTraitInfo();
            currencyInfo = new CurrencyInfo();
            gearInfo = new GearInfo();
            stageInfo = new StageInfo();
            postBoxInfo = new PostBoxInfo();
            payInfo = new PayInfoData();
            randomBoxInfo = new RandomBoxInfo();
            missionInfo = new MissionInfo();
            clientInfo = new ClientInfo();
            playerRankInfo = new PlayerRankInfo();
            userProfileInfo = new UserProfileInfo();
            userCardInfo = new UserCardInfo();
        }

        public void PreLoad()
        {
            stageInfo.PreLoad();
            // userTraitInfo.Preload();
        }

        public class UserAccountInfo
        {
            public UserAccount PlayerData = new UserAccount();
            public Subject<int> PlayerDataUpdate;
            public UserAccountInfo()
            {
                PlayerDataUpdate = new Subject<int>();
                PlayerData.player = new PlayerData()
                {
                    energy_at = DateTime.MinValue
                };
            }

            public void SetPlayerData(PlayerData data)
            {
                PlayerData.player = data;
                PlayerDataUpdate.OnNext(1);
            }

            public PlayerData GetPlayerData()
            {
                return PlayerData.player;
            }
        }

        public class UserProfileInfo
        {
            public List<int> portraitList = new();
            public List<int> titleList = new();

            public void AddPortrait(PortraitInfo portraitInfo)
            {
                if (portraitList.Contains(portraitInfo.id) == false)
                    portraitList.Add(portraitInfo.id);
            }

            public void AddPortrait(List<PortraitInfo> portraitInfos)
            {
                portraitInfos.ForEach(AddPortrait);
            }
        }
        
        public class UserTraitInfo
        {
            public List<TraitData> Data = new List<TraitData>();
            public ReactiveProperty<int> TraitNum = new ReactiveProperty<int>();    // 특성포인트
            public ReactiveProperty<int> apMax = new ReactiveProperty<int>();       // 특성포인트
            public ReactiveProperty<int> apUse = new ReactiveProperty<int>();       // 특성포인트
            public Subject<bool> OnTraitRefresh = new Subject<bool>();              // 특성데이터 업데이트

            public UserTraitInfo()
            {
                apMax.Subscribe(_ => PointRefresh());
                apUse.Subscribe(_ => PointRefresh());
            }
            public void SetData(TraitInfoData netData)
            {
                if (!netData.traits.IsNullOrEmpty())
                {
                    Data.Clear();
                    Data.AddRange(netData.traits);    
                }

                if (netData.trait != null)
                {
                    AddTrait(netData.trait);
                }
                OnTraitRefresh.OnNext(true);

                Instance.Gears.ForEach(g => Instance.gearInfo.SetGearPowerAndBestGear(g.Value));
                
                if (ContainerInventory.Instance)
                    ContainerInventory.Instance.playerStateInfo.SetPowerInfo();
            }
            
            public void AddTrait(TraitData trait)
            {
                TraitData deleteTrait = Data.SingleOrDefault(a => a.tab == trait.tab && a.trait_id == trait.trait_id);
                if (deleteTrait != null)
                {
                    Data.Remove(deleteTrait);
                }
                Data.Add(trait);
            }

            public List<TraitData> GetActiveTraits()
            {
                var _datas = Data.Where(t => t.level > 0).ToList();
                return _datas;
            }

            // 접속 & 레벨업시
            public void PointRefresh()
            {
                var num = apMax.Value - apUse.Value;
                TraitNum.Value = num;
                
                // Debug.Log($"남은 특성 횟수 {num}");
            }
            // 횟수
            // 추가
            // 삭제
            public int GetTraitLevel(int idx)
            {
                var traitData = Data.SingleOrDefault(t => t.trait_id == idx);
                var lv = (traitData == null) ? 0 : traitData.level;
                return lv;
            }
            
            public bool IsTraitMaxLevel(int idx)
            {
                var traitData = Data.SingleOrDefault(t => t.trait_id == idx);
                var lv = (traitData == null) ? 0 : traitData.level;
                if (lv == 0) return false;
                 
                var levelGroupID = TableDataManager.Instance.data.Trait.Single(t => t.Index == idx).TraitLevelGroupID;
                var maxLevel = TableDataManager.Instance.GetTraitMaxLevel(levelGroupID);
                    
                return lv == maxLevel;
            }

            public InfVal GetResetCost()
            {
                var config = TableDataManager.Instance.data.TraitConfig;
                var baseCost = config.Single(t => t.Index == 2).Value;
                
                var weight = apUse.Value / config.Single(t=> t.Index == 3).Value;
                var addWeightValue =config.Single(t=> t.Index == 4).Value;
                var weightValue = weight > 0 ? weight * addWeightValue : 1;
                
                return apUse.Value * baseCost * weightValue;
            }

            public int GetTraitValue(int id)
            {
                // 특성 넣으면 수치 리턴
                var levelGroup = TableDataManager.Instance.data.Trait.Single(t => t.Index == id).TraitLevelGroupID;
                var level = Data.SingleOrDefault(t => t.trait_id == id);
                if (level != null && level.level > 0)
                    return TableDataManager.Instance.data.TraitLevel.Single(t => t.GroupID == levelGroup && t.Level == level.level).Value;
                return 0;

            }
        }

        #region 재화
        // 잼 = 유료잼(pay) + 무료잼(free)
        public class CurrencyInfo
        {
            public Dictionary<CurrencyType, ReactiveProperty<InfVal>> data = new Dictionary<CurrencyType, ReactiveProperty<InfVal>>();

            public CurrencyInfo()
            {
                AddItem(CurrencyType.Gem, 0);
                AddItem(CurrencyType.FreeGem, 0);
                AddItem(CurrencyType.PayGem, 0);
            }
            
            void CombineGem()
            {
                var freeGem = new InfVal(data[CurrencyType.FreeGem].Value, 29);
                var payGem = new InfVal(data[CurrencyType.PayGem].Value, 29);
                data[CurrencyType.Gem].Value = freeGem + payGem;
            }
            void AddItem(CurrencyType type, InfVal value)
            {
                if(data.ContainsKey(type))
                {
                    data[type].Value += value;
                }
                else
                {
                    data.Add(type, new ReactiveProperty<InfVal>(){Value = new InfVal(value,29)});
                }
            }
            
            void SetItem(CurrencyType type, InfVal value)
            {
                if (data.ContainsKey(type))
                {
                    data[type].Value = value;
                }
                else
                {
                    data.Add(type, new ReactiveProperty<InfVal>(){Value = new InfVal(value,29)});
                }
                
                // 잼 = 유료잼 + 무료잼
                if (type == CurrencyType.Gem)
                {
                    SetItem(CurrencyType.PayGem, value);
                    CombineGem();
                }

                if (type == CurrencyType.FreeGem)
                {
                    CombineGem();
                }
            }
            
            public void SetItem(Currency currency)
            {
                if (data.ContainsKey(currency.type))
                {
                    data[currency.type].Value = InfVal.Parse(currency.value);
                }
                else
                {
                    data.Add(currency.type, new ReactiveProperty<InfVal>(){Value = new InfVal(currency.value,29)});
                }
                
                // 잼 = 유료잼 + 무료잼
                if (currency.type == CurrencyType.Gem)
                {
                    SetItem(CurrencyType.PayGem, InfVal.Parse(currency.value));
                    CombineGem();
                }
                
                if (currency.type == CurrencyType.FreeGem)
                {
                    CombineGem();
                }
            }

            public void SetItem(List<Currency> Currencies)
            {
                Currencies.ForEach(_data =>
                {
                    SetItem(_data);
                });
            }
            
            // item Subscribe
            public IDisposable SubscribeItemRx(CurrencyType type, Action<InfVal> action)
            {
                if(!data.ContainsKey(type)) SetItem(type, 0);
                return data[type].Subscribe(_ => action(_));
            }
            
            public bool ValidGoods(InfVal value, InfVal needValue)
            {
                return value >= needValue;
            }
            
            public bool ValidGoods(CurrencyType type, InfVal val)
            {
                // 젬 = 유료잼 + 무료잼 
                switch (type)
                {
                    default:
                        return ValidItem(type, val);
                }
            }
            
            bool ValidItem(CurrencyType type, InfVal val)
            {
                if (data.ContainsKey(type))
                {
                    return data[type].Value >= val;
                }

                return false;
            }
        }
        
        // public class CurrencyDataInfo
        // {
        //     public CurrencyType type { get; set; }
        //     public InfVal value { get; set; }
        // }
        #endregion
        
        public class StageInfo
        {
            // Type 1번은 생성되어있음.
            public List<StageData> Data = new List<StageData>();
            public ReactiveProperty<int> SelectStage = new ReactiveProperty<int>();
            public ReactiveProperty<int> SelectDungeonGold = new ReactiveProperty<int>();

            public void PreLoad()
            {
                var serverSelectStageData = TableDataManager.Instance.GetStageData(StageData.chapter_id, StageData.level, 1);
                var stageCap = TableDataManager.Instance.GetStageData(StageData.chapter_id_cap, StageData.level_cap, 1);
                var playStage = TableDataManager.Instance.GetStageData(PlayStage);
                PlayStage = playStage.Index <= stageCap.Index + 1 ? playStage.Index : serverSelectStageData.Index;
                SelectStage.Value = PlayStage;
                
                PlayDungeonGold = PlayDungeonGold <= GoldDungeonData.level_cap ? PlayDungeonGold : GoldDungeonData.level_cap;
                SelectDungeonGold.Value = PlayDungeonGold;
            }
            public StageInfo()
            {
            }

            public StageData StageData
            {
                get { return Data.SingleOrDefault(t => t.type == 1); }
                set { Data[Data.FindIndex(t=> t.type == 1)] = value; }
            }
            
            public StageData GoldDungeonData
            {
                get
                {
                    var data = Data.SingleOrDefault(t => t.type == 2);
                    if (data == null)
                        return new StageData();
                    return data;

                }
                set { Data[Data.FindIndex(t=> t.type == 2)] = value; }
            }

            public StageData HardDungeonData
            {
                get
                {
                    var data = Data.SingleOrDefault(t => t.type == 3);
                    if (data == null)
                        return new StageData();
                    return data;
                }

                set { Data[Data.FindIndex(t => t.type == 3)] = value; }
            }
            
            public int PlayStage
            {
                get
                {
                    var value = PlayerPrefs.GetInt(MyPlayerPrefsKey.PlayStage, 0);
                    if (value == 0)
                    {
                        var stageData = TableDataManager.Instance.GetStageData(StageData.chapter_id_cap, StageData.level_cap, 1);
                        if (stageData.NextStage > 0)
                        {
                            value = stageData.NextStage;
                        }
                        else
                            value = stageData.Index;
                    }
                    
                    return value;
                }
                set
                {
                    PlayerPrefs.SetInt(MyPlayerPrefsKey.PlayStage, value);
                    PlayerPrefs.Save();
                    SelectStage.Value = value;
                }
            }

            public int PlayDungeonGold
            {
                get
                {
                    var value = PlayerPrefs.GetInt(MyPlayerPrefsKey.PlayDungeonGold, -1);
                    if (value == -1)
                    {
                        value = GoldDungeonData.level_cap;
                    }
                    return value;
                }
                set
                {
                    PlayerPrefs.SetInt(MyPlayerPrefsKey.PlayDungeonGold, value);
                    PlayerPrefs.Save();
                    SelectDungeonGold.Value = value;
                }
            }
            public int Stage(StageData NewData)
            {
                var current = TableDataManager.Instance.GetStageData(StageData.chapter_id_cap, StageData.level_cap, 1);
                var newData = TableDataManager.Instance.GetStageData(NewData.chapter_id_cap, NewData.level_cap, 1);
                int index = current?.Index ?? 0;
                if (index < newData.Index)
                {
                    // 스테이지 증가
                    // 다음스테이지 진행
                    if (newData.NextStage > 0)
                        return newData.NextStage;
                    else
                        return newData.Index;
                }
                
                // 현재스테이지 진행
                return 0;
            }

            public int StageGoldDungeon(StageData NewData)
            {
                var current = TableDataManager.Instance.data.DungeonLevel.SingleOrDefault(t => t.Level == GoldDungeonData.level_cap);
                var newData = TableDataManager.Instance.data.DungeonLevel.SingleOrDefault(t => t.Level == NewData.level_cap);
                
                int index = current?.Level ?? 0;
                if (index < newData.Level)
                {
                    // 스테이지 증가
                    return newData.Level;
                }
                
                // 현재스테이지 진행
                return 0;
            }

            public bool RedDotDungeon(StageData userStageData, Dungeon dungeonData)
            {
                return userStageData.enter_count < dungeonData.EnterCount || userStageData.ffwd_ad_count < dungeonData.SweepCount ||
                       userStageData.enter_extra_count < dungeonData.AddEnterCount;
            }
        }

        public class PostBoxInfo
        {
            public List<PostBox> PostBoxes = new();

            public void AddPostBox(List<PostBox> postBoxList)
            {
                postBoxList.ForEach(AddPostBox);
            }
            
            public void AddPostBox(PostBox postBox)
            {
                PostBox deletePostBox = PostBoxes.SingleOrDefault(d => d.id == postBox.id);
                
                if (deletePostBox != null)
                    PostBoxes.Remove(deletePostBox);
                
                PostBoxes.Add(postBox);

                if (postBox.items.IsNullOrEmpty() == false)
                    postBox.items = APIRepository.SortRewardData(postBox.items);
            }

            public void DeleteConfirmPostBox()
            {
                List<PostBox> deletePosBoxList = PostBoxes.Where(d => d.is_confirm).ToList();
                deletePosBoxList.ForEach(d => PostBoxes.Remove(d));
                
                PostBoxPopup postBoxPopup = UIManager.Instance.GetPopup(PopupName.Post_Box) as PostBoxPopup;
                if (postBoxPopup) postBoxPopup.SetData();
            }

            public void DeletePostBox(PostBox deletePostBox)
            {
                if (PostBoxes.Contains(deletePostBox))
                {
                    var popup = UIManager.Instance.GetPopup(PopupName.Post_Box_Detail) as PostBoxDetailPopup;
                    if (popup && popup.GetPostPox() == deletePostBox)
                        popup.Hide();
                    
                    PostBoxes.Remove(deletePostBox);
                    
                    PostBoxPopup postBoxPopup = UIManager.Instance.GetPopup(PopupName.Post_Box) as PostBoxPopup;
                    if (postBoxPopup) postBoxPopup.SetData();
                }
            }
            
            public void SortPostBoxes()
            {
                if (PostBoxes.IsNullOrEmpty() == false)
                {
                    foreach (var postBox in PostBoxes)
                        postBox.orderTime = DateTime.UtcNow - postBox.inserted_at;
                    
                    List<PostBox> orderByNotConfirm = PostBoxes
                        .Where(d => d.is_confirm == false)
                        .OrderBy(d => d.orderTime.TotalSeconds).ToList();
                    List<PostBox> orderByConfirm = PostBoxes
                        .Where(d => d.is_confirm)
                        .OrderBy(d => d.orderTime.TotalSeconds).ToList();

                    PostBoxes = orderByNotConfirm.Concat(orderByConfirm).ToList();
                }
            }

            public bool HaveDeleteMail()
            {
                for (int i = 0; i < PostBoxes.Count; ++i)
                {
                    if (PostBoxes[i].is_confirm)
                        return true;
                }

                return false;
            }

            public bool IsAllConfirm()
            {
                return IsItemPostBoxAllConfirm() && IsNoTimePostBoxAllConfirm();
            }

            private bool IsItemPostBoxAllConfirm()
            {
                foreach (var postBox in PostBoxes)
                {
                    if (postBox.items.IsNullOrEmpty() == false && postBox.is_confirm == false)
                        return false;
                }

                return true;
            }

            private bool IsNoTimePostBoxAllConfirm()
            {
                foreach (var postBox in PostBoxes)
                {
                    if (postBox.items.IsNullOrEmpty() && postBox.is_confirm == false)
                        return false;
                }

                return true;
            }
        }

        public class PayInfoData
        {
            public List<Membership> Memberships = new();
            public List<OrderHistory> OrderHistories = new();
            public List<AdBuff> AdBuffs = new();
            public Dictionary<string, int> payinfoStartTimerIdDic = new();
            public Dictionary<string, int> payinfoEndTimerIdDic = new();

            public int adSkipCount;
            public int IncreaseMaxEnergy;
            public int IncreaseQuickBattleRewardTime;
            public int IncreaseStageClearBaseReward;
            public int IncreaseAutoBattleReward;
            public int DecreaseEnergyChargeCoolDown;
            public int IncreaseCrackReward;
            public int IncreaseSelectSkillInitCount;
            public int IncreaseEssenceReward;

            public void AddMembership(List<Membership> data)
            {
                data.ForEach(AddMembership);
            }
            public void AddMembership(Membership membership)
            {
                Membership deleteMembership = Memberships
                    .SingleOrDefault(d => d.package_id == membership.package_id);

                if (deleteMembership != null)
                {
                    TimerManager.Instance.StopTimer(deleteMembership.timer_id);
                    Memberships.Remove(deleteMembership);
                }

                Memberships.Add(membership);
                
                AddMemberShipTimer(membership);
            }

            public void AddADBuff(List<AdBuff> adBuffs)
            {
                adBuffs.ForEach(AddADBuff);
            }
            
            public void AddADBuff(AdBuff adBuff)
            {
                AdBuff deleteAdBuff = AdBuffs.SingleOrDefault(a => a.type == adBuff.type);

                if (deleteAdBuff != null)
                {
                    TimerManager.Instance.StopTimer(deleteAdBuff.timer_id);
                    AdBuffs.Remove(deleteAdBuff);
                }

                AdBuffs.Add(adBuff);
                AddAdBuffTimer(adBuff);
            }

            public bool IsDisableBuff()
            {
                if (AdBuffs.IsNullOrEmpty()) return true;
                if (AdBuffs.Count < TableDataManager.Instance.data.ADBuffType.Length) return true;

                foreach (var adBuff in AdBuffs)
                {
                    Timer timer = TimerManager.Instance.GetTimer(adBuff.timer_id);

                    if (timer == null || timer.IsFinished)
                        return true;
                }

                return false;
            }

            public bool IsEnableBuff()
            {
                if (AdBuffs.IsNullOrEmpty()) return false;

                foreach (var adBuff in AdBuffs)
                {
                    Timer timer = TimerManager.Instance.GetTimer(adBuff.timer_id);
                    
                    if (timer != null && timer.IsFinished == false)
                        return true;
                }

                return false;
            }

            public bool IsCheckedCostume()
            {
                bool bShowRedDot = false;

                TableDataManager.Instance.data.Costume.ForEach(t =>
                {
                    int isCheck = PlayerPrefs.GetInt(t.Name, -1);

                    if (isCheck == 1)
                        bShowRedDot = true;
                });

                return bShowRedDot;
            }

            public bool IsSpecialShopRedDot()
            {
                List<PayInfo> goodsCostume = TableDataManager.Instance.data.PayInfo
                    .Where(t => (t.TabType == 5 && t.PriceType == 1) 
                                || (t.PurchaseStartTime.IsNullOrEmpty() == false && t.PurchaseEndTime.IsNullOrEmpty() == false)).ToList();

                foreach (PayInfo g in goodsCostume)
                {
                    OrderHistory orderHistory = Instance.payInfo.OrderHistories
                        .SingleOrDefault(u => u.package_id == g.Index);

                    if (orderHistory == null)
                    {
                        if (g.PurchaseStartTime.IsNullOrEmpty() == false && g.PurchaseEndTime.IsNullOrEmpty() == false)
                        {
                            if (GetPayInfoTimer(g.ProductionName) != null)
                            {
                                if (CheckPayInfoDisplayConditionType(g))
                                    return true;
                            }
                        }
                        else
                        {
                            if (g.PriceType == 1)
                            {
                                if (CheckPayInfoDisplayConditionType(g))
                                    return true;
                            }
                        }
                    }
                }

                return false;
            }

            private void AddAdBuffTimer(AdBuff adBuff)
            {
                DateTime currentTime = DateTime.UtcNow;
                DateTime expiryTime = adBuff.end_at;
                TimeSpan totalTime = expiryTime - currentTime;

                if (0 < totalTime.TotalSeconds)
                {
                    UnityEvent onComplete = new UnityEvent();
                    onComplete.AddListener(APIRepository.SubscriptionAdBuff);
                    
                    adBuff.timer_id = TimerManager.Instance
                        .AddTimer((float)totalTime.TotalSeconds, false, false, onComplete, null, null);
                }
            }
            
            private void AddMemberShipTimer(Membership membership)
            {
                DateTime currentTime = DateTime.UtcNow;
                DateTime expiryTime = membership.expiry_at;
                TimeSpan totalTime = expiryTime - currentTime;
            
                if (0 < totalTime.TotalSeconds)
                {
                    UnityEvent onComplete = new UnityEvent();
                    onComplete.AddListener(APIRepository.SubscriptionMemberships);
            
                    membership.timer_id = TimerManager.Instance
                        .AddTimer((float)totalTime.TotalSeconds, false, false, onComplete, null, null);
                }
            }
            
            public void MembershipEnableCheck()
            {
                adSkipCount = 0;
                IncreaseMaxEnergy = 0;
                IncreaseQuickBattleRewardTime = 0;
                IncreaseStageClearBaseReward = 0;
                IncreaseAutoBattleReward = 0;
                DecreaseEnergyChargeCoolDown = 0;
                IncreaseCrackReward = 0;
                IncreaseSelectSkillInitCount = 0;
                IncreaseEssenceReward = 0;

                Memberships.ForEach(SetMemberShipEnableState);
            }
            
            private void SetMemberShipEnableState(Membership membership)
            {
                Timer timer = TimerManager.Instance.GetTimer(membership.timer_id);
                bool isEnable = timer != null && timer.IsCompleted == false;
                
                PayInfo payInfo = TableDataManager.Instance.data.PayInfo.SingleOrDefault(t => t.Index == membership.package_id);
                if (payInfo == null) return;
                
                PayMembershipReward payMembershipReward = TableDataManager.Instance.data.PayMembershipReward
                    .Single(t => t.Index == payInfo.RewardGroupID);
                List<PayMembershipBenefit> benefitList = TableDataManager.Instance.data.PayMembershipBenefit
                    .Where(t => t.GroupID == payMembershipReward.BenefitGroupID).ToList();

                benefitList.ForEach(d =>
                {
                    switch (d.BenefitType)
                    {
                        case 1:
                        {
                            adSkipCount = isEnable ? adSkipCount + 1 : adSkipCount - 1;
                            if (adSkipCount < 0) adSkipCount = 0;
                            break;
                        }
                        case 2:
                        {
                            IncreaseMaxEnergy = isEnable ? IncreaseMaxEnergy + d.BenefitValue 
                                : IncreaseMaxEnergy - d.BenefitValue;
                            if (IncreaseMaxEnergy < 0) IncreaseMaxEnergy = 0;
                            break;
                        }
                        case 3:
                        {
                            IncreaseQuickBattleRewardTime = isEnable ? IncreaseQuickBattleRewardTime + d.BenefitValue
                                : IncreaseQuickBattleRewardTime - d.BenefitValue;
                            if (IncreaseQuickBattleRewardTime < 0) IncreaseQuickBattleRewardTime = 0;
                            break;
                        }
                        case 4:
                        {
                            IncreaseStageClearBaseReward = isEnable ? IncreaseStageClearBaseReward + d.BenefitValue
                                : IncreaseStageClearBaseReward - d.BenefitValue;
                            if (IncreaseStageClearBaseReward < 0) IncreaseStageClearBaseReward = 0;
                            break;
                        }
                        case 5:
                        {
                            IncreaseAutoBattleReward = isEnable ? IncreaseAutoBattleReward + d.BenefitValue
                                : IncreaseAutoBattleReward - d.BenefitValue;
                            if (IncreaseAutoBattleReward < 0) IncreaseAutoBattleReward = 0;
                            break;
                        }
                        case 6:
                        {
                            DecreaseEnergyChargeCoolDown = isEnable? DecreaseEnergyChargeCoolDown + d.BenefitValue
                                : DecreaseEnergyChargeCoolDown - d.BenefitValue;
                            if (DecreaseEnergyChargeCoolDown < 0) DecreaseEnergyChargeCoolDown = 0;
                            break;
                        }
                        case 7:
                        {
                            IncreaseCrackReward = isEnable ? IncreaseCrackReward + d.BenefitValue 
                                : IncreaseCrackReward - d.BenefitValue;
                            if (IncreaseCrackReward < 0) IncreaseCrackReward = 0;
                            break;
                        }
                        case 8:
                        {
                            IncreaseSelectSkillInitCount = isEnable ? IncreaseSelectSkillInitCount + d.BenefitValue
                                : IncreaseSelectSkillInitCount - d.BenefitValue;
                            if (IncreaseSelectSkillInitCount < 0) IncreaseSelectSkillInitCount = 0;
                            break;
                        }
                        case 9:
                        {
                            IncreaseEssenceReward = isEnable
                                ? IncreaseEssenceReward + d.BenefitValue
                                : IncreaseEssenceReward - d.BenefitValue;
                            if (IncreaseEssenceReward < 0) IncreaseEssenceReward = 0;
                            break;
                        }
                    }
                });
            }

            public void AddOrderHistory(List<OrderHistory> data)
            {
                data.ForEach(AddOrderHistory);
            }

            public void AddOrderHistory(OrderHistory orderHistory)
            {
                OrderHistory deleteOrderHistory = OrderHistories
                    .SingleOrDefault(d => d.package_id == orderHistory.package_id);

                if (deleteOrderHistory != null)
                    OrderHistories.Remove(deleteOrderHistory);

                OrderHistories.Add(orderHistory);
            }

            public List<PayInfo> GetPayInfoDisplayConditionType(int payTabType, ref List<int> subTypes)
            {
                foreach (var p in payinfoStartTimerIdDic)
                    TimerManager.Instance.StopTimer(p.Value);
                
                foreach (var p in payinfoEndTimerIdDic)
                    TimerManager.Instance.StopTimer(p.Value);
                
                payinfoStartTimerIdDic.Clear();
                payinfoEndTimerIdDic.Clear();
                
                List<PayInfo> payList = new();
                List<PayInfo> payInfoList = TableDataManager.Instance.data.PayInfo
                    .Where(t => t.TabType == payTabType).OrderBy(t => t.Order).ToList();

                if (payTabType == 1)
                {
                    subTypes = payInfoList.Select(d => d.SubType).Distinct().ToList();

                    foreach (int type in subTypes)
                    {
                        List<PayInfo> subPayInfo =
                            payInfoList.Where(d => d.SubType == type).OrderBy(d => d.Order).ToList();
                        payList = payList.Concat(subPayInfo).ToList();
                    }

                    payInfoList = payList;
                }
                else if (payTabType == 5)
                {
                    List<PayInfo> soldOutList = new();
                    List<PayInfo> notSoldOutList = new();

                    payInfoList.ForEach(p =>
                    {
                        OrderHistory orderHistory = UserDataManager.Instance.payInfo.OrderHistories
                            .SingleOrDefault(u => u.package_id == p.Index);

                        if (orderHistory == null)
                        {
                            notSoldOutList.Add(p);
                        }
                        else
                        {
                            if (p.PurchaseLimitCount <= orderHistory.count)
                            {
                                soldOutList.Add(p);
                            }
                            else
                            {
                                notSoldOutList.Add(p);
                            }
                        }
                    });
                    
                    payList.AddRange(notSoldOutList);
                    payList.AddRange(soldOutList);

                    payInfoList = payList;
                }

                List<PayInfo> customPayInfoList = new();
                
                foreach (var payInfo in payInfoList)
                {
                    if (payInfo.PurchaseStartTime.IsNullOrEmpty() == false
                        && payInfo.PurchaseEndTime.IsNullOrEmpty() == false)
                    {
                        int startTimerId = AddPayInfoPurchaseTime(payInfo.PurchaseStartTime);
                        int endTimerId = AddPayInfoPurchaseTime(payInfo.PurchaseEndTime);

                        if (0 < startTimerId)
                        {
                            payinfoStartTimerIdDic.Add(payInfo.ProductionName, startTimerId);
                            continue;
                        }

                        if (0 < endTimerId)
                            payinfoEndTimerIdDic.Add(payInfo.ProductionName, endTimerId);
                        else
                            continue;
                    }

                    if (payInfo.PurchaseConditionType == 2)
                    {
                        if (CheckPayInfoDisplayConditionType(payInfo))
                            customPayInfoList.Add(payInfo);
                    }
                    else if (payInfo.PurchaseConditionType == 1)
                    {
                        customPayInfoList.Add(payInfo);
                    }
                }

                return customPayInfoList;
            }

            public bool CheckPayInfoDisplayConditionType(PayInfo payInfo)
            {
                switch (payInfo.DisplayConditionType)
                {
                    case 1: // 계정 레벨
                    {
                        if (payInfo.DisplayConditionValue <= Instance.userInfo.GetPlayerData().level)
                            return true;
                        
                        break;
                    }
                    case 2: // 스테이지 클리어
                    {
                        int level = Instance.stageInfo.StageData.level_cap;
                        int chapter = Instance.stageInfo.StageData.chapter_id_cap;
                        Stage stage = TableDataManager.Instance.data.Stage
                            .SingleOrDefault(t => t.ChapterID == chapter && t.StageLevel == level);

                        if (stage != null && payInfo.DisplayConditionValue <= stage.Index)
                            return true;
                        
                        break;
                    }
                    case 3: // 특정 희귀도 장비 
                    {
                        foreach (var gear in Instance.Gears)
                        {
                            Equipment equipment = gear.Value.GetGearEquipment();

                            if (equipment != null && equipment.EquipRarity <= payInfo.DisplayConditionValue)
                                return true;
                        }

                        break;
                    }
                    default:
                        return true;
                }

                return false;
            }

            private int AddPayInfoPurchaseTime(string payInfoPurchaseTime)
            {
                DateTime nowTime = DateTime.UtcNow;
                DateTime purchaseTime = Convert.ToDateTime(payInfoPurchaseTime).AddHours(-9.0);
                TimeSpan remainTime = purchaseTime - nowTime;

                if (remainTime.TotalSeconds <= 0) return 0; 

                UnityEvent onComplete = new UnityEvent();
                onComplete.AddListener(() =>
                {
                    if (ContainerShop.Instance)
                        ContainerShop.Instance.ReloadDataCurrentTab();
                    
                    SpecialShopPopup specialShopPopup = UIManager.Instance.GetPopup(PopupName.SpecialShop) as SpecialShopPopup;
                    if (specialShopPopup) specialShopPopup.SetData();
                });

                int timerId =
                    TimerManager.Instance.AddTimer((float)remainTime.TotalSeconds, false, false, onComplete);

                return timerId;
            }

            public Timer GetPayInfoTimer(string productionName, bool isEndTimer = true)
            {
                Dictionary<string, int> selectTimer = isEndTimer ? payinfoEndTimerIdDic : payinfoStartTimerIdDic;

                selectTimer.TryGetValue(productionName, out int timerId);
                
                Timer timer = TimerManager.Instance.GetTimer(timerId);
                
                return timer;
            }
        }

        public class RandomBoxInfo
        {
            public Dictionary<int, RandomBox> RandomBoxes = new();

            public void AddRandomBox(List<RandomBox> randomBoxList)
            {
                randomBoxList.ForEach(AddRandomBox);
            }

            public void AddRandomBox(RandomBox randomBox)
            {
                RandomBoxes.Remove(randomBox.box_id);
                RandomBoxes.TryAdd(randomBox.box_id, randomBox);
            }
        }

        public class MissionInfo
        {
            public List<MissionData> MissionDataList = new();
            public List<AchievementInfo> AchievementInfoList = new();
            public List<Attendance> AttendanceList = new();

            public void AddAchievementInfo(List<AchievementInfo> achievementInfoList)
            {
                achievementInfoList.ForEach(AddAchievementInfo);
            }

            public void AddAchievementInfo(AchievementInfo achievementInfo)
            {
                List<AchievementInfo> deleteAchievementInfoList = AchievementInfoList
                    .Where(a => a.group == achievementInfo.group && a.level <= achievementInfo.level).ToList();

                deleteAchievementInfoList.ForEach(d =>
                {
                    AchievementInfoList.Remove(d);
                });

                AchievementInfoList.Add(achievementInfo);

                Achievement achievement = TableDataManager.Instance.data.Achievement
                    .Single(t => t.Group == achievementInfo.group && t.Level == achievementInfo.level);
                MissionCondition missionCondition = TableDataManager.Instance.data.MissionCondition
                    .Single(t => t.Index == achievement.ConditionID);
                int maxLevel = TableDataManager.Instance.data.Achievement
                    .Where(t => t.Group == achievementInfo.group)
                    .Select(t => t.Level).Max();

                InfVal conditionCount = InfVal.Parse(missionCondition.ConditionCount);
                InfVal achievementCount = InfVal.Parse(achievementInfo.count);

                if (conditionCount <= achievementCount && achievementInfo.reward_level < maxLevel)
                {
                    achievementInfo.stateType = 1; // 미션 완료, 보상 받기 전
                }
                else if (achievementCount <= -1 && maxLevel == achievementInfo.reward_level)
                {
                    achievementInfo.stateType = 2; // 미션 완료, 보상 받음
                }
                else
                {
                    achievementInfo.stateType = 0; // 진행중
                }
            }
            
            public void AddMissionData(List<MissionData> missionDataList)
            {
                missionDataList.ForEach(AddMissionData);
            }

            public void AddMissionData(MissionData missionData)
            {
                MissionData deleteMissionData = MissionDataList
                    .SingleOrDefault(d => d.index == missionData.index);

                if (deleteMissionData != null)
                    MissionDataList.Remove(deleteMissionData);
                
                MissionDataList.Add(missionData);

                Mission mission = TableDataManager.Instance.data.Mission
                    .Single(t => t.Index == missionData.index);

                MissionCondition missionCondition = TableDataManager.Instance.data.MissionCondition
                        .Single(t => t.Index == mission.Index);
                
                InfVal conditionCount = InfVal.Parse(missionCondition.ConditionCount);
                InfVal missionDataCount = InfVal.Parse(missionData.count);
                
                if (conditionCount <= missionDataCount)
                    missionData.stateType = 1; // 미션 완료, 보상 받기 전
                else if (missionDataCount <= -1)
                    missionData.stateType = 2; // 미션 완료, 보상 받음
                else
                    missionData.stateType = 0; // 미션 진행중
            }

            public void AddAttendanceData(List<Attendance> attendanceDataList)
            {
                attendanceDataList.ForEach(AddAttendanceData);
            }

            public void AddAttendanceData(Attendance attendance)
            {
                Attendance deleteAttendance = AttendanceList
                    .SingleOrDefault(a => a.index == attendance.index);

                if (deleteAttendance != null)
                    AttendanceList.Remove(deleteAttendance);
                
                AttendanceList.Add(attendance);
            }

            public bool IsGetMissionReward()
            {
                return MissionDataList.Any(m => m.stateType == 1)
                       || AchievementInfoList.Any(a => a.stateType == 1);
            }

            public bool IsGetAttendanceReward()
            {
                return AttendanceList.Any(a => a.is_check == false);
            }
        }

        [Serializable]
        public class ClientInfo
        {
            public List<int> ClearGroupID = new();
            public List<int> unLockID = new();
            public Subject<int> DataUpdate = new Subject<int>();
            public void LinkTutorialData(ClientInfo data)
            {
                this.ClearGroupID = data.ClearGroupID;
                this.unLockID = data.unLockID;
            }

            public void AddClearGroup(int group)
            {
                if (ClearGroupID.Contains(group))
                {
                    return;
                }
                
                ClearGroupID.Add(group);
                
                var payload = new Dictionary<string, object>
                {
                    { "type", 1 },
                    { "data" , JObject.FromObject(this).ToString()},
                };
                APIRepository.RequestClientInfo(payload, null);
            }

            public void AllClear()
            {
                ClearGroupID = new List<int>() { 0, 10, 11, 12, 20, 30, 40, 50, 60 };
                var payload = new Dictionary<string, object>
                {
                    { "type", 1 },
                    { "data" , JObject.FromObject(this).ToString()},
                };
                APIRepository.RequestClientInfo(payload, null);
            }

            public void Reset()
            {
                ClearGroupID = new List<int>() { 0 };
                unLockID = new List<int>();
                var payload = new Dictionary<string, object>
                {
                    { "type", 1 },
                    { "data" , JObject.FromObject(this).ToString()},
                };
                APIRepository.RequestClientInfo(payload, null);
            }

            // reutnr -1 NULL
            public int NextGroupID()
            {
                if (!ClearGroupID.Any())
                {
                    // goto Intro
                    return -1;
                }


                if (!ClearGroupID.Contains(12))
                {
                    // 기본전투 조작
                    return 10;
                }

                var lastClearGroupID = ClearGroupID.Max();
                var group = TableDataManager.Instance.data.TutorialType.FirstOrDefault(t => t.ConditionValue == lastClearGroupID);
                if (group != null) return group.TutorialGroupID;
                
                if (lastClearGroupID == 13)
                {
                    return 20;
                }

                return -100;
            }

            // 언락타입체크
            public bool GetUnlockData(int type)
            {
                return unLockID.Contains(type);
            }
            
            public void AddShowUnlockPopup(int type)
            {
                if (unLockID.Contains(type))
                {
                    return;
                }
                
                unLockID.Add(type);
                
                var payload = new Dictionary<string, object>
                {
                    { "type", 1 },
                    { "data" , JObject.FromObject(this).ToString()},
                };
                
                APIRepository.RequestClientInfo(payload, (t) =>
                {
                    DataUpdate.OnNext(1);
                });
            }
        }

        public class PlayerRankInfo
        {
            public PlayerRankData[] rankDataList = new PlayerRankData[3];

            public void AddRankingInfo(PlayerRankData playerRankData)
            {
                rankDataList[playerRankData.type - 1] = playerRankData;
            }
        }
        
        public class UserCardInfo
        {
            public UserCardInfo()
            {
                userCardDataList = new List<UserCardData>();
                
                foreach (var card in TableDataManager.Instance.data.Card)
                {
                    UserCardData userCardData = new();
                    userCardData.card = card;
                    userCardData.ClearData();
                    userCardDataList.Add(userCardData);
                }
            }
            
            public List<UserCardData> userCardDataList;

            public void ClearUserCardInfoAll()
            {
                userCardDataList.ForEach(u => u.ClearData());
            }
            
            public void AddHeroCard(HeroCard heroCard)
            {
                if (heroCard == null) return;
                
                var userCardData = userCardDataList.SingleOrDefault(u => u.card.Index == heroCard.item_id);
                
                if (userCardData == null) return;
                
                userCardData.ClearData();
                userCardData.level = heroCard.level;
                userCardData.count = heroCard.count;
                userCardData.awake = heroCard.awake;
                userCardData.isHave = true;
                userCardData.SetUseAwake();
            }

            public void AddHeroCard(List<HeroCard> heroCardList)
            {
                if (heroCardList.IsNullOrEmpty()) return;
                
                heroCardList.ForEach(AddHeroCard);
            }

            // public void AddEquipHeroCard(EquipHeroCard equipHeroCard)
            // {
            //     if (equipHeroCard == null) return;
            // }

            public void AddEquipHeroCard(List<EquipHeroCard> equipHeroCardList)
            {
                userCardDataList.ForEach(u =>
                {
                    u.equipIndex = 0;
                    u.isEquip = false;
                    u.isEquiping = false;
                });
                
                if (equipHeroCardList.IsNullOrEmpty()) return;

                for (var i = 0; i < equipHeroCardList.Count; ++i)
                {
                    var userCardData = userCardDataList
                        .SingleOrDefault(u => u.card.Index == equipHeroCardList[i].item_id);

                    if (userCardData == null) continue;
                    
                    userCardData.equipIndex = i + 1;
                    userCardData.isEquip = true;
                }
            }
        }
    }
    
    // 카드 정보 커스텀 클래스
    public class UserCardData
    {
        public Card card;
        public int level;
        public int count;
        public int awake;
        public bool isHave;
        public bool isEquip;
        public bool isEquiping;
        public bool isUseAwake;
        public int equipIndex;

        public void ClearData()
        {
            level = 0;
            count = 0;
            awake = 0;
            isHave = false;
            isEquip = false;
            isUseAwake = false;
            equipIndex = 0;
        }

        public void SetUseAwake()
        {
            CardRarityType cardRarityType = TableDataManager.Instance.data.CardRarityType
                .Single(t => t.Index == card.CardRarity);
            CardAwakening cardAwakening = TableDataManager.Instance.data.CardAwakening
                .Single(t => t.GroupID == cardRarityType.CardAwakeningGroupID && t.Level == awake);

            isUseAwake = cardAwakening.CardConsumeCount <= count;
        }
}
    
    // 카드 오름 차순 정렬
    public class CardSortAsc : IComparer<UserCardData>
    {
        public int Compare(UserCardData a, UserCardData b)
        {
            if (a.card == b.card) return 0;
            if (a.card == null) return -1;
            if (b.card == null) return 1;

            var ret1 = a.card.CardRarity.CompareTo(b.card.CardRarity);
            var ret2 = ret1 != 0 ? ret1 : a.awake.CompareTo(b.awake);
            var ret3 = ret2 != 0 ? ret2 : a.level.CompareTo(b.level);
            var ret4 = ret3 != 0 ? ret3 : a.card.Index.CompareTo(b.card.Index);

            return ret4;
        }
    }
    
    // 카드 내림 차순 정렬
    public class CardSortDesc : IComparer<UserCardData>
    {
        public int Compare(UserCardData a, UserCardData b)
        {
            if (a.card == b.card) return 0;
            if (b.card == null) return -1;
            if (a.card == null) return 1;

            var ret1 = b.card.CardRarity.CompareTo(a.card.CardRarity);
            var ret2 = ret1 != 0 ? ret1 : b.awake.CompareTo(a.awake);
            var ret3 = ret2 != 0 ? ret2 : b.level.CompareTo(a.level);
            var ret4 = ret3 != 0 ? ret3 : b.card.Index.CompareTo(a.card.Index);

            return ret4;
        }
    }
}
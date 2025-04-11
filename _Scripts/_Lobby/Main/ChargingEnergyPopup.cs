using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using Sirenix.Utilities;
using TimerTool;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class ChargingEnergyPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.ChargingEnergy;

        public UIButton closeBtn;
        [Space(20)]
        public List<Image> energyIconList;
        public Image buyGoodsIconImg;
        [Space(20)]
        public List<TMP_Text> countTxtList;
        public TMP_Text buyGoodsCountTxt;
        [Space(20)]
        public List<TMP_Text> dayBuyCountTxtList;
        [Space(20)]
        public List<UIButton> buyBtnList;
        [Space(20)]
        public Transform adDisable;
        public TMP_Text adDisableRemainTimeTxt;
        
        private IDisposable _adDisableDisposable; 
        
        private List<PayInfo> _payInfoList = new();

        protected override void Init()
        {
        }

        private void Start()
        {
            closeBtn.AddEvent(Hide);

            List<PayInfo> payInfoList = TableDataManager.Instance.data.PayInfo.Where(t => t.TabType == 99).ToList();
            
            if (payInfoList.Count == 2)
            {
                _payInfoList.Add(payInfoList.First(p => p.PriceID != 99));
                _payInfoList.Add(payInfoList.First(p => p.PriceID == 99));

                // for (int i = 0; i < _payInfoList.Count; ++i)
                // {
                //     int index = i;
                //     buyBtnList[index].AddEvent(() => OnBuyEnergy(_payInfoList[index]));
                // }
            }

            SetData();
        }

        public void SetData()
        {
            if (_payInfoList.IsNullOrEmpty() || _payInfoList.Count < 2) return;
            
            buyBtnList.ForEach(b => b.ClearEvent());
            
            for (int i = 0; i < _payInfoList.Count; ++i)
            {
                int index = i;
                buyBtnList[index].AddEvent(() => OnBuyEnergy(_payInfoList[index]));
            }

            for (int i = 0; i < _payInfoList.Count; ++i)
            {
                buyBtnList[i].interactable = true;
                
                energyIconList[i].sprite = ResourcesManager.Instance
                    .GetAtlas(MyAtlas.Summon_Shop).GetSprite(_payInfoList[i].Icon);

                RewardGroup rewardGroup = TableDataManager.Instance.data.RewardGroup
                    .SingleOrDefault(t => t.GroupID == _payInfoList[i].RewardGroupID);

                countTxtList[i].SetText(LocaleManager.GetLocale("Common_Reward_Count", rewardGroup.RewardMaxCnt));

                OrderHistory orderHistory = UserDataManager.Instance.payInfo.OrderHistories
                    .SingleOrDefault(u => u.package_id == _payInfoList[i].Index);

                if (orderHistory == null)
                {
                    dayBuyCountTxtList[i].SetText(LocaleManager.GetLocale("Purchase_Limit_Type_1",
                        0, _payInfoList[i].PurchaseLimitCount));
                    
                    buyBtnList[i].interactable = true;
                }
                else
                {
                    dayBuyCountTxtList[i].SetText(LocaleManager.GetLocale(
                        "Purchase_Limit_Type_1", orderHistory.count, _payInfoList[i].PurchaseLimitCount));

                    if (_payInfoList[i].PurchaseLimitCount <= orderHistory.count)
                        buyBtnList[i].interactable = false;
                    else
                        buyBtnList[i].interactable = true;
                }
            }

            GoodsType goodsType = TableDataManager.Instance.data.GoodsType
                .Single(t => t.TypeID == _payInfoList[0].PriceID);

            buyGoodsIconImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite(goodsType.Icon);
            InfVal goodsCount = _payInfoList[0].Price;
            buyGoodsCountTxt.SetText(goodsCount.ToGoodsString());
            
            if (TimerManager.Instance.GetAdTimer(TimerManager.AdTimerType.ENERGY_CHARGE) != null)
            {
                buyBtnList[1].ClearEvent();
                buyBtnList[1].interactable = false;
                adDisable.SetActive(true);
                
                if (_adDisableDisposable == null)
                    _adDisableDisposable = this.UpdateAsObservable().Subscribe(_ => UpdateAdRemainTime()).AddTo(this);
            }
            else
            {
                buyBtnList[1].AddEvent(() => OnBuyEnergy(_payInfoList[1]));
                adDisable.SetActive(false);
            }
        }
        
        private void UpdateAdRemainTime()
        {
            Timer adTimer = TimerManager.Instance.GetAdTimer(TimerManager.AdTimerType.ENERGY_CHARGE);
            if (adTimer == null) return;

            TimeSpan remainTime = TimeSpan.FromSeconds(adTimer.GetTimeRemaining());
            if (remainTime.TotalSeconds <= 0)
            {
                _adDisableDisposable?.Dispose();
                _adDisableDisposable = null;
                
                TimerManager.Instance.RemoveAdTimer(TimerManager.AdTimerType.ENERGY_CHARGE);
                return;
            }
            
            adDisableRemainTimeTxt.SetText($"{remainTime.Minutes:D2}"+":"+$"{remainTime.Seconds:D2}");
        }

        private void OnBuyEnergy(PayInfo payInfo)
        {
            OrderHistory orderHistory = UserDataManager.Instance.payInfo.OrderHistories
                .SingleOrDefault(u => u.package_id == payInfo.Index);

            if (payInfo.PriceID == 99)
            {
                if (orderHistory != null)
                {
                    if (payInfo.PurchaseLimitCount <= orderHistory.count)
                    {
                        Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                        alarm.InitBuilder()
                            .SetMessage(LocaleManager.GetLocale("Purchase_limits_info"))
                            .Build();
                        
                        return;
                    }
                }
                
                AdMobManager.Instance.ShowAD(() =>
                {
                    var payload = new Dictionary<string, object>
                    {
                        { "index", payInfo.Index }
                    };

                    APIRepository.RequestShopBuy(payload, data =>
                    {
                        List<ScrollDataModel> rewardDatas = APIRepository.ConvertReward(data.box_rewards);
                        CommonRewardPopup commonRewardPopup = UIManager.Instance.Get(PopupName.Common_Reward) as CommonRewardPopup;
                        commonRewardPopup.AddShowCallback(SetData);
                        commonRewardPopup.SetData(rewardDatas);
                        commonRewardPopup.Show();
                    }, reply =>
                    {
                        Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                        alarm.AddShowCallback(SetData);
                        alarm.InitBuilder()
                            .SetMessage(LocaleManager.GetLocale("No_response_server"))
                            .Build();
                    });
                },
                () =>
                {
                    Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    alarm.AddShowCallback(SetData);
                    alarm.InitBuilder()
                        .SetMessage(LocaleManager.GetLocale("No_Ad"))
                        .Build();
                },
                () =>
                {
                    Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    alarm.AddShowCallback(SetData);
                    alarm.InitBuilder()
                        .SetMessage(LocaleManager.GetLocale("Ad_Not_Completed"))
                        .Build();
                }, TimerManager.AdTimerType.ENERGY_CHARGE);
            }
            else
            {
                if (orderHistory != null)
                {
                    if (payInfo.PurchaseLimitCount <= orderHistory.count)
                    {
                        Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                        alarm.InitBuilder()
                            .SetMessage(LocaleManager.GetLocale("Purchase_limits_info"))
                            .Build();

                        return;
                    }
                }
                
                if (UserDataManager.Instance.currencyInfo
                        .ValidGoods((CurrencyType)payInfo.PriceID, payInfo.Price) == false)
                {
                    GoodsType goodsType = TableDataManager.Instance.data.GoodsType
                        .Single(t => t.TypeID == payInfo.PriceID);

                    Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    alarm.InitBuilder()
                        .SetMessage(LocaleManager.GetLocale("Not_Enough_Price", 
                            LocaleManager.GetLocale(goodsType.Name)))
                        .Build();
                }
                else
                {
                    ShopPurchasePopup shopPurchasePopup = UIManager.Instance.Get(PopupName.Shop_Purchase) as ShopPurchasePopup;
                    shopPurchasePopup.AddHideCallback(SetData);
                    shopPurchasePopup.SetData(payInfo);
                    shopPurchasePopup.Show();
                }
            }
        }
    }
}
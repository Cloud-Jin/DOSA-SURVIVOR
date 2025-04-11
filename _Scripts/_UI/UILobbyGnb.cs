using System;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Unity.VisualScripting;

namespace ProjectM
{
    public class UILobbyGnb : MonoBehaviour
    {
        public Image expGage;
        public Image portraitImg;
        [Space(20)]
        public TMP_Text Nickname, AccountLv;
        public TMP_Text GoldCount, EnergyCount, ObsidianCount, GemCount, GuardianEssenceCount, ExplorationCloudCount;
        public TMP_Text EnergyTime;
        [Space(20)]
        public UIButton chargingDiaBtn;
        public UIButton chargingGoldBtn;
        public UIButton chargingEnergyBtn;
        public UIButton profileBtn;
        [Space(20)]
        public Transform portraitRedDot;
        
        private int rechargeSec, maxEnergy;
        private bool pauseTime, isMaxEnergy;

        public void Start()
        {
            pauseTime = true;
            
            var userData = UserDataManager.Instance;
            Nickname.SetText(userData.userInfo.PlayerData.player.name);
            maxEnergy = TableDataManager.Instance.GetConfig(20).Value
                        + UserDataManager.Instance.payInfo.IncreaseMaxEnergy;
            
            userData.currencyInfo.SubscribeItemRx(CurrencyType.Gold, (i) => GoldCount.SetText(i.ToGoldString())).AddTo(this);
            userData.currencyInfo.SubscribeItemRx(CurrencyType.Energy, (i) => EnergyCount.SetText($"{i.ToGoodsString()}/{maxEnergy}" )).AddTo(this);
            userData.currencyInfo.SubscribeItemRx(CurrencyType.Obsidian, (i) => ObsidianCount.SetText(i.ToGoodsString())).AddTo(this);
            userData.currencyInfo.SubscribeItemRx(CurrencyType.Gem, (i) => GemCount.SetText(i.ToGoodsString())).AddTo(this);
            userData.currencyInfo
                .SubscribeItemRx(CurrencyType.Guardian_Essence, i => GuardianEssenceCount.SetText(i.ToGoodsString()))
                .AddTo(this);
            userData.currencyInfo
                .SubscribeItemRx(CurrencyType.Exploration_Cloud, i => ExplorationCloudCount.SetText(i.ToGoodsString()))
                .AddTo(this);
            
            rechargeSec = TableDataManager.Instance.GetConfig(21).Value;
            userData.currencyInfo.SubscribeItemRx(CurrencyType.Energy, (i) =>
            {
                isMaxEnergy = i >= maxEnergy;
                NotificationsManager.Instance.SetNotificationType(3);
            }).AddTo(this);
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1f)).Where(t=> pauseTime).Subscribe(EnegyRechargeTime).AddTo(this);
            
            // 레벨에 따른 경험치 % 표기.
            userData.userInfo.PlayerDataUpdate.Subscribe(t =>
            {
                int level = userData.userInfo.PlayerData.player.level;
                AccountLv.SetText($"{level}");
                var tbExp = TableDataManager.Instance.data.AccountLevel.Single(t => t.Level == level).Exp;
                expGage.fillAmount = userData.userInfo.PlayerData.player.experience / (float)tbExp;
                Nickname.SetText(userData.userInfo.PlayerData.player.name);
                ReloadPlayerData();
            }).AddTo(this);
            userData.userInfo.PlayerDataUpdate.OnNext(1);
            
            chargingDiaBtn.AddEvent(ShowChargingDiaPopup);
            chargingGoldBtn.AddEvent(GoChargingGold);
            chargingEnergyBtn.AddEvent(GoChargingEnergy);
            profileBtn.AddEvent(OnClickProfile);

            ReloadPlayerData();
        }
        
        private void ShowChargingDiaPopup()
        {
            ChargingDiaPopup chargingDiaPopup = UIManager.Instance.Get(PopupName.ChargingDia) as ChargingDiaPopup;
            
            GuardianPopup guardianPopup = UIManager.Instance.GetPopup(PopupName.Guardian) as GuardianPopup;
            if (guardianPopup) chargingDiaPopup.SetOverrideSorting(10020);

            chargingDiaPopup.Show();
        }
        
        private void GoChargingGold()
        {
            if (UILobby.Instance)
            {
                UILobby.Instance.SetSelectContentsTabs(LobbyTap.Shop);

                if (ContainerShop.Instance)
                    ContainerShop.Instance.SetSelectCategoryTab(3);
                
                GuardianPopup guardianPopup = UIManager.Instance.GetPopup(PopupName.Guardian) as GuardianPopup;
                if (guardianPopup) guardianPopup.Hide();
            }
        }

        private void GoChargingEnergy()
        {
            ChargingEnergyPopup chargingEnergyPopup = UIManager.Instance.Get(PopupName.ChargingEnergy) as ChargingEnergyPopup;
            if (chargingEnergyPopup)
            {
                chargingEnergyPopup.Show();
            }
        }

        void EnegyRechargeTime(long i)
        {
            var timeSpan = DateTime.UtcNow - UserDataManager.Instance.userInfo.PlayerData.player.energy_at;
            if (isMaxEnergy)
            {
                EnergyTime.SetActive(false);
                return;
            }
            
            // 10분 텀보다 크면 빼자.
            if (timeSpan.TotalSeconds >= rechargeSec)
            {
                pauseTime = false;
                //충전
                EnergyTime.SetActive(false);
                APIRepository.RequestCheckEnegy(box =>
                {
                    pauseTime = true;
                });
                return;
            }


            var RechargeGap = TimeSpan.FromSeconds(rechargeSec);

            var _time = (RechargeGap - timeSpan).TotalSeconds;
            
            int _hour = (int)(_time / 3600);
            int _min = (int)(_time % 3600 / 60);
            int _sec = (int)(_time % 3600 % 60);
            
            // string saveTime = String.Format("{0:D2} : {1:D2} : {2:D2}",_hour,_min,_sec);
            // string saveTime = String.Format("{0:D2} : {1:D2}",_hour,_min);
            string saveTime = String.Format("{0:D2} : {1:D2} ",_min,_sec);
            EnergyTime.SetText(saveTime);
            EnergyTime.SetActive(true);
        }

        public void ReloadMaxEnergy()
        {
            maxEnergy = TableDataManager.Instance.GetConfig(20).Value
                        + UserDataManager.Instance.payInfo.IncreaseMaxEnergy;

            if (UserDataManager.Instance.currencyInfo.data.TryGetValue(CurrencyType.Energy, out var value))
            {
                InfVal nowEnergy = value.Value;
                UserDataManager.Instance.currencyInfo.data[CurrencyType.Energy].SetValueAndForceNotify(nowEnergy);
            }
        }

        public void EnableChargingBtn(bool isEnable)
        {
            chargingDiaBtn.SetActive(isEnable);
            chargingGoldBtn.SetActive(isEnable);
            chargingEnergyBtn.SetActive(isEnable);
        }
        
        private void ReloadPlayerData()
        {
            int portraitID = UserDataManager.Instance.userInfo.PlayerData.player.portrait_id;
            Portrait portrait = TableDataManager.Instance.data.Portrait.Single(t => t.Index == portraitID);
            portraitImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(portrait.Icon);
 
            SetPortaitRedDot();
        }

        public void SetPortaitRedDot()
        {
            portraitRedDot.SetActive(false);
            
            TableDataManager.Instance.data.Portrait.ForEach(t =>
            {
                if (PlayerPrefs.GetInt(t.Icon, 0) == 1)
                    portraitRedDot.SetActive(true);
            });
        }

        void OnClickProfile()
        {
            GuardianPopup guardianPopup = UIManager.Instance.GetPopup(PopupName.Guardian) as GuardianPopup;
            if (guardianPopup) return;

            var popup = UIManager.Instance.Get(PopupName.Lobby_Profile);
            popup.Show();
        }
    }
}

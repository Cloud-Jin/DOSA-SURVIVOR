using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Phoenix;
using ProjectM.AutoBattle;
using Sirenix.Utilities;
using UnityEngine;

namespace ProjectM
{
    public partial class APIRepository
    {
        // 계정 로그인
        public static void RequestSnsLogin(Dictionary<string, object> payload, Action<JsonBox> onResponse, Action<Reply> onFail)
        {
            Action<JsonBox> resp = data =>
            {
                if (string.IsNullOrEmpty(data.Element.ToString())) 
                    return;
                
                {
                    PlayerPrefs.SetString(MyPlayerPrefsKey.IdToken, data.Element.ToString());
                    PlayerPrefs.Save();

                    Debug.Log($"토큰 저장 : {data.Element.ToString()}");
                }
            };
            resp += onResponse;

            Net.JoinUserChannel(payload, resp, onFail);
        }
        
        // 계정연동 게스트 ->  Social
        public static void RequestSnsLink(Dictionary<string, object> payload, Action<JsonBox> onResponse, Action<Reply> onFail)
        {
            Action<JsonBox> resp = data =>
            {
               
            };
            resp += onResponse;

            Net.PushUserChannel("federation_link", payload, resp, false, onFail);
        }
        
        // 계정탈퇴
        public static void RequestSnsRevoke(Dictionary<string, object> payload, Action<JsonBox> onResponse, Action<Reply> onFail)
        {
            Action<JsonBox> resp = data =>
            {
               
            };
            resp += onResponse;

            Net.PushUserChannel("federation_revoke", payload, resp, false, onFail);
        }

        public static void RequestClientInfo(Dictionary<string, object> payload, Action<JsonBox> onResponse)
        {
            // 1번 튜토리얼만 사용 추가시 서버문의
            // var payload = new Dictionary<string, object>
            // {
            //     { "type", 1 },
            //     { "data" , 1},
            // };
            
            // 나중에 튜토리얼 정보 등.
            // DB 저장용
            Action<JsonBox> resp = data =>
            {
                // Debug.Log("클라이언트 정보");
            };
            resp += onResponse;

            Net.PushUserChannel("client_info",payload, resp, false);
        }
        
        public static void RequestPlayer(Action<UserAccount> onResponse)
        {
            Action<UserAccount> resp = resp =>
            {
                UserDataManager.Instance.userInfo.PlayerData = resp;
            };
            resp += onResponse;

            Net.PushUserChannel("player", resp, false);
        }
        
        public static void RequestCheckEnegy(Action<CheckEnegyData> onResponse)
        {
            Action<CheckEnegyData> resp = data =>
            {
                if(data.energy_at > DateTime.MinValue)
                    UserDataManager.Instance.userInfo.PlayerData.player.energy_at = data.energy_at;
                
                UserDataManager.Instance.currencyInfo.SetItem(data.currencies);
                
            };
            resp += onResponse;

            Net.PushUserChannel("check_enegy", resp, false);
        }
        
        public static void RequestConfirmPostBox(Dictionary<string, object> payload, Action<CommonApiResultData> onResponse)
        {
            Action<CommonApiResultData> resp = data =>
            {
                data.SetData();
                
                if (data.gears.IsNullOrEmpty() == false)
                    UserDataManager.Instance.gearInfo.SetGearTotalLevelUpCheck(data.gears);
            };
            resp += onResponse;
         
            Net.PushUserChannel("post_confirm", payload, resp, true);
        }
        
        public static void RequestConfirmAllPostBox(Action<CommonApiResultData> onResponse)
        {
            Action<CommonApiResultData> resp = data =>
            {
                data.SetData();
                
                UserDataManager.Instance.postBoxInfo.PostBoxes.ForEach(d => d.is_confirm = true);
                UserDataManager.Instance.postBoxInfo.SortPostBoxes();
                
                if (data.gears.IsNullOrEmpty() == false)
                    UserDataManager.Instance.gearInfo.SetGearTotalLevelUpCheck(data.gears);
            };
            resp += onResponse;
         
            Net.PushUserChannel("post_confirm", resp, true);
        }

        public static void RequestRemovePostBox()
        {
            Action<CommonApiResultData> resp = data =>
            {
            };

            Net.PushUserChannel("post_hide", resp, true);
        }

        public static void RequestMissionReward(Dictionary<string, object> payload, Action<CommonApiResultData> onResponse, Action<Reply> error)
        {
            Action<CommonApiResultData> resp = data =>
            {
                data.SetData();
            };
            resp += onResponse;

            Net.PushUserChannel("mission_complete", payload, resp, true, error);
        }
        
        public static void RequestAttendanceReward(Dictionary<string, object> payload, Action<AttendanceData> onResponse)
        {
            Action<AttendanceData> resp = data =>
            {
                if (data.attendance != null)
                    UserDataManager.Instance.missionInfo.AddAttendanceData(data.attendance);
                
                if (data.attendances.IsNullOrEmpty() == false)
                    UserDataManager.Instance.missionInfo.AddAttendanceData(data.attendances);
                
                if (data.currencies.IsNullOrEmpty() == false) 
                    UserDataManager.Instance.currencyInfo.SetItem(data.currencies);
            };
            resp += onResponse;

            Net.PushUserChannel("attendance_confirm", payload, resp, true);
        }
        
        public static void SubscriptionPostPoxes(CommonApiResultData data)
        {
            data.SetData();

            PostBoxPopup postBoxPopup = UIManager.Instance.GetPopup(PopupName.Post_Box) as PostBoxPopup;
            if (postBoxPopup) postBoxPopup.SetData();
            
            if (LobbyMain.Instance)
                LobbyMain.Instance.ReloadLobbyRedDot();
        }

        public static void SubscriptionOrderHistories(CommonApiResultData data)
        {
            data.SetData();
            
            if (ContainerShop.Instance)
                ContainerShop.Instance.ReloadDataCurrentTab();
            
            ChargingEnergyPopup chargingEnergyPopup = UIManager.Instance.GetPopup(PopupName.ChargingEnergy) as ChargingEnergyPopup;
            if (chargingEnergyPopup) chargingEnergyPopup.SetData();
        }

        public static void SubscriptionMemberships()
        {
            UserDataManager.Instance.payInfo.MembershipEnableCheck();
            
            if (ContainerShop.Instance)
                ContainerShop.Instance.ReloadDataCurrentTab();
            
            if (UILobby.Instance)
                UILobby.Instance.LobbyGnb.ReloadMaxEnergy();

            PopupBattleAutoFast popupBattleAutoFast = UIManager.Instance.GetPopup(PopupName.AutoBattle_BattleAutoFast) as PopupBattleAutoFast;
            if (popupBattleAutoFast)
                popupBattleAutoFast.ReloadAutoBattleMaxCount();

            GuardianPopup guardianPopup = UIManager.Instance.GetPopup(PopupName.Guardian) as GuardianPopup;
            if (guardianPopup)
                guardianPopup.SetExplorationData();
        }

        
        public static void SubscriptionPortraits()
        {
            UserDataManager.Instance.userProfileInfo.portraitList.ForEach(u =>
            {
                Portrait portrait = TableDataManager.Instance.data.Portrait
                    .Single(t => t.Index == u);

                if (portrait.Index == 1)
                {
                    PlayerPrefs.SetInt(portrait.Icon, 2);
                    PlayerPrefs.Save();
                }
            });
            
            if (UILobby.Instance)
                UILobby.Instance.LobbyGnb.SetPortaitRedDot();
            
            PopupProfile popup = UIManager.Instance.GetPopup(PopupName.Lobby_Profile) as PopupProfile;
            if (popup) popup.SetDataPortrait();
        }

        public static void RequestNickChange(Dictionary<string, object> payload, Action<JObject> onResponse, Action<Reply> onFail)
        {
            Action<JObject> resp = data =>
            {
                if (!string.IsNullOrEmpty(data.ToString()))
                {
                    var cu = data.ToObject<CurrenciesData>();
                    if (cu.Currencies != null)
                    {
                        foreach (var value in cu.Currencies)
                        {
                            UserDataManager.Instance.currencyInfo.SetItem(value);
                        }
                    }
                }

                UserDataManager.Instance.userInfo.GetPlayerData().name = payload["name"].ToString();
                UserDataManager.Instance.userInfo.PlayerDataUpdate.OnNext(1);
            };
            resp += onResponse;

            Net.PushUserChannel("player_name_change", payload, resp, true, onFail);
        }
        
        public static void RequestTraitLevelUp(Dictionary<string, object> payload, Action<TraitInfoData> onResponse, Action<Reply> onFail)
        {
            // tab, index
            
            Action<TraitInfoData> resp = data =>
            {
                UserDataManager.Instance.userTraitInfo.SetData(data);
                UserDataManager.Instance.userTraitInfo.apUse.Value = data.trait_point_use;
            };
            resp += onResponse;

            Net.PushUserChannel("trait_level_up", payload, resp, true, onFail);
        }
        
        public static void RequestTraitReset(Action<TraitInfoData> onResponse, Action<Reply> onFail)
        {
            Action<TraitInfoData> resp = data =>
            {
                UserDataManager.Instance.userTraitInfo.SetData(data);
                UserDataManager.Instance.userTraitInfo.apUse.Value = data.trait_point_use;
                if (data.currencies.IsNullOrEmpty() == false) UserDataManager.Instance.currencyInfo.SetItem(data.currencies);
            };
            resp += onResponse;

            Net.PushUserChannel("trait_reset", resp, true, onFail);
        }

        public static void RequestRankingInfo(Dictionary<string, object> payload, Action<PlayerRankData> onResponse, Action<Reply> onFail)
        {
            Action<PlayerRankData> resp = data =>
            {
                UserDataManager.Instance.playerRankInfo.AddRankingInfo(data);
            };

            resp += onResponse;
            
            Net.PushUserChannel("player_ranks", payload, resp, true, onFail);
        }
        
        public static void RequestPortraitChange(Dictionary<string, object> payload, Action<UserAccount> onResponse, Action<Reply> onFail)
        {
            Action<UserAccount> resp = data =>
            {
                UserDataManager.Instance.userInfo.SetPlayerData(data.player);
            };
            
            resp += onResponse;
            
            Net.PushUserChannel("portrait_change", payload, resp, true, onFail);
        }
    }
}
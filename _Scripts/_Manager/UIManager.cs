using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using Phoenix;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using UnityEngine.Rendering.Universal;

namespace ProjectM
{
    public class UIManager : Singleton<UIManager>
    {
        public Camera UICamera;
        public Transform mainCanvas;
        
        private Dictionary<ViewName, string> dic_UIView = new Dictionary<ViewName, string>();
        private Dictionary<PopupName, string> dic_UIPopup = new Dictionary<PopupName, string>();

        public ReactiveCollection<Popup> popups;
        public ReactiveCollection<Popup> alarms;

        private Queue<Action> pendingPopups = new Queue<Action>();
        public bool EnableTouch { get; set; }

        private LoadingPopup _loadingPopup;

        protected override void Init()
        {
            Debug.Log("UI manager Init");
            EnableTouch = true;
            
            dic_UIView = new Dictionary<ViewName, string>();
            dic_UIPopup = new Dictionary<PopupName, string>();
            popups = new ReactiveCollection<Popup>();
            alarms = new ReactiveCollection<Popup>();

            TitleInstanceGameObject();
            LobbyInstanceGameObject();
            BattleInstanceGameObject();
            initComplete = true;
            Debug.Log("UI manager InitComplete");
        }

        public void TitleInstanceGameObject()
        {
            Add(ViewName.Title_Main, "UI_Start_Login");
            Add(PopupName.Common_Confirm,"UI_Popup_Common_Confirm");
            Add(PopupName.Common_Account, "UI_Popup_Account");
        }

        public void LobbyInstanceGameObject()
        {
            Add(ViewName.Lobby_Main, "UI_Main");
            Add(ViewName.Common_Loading, "UI_Loading");
            Add(PopupName.Gear_Select, "UI_Popup_ItemSellect");
            Add(PopupName.Gear_LimitBreak, "UI_Popup_Getover");
            Add(PopupName.Gear_Compose, "UI_Popup_Compose");
            Add(PopupName.Gear_GradeProbability, "UI_Popup_Probability");
            Add(PopupName.Gear_OptionAutoChange, "UI_Popup_ChangeAutoSetting");
            Add(PopupName.Gear_Equip_Alram, "UI_Popup_Alarm_Gear");
            Add(PopupName.Gear_Auto_Equip_Alram, "UI_Popup_Alarm_Gear02");
            Add(PopupName.Gear_All_Compose_Alram, "UI_Popup_Alarm_Iventory02");
            Add(PopupName.Summon_Result, "UI_Popup_Recall_Result");
            Add(PopupName.Summon_Detail, "UI_Popup_Recall_Detail");
            Add(PopupName.AutoBattle_ChapterSelect, "UI_Popup_BattleAuto_ChapterSelect");
            Add(PopupName.AutoBattle_BattleAuto, "UI_Popup_BattleAuto");
            Add(PopupName.AutoBattle_BattleAutoFast, "UI_Popup_BattleAutoFast");
            Add(PopupName.Lobby_AccountLevelUp, "UI_Popup_AccountLevelUp");
            Add(PopupName.Lobby_Profile, "UI_Popup_Profile");
            Add(PopupName.Lobby_ProfileNick, "UI_Popup_Profile_Nick");
            Add(PopupName.Common_Reward, "UI_Popup_Common_Reward");
            Add(PopupName.Lobby_Terms, "UI_Popup_Terms");    // 약관
            Add(PopupName.Option_Setting, "UI_Popup_Setting");
            Add(PopupName.Lang_Select, "UI_Popup_Setting_Lang");
            Add(PopupName.Post_Box, "UI_Popup_Mail");
            Add(PopupName.Post_Box_Detail, "UI_Popup_Mail_Detail");
            Add(PopupName.Common_Alarm, "UI_Popup_Alarm_Common_Toast");
            Add(PopupName.Common_Information, "UI_Popup_Information");
            Add(PopupName.Common_AccountDelete, "UI_Popup_Alarm_Delete");
            Add(PopupName.Common_Loading, "UI_Loading02");
            Add(PopupName.Common_NewContent, "UI_Popup_Alarm_Menu");
            Add(PopupName.Shop_Purchase_Receive, "UI_Popup_Alarm_Shop_Package_Recevie");
            Add(PopupName.Shop_Purchase, "UI_Popup_Alarm_Shop_Purchase");
            Add(PopupName.Mission, "UI_Popup_Mission");
            Add(PopupName.ChargingDia, "UI_Popup_Charging_Dia");
            Add(PopupName.ChargingEnergy, "UI_Popup_Charging_Energy");
            Add(PopupName.Attendance, "UI_Popup_Attendance");
            Add(PopupName.Costume, "UI_Popup_Costume");
            Add(PopupName.Tutorial_Intro, "Popup_Intro");
            Add(PopupName.Tutorial_PopupDialog, "UI_Popup_Dialog01");
            Add(PopupName.Dungeon_Select, "UI_Popup_Dungeon_Gold");
            Add(PopupName.DungeonClear, "UI_Popup_DungeonClear");
            Add(PopupName.Boost, "UI_Popup_Booster");
            Add(PopupName.SpecialShop, "UI_Popup_SpecialShop");
            Add(PopupName.SpecialShopDetail, "UI_Popup_SpecialShop_Detail");
            Add(PopupName.Lobby_Trait, "UI_Popup_Attribute");
            Add(PopupName.Ranking, "UI_Popup_Ranking");
            Add(PopupName.RankingUserInfo, "UI_Popup_PlayerInfo");
            Add(PopupName.HardDungeonSelect, "UI_Popup_Dungeon_Hard");
            Add(PopupName.Guardian, "UI_Popup_Sacred");
            Add(PopupName.GuardianAccelerate, "UI_Popup_Research");
            Add(PopupName.ExplorationStage, "UI_Popup_Exploration_Stage");
            Add(PopupName.Card, "UI_Popup_Card");
        }

        public void BattleInstanceGameObject()
        {
            Add(ViewName.Battle_Top, "UI_Battle_Top");
            Add(ViewName.Battle_Cheat, "BattleCheat");
            Add(PopupName.Battle_LevelUp, "UI_Popup_Levelup");
            Add(PopupName.Battle_Pause, "UI_Popup_Pause");
            Add(PopupName.Battle_Giveup, "UI_Popup_Giveup");
            Add(PopupName.Battle_ResultVictory, "UI_Popup_Result_Victory");
            Add(PopupName.Battle_ResultDefeat, "UI_Popup_Result_Defeat");
            Add(PopupName.Battle_DimensionRift, "UI_Popup_Dimension");
            Add(PopupName.Battle_AlarmEnemyWave, "UI_Popup_Waring_00");
            Add(PopupName.Battle_AlarmBoss, "UI_Popup_Waring_01");
            Add(PopupName.Battle_BossName, "UI_Popup_BossName_00");
            Add(PopupName.Battle_GetHero, "UI_Popup_Get_Hero");
            Add(PopupName.Battle_GetItem, "UI_Popup_Get_Item");
            Add(PopupName.Battle_Revive, "UI_Popup_Revive");
            Add(PopupName.Battle_Damage, "UI_Popup_Damage");
            Add(PopupName.Battle_SkillSellect, "UI_Popup_Skillsellect");
            Add(PopupName.Battle_ResultDungeon, "UI_Popup_Dungeon_Victory");
            Add(PopupName.Tutorial_Popup_Finger, "UI_Popup_Finger");
            Add(PopupName.HardModeInfo, "UI_Popup_HardInfo");
        }

        [Button]
        public void ResetUI()
        {
            foreach (var view in UIView.database)
            {
                Destroy(view.gameObject);
            }
            
            foreach (var popup in UIPopup.database)
            {
                popup.GetComponent<Popup>().HideCallback(null);
                Destroy(popup.gameObject);
            }
            
            popups.Clear();
            alarms.Clear();
        }
        public void HidePopup()
        {
            UIPopup.database.First().Hide();
            Debug.Log("팝업 HIDE");
        }

        public void Add(ViewName viewName, string name)
        {
            dic_UIView.Add(viewName, name);
        }

        public View Get(ViewName viewName)
        {
            Debug.Log($@"{viewName} Get View");
            if (dic_UIView.ContainsKey(viewName))
            {
                var obj = ResourcesManager.Instance.GetResources<GameObject>(dic_UIView[viewName]);
                var view = Instantiate(obj, mainCanvas).GetComponent<View>();
                return view;
            }
            
            Debug.LogFormat($"@{viewName} Not Find ");

            return null;
        }

        public void Add(PopupName popupName, string name)
        {
            dic_UIPopup.Add(popupName, name);
        }
        
        public Popup Get(PopupName popupName)
        {
            Debug.Log($@"{popupName} Get Popup");
            
            if (dic_UIPopup.ContainsKey(popupName))
            {
                EnableTouch = false;
                var obj = ResourcesManager.Instance.GetResources<GameObject>(dic_UIPopup[popupName]);
                var popup = Instantiate(obj, mainCanvas).GetComponent<Popup>();
                
                switch (popup.PopupType)
                {
                    case PopupType.Popup:
                        popups.Add(popup);      // 팝업컬렉션에 추가
                        break;
                    case PopupType.Alarm:
                        alarms.Add(popup);      // 알람컬렉션에 추가
                        break;
                }
                var uipopup = popup.uiPopup;
                uipopup.Reset();
                
                uipopup.Validate();
                uipopup.ApplyHideOnAnyButton();
                uipopup.SetParent(uipopup.GetParent());
                uipopup.InstantHide(false);
                
                
                //destroy the popup when it is hidden
                uipopup.OnHiddenCallback.Event.AddListener(() =>
                {
                    //sanity check to make sure the popup is not already destroyed
                    if (popup == null) return;
                    uipopup.RestorePreviouslySelectedGameObject();
                    
                    // 팝업대기열
                    if(popup.ReserveCheck)// && !PendingDelay)
                        GetPendingPopups();
                    
                    Destroy(popup.gameObject);
                    popup = null;
                });
                
                uipopup.OnShowCallback.Event.AddListener(() =>
                {
                    EnableTouch = true;
                });
                
                return popup;
            }

            Debug.LogWarning($"@{popupName} Not Find ");
            return null;
        }

        public UITag GetCanvas(string category, string name = "Canvas")
        {
            return UITag.GetTags(category, name).FirstOrDefault();
        }

        public void LoadingScreen()
        {
            UICamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
            ResetUI();
            Get(ViewName.Common_Loading);
        }
        
        public void UIScreen()
        {
            UICamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            ResetUI();
        }

        public Popup GetPopup(PopupName id)
        {
            Popup popup = popups.FirstOrDefault(t => t.ID == id);
            if (popup)
                return popup;

            Popup alarm = alarms.FirstOrDefault(t => t.ID == id);
            if (alarm)
                return alarm;

            Debug.LogWarning($"Not Found Popup{id}");
            return null;
        }

        public View GetView(ViewName id)
        {
            var _ID = id.ToString().Split('_');
            var view = UIView.visibleViews.First(g => g.Id.Name == _ID[1]);

            return view.GetComponent<View>();
        }
        
        // 팝업예약
        public void ReservePopup(Action callback)
        {
            pendingPopups.Enqueue(callback);
        }

        // 팝업 대기열 삭제
        public void PendingPopupClear()
        {
            pendingPopups.Clear();
        }
        
        public void GetPendingPopups()
        {
            if (pendingPopups.Count <= 0) return;
            
            pendingPopups.Dequeue()?.Invoke();
            // var pendingPopup = pendingPopups.First();
            // pendingPopup?.Invoke();
            // pendingPopups.RemoveAt(0);
        }
        
        // public bool IsShowLoadingPopup()
        // {
        //     LoadingPopup popup = GetPopup(PopupName.Common_Loading) as LoadingPopup;
        //     return popup;
        // }

        public void ShowLoadingPopup()
        {
            LoadingPopup popup = GetPopup(PopupName.Common_Loading) as LoadingPopup;
            if (popup == true) return;
                
            popup = Get(PopupName.Common_Loading) as LoadingPopup;
            popup.Show();
        }

        public void ShowLoadingPopup(Action action)
        {
            LoadingPopup popup = GetPopup(PopupName.Common_Loading) as LoadingPopup;
            if (popup == true)
            {
                action?.Invoke();
                return;
            }

            popup = Get(PopupName.Common_Loading) as LoadingPopup;
            popup.AddShowCallback(action);
            popup.Show();
        }
        
        public void HideLoadingPopup()
        {
            LoadingPopup popup = GetPopup(PopupName.Common_Loading) as LoadingPopup;
            if (popup) popup.Hide();
        }
        
        public void ShowDefaultErrorPopup(Reply error)
        {
            string langPackKey = error.Response.Unbox<JsonBox>().Element["resaon"]?.ToString();

            var popup = Get(PopupName.Common_Confirm) as PopupConfirm;
            popup.AddShowCallback(HideLoadingPopup);
            popup.uiPopup.canvas.sortingLayerName = "UI";
            popup.uiPopup.SetOverrideSorting(true, false);

            if (string.IsNullOrEmpty(langPackKey))
            {
                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage("Not_match_server".Locale())
                    .SetYesButton(() =>
                    {
                        SceneLoadManager.Instance.LoadScene("Start");
                    }, "Restart_Btn".Locale())
                    .Build();
            }
            else
            {
                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage(LocaleManager.GetLocale(langPackKey))
                    .SetYesButton(() => { popup.Hide(); }, "UI_Key_011".Locale())
                    .Build();
            }
        }
    }
}
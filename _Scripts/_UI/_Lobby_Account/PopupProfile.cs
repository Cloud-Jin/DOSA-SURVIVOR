using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class PopupProfile : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Lobby_Profile;
        public TMP_Text nick, uid;
        public TMP_Text needDescTxt;
        public TMP_Text changePortraitTxt;
        public UIButton closeBtn, changeBtn;
        public UIButton changePortraitBtn;
        [Space(20)]
        public PopupProfileCharInfoScrollView charInfoScrollView;
        public ProfilePortraitScrollView portraitScrollView;
        [Space(20)]
        public UIToggleGroup toggleGroup;
        [Space(20)]
        public List<UITab> tabList;
        [Space(20)]
        public Image portraitImg;
        [Space(20)]
        public Transform portraitRedDot;
        
        private UITab _currentTab;
        private Portrait _selectPortrait;
        
        protected override void Init()
        {
            var data = UserDataManager.Instance.userInfo.GetPlayerData();
            nick.SetText(data.name);
            uid.SetText(data.id);

            closeBtn.AddEvent(Hide);
            changeBtn.AddEvent(OnClickChange);
            changePortraitBtn.AddEvent(OnChangePortrait);
        }

        private void Start()
        {
            tabList.ForEach(t => t.AddBehavioursPointerClick(() =>
            {
                OnSelectTab();
            }));

            AddShowCallback(() =>
            {
                OnSelectTab();
            });

            SetPortraitRedDot();
            
            int equipPortraitID = UserDataManager.Instance.userInfo.PlayerData.player.portrait_id;
            Portrait portrait = TableDataManager.Instance.data.Portrait.Single(t => t.Index == equipPortraitID);
            UpdateChangeBtnState(portrait);
        }
        
        void OnClickChange()
        {
            Hide();
            var popup = UIManager.Instance.Get(PopupName.Lobby_ProfileNick);
            popup.Show();
        }

        public void SetPortraitRedDot()
        {
            portraitRedDot.SetActive(false);
            
            TableDataManager.Instance.data.Portrait.ForEach(t =>
            {
                if (PlayerPrefs.GetInt(t.Icon, 0) == 1)
                    portraitRedDot.SetActive(true);
            });
        }

        public void SetDataPortrait()
        {
            if (Int32.Parse(_currentTab.Id.Category) == 2)
            {
                portraitScrollView.SetData();
                portraitScrollView.ReloadScrollView();
            }
        }
        
        public void SetSelectPortraitData(Portrait selectPortrait)
        {
            _selectPortrait = selectPortrait;
            
            if (PlayerPrefs.GetInt(_selectPortrait.Icon, 0) == 1)
            {
                PlayerPrefs.SetInt(_selectPortrait.Icon, 2);
                PlayerPrefs.Save();
            }

            needDescTxt.SetText(LocaleManager.GetLocale(selectPortrait.Description));
            portraitScrollView.SetSelectPortraitData(selectPortrait, SetSelectPortraitData);
            
            UpdateChangeBtnState(_selectPortrait);
            SetPortraitRedDot();
            
            if (UILobby.Instance)
                UILobby.Instance.LobbyGnb.SetPortaitRedDot();
        }

        private void UpdateChangeBtnState(Portrait selectPortrait)
        {
            _selectPortrait = selectPortrait;
            
            if (UserDataManager.Instance.userProfileInfo.portraitList.Any(u => u == _selectPortrait.Index) == false)
            {
                changePortraitBtn.interactable = false;
                changePortraitTxt.SetText(LocaleManager.GetLocale("Portrait_Not_Obtain"));
            }
            else
            {
                int equipPortraitID = UserDataManager.Instance.userInfo.PlayerData.player.portrait_id;
                
                if (_selectPortrait.Index == equipPortraitID)
                {
                    changePortraitBtn.interactable = false;
                    changePortraitTxt.SetText(LocaleManager.GetLocale("Portrait_Using"));
                }
                else
                {
                    changePortraitBtn.interactable = true;
                    changePortraitTxt.SetText(LocaleManager.GetLocale("Portrait_Use"));
                }
            }
        }

        private void UpdatePortrait()
        {
            int portraitID = UserDataManager.Instance.userInfo.PlayerData.player.portrait_id;
            Portrait portrait = TableDataManager.Instance.data.Portrait.Single(t => t.Index == portraitID);
            portraitImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(portrait.Icon);
            needDescTxt.SetText(LocaleManager.GetLocale(portrait.Description));
        }
        
        private void OnSelectTab()
        {
            UITab selectTab = toggleGroup.firstToggleOn as UITab;

            if (_currentTab == true && selectTab.Equals(_currentTab)) return;

            _currentTab = selectTab;
           int selectIndex = Int32.Parse(_currentTab.Id.Category);

            if (selectIndex == 1)
            {
                charInfoScrollView.SetActive(true);
                portraitScrollView.SetActive(false);
            }
            else if (selectIndex == 2)
            {
                charInfoScrollView.SetActive(false);
                portraitScrollView.SetActive(true);
                portraitScrollView.SetSelectPortraitData(null, SetSelectPortraitData);
                portraitScrollView.ReloadScrollView();
            }
            
            UpdatePortrait();
        }

        private void OnChangePortrait()
        {
            int portraitID = UserDataManager.Instance.userInfo.PlayerData.player.portrait_id;

            if (_selectPortrait == null || _selectPortrait?.Index == portraitID) return;

            if (UserDataManager.Instance.userProfileInfo.portraitList
                .All(u => u != _selectPortrait.Index)) return;
            
            var payload = new Dictionary<string, object> {
                {"id", _selectPortrait?.Index}
            };
            
            APIRepository.RequestPortraitChange(payload, data =>
            {
                UpdatePortrait();
                UpdateChangeBtnState(_selectPortrait);
                
                portraitScrollView.SetSelectPortraitData(_selectPortrait, SetSelectPortraitData);
                portraitScrollView.ReloadScrollView();

            }, reply =>
            {
                var reason = reply.Response.Unbox<JObject>()["reason"].ToString();
                
                Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale(reason))
                    .Build();
            });
        }
    }
}
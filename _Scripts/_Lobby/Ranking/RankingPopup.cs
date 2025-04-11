using System;
using System.Collections.Generic;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Animators;
using Doozy.Runtime.UIManager.Components;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ProjectM
{
    public class RankingPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Ranking;

        public UIButton rankingInfoBtn;
        public UIButton closeBtn;
        [Space(20)]
        public UIToggleGroup toggleGroup;
        [Space(20)]
        public RankingScrollView scrollView;
        [Space(20)]
        public RankingScrollCellView myRanking;
        [Space(20)]
        public List<UITab> tabList;
        [Space(20)]
        public List<Transform> tabContent;
        [Space(20)]
        public List<Transform> bannerList;

        private PlayerRank _myPlayerRank;
        private UITab _currentTab;

        private int _selectTabIndex;
        private float _scrollPosition;
        
        protected override void Init()
        {
        }

        private void Start()
        {
            rankingInfoBtn.AddEvent(ShowRankingPopupInfo);
            closeBtn.AddEvent(Hide);
            
            tabList.ForEach(t => t.AddBehavioursPointerClick(() =>
            {
                _scrollPosition = 0;
                OnSelectTab();
            }));

            AddShowCallback(() => 
            {
                OnSelectTab();
            });
        }

        public void SetData(int selectTabIndex, float scrollPosition)
        {
            _selectTabIndex = selectTabIndex;
            _scrollPosition = scrollPosition;
            
            bannerList.ForEach(b => b.SetActive(false));
            bannerList[_selectTabIndex].SetActive(true);
            
            toggleGroup.toggles.ForEach(t => t.isOn = false);
            
            tabContent.ForEach(t =>
            {
                for (int i = 0; i < t.childCount; ++i)
                {
                    if (t.GetChild(i).GetComponent<UISelectableColorAnimator>())
                        t.GetChild(i).GetComponent<UISelectableColorAnimator>().Play(UISelectionState.Normal);
                }
            });
            
            toggleGroup.toggles[_selectTabIndex].isOn = true;

            for (int i = 0; i < tabContent[_selectTabIndex].childCount; ++i)
            {
                if (tabContent[_selectTabIndex].GetChild(i).GetComponent<UISelectableColorAnimator>())
                    tabContent[_selectTabIndex].GetChild(i).GetComponent<UISelectableColorAnimator>().Play(UISelectionState.Selected);
            }
            
            _myPlayerRank = UserDataManager.Instance.playerRankInfo.rankDataList[_selectTabIndex].player_rank;
            
            myRanking.SetData(_myPlayerRank, _selectTabIndex, scrollView, true);
            scrollView.SetData(_selectTabIndex, _scrollPosition);
        }

        private void OnSelectTab()
        {
            UITab selectTab = toggleGroup.firstToggleOn as UITab;

            if (_currentTab == true && selectTab.Equals(_currentTab)) return;
            
            _currentTab = selectTab;
            _selectTabIndex = Int32.Parse(_currentTab.Id.Category);
            
            PlayerRankData playerRankData = UserDataManager.Instance.playerRankInfo.rankDataList[_selectTabIndex];
            
            if (playerRankData != null)
            {
                DateTime nowTime = DateTime.UtcNow;
                DateTime refreshTime = playerRankData.expiry_at;

                if (nowTime < refreshTime)
                {
                    SetData(_selectTabIndex, _scrollPosition);
                    return;
                }
            }
            
            var payload = new Dictionary<string, object> {
                { "type", _selectTabIndex + 1 }
            };
            
            APIRepository.RequestRankingInfo(payload, data =>
            {
                SetData(_selectTabIndex, 0);
            }, reply =>
            {
                var reason = reply.Response.Unbox<JObject>()["reason"].ToString();
                
                Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale(reason))
                    .Build();
            });
        }
        
        private void ShowRankingPopupInfo()
        {
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            
            if (popup)
            {
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale("Ranking_Guide_Title"))
                    .SetMessage(LocaleManager.GetLocale("Ranking_Guide_Desc"))
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("UI_Key_011"))
                    .SetHideOverlay(true)
                    .Build();
            }
        }
    }
}

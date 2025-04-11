using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class RankingScrollCellView : EnhancedScrollerCellView
    {
        public List<Transform> rankFrameList;
        [Space(20)]
        public TMP_Text rankTxt;
        public TMP_Text playerNameTxt;
        public TMP_Text stageCountTxt;
        public TMP_Text stageTimeTxt;
        [Space(20)]
        public Image playerIconImg;
        [Space(20)]
        public UIButton playerInfoBtn;

        private PlayerRank _playerRank;
        private int _selectTabIndex;
        private RankingScrollView _scrollView;

        private void Start()
        {
            playerInfoBtn.AddEvent(ShowPlayerInfo);
        }

        public void SetData(PlayerRank playerRank, int selectTabIndex, RankingScrollView scrollView, bool isMy = false)
        {
            _selectTabIndex = selectTabIndex;
            _scrollView = scrollView;
            _playerRank = playerRank;

            rankFrameList.ForEach(r => r.SetActive(false));
            
            if (_playerRank != null)
            {
                if (isMy)
                {
                    int portraitID = UserDataManager.Instance.userInfo.PlayerData.player.portrait_id;
                    Portrait portrait = TableDataManager.Instance.data.Portrait.Single(t => t.Index == portraitID);
                    playerIconImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(portrait.Icon);
                    playerNameTxt.SetText(UserDataManager.Instance.userInfo.PlayerData.player.name);
                }
                else
                {
                    Portrait portrait = TableDataManager.Instance.data.Portrait
                        .SingleOrDefault(t => t.Index == playerRank.p.p);
                    playerIconImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(portrait.Icon);
                    playerNameTxt.SetText(_playerRank.p.n);
                }

                Stage clearStage = TableDataManager.Instance.data.Stage.Single(t => t.Index == _playerRank.i);
                TimeSpan clearTime = TimeSpan.FromSeconds(_playerRank.t); 
                
                if (selectTabIndex == 0 || selectTabIndex == 2)
                    stageCountTxt.SetText(LocaleManager.GetLocale(clearStage.Name));
                else if (selectTabIndex == 1)
                    stageCountTxt.SetText(LocaleManager.GetLocale("Gold_Dungeon_Stage", _playerRank.l));
                
                stageTimeTxt.SetText(LocaleManager.GetLocale("Remain_Time", clearTime.Minutes, clearTime.Seconds));

                if (_playerRank.r <= 0)
                {
                    rankTxt.SetText("-");
                    rankFrameList[3].SetActive(true);
                }
                else
                {
                    if (3 < _playerRank.r)
                        rankFrameList[3].SetActive(true);
                    else
                        rankFrameList[_playerRank.r - 1].SetActive(true);
                    
                    rankTxt.SetText(String.Format($"{_playerRank.r}"));
                }
                
                playerInfoBtn.SetActive(true);
            }
            else
            {
                int portraitID = UserDataManager.Instance.userInfo.PlayerData.player.portrait_id;
                Portrait portrait = TableDataManager.Instance.data.Portrait.Single(t => t.Index == portraitID);
                playerIconImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(portrait.Icon);
                playerNameTxt.SetText(UserDataManager.Instance.userInfo.PlayerData.player.name);

                playerInfoBtn.SetActive(false);
                rankFrameList[3].SetActive(true);
                
                rankTxt.SetText("-");
                stageCountTxt.SetText("");
                stageTimeTxt.SetText(LocaleManager.GetLocale("Not_Measured"));
            }
        }

        private void ShowPlayerInfo()
        {
            RankingPopup rankingPopup = UIManager.Instance.GetPopup(PopupName.Ranking) as RankingPopup;
            if (rankingPopup) rankingPopup.Hide();
            
            RankingUserInfoPopup popup = UIManager.Instance.Get(PopupName.RankingUserInfo) as RankingUserInfoPopup;
            popup.SetData(_playerRank, _selectTabIndex, _scrollView.enhancedScroller._scrollPosition);
            popup.Show();
        }
    }
}

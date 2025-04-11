using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Containers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class ContainerDungeon : MonoBehaviour
    {
        public UIContainer uiContainer;
        [Space(20)]
        public ContainerDungeonScrollView scrollView;

        private bool isDungenRedDot;                // 레드닷 표시해야할 던전 존재
        
        private void Start()
        {
            uiContainer.OnShowCallback.Event.AddListener(OnShowDungeon);
            uiContainer.OnHiddenCallback.Event.AddListener(OnHideDungeon);
            RedDotCheck();
        }

        public void OnShowDungeon()
        {
            scrollView.SetActive(true);
        }

        public void OnHideDungeon()
        {
            // Instance = null;
            // Debug.Log("hide");
            var popup = UIManager.Instance.GetPopup(PopupName.Dungeon_Select);
            if(popup) popup.Hide();

            popup = UIManager.Instance.GetPopup(PopupName.HardDungeonSelect);
            if (popup) popup.Hide();
        }

        public void RedDotCheck()
        {
            var dungenList = TableDataManager.Instance.data.Dungeon.ToList();
            // 황금 던전 기본 입장 남은 횟수, 광고 시청(입장 초기화)가 있을 경우 표시
            var goldDungeonData = UserDataManager.Instance.stageInfo.GoldDungeonData;
            var tbGold = dungenList.Single(t => t.Index == 1);
            bool isEnter = tbGold.EnterCount * (goldDungeonData.enter_extra_count + 1) - (goldDungeonData.enter_count + goldDungeonData.ffwd_count) > 0;
            var isRedDot =  isEnter || (goldDungeonData.enter_extra_count < tbGold.AddEnterCount);

            isDungenRedDot = isRedDot;

            if (UILobby.Instance)
            {
                UILobby.Instance.SetRedDot(LobbyTap.Dungeon, isDungenRedDot);
                scrollView.ReloadScrollView();
            }
        }
    }
}

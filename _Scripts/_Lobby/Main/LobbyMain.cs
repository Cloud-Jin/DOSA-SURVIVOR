using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using ProjectM;
using UnityEngine;

namespace ProjectM
{
    public class LobbyMain : MonoBehaviour
    {
        public static LobbyMain Instance;

        public Transform redDotMail;
        public Transform redDotMission;
        public Transform redDotAttendance;
        public Transform redDotBoost;
        public Transform redDotSpecialShop;
        public Transform redDotCostume;
        public Transform redDotSacred;
        [Space(20)]
        public UIButton menuOptionBtn;
        public UIButton menuMailBtn;
        public UIButton missionBtn;
        public UIButton attendanceBtn;
        public UIButton boostBtn;
        public UIButton specialShopBtn;
        public UIButton constumeBtn;
        public UIButton rankingBtn;
        public UIButton guardianBtn;
        [Space(20)] 
        public Transform boostEffect;
        [Space(20)]
        public UIContainer uiContainer;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
            
            menuOptionBtn.AddEvent(ShowOptionPopup);
            menuMailBtn.AddEvent(ShowPostBoxPopup);
            missionBtn.AddEvent(ShowMissionPopup);
            attendanceBtn.AddEvent(ShowAttendancePopup);
            boostBtn.AddEvent(ShowBoostPopup);
            specialShopBtn.AddEvent(ShowSpecialPopup);
            constumeBtn.AddEvent(ShowCostumePopup);
            rankingBtn.AddEvent(ShowRankingPopup);
            guardianBtn.AddEvent(ShowGuardianPopup);
            uiContainer.OnShowCallback.Event.AddListener(OnShowMain);
            uiContainer.OnHiddenCallback.Event.AddListener(OnHideMain);

            ReloadLobbyRedDot();
        }

        public void OnHideMain()
        {
            Instance = null;
        }

        public void OnShowMain()
        {
            Instance = this;

            ReloadLobbyRedDot();
        }

        public void ReloadLobbyRedDot()
        {
            redDotBoost.SetActive(UserDataManager.Instance.payInfo.IsDisableBuff());
            boostEffect.SetActive(UserDataManager.Instance.payInfo.IsEnableBuff());
            redDotMail.SetActive(!UserDataManager.Instance.postBoxInfo.IsAllConfirm());
            redDotMission.SetActive(UserDataManager.Instance.missionInfo.IsGetMissionReward());
            redDotAttendance.SetActive(UserDataManager.Instance.missionInfo.IsGetAttendanceReward());
            redDotSpecialShop.SetActive(UserDataManager.Instance.payInfo.IsSpecialShopRedDot());
            redDotCostume.SetActive(UserDataManager.Instance.payInfo.IsCheckedCostume());
            redDotSacred.SetActive(UserDataManager.Instance.gearInfo.GetGuardianRedDot()
                                   || UserDataManager.Instance.gearInfo.GetExploreRedDot());
        }

        private void ShowOptionPopup()
        {
            var popup = UIManager.Instance.Get(PopupName.Option_Setting);
            popup.Show();
        }

        private void ShowPostBoxPopup()
        {
            var popup = UIManager.Instance.Get(PopupName.Post_Box) as PostBoxPopup;
            popup.Show();
        }

        private void ShowMissionPopup()
        {
            var popup = UIManager.Instance.Get(PopupName.Mission) as MissionPopup;
            popup.Show();
        }

        private void ShowAttendancePopup()
        {
            var popup = UIManager.Instance.Get(PopupName.Attendance) as AttendancePopup;
            popup.Show();
        }

        private void ShowBoostPopup()
        {
            var popup = UIManager.Instance.Get(PopupName.Boost) as BoostPopup;
            popup.Show();
        }

        private void ShowSpecialPopup()
        {
            var popup = UIManager.Instance.Get(PopupName.SpecialShop) as SpecialShopPopup;
            popup.Show();
        }
        
        private void ShowCostumePopup()
        {
            CostumePopup costumePopup = UIManager.Instance.Get(PopupName.Costume) as CostumePopup;
            costumePopup.Show();
        }

        private void ShowRankingPopup()
        {
            RankingPopup rankingPopup = UIManager.Instance.Get(PopupName.Ranking) as RankingPopup;
            rankingPopup.Show();
        }

        private void ShowGuardianPopup()
        {
            GuardianPopup guardianPopup = UIManager.Instance.Get(PopupName.Guardian) as GuardianPopup;
            guardianPopup.Show();
        }
    }
}

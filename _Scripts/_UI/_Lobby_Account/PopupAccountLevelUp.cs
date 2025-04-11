using System.Collections.Generic;
using TMPro;

namespace ProjectM
{
    public class PopupAccountLevelUp : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Lobby_AccountLevelUp;
        public TMP_Text prevLevel, nextLevel;
        public PopupAccountLevelUpScrollView popupAccountLevelUpScrollView;

        protected override void Init()
        {
            UserDataManager.Instance.userTraitInfo.PointRefresh();
            uiPopup.OnShowCallback.Event.AddListener(() =>
            {
                popupAccountLevelUpScrollView.SetActive(true);
                popupAccountLevelUpScrollView.ReloadScrollView();
                
            });
            
            // uiPopup.OnHideCallback.Event.AddListener(() =>
            // {
            //     SetShowPendingPopup(true);
            // });
        }
        
        public void SetRewardData(List<ScrollDataModel> data)
        {
            popupAccountLevelUpScrollView.SetData(data);
        }

        public void SetLevelData(int prevLv, int nextLv)
        {
            prevLevel.SetText($"{prevLv}");
            nextLevel.SetText($"{nextLv}");
        }
    }
}
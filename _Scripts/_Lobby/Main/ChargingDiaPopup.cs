using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using TMPro;
using UnityEngine;

namespace ProjectM
{
    public class ChargingDiaPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.ChargingDia;

        public UIButton closeBtn;
        public UIButton buyDiaBtn;
        [Space(20)]
        public TMP_Text totalDiaTxt;
        public TMP_Text freeDiaTxt;
        public TMP_Text buyDiaTxt;
        
        protected override void Init()
        {
        }

        private void Start()
        {
            closeBtn.AddEvent(Hide);
            buyDiaBtn.AddEvent(OnBuyDia);

            InfVal totalDia = UserDataManager.Instance.currencyInfo.data[CurrencyType.Gem].Value;
            InfVal freeDia = UserDataManager.Instance.currencyInfo.data[CurrencyType.FreeGem].Value;
            InfVal buyDia = UserDataManager.Instance.currencyInfo.data[CurrencyType.PayGem].Value;
            
            totalDiaTxt.SetText(totalDia.ToGoodsString());
            freeDiaTxt.SetText(freeDia.ToGoodsString());
            buyDiaTxt.SetText(buyDia.ToGoodsString());
        }

        private void OnBuyDia()
        {
            if (UILobby.Instance)
            {
                UILobby.Instance.SetSelectContentsTabs(LobbyTap.Shop);

                if (ContainerShop.Instance)
                    ContainerShop.Instance.SetSelectCategoryTab(3);
                
                GuardianPopup guardianPopup = UIManager.Instance.GetPopup(PopupName.Guardian) as GuardianPopup;
                if (guardianPopup) guardianPopup.Hide();
            }

            Hide();
        }
    }
}

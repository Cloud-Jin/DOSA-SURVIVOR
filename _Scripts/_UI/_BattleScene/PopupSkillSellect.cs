using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;
using UniRx;

namespace ProjectM.Battle
{
    public class PopupSkillSellect : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_SkillSellect;

        public UIToggle toogle;
        public GameObject progress;
        protected override void Init()
        {
            // 스킬창이 뜨면 Layer 위로
            SetOverrideSorting(10001);
            // auto 버튼. 레벨업 끝날때까지.
            var bm = GoldDungeonBattleManager.Instance as GoldDungeonBattleManager;
            bm.OnAutoSkillUp.Subscribe(b =>
            {
                progress.gameObject.SetActive(b);
                toogle.isOn = b;
            }).AddTo(this);
            
            toogle.ObserveEveryValueChanged(x => x.isOn).Subscribe(t =>
            {
                uiPopup.Overlay.SetActive(t);
                bm.OnAutoSkillUp.Value = t;
                // Debug.Log(t);
            }).AddTo(this);

        }
    }
}
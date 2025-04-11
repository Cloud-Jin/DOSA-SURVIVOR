using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;

namespace ProjectM
{
    public class CommonRewardPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Common_Reward;
        public UIButton closeBtn;
        [Space(20)]
        public CommonRewardScrollView commonRewardScrollView;
        
        private List<ScrollDataModel> _rewardDataList = new();

        private bool _isShowEffect;

        protected override void Init()
        {
            AddShowCallback(ReloadData);
            AddHideCallback(OnHide);
        }

        public void SetData(List<ScrollDataModel> rewardDataList)
        {
            _rewardDataList = rewardDataList;
            commonRewardScrollView.SetData(_rewardDataList);
        }

        private void ReloadData()
        {
            commonRewardScrollView.SetActive(true);
            commonRewardScrollView.ReloadScrollView();
        }

        private void OnHide()
        {
            // SetShowPendingPopup(true);
            
            if (GoodsAttractionEffect.Instance)
                GoodsAttractionEffect.Instance.ShowRewardEffect(_rewardDataList);
        }
    }
}
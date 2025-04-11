
using System;
using UniRx;
using UnityEngine;

namespace ProjectM
{
    public class LoadingPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Common_Loading;
        
        public Transform overlayWhite;
        public Transform overlayBlack;
        public Transform indicator;
        
        protected override void Init()
        {
            ShowPendingPopup = false;
            overlayWhite.SetActive(true);
                
            Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
            {
                overlayBlack.SetActive(true);
                indicator.SetActive(true);
            }).AddTo(this);    
        }
    }
}

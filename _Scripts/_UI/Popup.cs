using System;
using System.Collections;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using AssetKits.ParticleImage.Enumerations;
using Doozy.Runtime.UIManager.Containers;
using UniRx;
using UnityEngine;

namespace ProjectM
{
    public abstract class Popup : MonoBehaviour
    {
        public abstract PopupName ID { get; set; }
        public virtual PopupType PopupType => PopupType.Popup;
        [HideInInspector] public UIPopup uiPopup;
        protected bool ShowPendingPopup;                     // 예약팝업 노출여부
        public bool ReserveCheck => ShowPendingPopup;
       
        
        // protected string ID_category;
        // protected string ID_Name;
        
        public Action HideAction;
        public Action ShowAction;
        
        protected virtual void Awake()
        {
            uiPopup = GetComponent<UIPopup>();
            uiPopup.OverrideSorting = false;
            ShowPendingPopup = true;
            //var _ID = ID.ToString().Split('_');
            
            uiPopup.OnShowCallback.Event.AddListener(() =>
            {
                ShowAction?.Invoke();
            });
            
            uiPopup.OnHiddenCallback.Event.AddListener(() =>
            {
                RemovePopup();
                HideAction?.Invoke();
            });
            Init();
        }
        
        protected abstract void Init();
        
        private void RemovePopup()
        {
            switch (PopupType)
            {
                case PopupType.Popup:
                    UIManager.Instance.popups.Remove(this);
                    break;
                case PopupType.Alarm:
                    UIManager.Instance.alarms.Remove(this);
                    break;
            }
        }

        public void Show()
        {
            uiPopup.Show();
        }
        public void Hide()
        {
            uiPopup.Hide();
        }

        public void HideCallback(Action action)
        {
            this.HideAction = action;
        }
        
        public void AddHideCallback(Action action)
        {
            this.HideAction += action;
        }

        public void AddShowCallback(Action action)
        {
            ShowAction += action;
        }

        protected void ParticleImageUnscaled()
        {
            foreach (var pi in GetComponentsInChildren<ParticleImage>())
            {
                pi.timeScale = TimeScale.Unscaled;
            }
        }
        
        public void SetShowPendingPopup(bool isShow)
        {
            ShowPendingPopup = isShow;
        }

        public void SetOverrideSorting(int sortingOrder, string layerName = "UI")
        {
            // doozy에서 3프레임에 레이어변경하므로 다음프레임인 4프레임에 수정
            var p = uiPopup.SetOverrideSorting(true);
            Observable.TimerFrame(4).Subscribe(t =>
            {
                p.canvas.overrideSorting = true;
                p.canvas.sortingLayerName = layerName;
                p.canvas.sortingOrder = sortingOrder;
            });
        }
    }
}
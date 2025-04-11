using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


namespace ProjectM
{
    public class LockContent : MonoBehaviour
    {
        public GameObject LockGameObject, redDotObject;
        protected UIToggle uiToggle;
        protected UIButton uiButton;
        protected UnlockType _unlockType;
        protected UnlockCondition _unlockCondition;
        protected IDisposable _click;
        protected int type;
        protected bool isLock;
        protected CompositeDisposable disposables = new CompositeDisposable();
        public void SetLockEventUIToggle(int type)
        {
            this.type = type;
            _unlockType = TableDataManager.Instance.data.UnlockType.Single(t => t.Type == type);
            _unlockCondition = TableDataManager.Instance.GetUnlockCondition(_unlockType.UnlockConditionID);
            
            uiToggle = GetComponent<UIToggle>();
            SetData();
            UserDataManager.Instance.clientInfo.DataUpdate.Where(t => isLock).Subscribe(t =>
            {
                SetData();
            }).AddTo(this);



        }
        
        public virtual void SetLockEventUIButton(int type)
        {
        
        }

        void SetData()
        {
            disposables.Clear();
            isLock = !UserDataManager.Instance.clientInfo.GetUnlockData(type);
            uiToggle.isLocked = isLock;
            uiToggle.interactable = !isLock;
            LockGameObject.SetActive(isLock);
         
            if (isLock)
            {
                if(redDotObject)
                    redDotObject.SetActive(false);
                uiToggle.OnPointerClickAsObservable().Subscribe(t =>
                {
                    var alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    alarm.InitBuilder()
                        .SetMessage(ConditionDescription)
                        .Build();
                }).AddTo(disposables);    
            }
            else
            {
                if(redDotObject)
                    redDotObject.SetActive(true);
            }
        }

        string ConditionDescription
        {
            get
            {
                switch (_unlockCondition.ConditionType)
                {
                    case 1:         // 스테이지 클리어
                        var _stage = TableDataManager.Instance.GetStageData(_unlockCondition.ConditionValue);
                        return "UnlockType_Desc_1".Locale($"{_stage.Name.Locale()}");
                    
                    case 2:         // 계정 레벨
                        return "미작업";
                    default:
                        return "타입추가";
                }
            }
        }
    }
}
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
    public class LockContentHide : LockContent
    {
        public override void SetLockEventUIButton(int type)
        {
            this.type = type;
            _unlockType = TableDataManager.Instance.data.UnlockType.Single(t => t.Type == type);
            _unlockCondition = TableDataManager.Instance.GetUnlockCondition(_unlockType.UnlockConditionID);
            
            uiButton = GetComponent<UIButton>();
            SetData();
            UserDataManager.Instance.clientInfo.DataUpdate.Where(t => isLock).Subscribe(t =>
            {
                SetData();
            }).AddTo(this);
        }

        void SetData()
        {
            disposables.Clear();
            isLock = !UserDataManager.Instance.clientInfo.GetUnlockData(type);
            gameObject.SetActive(!isLock);
            
            if (isLock)
            {
                if(redDotObject)
                    redDotObject.SetActive(false);
            }
            else
            {
                if(redDotObject)
                    redDotObject.SetActive(true);
            }
        }
    }
}
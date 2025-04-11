using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ProjectM.Battle
{
    public class IndicatorBase : ObjectBase, IPoolObject
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }

        public void ReturnPool()
        {
            Pool.Return(this);
        }

        public void SetReturnTime(float time, Action callback)
        {
            Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(_ =>
            {
                ReturnPool();
                callback?.Invoke();
            }).AddTo(this);
        }
    }

}
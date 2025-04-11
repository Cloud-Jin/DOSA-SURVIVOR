using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class ParticleBase : ObjectBase, IPoolObject
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }
        public ParticleSystem _particleSystem;
        public ParticleSystem _SubParticleSystem;
        public void ReturnPool()
        {
            Pool.Return(this);
        }

        public void SetStartLifeTime(float time)
        {
            ParticleSystem.MainModule mainModule = _particleSystem.main;
            mainModule.startLifetimeMultiplier = time;

            if (_SubParticleSystem)
            {
                ParticleSystem.MainModule subModule = _SubParticleSystem.main;
                subModule.startLifetimeMultiplier = time;
            }
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
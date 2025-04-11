using System;
using UnityEngine;
using UniRx;

namespace ProjectM
{
    public class ParticleSystemObserver : MonoBehaviour
    {
        private ParticleSystem particleSystem;
        public Subject<int> OnParticleEnd;

        private void Awake()
        {
            OnParticleEnd = new Subject<int>();

            particleSystem = GetComponent<ParticleSystem>();
            var m = particleSystem.main;
            m.stopAction = ParticleSystemStopAction.Callback;
        }

        
        private void OnParticleSystemStopped()
        {
            OnParticleEnd.OnNext(1);
        }
    }
}
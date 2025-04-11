using System;
using System.Collections;
using System.Collections.Generic;
using ProjectM;
using Unity.VisualScripting;
using UnityEngine;
using UniRx;
//ExtentionAction

public static class Extension
{
    public static void SafeAction(this Action action)
    {
        action?.Invoke();
    }

    public static void EndAction(this ParticleSystem particleSystem, Action action)
    {
        var observer = particleSystem.GetOrAddComponent<ParticleSystemObserver>();

        observer.OnParticleEnd.Subscribe(_ => action?.Invoke()).AddTo(particleSystem);
    }
}
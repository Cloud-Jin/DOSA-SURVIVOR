using System.Collections;
using System.Collections.Generic;
using InfiniteValue;
using ProjectM.Battle;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public interface IDamageable
{
    public UnitBase TakeDamage(InfVal damage, HitType hitType);
    public void TakeDamageDot(InfVal value, float time)
    {
        
    }
    
    public void TakeKnockBack(Vector2 dir, float dist)
    {
        // Observable.FromCoroutine(t => KnockBack(dir, dist));
    }
}

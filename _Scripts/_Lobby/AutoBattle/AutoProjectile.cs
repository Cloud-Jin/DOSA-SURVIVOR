using System;
using DG.Tweening;
using InfiniteValue;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.AutoBattle
{
    public class AutoProjectile : MonoBehaviour
    {
        public LayerMask targetLayer;
        public ParticleSystem skillParticle, hitParticle;
        private InfVal attack;

        public void Shot(AutoUnit Target, AutoBattleSkillAI data, InfVal attack)
        {
            skillParticle.SetActive(true);
            if(hitParticle)
                hitParticle.SetActive(false);
            
            var dir = MyMath.GetDirection(transform.position, Target.transform.position);
            if(data.RotationAble == 1)
                transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            
            transform.DOMoveInTargetLocalSpace(Target.transform, Vector3.zero, data.Speed / 10f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    Target.DamageHitUI(attack, HitType.Normal, transform.transform.position);
                    Target.State = AutoUnit.AutoState.Dead;
                    
                    if (hitParticle)
                    {
                        skillParticle.SetActive(false);
                        hitParticle.SetActive(true);

                        Observable.Timer(TimeSpan.FromSeconds(hitParticle.main.duration)).Subscribe(_ =>
                        {
                            gameObject.SetActive(false);
                        }).AddTo(this);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                });
            
        }
    }
}
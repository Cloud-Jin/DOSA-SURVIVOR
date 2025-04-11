using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class EffectArea : ObjectBase, IPoolObject
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }
        public LayerMask targetLayer;
        float duration;
        private AnimatorPlayer animatorPlayer;
        
        public override void Awake()
        {
            base.Awake();
            TryGetComponent(out animatorPlayer);
            
            this.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnit).AddTo(this);
            this.OnTriggerExit2DAsObservable().Subscribe(ExitUnit).AddTo(this);
        }

        public void Init(int type, float duration)
        {
            this.duration = duration;
            Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(t =>
            {
                animatorPlayer.Play("End", null, null, () => Pool.Return(this));
            }).AddTo(this);

        }
        
        
        private void OverlapUnit(Collider2D col)
        {
            //버프 주기
            if (targetLayer.Contanis(col.gameObject.layer))
            { 
                if(col.attachedRigidbody.TryGetComponent<Playerble>(out Playerble p))
                {
                    PlayerManager.Instance.player.IsNoDamage = true;
                    // p.IsNoDamage = true;
                }
            }
        }
        
        private void ExitUnit(Collider2D col)
        {
            // 버프 해제
            if (!targetLayer.Contanis(col.gameObject.layer)) return;
            {
                if(col.attachedRigidbody.TryGetComponent<Playerble>(out Playerble p))
                {
                    PlayerManager.Instance.player.IsNoDamage = false;
                }
            }
        }
        // event
        
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using InfiniteValue;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class ProjectileExplosion : Bullet
    {
        public ParticleSystem skillParticle, damageParticle;
        public Collider2D hitBox;
        private bool explosionReady;
        
        // option
        // private int per;
        private InfVal ExplosionDamage;
        private UnitBase Target;
        
        public override void Awake()
        {
            base.Awake();
            
            rigid = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            
            hitBox.OnTriggerEnter2DAsObservable().Where(_ =>  explosionReady).Subscribe(ExplosionHit).AddTo(this);
            this.OnTriggerEnter2DAsObservable().Where(_ =>  !explosionReady).Subscribe(OverlapUnit).AddTo(this);
            this.OnTriggerExit2DAsObservable().Subscribe(ExitArea).AddTo(this);
        }

        private void OnDisable()
        {
            disposables.Clear();
        }
        
        public ProjectileExplosion InitBuilder()
        {
            Init();
            Per = 0;
            Target = null;
            explosionReady = false;
            return this;
        }
        
        public ProjectileExplosion SetVelocity(Vector2 velocity)
        {
            this.velocity = velocity;
            return this;
        }

        public ProjectileExplosion SetDamage(InfVal damage)
        {
            this.attack = damage;
            return this;
        }
        
        public ProjectileExplosion SetSound(string shotSound = null, string hitSound = null, string explosionSound = null)
        {
            this.ShotSound = shotSound;
            this.HitSound = hitSound;
            this.ExplosionSound = explosionSound;
            return this;
        }

        public ProjectileExplosion SetExplosionDamage(InfVal damage)
        {
            this.ExplosionDamage = damage;
            return this;
        }
        
        public ProjectileExplosion SetAccDamageFunc(Action<InfVal> accDamage)
        {
            this.AccDamageFunc = accDamage;
            return this;
        }
        
        public ProjectileExplosion SetSpeed(float speed)
        {
            this.Speed = speed;
            return this;
        }
        
        public ProjectileExplosion SetPer(int per)
        {
            this.Per = per;
            return this;
        }
        
        public ProjectileExplosion SetAutoTarget(UnitBase targetUnit, float speed)
        {
            // n초 자동타겟
            // this.targetUnit = targetUnit;
            if (targetUnit == null)  return this;
            
            Observable.EveryFixedUpdate().Where(t=> targetUnit.isLive).Subscribe(t =>
            {
                var dir = MyMath.GetDirection(transform.position, targetUnit.transform.position);
                rigid.velocity = dir * speed;
            }).AddTo(disposables);
            return this;
        }

        public ProjectileExplosion SetBlockType(int blockType, int blockedType)
        {
            this.ProjectileBlockType = blockType;
            this.ProjectileBlockedType = blockedType;
            return this;
        }

        public ProjectileExplosion SetDuration(float duration)
        {
            this.duration = duration;
            if (duration > 0)
            {
                Observable.Timer(TimeSpan.FromSeconds(this.duration)).Subscribe(_ =>
                {
                    disposables.Clear();
                    ExplosionProjectile();
                }).AddTo(disposables);
            }

            return this;
        }
        
        public ProjectileExplosion SetPool(ObjectPooling<ObjectBase> Pool)
        {
            this.Pool = Pool;
            return this;
        }
        
        public ProjectileExplosion SetKnockBack(float dist)
        {
            KnockBackDist = dist;

            return this;
        }

        public ProjectileExplosion SetUnit(UnitBase unit)
        {
            this.unit = unit;
            return this;
        }

        public ProjectileExplosion SetTarget(UnitBase unit)
        {
            this.Target = unit;
            return this;
        }
        
        public void Build()
        {
            if(coll)
                coll.enabled = true;
            if(rigid)
                rigid.velocity = velocity * Speed;
            
            skillParticle.SetActive(true);
            damageParticle.SetActive(false);
            
            // sound
            if(!string.IsNullOrEmpty(ShotSound))
                SoundManager.Instance.PlayFX(ShotSound);
        }
        
        private void OverlapUnit(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                if (Per >= 0)
                {
                    IDamageable _IDamageable;// = col.attachedRigidbody.GetComponent<IDamageable>();
                    UnitBase _unitBase;// = col.attachedRigidbody.GetComponent<UnitBase>();
                    IsUnitBase(col, out _IDamageable, out _unitBase);
                    if (_IDamageable == null) return;
                    if (!_unitBase.isLive) return;
                    
                    // 크리티컬 증가량.
                    var _damage = unit.CalcCriticalDamage(attack, _unitBase, out var hitType);
                    if(_damage > 0)
                        _IDamageable.TakeDamage(_damage, hitType);
                    
                    var dir = MyMath.GetDirection(unit.transform.position, col.transform.position);
                    _IDamageable.TakeKnockBack(dir, KnockBackDist);
                    AccDamageFunc?.Invoke(_damage);
                    
                    if(this.Target != null && this.Target == _unitBase)
                    {
                        ExplosionProjectile();
                    }
                    else
                    {
                        OverlapEnemy(col);
                    }
                    
                }
            }
        }
        
        private void OverlapEnemy(Collider2D col)
        {
            // 적이랑 충돌
            if (!targetLayer.Contanis(col.gameObject.layer)) return;
            if(Per == 99) return;

            --Per;

            if (Per < 0)
            {
                ExplosionProjectile();
            }
        }
        
        void ExplosionProjectile()
        {
            // 폭발한후 damage
            disposables.Clear();
            skillParticle.SetActive(false);
            damageParticle.SetActive(true);
            damageParticle.Play(true);
            explosionReady = true;
            rigid.velocity = Vector2.zero;
            
            
            Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ =>
            {
                explosionReady = false;
            }).AddTo(disposables);
            
            Observable.Timer(TimeSpan.FromSeconds(damageParticle.main.duration)).Subscribe(_ =>
            {
                gameObject.SetActive(false);
            }).AddTo(disposables);
            
        }

        void ExplosionHit(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                
                IDamageable _IDamageable;
                UnitBase _unitBase;
                IsUnitBase(col, out _IDamageable, out _unitBase);
                
                if (_IDamageable == null) return;
                if (!_unitBase.isLive) return;
                
                var _damage = unit.CalcCriticalDamage(ExplosionDamage, _unitBase, out var hitType);
                if(_damage > 0)
                    _IDamageable.TakeDamage(_damage, hitType);
                
                AccDamageFunc?.Invoke(ExplosionDamage);
            }
        }
        
        private void ExitArea(Collider2D col)
        {
            if (!col.CompareTag("Area"))
                return;
            
            // coll.enabled = false;
            Pool?.Return(this);
        }
    }
}
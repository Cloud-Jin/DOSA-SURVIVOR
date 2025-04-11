using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Unit = UniRx.Unit;

namespace ProjectM.Battle
{
    public class DamageOverTime : Bullet
    {
        public List<UnitData> targets;
        public Transform hitPivot;              // 공격범위
        public ParticleSystem hitParticle;      // 공격 텀 이펙트
        public Collider2D hitBox;               // 추가타
        //option
        public float tick;
        public float damageTime;
        private InfVal explosionDamage;
        private CompositeDisposable explosionDisposable = new CompositeDisposable();
        
        public override void Awake()
        {
            base.Awake();
            targets = new List<UnitData>();
            animatorPlayer = GetComponent<AnimatorPlayer>();
        }

        public DamageOverTime InitBuilder()
        {
            Init();
            this.tick = 0;
            this.explosionDamage = 0;
            explosionDisposable.Clear();
            return this;
        }
        
        public DamageOverTime SetVelocity(Vector2 velocity)
        {
            this.velocity = velocity;
            return this;
        }
        public DamageOverTime SetBounceCount(int count)
        {
            this.BounceCount = count;
            return this;
        }
        
        public DamageOverTime SetPer(int per)
        {
            this.Per = per;
            return this;
        }
        
        public DamageOverTime SetSound(string shotSound = null, string hitSound = null, string explosionSound = null)
        {
            this.ShotSound = shotSound;
            this.HitSound = hitSound;
            this.ExplosionSound = explosionSound;
            return this;
        }

        public DamageOverTime SetSpeed(float speed)
        {
            this.Speed = speed;
            return this;
        }
        
        public DamageOverTime SetTick(float tick)
        {
            this.tick = tick;
            return this;
        }

        public DamageOverTime SetDamage(InfVal damage)
        {
            this.attack = damage;
            return this;
        }
        
        public DamageOverTime SetExplosionDamage(InfVal damage)
        {
            this.explosionDamage = damage;
            return this;
        }
        
        public DamageOverTime SetAutoTarget(UnitBase targetUnit, float speed)
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
        
        public DamageOverTime SetBlockType(int blockType, int blockedType)
        {
            this.ProjectileBlockType = blockType;
            this.ProjectileBlockedType = blockedType;
            return this;
        }

        public DamageOverTime SetDamageTime(float damageTime)
        {
            this.damageTime = damageTime;
            return this;
        }

        public DamageOverTime SetDuration(float duration, Action endCallback = null)
        {
            this.duration = duration;
            Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
            {
                endCallback?.Invoke();
                disposables.Clear();
                
                if (animatorPlayer)
                {
                    animatorPlayer?.Play("End", () =>
                    {
                        Pool.Return(this);
                    });
                }
                else
                {
                    Pool.Return(this);
                }

            }).AddTo(disposables);
            return this;
        }
        
        public DamageOverTime SetAccDamageFunc(Action<InfVal> accDamage)
        {
            this.AccDamageFunc = accDamage;
            return this;
        }

        public DamageOverTime SetPool(ObjectPooling<ObjectBase> Pool)
        {
            this.Pool = Pool;
            return this;
        }

        public void Build()
        {
            targets.Clear();
            DamageCheck();
            
            var hitCollider = hitPivot?  hitPivot.GetComponent<Collider2D>() : coll;
            
            hitCollider.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnit).AddTo(disposables);
            if (tick > 0)
            {
                hitCollider.UpdateAsObservable().Subscribe(TickUpdate).AddTo(disposables);
                hitCollider.OnTriggerExit2DAsObservable().Subscribe(ExitEnemy).AddTo(disposables);
            }
            if(rigid)
                rigid.velocity = velocity * Speed;
            if(coll)
                coll.enabled = true;
            animatorPlayer?.Play("Play");

            if (BounceCount > 0)
            {
                var sharedMaterial = new PhysicsMaterial2D
                {
                    friction = 0,
                    bounciness = 1
                };
                rigid.sharedMaterial = sharedMaterial;
                rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 충돌 통과 불가.
                rigid.angularDrag = 0;
                
                coll.includeLayers = LayerMask.GetMask("Wall");
                coll.excludeLayers = ~LayerMask.GetMask("Wall");
                IsBound = true;
                // 체크. 스피드 1 이하면 안튕기는 이슈있음!
                this.OnCollisionEnter2DAsObservable().Subscribe(BoundWall).AddTo(disposables);   // 벽 충돌체크
            }

            if (damageTime > 0)
            {
                this.UpdateAsObservable().Subscribe(DamageTimeUpdate).AddTo(disposables);
                if(hitParticle)
                    hitParticle.EndAction(HitParicleEnd);
            }

            // 미사일블록 처리
            if (ProjectileBlockType > 0)
            {
                this.OnTriggerEnter2DAsObservable().Subscribe(OverlapBullet).AddTo(disposables);
            }

            if (explosionDamage > 0)
            {
                // 막타
                hitBox.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnitExplosion).AddTo(explosionDisposable);
            }
            
            // sound
            if(!string.IsNullOrEmpty(ShotSound))
                SoundManager.Instance.PlayFX(ShotSound);
        }
        public void SetAttack(InfVal attack)
        {
            this.attack = attack;
        }
        
        public DamageOverTime SetKnockBack(float dist)
        {
            KnockBackDist = dist;

            return this;
        }
        
        public DamageOverTime SetUnit(UnitBase unit)
        {
            this.unit = unit;
            return this;
        }
        
        private void OverlapBullet(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                if (col.GetComponent<Bullet>() == null) return;

                var bullet = col.GetComponent<Bullet>();

                if (ProjectileBlockType == bullet.BlockedType)
                {
                    bullet.ReturnPool();
                }
            }
        }

        private void OverlapUnit(Collider2D col)
        {
            // 데미지 준다
            if (!targetLayer.Contanis(col.gameObject.layer)) return;
            
            IDamageable _IDamageable;
            UnitBase _unitBase;
            IsUnitBase(col, out _IDamageable, out _unitBase);
            
            if (_IDamageable == null) return;
            if (!_unitBase.isLive) return;
            
            // var unit = col.GetComponent<UnitBase>();
            if (_unitBase == null)
                return;
            
            if (targets.Exists(t => t.id == _unitBase.unitID))
            {
                return;
            }
            var data = new UnitData();
            data.id = _unitBase.unitID;
            data.damageable = _IDamageable;
            data.tickTime = 0;
            data.unit = _unitBase;
            targets.Add(data);
            
            
            // if (col.GetComponent<IDamageable>() == null) return;
            
            var damage = this.unit.CalcCriticalDamage(attack, unit, out var hitType);
            _IDamageable.TakeDamage(damage, hitType);
            
            var dir = MyMath.GetDirection(this.unit.transform.position, col.transform.position);
            _IDamageable.TakeKnockBack(dir, KnockBackDist);

            AccDamageFunc?.Invoke(damage);

        }
        
        private void OverlapUnitExplosion(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                IDamageable _IDamageable;// = col.attachedRigidbody.GetComponent<IDamageable>();
                UnitBase _unitBase;// = col.attachedRigidbody.GetComponent<UnitBase>();
                IsUnitBase(col, out _IDamageable, out _unitBase);
                
                if (_IDamageable == null) return;
                if (!_unitBase.isLive) return;
                
                // if (col.GetComponent<IDamageable>() == null) return;
                // var target = col.GetComponent<UnitBase>(); 
                // if (!target.isLive) return;
                
                // 크리티컬 증가량.
                var _attack = unit.CalcCriticalDamage(explosionDamage, _unitBase, out var hitType);
                _IDamageable.TakeDamage(_attack, hitType);
                
                var dir = MyMath.GetDirection(unit.transform.position, col.transform.position);
                _IDamageable.TakeKnockBack(dir, KnockBackDist);

                AccDamageFunc?.Invoke(_attack);
            }
        }
        private void ExitEnemy(Collider2D col)
        {
            // 데미지 끝
            if (!targetLayer.Contanis(col.gameObject.layer)) return;

            IDamageable _IDamageable;// = col.attachedRigidbody.GetComponent<IDamageable>();
            UnitBase _unitBase;// = col.attachedRigidbody.GetComponent<UnitBase>();
            IsUnitBase(col, out _IDamageable, out _unitBase);
            if (_unitBase == null) return;
            // var unit = col.GetComponent<UnitBase>();
            // if (unit == null)
            //     return;
            
            
            var target = targets.SingleOrDefault(s => s.id == _unitBase.unitID);
            if(target != null && target.id > 0)
                targets.Remove(target);
        }
        
        private void TickUpdate(Unit i)
        {
            //if (!targetLayer.Contanis(col.gameObject.layer)) return;
            if (targets.Count == 0) return;
            
            // var unit = col.GetComponent<UnitBase>();
            // if (unit == null)
            //     return;

            foreach (var target in targets.ToList())
            {
                target.tickTime += Time.deltaTime;
                if (!(target.tickTime >= tick)) continue;
                // 틱 타임 체크
                
                var damage = this.unit.CalcCriticalDamage(attack, unit, out var hitType);
                target.damageable?.TakeDamage(damage, hitType);
                target.tickTime = 0;
                    
                var dir = MyMath.GetDirection(this.unit.transform.position, target.unit.transform.position);
                target.damageable?.TakeKnockBack(dir, KnockBackDist);
                
                AccDamageFunc?.Invoke(damage);
            }
        }
                
        private void BoundWall(Collision2D col)
        {
            if (rigid.velocity == Vector2.zero) return;

            var angle = Vector2.Angle(Vector2.up, rigid.velocity);
            if (angle == 180 || angle == 90)
            {
                rigid.velocity = UnityEngine.Random.insideUnitCircle.normalized * Speed;
            }
            // 벽 충돌은 Physics Material bound 필요
            
            velocity = rigid.velocity.normalized;
            HitBound();
        }
        void HitBound()
        {
            if (!IsBound) return;
            if(BounceCount == 99) return;
            
            --BounceCount;

            if (BounceCount < 0)
            {
                rigid.velocity = Vector2.zero;
                Pool.Return(this);
            }
        }


        private float _cDamage = 0;
        private void DamageTimeUpdate(Unit i)
        {
            _cDamage += Time.deltaTime;

            if (_cDamage >= damageTime)
            {
                _cDamage = 0;
                HitParticlePlay();
            }
        }

        void HitParticlePlay()
        {
            // Rigid.drag = 1000f;
            if(hitPivot)
                hitPivot.SetActive(true);
            if(hitParticle)
                hitParticle.Play();
            
            if(!string.IsNullOrEmpty(HitSound))
                SoundManager.Instance.PlayFX(HitSound);
            // 0.5f
            // HitAll(hitPivot, radius);
        }

        void HitParicleEnd()
        {
            // Rigid.drag = 0;
            hitPivot.SetActive(false);
            rigid.velocity = velocity.normalized * Speed;
        }

        [Serializable]
        public class UnitData
        {
            public int id;
            public IDamageable damageable;
            public float tickTime;
            public UnitBase unit;
        }
    }
}

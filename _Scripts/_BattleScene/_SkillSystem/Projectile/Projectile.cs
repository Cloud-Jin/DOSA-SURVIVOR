using System;
using InfiniteValue;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class Projectile : Bullet
    {
        public ParticleSystem skillParticle, hitParticle;
        public Transform hitPivot;
        public Transform rotationRoot;
        public bool particleRotation;
        
        private TrailRenderer _trailRenderer;

        private bool autoReturn;
        // option
        protected UnitBase targetUnit;          // 타겟유닛
        
        public override void Awake()
        {
            base.Awake();
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
            rigid = GetComponent<Rigidbody2D>();
            coll = GetComponentInChildren<Collider2D>();
            animatorPlayer = GetComponent<AnimatorPlayer>();
            
            if (hitPivot)
            {
                hitPivot.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnit).AddTo(this);
                hitPivot.OnTriggerEnter2DAsObservable().Subscribe(OverlapBullet).AddTo(this);
                this.OnTriggerExit2DAsObservable().Subscribe(ExitArea).AddTo(this);
            }
            else
            {
                this.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnit).AddTo(this);
                this.OnTriggerEnter2DAsObservable().Subscribe(OverlapBullet).AddTo(this);
                this.OnTriggerExit2DAsObservable().Subscribe(ExitArea).AddTo(this);
            }

        }

        private void OnDestroy()
        {
            disposables.Clear();
        }

        private void OnDisable()
        {
            _trailRenderer?.Clear();
        }

        public Projectile InitBuilder()
        {
            disposables.Clear();
            Init();
            Per = 0;
            return this;
        }
        
        public Projectile SetVelocity(Vector2 velocity)
        {
            this.velocity = velocity;
            return this;
        }

        public Projectile SetDamage(InfVal damage)
        {
            this.attack = damage;
            return this;
        }
        
        public Projectile SetSpeed(float speed)
        {
            this.Speed = speed;
            return this;
        }
        
        public Projectile SetBounceCount(int count)
        {
            this.BounceCount = count;
            return this;
        }

        public Projectile SetBoundType(int type)
        {
            this.BoundType = type;
            return this;
        }

        public Projectile SetPer(int per)
        {
            this.Per = per;
            return this;
        }
        
        public Projectile SetAccDamageFunc(Action<InfVal> accDamage)
        {
            this.AccDamageFunc = accDamage;
            return this;
        }

        public Projectile SetBlockType(int blockType, int blockedType)
        {
            this.ProjectileBlockType = blockType;
            this.ProjectileBlockedType = blockedType;
            return this;
        }

        public Projectile SetDuration(float duration)
        {
            this.duration = duration;
            if (duration > 0)
            {
                Observable.Timer(TimeSpan.FromSeconds(this.duration)).Subscribe(_ =>
                {
                    disposables.Clear();
                    BulletHide();
                    // if (Per == 99 || BounceCount == 99)
                    // {
                    //     BulletHide();
                    // }
                    // else
                    // {
                    //     HitProjectile();    
                    // }
                }).AddTo(disposables);
            }

            return this;
        }

        public Projectile SetAutoTarget(UnitBase targetUnit, float speed)
        {
            // n초 자동타겟
            this.targetUnit = targetUnit;
            Observable.EveryFixedUpdate().Where(t=> targetUnit.isLive).Subscribe(t =>
            {
                var dir = MyMath.GetDirection(transform.position, targetUnit.transform.position);
                rigid.velocity = dir * speed;
                if(particleRotation)
                    transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                FlipX();
            }).AddTo(disposables);
            return this;
        }

        public Projectile SetPool(ObjectPooling<ObjectBase> Pool)
        {
            this.Pool = Pool;
            return this;
        }
        
        public Projectile SetKnockBack(float dist)
        {
            KnockBackDist = dist;

            return this;
        }

        public Projectile SetUnit(UnitBase unit)
        {
            this.unit = unit;
            return this;
        }

        public Projectile SetAnimation(string anim, Action callback)
        {
            animatorPlayer?.Play(anim, callback);
            return this;
        }

        public Projectile SetSound(string shotSound = null, string hitSound = null, string explosionSound = null)
        {
            this.ShotSound = shotSound;
            this.HitSound = hitSound;
            this.ExplosionSound = explosionSound;
            return this;
        }
        

        public void Build()
        {
            DamageCheck();
            if(coll)
                coll.enabled = true;
            if (rigid)
            {
                rigid.velocity = velocity * Speed;
                if (rotationRoot)
                {
                    Vector3 dir = rigid.velocity.normalized;
                    rotationRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                }
            }

            if (particleRotation)
            {
                var _localScale = transform.localScale;
                _localScale.x *= velocity.x > 0 ? -1 : 1;
                transform.localScale = _localScale;
            }

            autoReturn = true;
            skillParticle.SetActive(true);
            if(hitParticle)
                hitParticle.SetActive(false);
            
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
                
                coll.isTrigger = false;
                IsBound = true;
                // 체크. 스피드 1 이하면 안튕기는 이슈있음!
                this.OnCollisionEnter2DAsObservable().Subscribe(BoundWall).AddTo(disposables);  // 벽 충돌체크
                this.LateUpdateAsObservable().Subscribe(LateBound).AddTo(disposables);          // 벽 충돌체크
            }
            
            // sound
            if(!string.IsNullOrEmpty(ShotSound))
                SoundManager.Instance.PlayFX(ShotSound);
        }

        #region 충돌
        
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
                    
                    // if (col.GetComponent<IDamageable>() == null) return;
                    // var target = col.GetComponent<UnitBase>(); 
                    // if (!target.isLive) return;
                    
                    // 크리티컬 증가량.
                    var _attack = unit.CalcCriticalDamage(attack, _unitBase, out var hitType);
                    _IDamageable.TakeDamage(_attack, hitType);
                    
                    var dir = MyMath.GetDirection(unit.transform.position, col.transform.position);
                    _IDamageable.TakeKnockBack(dir, KnockBackDist);

                    AccDamageFunc?.Invoke(_attack);
                    
                    if ((BoundType == 3 || BoundType == 4) && Per >= 0)
                    {
                        Vector3 _dir = rigid.transform.position - col.transform.position; // 접촉지점에서부터 탄위치 의 방향
                        rigid.velocity = _dir.normalized * Speed;
                        rotationRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, _dir);    
                    }
                    
                    HitProjectile();
                    // HitBound();
                }
            }
        }
        
        public void HitProjectile()
        {
            if (Per == 99) return;
            // if (BounceCount > 0) return;
            --Per;
            
            if (Per < 0)
            {
                BulletHide();
            }
        }

        void BulletHide()
        {
            if(rigid)
                rigid.velocity = Vector2.zero; // 이펙트 고정
            
            var _col = skillParticle.transform.GetComponentInChildren<Collider2D>();
            if (_col)
                autoReturn = false;
            
            skillParticle.SetActive(false);
            disposables?.Clear();
            
            if (hitParticle)
            {
                if(hitPivot)
                    hitParticle.transform.position = hitPivot.transform.position;
                    
                hitParticle.SetActive(true);
                hitParticle.Play(true);
                AddExplosionCallback?.Invoke();
                Observable.Timer(TimeSpan.FromSeconds(hitParticle.main.duration)).Subscribe(_ =>
                {
                    // ExitArea에서 return 처리
                    if(autoReturn)
                        gameObject.SetActive(false);
                    else
                        Pool?.Return(this);
                }).AddTo(disposables);
            }
            else
            {
                AddExplosionCallback?.Invoke();
                if(autoReturn)
                    gameObject.SetActive(false);
                else
                    Pool?.Return(this);
            }
        }
        private void ExitArea(Collider2D col)
        {
            if (!col.CompareTag("Area"))
                return;

            if (autoReturn)
            {
                coll.enabled = false;
                Pool?.Return(this);
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
        
        private void LateBound(Unit i)
        {
            if (rotationRoot)
            {
                Vector3 dir = rigid.velocity.normalized;
                rotationRoot.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            }
        }
        
        void HitBound()
        {
            if (!IsBound) return;
            if(BounceCount == 99) return;
            --BounceCount;
            
            if (BounceCount < 0)
            {
                rigid.velocity = Vector2.zero;
                gameObject.SetActive(false);
            }
        }

        #endregion
        
        public void FlipX()
        {
            if (targetUnit == null) return;
            var dot = targetUnit.transform.position.x - transform.position.x;
            if (Math.Abs(dot) < 0.1f) return;
            
            var isFlip = targetUnit.Rigid.position.x > transform.position.x;
            var scale = transform.localScale;
            scale.x = (isFlip) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            transform.localScale = scale;
        }
    }
}
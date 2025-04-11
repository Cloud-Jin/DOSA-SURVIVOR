using System;
using InfiniteValue;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class ProjectileTrap : Bullet
    {
        public ParticleSystem skillParticle, hitParticle;
        [Header("데미지 폭발범위")] public float Radius;
        public bool particleRotation;

        // option
        private int per;
        private string aimResource;
        private ParticleBase aimParticle;
        private UnitBase targetUnit;          // 타겟유닛
        private float bombScale;
        
        public override void Awake()
        {
            base.Awake();
            rigid = GetComponent<Rigidbody2D>();
            coll = GetComponentInChildren<Collider2D>();
            animatorPlayer = GetComponentInChildren<AnimatorPlayer>();
            
            this.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnit).AddTo(this);
            // this.OnTriggerEnter2DAsObservable().Where(_ => isRun).Subscribe(OverlapBullet).AddTo(this);
            this.OnTriggerExit2DAsObservable().Subscribe(ExitArea).AddTo(this);
        }

        public ProjectileTrap InitBuilder()
        {
            disposables.Clear();
            velocity = Vector2.zero;
            rigid.drag = 0;
            attack = 0;
            per = 0;
            ProjectileBlockType = 0;
            ProjectileBlockedType = 0;
            AddExplosionCallback = null;
            bombScale = 1;
            return this;
        }
        
        public ProjectileTrap SetVelocity(Vector2 velocity)
        {
            this.velocity = velocity;
            return this;
        }

        public ProjectileTrap SetDamage(InfVal damage)
        {
            this.attack = damage;
            return this;
        }
        
        public ProjectileTrap SetPer(int per)
        {
            this.per = per;
            return this;
        }

        public ProjectileTrap SetAimResource(string aimName, Vector3 pos, float scale)
        {
            this.aimResource = aimName;
            var pool = PoolManager.Instance.GetPool(aimResource);
            aimParticle = pool.Rent().GetComponent<ParticleBase>();
            aimParticle.transform.position = pos;
            aimParticle.transform.localScale = Vector3.one * scale;
            aimParticle.Pool = pool;
            return this;
        }

        public ProjectileTrap SetBlockType(int blockType, int blockedType)
        {
            this.ProjectileBlockType = blockType;
            this.ProjectileBlockedType = blockedType;
            return this;
        }

        public ProjectileTrap SetDuration(float duration)
        {
            this.duration = duration;
            if (duration > 0)
            {
                Observable.Timer(TimeSpan.FromSeconds(this.duration)).Subscribe(_ =>
                {
                    disposables.Clear();
                    HitProjectile();
                }).AddTo(disposables);
            }

            return this;
        }

        public ProjectileTrap SetPool(ObjectPooling<ObjectBase> Pool)
        {
            this.Pool = Pool;
            return this;
        }
        
        public ProjectileTrap SetKnockBack(float dist)
        {
            KnockBackDist = dist;

            return this;
        }

        public ProjectileTrap SetUnit(UnitBase unit)
        {
            this.unit = unit;
            return this;
        }

        public ProjectileTrap SetAnimation(string anim, Action callback)
        {
            animatorPlayer?.Play(anim, callback);
            return this;
        }

        public ProjectileTrap SetbombScale(float bombScale)
        {
            this.bombScale = bombScale;
            return this;
        }
        
        public ProjectileTrap SetAutoTarget(UnitBase targetUnit, float speed)
        {
            // n초 자동타겟
            this.targetUnit = targetUnit;
            Observable.EveryFixedUpdate().Subscribe(t =>
            {
                var dir = MyMath.GetDirection(transform.position, targetUnit.transform.position);
                rigid.velocity = dir * speed;
                if(particleRotation)
                    transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                FlipX();
            }).AddTo(disposables);
            return this;
        }
        

        public void Build()
        {
            if(coll)
                coll.enabled = true;
            if(rigid)
                rigid.velocity = velocity;
            
            if(skillParticle)
                skillParticle.SetActive(true);
            if(hitParticle)
                hitParticle.SetActive(false);
        }

        public void FlipX()
        {
            var dot = targetUnit.transform.position.x - transform.position.x;
            if (Math.Abs(dot) < 0.01f) return;
            
            var isFlip = targetUnit.Rigid.position.x > transform.position.x;
            
            
            var scale = transform.localScale;
            scale.x = (isFlip) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            transform.localScale = scale;
        }
        
        #region 충돌

        private void OverlapUnit(Collider2D col)
        {
            // if (per == 0) return;
            
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                if (per > -1)
                {
                    IDamageable _IDamageable;// = col.attachedRigidbody.GetComponent<IDamageable>();
                    UnitBase _unitBase;// = col.attachedRigidbody.GetComponent<UnitBase>();
                    IsUnitBase(col, out _IDamageable, out _unitBase);
                    
                    if (_IDamageable == null) return;
                    if (!_unitBase.isLive) return;

                    HitProjectile();
                }
            }
        }
        
        private void OverlapBullet(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                if (per > 0 || per == -100)
                {
                    if (col.GetComponent<Bullet>() == null) return;

                    var bullet = col.GetComponent<Bullet>();

                    if (ProjectileBlockType == bullet.BlockedType)
                    {
                        bullet.SetActive(false);
                    }
                }
            }
        }
        
        public void HitProjectile()
        {
            if (per == -100) return;
            
            --per;

            if (per <= 0)
            {
                disposables.Clear();
                rigid.velocity = Vector2.zero; // 이펙트 고정
                rigid.drag = 1000f;
                if(skillParticle)
                    skillParticle.SetActive(false);
                aimParticle?.ReturnPool();
                HitAll(Radius * bombScale);
                
                if (hitParticle)
                {
                    hitParticle.SetActive(true);
                    hitParticle.Play(true);
                    AddExplosionCallback?.Invoke();
                    Observable.Timer(TimeSpan.FromSeconds(hitParticle.main.duration)).Subscribe(_ =>
                    {
                        // ExitArea에서 return 처리
                        gameObject.SetActive(false);
                    }).AddTo(disposables);
                }
                else
                {
                    AddExplosionCallback?.Invoke();
                    gameObject.SetActive(false);
                }
            }
        }

        private void ExitArea(Collider2D col)
        {
            if (!col.CompareTag("Area"))
                return;
            
            coll.enabled = false;
            Pool?.Return(this);
        }

        #endregion
    }
}
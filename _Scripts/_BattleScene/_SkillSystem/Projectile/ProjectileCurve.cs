using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using InfiniteValue;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class ProjectileCurve : Bullet
    {
        public ParticleSystem skillParticle, hitParticle;
        private TrailRenderer _trailRenderer;
        public Transform hitPivot;
        
        private Vector3[] pos;
        private float time;
        private float speed;
    
        private Action hitCallback;
        // option
        private string aimResource;
        private ParticleBase aimParticle;
        private int per;
        public override void Awake()
        {
            base.Awake();
            rigid = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
            if (hitPivot)
            {
                hitPivot.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnit).AddTo(this);
            }
        }

        private void OnDisable()
        {
            disposables.Clear();
        }

        public ProjectileCurve InitBuilder()
        {
            disposables.Clear();
            velocity = Vector2.zero;
            attack = 0;
            per = 0;
            ProjectileBlockType = 0;
            ProjectileBlockedType = 0;
            return this;
        }
        
        public ProjectileCurve SetVelocity(Vector2 velocity)
        {
            this.velocity = velocity;
            return this;
        }
        public ProjectileCurve SetAccDamageFunc(Action<InfVal> accDamage)
        {
            this.AccDamageFunc = accDamage;
            return this;
        }
        public ProjectileCurve SetDamage(InfVal damage)
        {
            this.attack = damage;
            return this;
        }
        
        public ProjectileCurve SetPer(int per)
        {
            this.per = per;
            return this;
        }

        public ProjectileCurve SetBlockType(int blockType, int blockedType)
        {
            this.ProjectileBlockType = blockType;
            this.ProjectileBlockedType = blockedType;
            return this;
        }

        public ProjectileCurve SetDuration(float duration)
        {
            this.duration = duration;
            if (duration > 0)
            {
                Observable.Timer(TimeSpan.FromSeconds(this.duration)).Subscribe(_ =>
                {
                    disposables.Clear();
                    gameObject.SetActive(false);
                }).AddTo(disposables);
            }

            return this;
        }

        public ProjectileCurve SetPool(ObjectPooling<ObjectBase> Pool)
        {
            this.Pool = Pool;
            return this;
        }
        
        public ProjectileCurve SetKnockBack(float dist)
        {
            KnockBackDist = dist;

            return this;
        }

        public ProjectileCurve SetUnit(UnitBase unit)
        {
            this.unit = unit;
            return this;
        }

        public ProjectileCurve SetAnimation(string anim, Action callback)
        {
            animatorPlayer?.Play(anim, callback);
            return this;
        }

        public void Build()
        {
            if(coll)
                coll.enabled = false;
            
            skillParticle.SetActive(true);
            if(hitParticle)
                hitParticle.SetActive(false);
            if(hitPivot)
                hitPivot.SetActive(false);
            
            time = 0;
            transform.position = pos[0];
            this.UpdateAsObservable().Subscribe(Move).AddTo(disposables);
            // 폭격위치 지정
            if (!string.IsNullOrEmpty(aimResource))
            {
                var pool = PoolManager.Instance.GetPool(aimResource);
                aimParticle = pool.Rent().GetComponent<ParticleBase>();
                aimParticle.transform.position = pos[2];
                aimParticle.Pool = pool;
            }
        }

        public ProjectileCurve SetPosition(Vector3[] pos)
        {
            this.pos = pos;
            return this;
        }

        public ProjectileCurve SetAimResource(string aimName)
        {
            this.aimResource = aimName;
            return this;
        }

        public ProjectileCurve SetSpeed(float speed)
        {
            this.speed = speed;
            return this;
        }

        public void HitCallback(SkillAI data, InfVal damage)
        {
            hitCallback = () =>
            {
                Pool.Return(this);
                aimParticle.ReturnPool();
                
                PoolManager.Instance.CreatePool(data.ObjectResource, 1);
                var pool = PoolManager.Instance.GetPool(data.ObjectResource);
                var obj = pool.Rent();
                obj.transform.position = pos[2];
                
                var bulletScript = obj.GetComponent<DamageOverTime>();
                bulletScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(damage)
                    .SetTick(data.DamegeTime / 1000f)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetUnit(unit)
                    .Build();
            };
        }

        public void EndCallback(float range)
        {
            hitCallback = () =>
            {
                _trailRenderer?.Clear();
                skillParticle.SetActive(false);
                aimParticle.ReturnPool();
                
                HitAll(range);
                if (hitPivot)
                {
                    hitPivot.SetActive(true);
                    Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ =>
                    {
                        hitPivot.SetActive(false);
                    }).AddTo(disposables);
                }
                
                if (hitParticle)
                {
                    hitParticle.SetActive(true);
                    hitParticle.Play(true);
                    Observable.Timer(TimeSpan.FromSeconds(hitParticle.main.duration)).Subscribe(_ =>
                    {
                        ReturnPool();
                    }).AddTo(disposables);
                }
                else
                {
                    ReturnPool();
                }
            };
        }
        

        void Move(Unit i)
        {
            var basePos = unit.transform.position;
            
            // transform.position = MyMath.Lerp(pos[0], pos[1] = basePos, pos[2] + basePos, time / speed);
            transform.position = MyMath.Lerp(pos[0], pos[1], pos[2], time);
            time += Time.deltaTime * speed;
            // aimParticle.transform.position = pos[2] + basePos;
            
            if (time >= 1)
            {
                // Pool.Return(this);
                disposables.Clear();
                hitCallback?.Invoke();
                AddExplosionCallback?.Invoke();
            }
        }
        
        private void OverlapUnit(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                // if (per > 0 || per == -100)
                {
                    IDamageable _IDamageable;
                    UnitBase _unitBase;
                    IsUnitBase(col, out _IDamageable, out _unitBase);
                    
                    
                    if (_IDamageable == null) return;
                    if (!_unitBase.isLive) return;
                    
                    // 크리티컬 증가량.
                    var _attack = unit.CalcCriticalDamage(attack, _unitBase, out var hitType);
                    _IDamageable.TakeDamage(_attack, hitType);
                    
                    var dir = MyMath.GetDirection(unit.transform.position, col.transform.position);
                    _IDamageable.TakeKnockBack(dir, KnockBackDist);

                    AccDamageFunc?.Invoke(_attack);
                }
            }
        }
    }
}
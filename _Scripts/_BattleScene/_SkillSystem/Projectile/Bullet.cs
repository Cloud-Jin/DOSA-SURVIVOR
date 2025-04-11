using System;
using InfiniteValue;
using NSubstitute.Exceptions;
using UniRx;
using UnityEngine;

namespace ProjectM.Battle
{
    public class Bullet : ObjectBase, IPoolObject
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }
        public LayerMask targetLayer;
        public Rigidbody2D rigid;
        protected Collider2D coll;
        
        protected AnimatorPlayer animatorPlayer;
        protected Action AddExplosionCallback;  // 추가타 콜백
        public InfVal attack;                   // 계산된 실제 공격력
        // option
        protected UnitBase unit;                // 시전자
        protected float duration;               // 유지시간
        protected Vector2 velocity;             // 진행방향
        protected float KnockBackDist;          // 넉백거리
        protected int ProjectileBlockType;      // 다른 투사체 제거 타입
        protected int ProjectileBlockedType;    // 스킬 충돌 시, 제거되는 타입
        protected Action<InfVal> AccDamageFunc; // 누적 데미지;
        protected int Per;                      // 99 무한, 0 관통x (1마리) , 1 관통(2 마리까지)
        protected int BounceCount;              // 벽 충돌 카운트  ( 99무한, 0 튕기지않음, 1이상 튕기는 횟수)
        protected int BoundType;                // 충돌 타입 1 = 화면, 2 = 벽,  3 = 적 객체, 4 = 화면, 벽, 적 객체 모두
        protected bool IsBound;                 // 바운드 여부
        protected float Speed;                  // 투사체 스피드
        protected string ShotSound;             // 발사 사운드
        protected string HitSound;              // 적중 사운드
        protected string ExplosionSound;        // 폭발 사운드
        
        public int BlockedType => ProjectileBlockedType;
        public bool BossBullet => unit as EnemyBoss;
        public CompositeDisposable disposables = new CompositeDisposable();

        public override void Awake()
        {
            base.Awake();
            rigid = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
        }

        protected void Init()
        {
            disposables.Clear();
            velocity = Vector2.zero;
            attack = 0;
            ProjectileBlockType = 0;
            ProjectileBlockedType = 0;
            duration = 0;
            KnockBackDist = 0;
            AddExplosionCallback = null;
            BounceCount = 0;
            IsBound = false;
        }

        protected void OnDestroy()
        {
            disposables.Clear();
        }

        public void HitAll(float range)
        {
            if (range <= 0) return;
            
            var collider2Ds = Physics2D.OverlapCircleAll(transform.position, range);

            foreach (var col in collider2Ds)
            {
                if(targetLayer.Contanis(col.gameObject.layer))
                {
                    IDamageable _IDamageable;
                    UnitBase _unitBase;
                    IsUnitBase(col, out _IDamageable, out _unitBase);
                    if (_IDamageable == null) continue;
                    if (!_unitBase.isLive) continue;

                    // 크리티컬 증가량.
                    var _damage = unit.CalcCriticalDamage(attack, _unitBase, out var hitType);
                    _IDamageable.TakeDamage(_damage, hitType);
                    AccDamageFunc?.Invoke(_damage);
                }
            }
        }

        public void DamageCheck()
        {
            var hero = unit as Hero;
            if (hero)
            {
                AccDamageFunc = hero.AccDamageFunc;
            }
        }
        
        public void HitAll(Transform pivot, float range)
        {
            var collider2Ds = Physics2D.OverlapCircleAll(pivot.position, range);

            foreach (var col in collider2Ds)
            {
                if(targetLayer.Contanis(col.gameObject.layer))
                {
                    IDamageable _IDamageable;
                    UnitBase _unitBase;
                    IsUnitBase(col, out _IDamageable, out _unitBase);
                    if (_IDamageable == null) continue;
                    if (!_unitBase.isLive) continue;

                    // 크리티컬 증가량.
                    var _damage = unit.CalcCriticalDamage(attack, _unitBase, out var hitType);
                    _IDamageable.TakeDamage(_damage, hitType);
                    AccDamageFunc?.Invoke(_damage);
                }
            }
        }

        protected void OverlapBullet(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                if (col.GetComponent<Bullet>() == null) return;

                var bullet = col.GetComponent<Bullet>();

                if (ProjectileBlockType == bullet.BlockedType)
                {
                    bullet.SetActive(false);
                }
            }
        }
        
        public void AddExplosion(Action callback)
        {
            AddExplosionCallback = () =>
            {
                // Debug.Log("폭발 콜백");
                callback?.Invoke();
            };
        }

        public void ReturnPool()
        {
            disposables.Clear();
            Pool?.Return(this);
        }
        
        public void IsUnitBase(Collider2D col, out IDamageable IDamageable, out UnitBase unitBase)
        {
            if (col.attachedRigidbody)
            {
                IDamageable = col.attachedRigidbody.GetComponent<IDamageable>();
                unitBase = col.attachedRigidbody.GetComponent<UnitBase>();
            }
            else
            {
                IDamageable = col.GetComponent<IDamageable>();
                unitBase = col.GetComponent<UnitBase>();
            }
        }
    }
}
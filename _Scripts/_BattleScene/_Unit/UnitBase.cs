using System;
using System.Collections;
using System.Collections.Generic;
using DamageNumbersPro;
using InfiniteValue;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectM.Battle
{
    public enum UnitState
    {
        Normal,
        NonTarget,
        NoHit,
    }
    public class UnitBase : ObjectBase
    {
        public virtual UnitType UnitType { get; set; }
        public int unitID;
        public bool isLive;
        public InfVal health;                               // 현재 체력
        public InfVal maxHealth;                            // 계산된 최대 체력
        public InfVal attack;                               // 계산된 실제 공격력

        public UnitState unitState;
        [HideInInspector] public InfVal baseAttack;         // 기본 공격력
        [HideInInspector] public InfVal baseHealth;         // 기본 체력
        [HideInInspector] public float baseSpeed;           // 기본 스피드
        [HideInInspector] public int baseCriticalRate;      // 기본 치명타확률
        public Dictionary<string, Transform> BodyParts;

        protected Monster monster;
        protected Rigidbody2D rigid;
        protected AnimatorPlayer animPlayer;
        protected Animator anim;
        protected SpriteRenderer spriter;
        protected Collider2D coll;
        protected bool isOnlyRun;   // Run state
        protected bool isRun;       // Run || Boss state
        protected bool IsRun(Unit i) => isRun;
        protected bool IsRunOnly(Unit i) => isOnlyRun;
        public Rigidbody2D Rigid => rigid;
        public SpriteRenderer Spriter => spriter;
        public Animator Anim => anim;
        public AnimatorPlayer AnimatorPlayer => animPlayer;
        public CompositeDisposable disposables = new CompositeDisposable();
        public bool isKnockBack;
        public bool isHitAniPlayEnable;
        public Action deadAction;


        protected int knockBackResistance;
        private IDisposable wallCheck;
        private bool isWall;
        
        public override void Awake()
        {
            base.Awake();
            BattleManager.Instance.BattleState.Subscribe(t =>
            {
                isOnlyRun = t == BattleState.Run;
                isRun = t is BattleState.Run or BattleState.Boss;
            }).AddTo(this);
            
            var parts = transform.Find("Parts");
            if (parts)
            {
                BodyParts = new Dictionary<string, Transform>();
                BodyParts.Add("Parts", parts);
                
                var body = parts.Find("Body");
                BodyParts.Add("Body",body);
                
                body.TryGetComponent(out spriter);
                body.TryGetComponent(out anim);
                body.TryGetComponent(out animPlayer);
                
                BodyParts.Add("Weapon_Left", body.Find("Weapon_Left"));
                BodyParts.Add("Weapon_Middle", body.Find("Weapon_Middle"));
                BodyParts.Add("Weapon_Right", body.Find("Weapon_Right"));
                BodyParts.Add("Weapon_Pivot", body.Find("Weapon_Pivot"));
            }
            
            rigid = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
        }

        public virtual void Init(Monster stat)
        {
            monster = stat;
            baseSpeed = stat.Speed;
            health = maxHealth = baseHealth = stat.HP;
            baseAttack = stat.Attack;
            baseCriticalRate = stat.CriticalRatio;
            isLive = true;

            knockBackResistance = stat.KnockBackResistance;
        }

        public void PlayAnim(string anim)
        {
            if (!isLive) return;

            this.anim.Play(anim);
        }
        
        protected IEnumerator KnockBack(Vector2 dir, float dist)
        {
            var resistance = knockBackResistance / 10000f;

            dist = dist - (dist * resistance);
            
            if(dist <= 0) // 사거리 미만이면 넉백저항
                yield break;
            // if(isKnockBack) // 넉백중 넉백안됨. 
            //     yield break;
            
            isWall = false;
            wallCheck?.Dispose();
            wallCheck = this.OnCollisionEnter2DAsObservable().Subscribe(WallCollision);
            // rigid.mass = 1f;
            var startPosition = Rigid.position;
            var remainDistance = (startPosition - Rigid.position).magnitude;
            isKnockBack = true;
            rigid.velocity = dir * (dist * 10f);//10f;  // 역방향으로 보냄
            
            while (remainDistance <= dist && !isWall) // range & 벽이면 넉백 풀림
            {
                remainDistance = (startPosition - Rigid.position).magnitude;
                yield return new WaitForFixedUpdate();
            }
            wallCheck.Dispose();
            rigid.velocity = Vector2.zero;
            // rigid.mass = 0.0001f;
            
            isKnockBack = false;
        }
        
        void WallCollision(Collision2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                isWall = true;
            }
        }
        
        protected virtual void Dead()
        {
            
        }

        // 데미지 UI
        public void DamageHitUI(InfVal value, HitType hitType, Vector3 position)
        {
            if(hitType == HitType.None) return;
            
            // 데미지 폰트 출력
            GameObject numberPrefab = null;
            string resourcesName = String.Empty;
            switch (hitType)
            {
                case HitType.Normal:
                    resourcesName = "Damage Number1";
                    break;
                case HitType.Fatal:
                    resourcesName = "Damage Number2";
                    break;
                case HitType.HyperFatal:
                    resourcesName = "Damage Number3";
                    break;
            }
            numberPrefab = ResourcesManager.Instance.GetResources<GameObject>(resourcesName);
            //<DamageNumber>
            DamageNumber damageNumber = numberPrefab.GetComponent<DamageNumber>().Spawn(position);
            // damageNumber.number = (int)value;
            damageNumber.topText = value.ToParseString();
        }

        /// <summary>
        /// true dir Right
        /// </summary>
        public void FlipX(bool isFlip)
        {
            // var scale = spriter.transform.localScale;
            var scale = BodyParts["Parts"].transform.localScale;
            scale.x = (isFlip) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            BodyParts["Parts"].transform.localScale = scale;
        }
        
        public void FlipX(float isFlip)
        {
            // var scale = spriter.transform.localScale;
            var scale = BodyParts["Parts"].transform.localScale;
            scale.x = (isFlip > 0) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            BodyParts["Parts"].transform.localScale = scale;
        }

      
        
        // 크리티컬 데미지계산.
        public virtual InfVal CalcCriticalDamage(InfVal value, UnitBase target, out HitType hitType)
        {
            hitType = HitType.Normal;
            
            // 치명타 계산.
            if (MyMath.RandomPer(10000, baseCriticalRate))
            {
                hitType = HitType.Fatal;
                value *= 2f;
            }
            
            // TODO 치명타시 하이퍼 치명타 계산식 추가
            return value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class Playerble : UnitBase, IDamageable
    {
        public override UnitType UnitType { get; set; } = UnitType.Player;
        public List<HitRange> hitRanges;
        public Transform slot;
        public float speed;

        [HideInInspector] public Vector2 inputVec;
        
        
        private Player player;
        private float itemRange;
        private LayerMask ItemLayer;
        private ReactiveCollection<UnitBase> enemys = new ReactiveCollection<UnitBase>();
        private IDisposable timer;
        public bool MovePause { get; set; }
        public void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
            itemRange = TableDataManager.Instance.GetStageConfig(10).Value / 10f;
            ItemLayer = LayerMask.GetMask("Objects");
            isLive = true;
        }

        public void SetPlayer()
        {
            player = PlayerManager.Instance.player;
            this.FixedUpdateAsObservable().Subscribe(FixedGetItem).AddTo(this);
            this.FixedUpdateAsObservable().Where(_=> !MovePause).Subscribe(FixedMove).AddTo(this);
            this.LateUpdateAsObservable().Where(_=> !MovePause).Subscribe(LateMove).AddTo(this);
            
            this.OnCollisionEnter2DAsObservable().Subscribe(OverlapEnterEnemy).AddTo(this);
            this.OnCollisionExit2DAsObservable().Subscribe(OverlapExitEnemy).AddTo(this);
            enemys.ObserveCountChanged().Subscribe(num =>
            {
                if (num == 0)
                {
                    // 타이머 정지
                    timer?.Dispose();
                }
                else if (num == 1)
                {
                    // 타이머 시작
                    timer?.Dispose();
                    timer = Observable.Interval(TimeSpan.FromSeconds(0.2f)).Subscribe(OverlapEnemyC).AddTo(this);
                }
            }).AddTo(this);

            rigid.bodyType = RigidbodyType2D.Dynamic;
            unitID = 1;
            
            BattleManager.Instance.BattleState.Subscribe(t =>
            {
                isOnlyRun = t == BattleState.Run;
                isRun = t is BattleState.Run or BattleState.Boss;
            }).AddTo(this);
        }

        public void SetHitBox(int num)
        {
            for (int i = 0; i < hitRanges.Count; i++)
            {
                bool enabled = i == num - 1;
                hitRanges[i].hit.SetActive(enabled);
                if (enabled)
                    player.hitEffect = hitRanges[i].hitParticleSystem;
            }
        }
        
        
        void FixedMove(Unit unit)
        {
            Vector2 nextVec = inputVec.normalized * (speed / 10f * Time.fixedDeltaTime);
            rigid.MovePosition(rigid.position + nextVec);
        }

        void LateMove(Unit unit)
        {
            PlayerManager.Instance.LateMove(inputVec);
        }
        
        void FixedGetItem(Unit unit)
        {
            var items = Physics2D.CircleCastAll(transform.position, itemRange, Vector2.zero, 0, ItemLayer);
            for (int i = 0; i < items.Length; i++)
            {
                bool isComponent = items[i].transform.TryGetComponent(out ItemBase itemBase);
                if(isComponent)
                    itemBase.Magnet();
            }
        }
        
        public void SetItemRange()
        {
            var baseRange = TableDataManager.Instance.GetStageConfig(10);
            itemRange = MyMath.Increase(baseRange.Value / 10f, SkillSystem.Instance.incValue.PickUpRange);
        }
        
        void OverlapEnterEnemy(Collision2D col)
        {
            if (!col.gameObject.CompareTag("Enemy"))
                return;

            var unit = col.gameObject.GetComponent<UnitBase>();
            if (!unit.isLive) return;
            
            enemys.Add(unit);
            if (enemys.Count == 1)
            {
                player.TakeDamage(unit.attack, HitType.Normal);
            }
        }
        
        void OverlapExitEnemy(Collision2D col)
        {
            if (!col.gameObject.CompareTag("Enemy"))
                return;

            enemys.Remove(col.gameObject.GetComponent<UnitBase>());
        }

        void OverlapEnemyC(long i)
        {
            if (enemys.Count(t => t.isLive) == 0)
            {
                enemys.Clear();
                timer.Dispose();
                return;
            }
            
            var a = enemys.OrderByDescending(t => t.attack).First();
            
            player.TakeDamage(a.attack, HitType.Normal);
        }

        public void OnDead()
        {
            isLive = false;
            rigid.simulated = false;
            enemys.Clear();
        }

        public void OnRevive()
        {
            isLive = true;
            rigid.simulated = true;
        }

        [System.Serializable]
        public class HitRange
        {
            public GameObject hit;
            public ParticleSystem hitParticleSystem;
            public List<Transform> pos;
            public Transform pivot;
        }

        public UnitBase TakeDamage(InfVal damage, HitType hitType)
        {
            player.TakeDamage(damage, hitType);
            return this;
        }
    }
}

using System.Linq;
using InfiniteValue;
using UnityEngine;
using UniRx;
using UnityEditor.Rendering;


namespace ProjectM.Battle
{
    public class Enemy : UnitBase// , IPoolObject, IDamageable
    {
        // public override UnitType UnitType { get; set; }
        // protected int expPieceGroupID;
        public Rigidbody2D target;

        WaitForFixedUpdate wait;
        public override void Init(Monster stat)
        {
            base.Init(stat);
            unitID = ++BattleManager.Instance.EnemyUnitID;

            spriter.sortingLayerName = MyLayer.Enemy;
            rigid.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rigid.mass = 0.0001f;
        }

        public override void Awake()
        {
            base.Awake();
            wait = new WaitForFixedUpdate();
        }

        void OnEnable()
        {
            target = PlayerManager.Instance.PlayerRigidBody2D;
            isLive = true;
            coll.enabled = true;
            rigid.simulated = true;
            isKnockBack = false;
            anim.SetBool("Dead", false);
            health = maxHealth;
        }

        private void OnDisable()
        {
            disposables.Clear();
        }

        protected override void Dead()
        {
            GetComponent<IPoolObject>().ReleaseObject(this);
        }

        public virtual UnitBase TakeDamage(InfVal value, HitType hitType)
        {
            // health -= value;
            // DamageHitUI(value, hitType, transform.position);
            //
            // if (health > 0)
            // {
            //     PlayAnim("Hit");
            //     // anim.SetTrigger("Hit");
            //     // AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
            // }
            // else
            // {
            //     isLive = false;
            //     coll.enabled = false;
            //     // rigid.simulated = false;
            //     animPlayer.Play("Dead", Dead);
            //     // anim.SetBool("Dead", true);
            //     
            //     BattleManager.Instance.kill.Value++;
            //     var tbExpPieceGroup = TableDataManager.Instance.GetMonsterExpPriceGroup(expPieceGroupID);
            //     int val = MyMath.Pick(tbExpPieceGroup.Select(t => t.RewardRatio).ToArray());
            //     if (tbExpPieceGroup[val].ExpPieceType > 0)
            //     {
            //         // 경험치 조각 드랍
            //         var pool = PoolManager.Instance.GetPool("ExpItem");
            //         var item = pool.Rent();
            //         var itemScript = item.GetComponent<ExpItem>();
            //         itemScript.Pool = pool;
            //         itemScript.Init(tbExpPieceGroup[val].ExpPieceType);
            //         itemScript.transform.position = transform.position;
            //     }
            //
            //     // if (GameManager.instance.isLive)
            //     //     AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
            // }

            return this;
        }

        public virtual void TakeDamageDot(float value, float time)
        {
            
        }

        public void TakeKnockBack(Vector2 dir, float dist)
        {
            Observable.FromCoroutine(_ => KnockBack(dir, dist)).Subscribe().AddTo(this);
        }

        public virtual void DropItem()
        {
            Debug.Log("드랍");
            var rewardGroups = TableDataManager.Instance.data.RewardGroup.Where(t => t.GroupID == 1).ToArray();

            // 개별확률
            foreach (var data in rewardGroups.Where(t => t.RewardPayType == 1).ToList())
            {
                if (MyMath.RandomPer(10000, data.RewardRatio))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var pos = transform.position + MyMath.RandomCirclePoint(1);
                        if (BattleManager.Instance.MapOverlapPoint(pos))
                        {
                            var item = BattleItemManager.Instance.DropItem(data.RewardID, pos);
                            break;
                        }
                    }
                }
            }

            // 그룹 확률
            {
                var itemlist = rewardGroups.Where(t => t.RewardPayType == 2).ToList();
                var pick = MyMath.Pick(itemlist.Select(t => t.RewardRatio).ToArray());
                if (pick == -1)
                    return;
               
                for (int i = 0; i < 100; i++)
                {
                    var pos = transform.position + MyMath.RandomCirclePoint(1);
                    if (BattleManager.Instance.MapOverlapPoint(pos))
                    {
                        var item = BattleItemManager.Instance.DropItem(itemlist[pick].RewardID, pos);
                        break;
                    }
                }
                
            }

            deadAction -= DropItem;
        }
    }
}
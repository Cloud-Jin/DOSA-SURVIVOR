using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class ProjectileBomb : MonoBehaviour
    {
        public LayerMask targetLayer;
        protected Rigidbody2D rigid;
        protected Collider2D coll;
        private Player player;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            player = PlayerManager.Instance.player;

            // PoolManager.Instance.CreatePool("Effect_Bomb", 1);
            this.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnit).AddTo(this);
        }

        private void OnEnable()
        {
            var seq = DOTween.Sequence();
            seq.AppendInterval(0.5f).AppendCallback(() => { gameObject.SetActive(false); });
            SoundManager.Instance.PlayFX("Stage_Item_Bomb");
        }

        private void OverlapUnit(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                // Bullet 검사
                if (col.transform.TryGetComponent(out Bullet bullet))
                {
                    if(bullet.BossBullet)
                        return;
                    
                    bullet.Pool.Return(bullet);
                    return;
                }

                // unit 검사
                if (col.GetComponent<IDamageable>() == null) return;
                

                if (col.TryGetComponent(out EnemyNormal enemy))
                {
                    if (enemy.EnemyType == EnemyType.Normal || enemy.EnemyType == EnemyType.Summon)
                    {
                        var damage = enemy.maxHealth;
                        enemy.TakeDamage(damage, HitType.None);
                    }
                    else if(enemy.EnemyType == EnemyType.Elite)
                    {
                        var rate = TableDataManager.Instance.GetStageConfig(50).Value / 10000f;
                        var damage = enemy.maxHealth * rate;
                        enemy.TakeDamage(damage, HitType.None);
                    }

                    return;
                }

                if (col.TryGetComponent(out EnemyBoss boss))
                {
                    if (boss.unitState != UnitState.Normal) return;
                    
                    var rate = TableDataManager.Instance.GetStageConfig(51).Value / 10000f;
                    var damage = boss.maxHealth * rate;
                    boss.TakeDamage(damage, HitType.None);
                    return;
                }
            }
        }
    }
}
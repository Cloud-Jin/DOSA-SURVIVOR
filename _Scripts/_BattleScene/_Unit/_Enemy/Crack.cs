using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DamageNumbersPro;
using DG.Tweening;
using Gbros.UniRx;
using InfiniteValue;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;


// 차원균열
// 일정시간마다 일정 횟수 등장
namespace ProjectM.Battle
{
    public class Crack : Enemy, IDamageable, IPoolObject
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }
        private float despawnTime = 30f;


        private UIFollow uiFollow;
        private FollowArrow followArrow;
        
        public ReactiveProperty<float> HP, Timer;
        private IDisposable _CrackDisposable;
        

        public override void Awake()
        {
            base.Awake();
            UnitType = UnitType.Crack;

        }

        public override void Init(Monster stat)
        {
            base.Init(stat);
            despawnTime = TableDataManager.Instance.GetStageConfig(24).Value;
            HP = new ReactiveProperty<float>(1);
            Timer = new ReactiveProperty<float>(1);

            //uiFollow Set
            var canvas = UIManager.Instance.GetCanvas("Follow").transform;
            uiFollow = ResourcesManager.Instance.Instantiate("HealthTime", canvas).GetComponent<UIFollow>();
            uiFollow.SetFollow(transform, new Vector3(0, -1.2f,0));
            
            HP.Value = 1f;
            HP.Subscribe(v => uiFollow.SetHPBar(v)).AddTo(this);
            Timer.Subscribe(v => uiFollow.SetTimeBar(v)).AddTo(this);
            
            followArrow = ResourcesManager.Instance.Instantiate("UI_Battle_Item_Icon", canvas).GetComponent<FollowArrow>();
            followArrow.SetFollow(transform, new Vector3(0, 0,0));
            
            _CrackDisposable = PowerObservable.Countdown(BattleManager.Instance.timerPause, despawnTime)
                .Select(t => (int)t.TotalSeconds)
                .Subscribe(TimerBarTick, CrackDespawnTimeComplete);

            rigid.mass = 9999f;
            
            unitID = ++BattleManager.Instance.EnemyUnitID;
            BattleManager.Instance.CrackDespawn.Subscribe(CrackDespawn).AddTo(disposables);
        }
        
        /// <param name="enhanceRatio">스테이지 계수</param>
        /// <param name="HPIncrease">체력 증가율</param>
        public void SetSpawnData(InfVal enhanceRatio, int HPIncrease)
        {
            // base * 계수 + ( %능력치 증가율)
            var _HP = MyMath.CalcCoefficient(baseHealth, enhanceRatio);
            health = maxHealth = MyMath.Increase(_HP, HPIncrease);
        }
        
        // 임시로 작업
        void OnEnable()
        {
            target = PlayerManager.Instance.PlayerRigidBody2D;
            isLive = true;
            coll.enabled = true;
            rigid.simulated = true;
            spriter.sortingOrder = 2;
            anim.SetBool("Dead", false);
            health = maxHealth;
        }
        void TimerBarTick(int i)
        {
            Timer.Value = (i / despawnTime);
        }
        
        void CrackDespawnTimeComplete()
        {
            Timer.Value = 0;
            BattleManager.Instance.CrackDespawn.OnNext(false);
        }

        void CrackDespawn(bool isKill)
        {
            _CrackDisposable.Dispose(); // 타이머 제거
            disposables.Clear();
            uiFollow.SetActive(false);
            followArrow.SetActive(false);
            
            GetComponent<IPoolObject>().ReleaseObject(this);

            if (isKill)
            {
                var popup = UIManager.Instance.Get(PopupName.Battle_DimensionRift);
                popup.Show();
                popup.HideCallback(() =>
                {
                    var sequence = DOTween.Sequence();
                    sequence.AppendCallback(() =>
                    {
                        var rate = TableDataManager.Instance.GetSkillAiData(402, 1).DamageRatio / 100f;
                        PlayerManager.Instance.OnHealing(rate);
                        BattleManager.Instance.Bomb();
                    });

                    sequence.AppendInterval(0.2f).AppendCallback(() =>
                    {
                        BattleItemManager.Instance.MagnetExpItem();
                    });
                });
            }
        }

        public override UnitBase TakeDamage(InfVal value, HitType hitType)
        {
            if (!isLive) return this;
            if (unitState != UnitState.Normal) return this;
            
            DamageHitUI(value, hitType, transform.position);
            health -= value;
            HP.Value = (float)(health / maxHealth);
            SoundManager.Instance.PlayFX("Hit_Common");
            
            if (health > 0) 
            {
                anim.SetTrigger("Hit");
            }
            else
            {
                isLive = false;
                rigid.velocity = Vector2.zero;
                // rigid.simulated = false;
                coll.enabled = false;
                spriter.sortingOrder = 1;
                anim.SetBool("Dead", true);
                
                // BattleManager.Instance.kill.Value++;
                BattleManager.Instance.CrackKill.Value++;
                BattleManager.Instance.CrackDespawn.OnNext(true);
            }

            return this;
        }
    }

}
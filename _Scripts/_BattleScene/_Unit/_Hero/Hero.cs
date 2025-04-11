using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using ProjectM.Battle._Fsm;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class Hero : PlayerUnitBase, IPoolObject//, IDamageable
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }
        protected Player player;
        public Scanner scanner;
        public HeroStateMachine stateMachine;
        public HeroStateMachine StateMachine => stateMachine;
        
        public UIFollow uiFollow;
        // public ReactiveProperty<float> HP;              //
        // public ReactiveProperty<float> TimerBar;        //
        
        private float MaxFollowDist = 6.5f;
        private float dist;
        
        private ReactiveCollection<UnitBase> enemys;    // 몸박 데미지 리스트
        private IDisposable timer;
        public InfVal accDamage;
        
        public Action<InfVal> AccDamageFunc; // 누적 데미지;

        public Card GetArtifact => TableDataManager.Instance.GetCard(monster.Index);//TableDataManager.Instance.data.Artifact.Single(t => t.HeroCharacterID == monster.Index);
        protected void AccDamage(InfVal val)
        {
            accDamage += val;
        }
        public override void Awake()
        {
            base.Awake();
            // HP = new ReactiveProperty<float>();
            // TimerBar = new ReactiveProperty<float>(0);
            scanner = gameObject.AddComponent<Scanner>();
            scanner.Init(5f, LayerMask.GetMask("Enemy"));
            UnitType = UnitType.Hero;
            // enemys = new ReactiveCollection<UnitBase>();
        }

        private void OnEnable()
        {
            isLive = true;
            coll.enabled = false;
            rigid.simulated = true;
            isKnockBack = false;
        }

        private void OnDisable()
        {
            disposables.Clear();
        }

        public override void Init(Monster stat)
        {
            base.Init(stat);
            player = PlayerManager.Instance.player;
            AccDamageFunc = AccDamage;
            rigid.bodyType = RigidbodyType2D.Kinematic;
            rigid.mass = 0.1f;
            rigid.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            baseSpeed = 0;
            // 어빌리티
            // Player Base
            // var artifact = TableDataManager.Instance.data.Artifact.Single(t => t.HeroCharacterID == stat.Index);
            // var factor = TableDataManager.Instance.data.ArtifactHeroAbilityFactor.Single(t => t.Rarity == artifact.ArtifactRarity);
            // int level = 1;
            // var rarityFactor = factor.AbilityFactor / 10000f;
            // var LevelFactor = (factor.LevelFactor / 10000f) * level;
            // var resultFactor = rarityFactor + LevelFactor;
            // player * resultFactor
            // var player = PlayerManager.Instance.player;

            // baseAttack = player.baseAttack * resultFactor;
            // baseHealth = player.baseHealth * resultFactor;
            //
            // attack = baseAttack;
            // health = maxHealth = baseHealth;
            
            
            unitID = stat.Index;
            
            // var canvas = UIManager.Instance.GetCanvas("Follow").transform;
            // uiFollow = ResourcesManager.Instance.Instantiate("HealthSkill", canvas).GetComponent<UIFollow>();
            // uiFollow.SetFollow(transform, new Vector3(0,-0.55f,0));
            // HP.Subscribe(v => uiFollow.SetHPBar(v)).AddTo(this);
            // HP.Value = (float)(health/maxHealth);
            //
            // TimerBar.Subscribe(v => uiFollow.SetTimeBar(v)).AddTo(this);
            // TimerBar.Value = 0;

            // this.OnCollisionEnter2DAsObservable().Where(_ => isRun).Subscribe(OverlapEnterEnemy).AddTo(this);
            // this.OnCollisionExit2DAsObservable().Where(_ => isRun).Subscribe(OverlapExitEnemy).AddTo(this);

            // enemys.ObserveCountChanged().Subscribe(num =>
            // {
            //     if (num == 0)
            //     {
            //         // 타이머 정지
            //         timer?.Dispose();
            //     }
            //     else if (num == 1)
            //     {
            //         // 타이머 시작
            //         timer?.Dispose();
            //         timer = Observable.Interval(TimeSpan.FromSeconds(0.2f)).Subscribe(OverlapEnemyC);
            //     }
            // }).AddTo(this);
        }

        public void Init(Monster stat, InfVal damage, Action<InfVal> accDamage)
        {
            base.Init(stat);
            baseAttack = damage;
            attack = baseAttack;
            rigid.mass = 0.1f;
            AccDamageFunc = accDamage;
        }
        
        private void Start()
        {
            stateMachine = new HeroStateMachine(this);
            var patterns = TableDataManager.Instance.GetSkillPattern(monster.PatternGroupID);
            foreach (var pattern in patterns)
            {
                foreach (var skill in TableDataManager.Instance.GetSkillAiDatas(pattern.SkillGroupID))
                {
                    if (skill.Level == 1)
                        stateMachine.AddSkillState(this, skill, stateMachine);
                }
            }
            
            SkillSystem.Instance.heroLevel.Subscribe(lv =>
            {
                stateMachine.ChangeSkillData(lv);
            }).AddTo(this);
            stateMachine.Initialize(stateMachine.teleportState);
            
            
            this.UpdateAsObservable().Where(IsRun).Where(t=> isLive).Subscribe(StateMachineUpdate).AddTo(this);
            // Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(5f)).Where(_ => isLive).Subscribe(CheckMap).AddTo(this);
        }
        
        public void LateMove(Vector2 inputVec)
        {
            anim.SetFloat("Speed", inputVec.sqrMagnitude);
            
            if (inputVec.x != 0) 
            {
                FlipX(inputVec.x);
            }
        }

        public override void Revive()
        {
            isLive = true;
            anim.SetActive(false);
            anim.SetActive(true);
            stateMachine.ResetCoolTime();
            stateMachine.Initialize(stateMachine.idleState);
        }

        // void Revive()
        // {
        //     // 살아나다.
        //     health = maxHealth;
        //     isLive = true;
        //     coll.enabled = true;
        //     rigid.simulated = true;
        //     uiFollow.SetActive(true);
        //     // HP.Value = (float)(health / maxHealth);
        //     stateMachine.ResetCoolTime();
        //     stateMachine.Initialize(stateMachine.teleportState);
        // }
        void StateMachineUpdate(Unit i)
        {
            stateMachine.Tick();
        }
        
        void CheckMap(long i)
        {
            dist = (player.transform.position - transform.position).magnitude;
            if (dist > MaxFollowDist)
            {
                Teleport();
                // 7.5 ~ 8 (화면 밖 )거리면 순간이동
                // 웨이브모드시 화면 증가하면 거리도 증가 ? 
                return;
            }

            var pos = transform.position;
            if (!BattleManager.Instance.MapOverlapPoint(pos))
            {
                Teleport();
            }
            
            
        }
        
        /*void OverlapEnterEnemy(Collision2D col)
        {
            if (!col.gameObject.CompareTag("Enemy"))
                return;

            var unit = col.gameObject.GetComponent<UnitBase>();
            enemys.Add(unit);
            if (enemys.Count == 1)
            {
                TakeDamage(unit.attack, HitType.Normal);
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
            TakeDamage(a.attack, HitType.Normal);
        }*/

        protected override void Dead()
        {
            isLive = false;
            
            // if (monster.RespawnCoolTime == 0)
            // {
            //     Pool.Return(this);
            //     return;
            // }
            //
            // gameObject.SetActive(false);
            // var battleUI = UIManager.Instance.GetView(ViewName.Battle_Top).GetComponent<UIBattleTop>();
            //
            // battleUI.SetHeroRespawn(monster.Index, monster.RespawnCoolTime/1000f, () =>
            // {
            //     Revive();
            //     // Debug.Log("영웅 다시 살아남"); 
            // });
        }
        public void Return(float time)
        {
            Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(_ =>
            {
                disposables.Clear();
                stateMachine.SetIdelState();
                stateMachine.ResetCoolTime();
                
                rigid.MovePosition(rigid.position);
                animPlayer.Play("Dead", Dead);
                isLive = false;
                rigid.simulated = false;
                coll.enabled = false;

            }).AddTo(this);
        }

        public override void SetAttackStat()
        {
            base.SetAttackStat();
            attack = player.attack;
        }

        public override void SetCriticalRate()
        {
            base.SetCriticalRate();
            CriticalRatio = player.criticalRatio;
        }

        public override InfVal CalcCriticalDamage(InfVal value, UnitBase target, out HitType hitType)
        {
            hitType = HitType.Normal;
            // 크리티컬 패시브 추가.
            // 치명타 계산.
            if (MyMath.RandomPer(10000, CriticalRatio))
            {
                hitType = HitType.Fatal;
                value *= 2f;
            }
            
            return value;
        }

        // public UnitBase TakeDamage(InfVal damage, HitType hitType)
        // {
        //     if (maxHealth == 0) return this;
        //     if (unitState != UnitState.Normal) return this;
        //     var cValue = CalcDmgReduce(damage); 
        //     health -= cValue;
        //     // HP.Value = (float)(health / maxHealth);
        //     
        //     if (health > 0) {
        //         // anim.SetTrigger("Hit");
        //     }
        //     else
        //     {
        //         isLive = false;
        //         coll.enabled = false;
        //         rigid.simulated = false;
        //         disposables.Clear();
        //         spriter.sortingOrder = 1;
        //         enemys.Clear();
        //         if(uiFollow)
        //             uiFollow.SetActive(false);
        //         animPlayer.Play("Dead", Dead);
        //     }
        //     return this;
        // }

        [Button]
        public void Teleport()
        {
            if (isLive)
            {
                stateMachine.TransitionTo(stateMachine.teleportState);
            }
        }
    }
}
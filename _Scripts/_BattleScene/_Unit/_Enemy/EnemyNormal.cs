using System.Linq;
using InfiniteValue;
using ProjectM.Battle._Fsm.Enemy;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 일반몬스터
namespace ProjectM.Battle
{
    public enum EnemyType
    {
        Normal,
        Elite,
        Summon
    }
    public class EnemyNormal : Enemy, IPoolObject, IDamageable
    {
        public EnemyType EnemyType = EnemyType.Normal;
        EnemyStateMachine stateMachine;
        public EnemyStateMachine StateMachine => stateMachine;
        public ObjectPooling<ObjectBase> Pool { get; set; }
        
        int expPieceGroupID;

        WaitForFixedUpdate wait;
        private int exp;
        Reposition reposition;
        
        public override void Awake()
        {
            base.Awake();
            wait = new WaitForFixedUpdate();
            UnitType = UnitType.Enemy; 
            reposition = GetComponent<Reposition>();
        }
        public override void Init(Monster stat)
        {
            base.Init(stat);
            exp = stat.Exp;
            if (EnemyType == EnemyType.Elite)
            {
                knockBackResistance = 10000;
                rigid.mass = 100f;
                reposition?.EventClear();
                return;
            }

            if (EnemyType == EnemyType.Summon)
            {
                reposition?.EventClear(); 
                return;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enhanceRatio">스테이지 계수</param>
        /// <param name="HPIncrease">체력 증가율</param>
        /// <param name="AttackIncrease">공격력 증가율</param>
        /// <param name="expMultiple">경험치</param>
        public void SetSpawnData(InfVal enhanceRatio, int HPIncrease, int AttackIncrease, int expMultiple)
        {
            // base * 계수 + ( %능력치 증가율)
            var _HP = MyMath.CalcCoefficient(baseHealth, enhanceRatio);
            health = maxHealth = MyMath.Increase(_HP, HPIncrease);
            
            var _attack = MyMath.CalcCoefficient(baseAttack, enhanceRatio);
            baseAttack = attack = MyMath.Increase(_attack, AttackIncrease);
            
            var expRangeVal =  exp * expMultiple;
            expPieceGroupID = TableDataManager.Instance.data.StageMonsterExpRange
                .Where(t => t.ExpMinCount <= expRangeVal).Last().ExpPieceGroupID;

            // 스킬 패턴
            if (stateMachine == null)
            {
                stateMachine = new EnemyStateMachine(this);
                stateMachine.idleState = new IdleState(this, stateMachine);

                var Patterns = TableDataManager.Instance.GetSkillPattern(monster.PatternGroupID);
                foreach (var pattern in Patterns)
                {
                    stateMachine.AddPattern(pattern.SkillGroupID);
                    foreach (var skill in TableDataManager.Instance.GetSkillAiDatas(pattern.SkillGroupID))
                    {
                        stateMachine.AddSkillState(this, skill, stateMachine);
                    }
                }
            }

            stateMachine.Initialize(stateMachine.idleState);
            stateMachine.GlobalCoolTime = 1f;
            this.UpdateAsObservable().Where(IsRun).Where(t=> isLive).Subscribe(StateMachineUpdate).AddTo(disposables);
        }

        public void SetPenalty()
        {
            var challengeBattleManager = ChallengeBattleManager.Instance as ChallengeBattleManager;
            var speedUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.SpeedUp)??0;
            if(speedUpValue > 0)
            { 
                baseSpeed = MyMath.Increase(baseSpeed, speedUpValue / 100f); 
            }

            float atkUp = 0, hpUp = 0; 
            
            var statUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.StatUp)??0;
            if(statUpValue > 0)
            {
                hpUp += statUpValue / 100f;
                atkUp += statUpValue / 100f;
                // health = maxHealth = MyMath.Increase(maxHealth, statUpValue/100f);
                // baseAttack = attack = MyMath.Increase(attack, statUpValue/100f);
            }
            
            var atkUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.AtkUp)??0;
            if(atkUpValue > 0)
            {
                atkUp += atkUpValue / 100f;
                // baseAttack = attack = MyMath.Increase(attack, atkUpValue/100f);
            }
            
            var hpUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.HpUp)??0;
            if(hpUpValue > 0)
            {
                hpUp += hpUpValue / 100f;
                // health = maxHealth = MyMath.Increase(maxHealth, hpUpValue/100f);
            }

            if (atkUp > 0)
            {
                baseAttack = attack = MyMath.Increase(attack, atkUp);
            }

            if (hpUp > 0)
            {
                health = maxHealth = MyMath.Increase(maxHealth, hpUp);
            }
            var sizeUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.SizeUp)??0;
            if(sizeUpValue > 0)
            {
                var size = MyMath.Increase(1, sizeUpValue/100f);
                transform.localScale = Vector3.one * size;
            }
            
            var superArmourUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.SuperArmour)??0;
            if(superArmourUpValue > 0)
            {
                knockBackResistance = 10000;
            }
        }

        public void SetDungeonSpawnData(InfVal enhanceRatio, InfVal dungeonRatio)
        {
            // 던전 몬스터
            // base * (%던전 레벨 능력치 증가율)
            var _HP = MyMath.CalcCoefficient(baseHealth, enhanceRatio);
            health = maxHealth = MyMath.CalcCoefficient(_HP, dungeonRatio);

            var _attack = MyMath.CalcCoefficient(baseAttack, enhanceRatio);
            baseAttack = attack = MyMath.CalcCoefficient(_attack, dungeonRatio);

            var expRangeVal = exp * 0;
            expPieceGroupID = TableDataManager.Instance.data.StageMonsterExpRange
                .Where(t => t.ExpMinCount <= expRangeVal).Last().ExpPieceGroupID;

            // 스킬 패턴
            if (stateMachine == null)
            {
                stateMachine = new EnemyStateMachine(this);
                stateMachine.idleState = new IdleDungeonState(this, stateMachine);
                
                var Patterns = TableDataManager.Instance.GetSkillPattern(monster.PatternGroupID);
                foreach (var pattern in Patterns)
                {
                    stateMachine.AddPattern(pattern.SkillGroupID);
                    foreach (var skill in TableDataManager.Instance.GetSkillAiDatas(pattern.SkillGroupID))
                    {
                        stateMachine.AddSkillState(this, skill, stateMachine);
                    }
                }
            }

            stateMachine.Initialize(stateMachine.idleState);
            stateMachine.GlobalCoolTime = 1f;
            this.UpdateAsObservable().Where(IsRun).Where(t=> isLive).Subscribe(StateMachineUpdate).AddTo(disposables);
            
            rigid.excludeLayers = LayerMask.GetMask("Enemy");
        }

        public void SetType(UnitType unitType, EnemyType enemyType)
        {
            this.UnitType = unitType;
            this.EnemyType = enemyType;
        }

        public void SetCoefficient(int ratio)
        {
            var _HP = MyMath.CalcCoefficient(maxHealth, ratio);
            health = maxHealth = _HP;
            
            var _attack = MyMath.CalcCoefficient(baseAttack, ratio);
            baseAttack = attack = _attack;
        }
        
        void StateMachineUpdate(Unit i)
        {
            stateMachine.Tick();
        }
        void OnEnable()
        {
            target = PlayerManager.Instance.PlayerRigidBody2D;
            isLive = true;
            coll.enabled = true;
            rigid.simulated = true;
            rigid.drag = 0;
            isKnockBack = false;
            anim.SetBool("Dead", false);
            isHitAniPlayEnable = true;
            health = maxHealth;
        }

        private void OnDisable()
        {
            disposables.Clear();
        }
        
        private void OnDestroy()
        {
            stateMachine = null;
            disposables.Clear();
        }

        protected override void Dead()
        {
            GetComponent<IPoolObject>().ReleaseObject(this);
        }

        public override UnitBase TakeDamage(InfVal value, HitType hitType)
        {
            if (!isLive) return this;
            if (unitState != UnitState.Normal) return this;
            
            health -= value;
            DamageHitUI(value, hitType, transform.position);
            SoundManager.Instance.PlayFX("Hit_Common");
            
            if (health > 0)
            {
                if(isHitAniPlayEnable)
                    PlayAnim("Hit");
                // anim.SetTrigger("Hit");
                // AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
            }
            else
            {
                if (EnemyType == EnemyType.Normal)
                {
                    reposition?.EventClear();
                    BattleManager.Instance.enemyCount--;
                }
                if(EnemyType != EnemyType.Summon)
                    BattleManager.Instance.EnemyKill();
                
                isLive = false;
                rigid.velocity = Vector2.zero;
                // rigid.simulated = false;
                coll.enabled = false;
                disposables.Clear();
                animPlayer.Play("Dead", Dead);
                deadAction?.Invoke();
                
                
                var tbExpPieceGroup = TableDataManager.Instance.GetMonsterExpPriceGroup(expPieceGroupID);
                int val = MyMath.Pick(tbExpPieceGroup.Select(t => t.RewardRatio).ToArray());
                if (tbExpPieceGroup[val].ExpPieceType > 0)
                {
                    var item = BattleItemManager.Instance.DropItem(tbExpPieceGroup[val].ExpPieceType, transform.position);
                }

              

                // if (GameManager.instance.isLive)
                //     AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
            }

            return this;
        }

        public override void TakeDamageDot(float value, float time)
        {
            
        }

        public void ReleaseObject(ObjectBase unit)
        {
            isLive = false;
            Pool.Return(unit);
        }
    }
}
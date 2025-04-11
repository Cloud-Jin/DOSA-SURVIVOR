using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using InfiniteValue;
using ProjectM.Battle._Fsm.Boss;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class EnemyBoss : Enemy, IDamageable
    {
        private BossMonsterGroup tbBossMonsterGroup;
        private FloatReactiveProperty Hp;
        public BossStateMachine stateMachine;
        public BossStateMachine StateMachine => stateMachine;
        
        private List<SkillAI> skillAis;
        
        [Header("# Game Object")]
        public Transform damagePivot;
        
        public override void Awake()
        {
            base.Awake();
            damagePivot = transform.Find("Parts/Body/Damage");
            UnitType = UnitType.Boss;
            rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        public override void Init(Monster stat)
        {
            base.Init(stat);
        }

        public void SetSpawnData(BossMonsterGroup bossTable, InfVal BaseRatio, int num)
        {
            tbBossMonsterGroup = bossTable;
            
            var _HP = MyMath.CalcCoefficient(baseHealth, BaseRatio);
            health = maxHealth = MyMath.Increase(_HP, tbBossMonsterGroup.MonsterHPIncrease);
            
            var _attack = MyMath.CalcCoefficient(baseAttack, BaseRatio);
            baseAttack = attack = MyMath.Increase(_attack, tbBossMonsterGroup.MonsterAttackIncrease);

            if (baseSpeed == 0)
            {
                rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            var popup = UIManager.Instance.GetPopup(PopupName.Battle_BossName) as PopupBossName;
            Hp = popup.SetData(num, monster.Name);
            Hp.Value = 1;
            rigid.mass = 1f;
            
            // 스킬패턴
            stateMachine = new BossStateMachine(this);
            stateMachine.Initialize(stateMachine.idleState);
            stateMachine.GlobalCoolTime = 4f;
            
            var Patterns = TableDataManager.Instance.GetSkillPattern(monster.PatternGroupID);
            foreach (var pattern in Patterns)
            {
                stateMachine.AddPattern(pattern.SkillGroupID);
                foreach (var skill in TableDataManager.Instance.GetSkillAiDatas(pattern.SkillGroupID))
                {
                    stateMachine.AddSkillState(this, skill, stateMachine);
                }
            }
            this.UpdateAsObservable().Where(IsRun).Where(_=> isLive).Subscribe(StateMachineUpdate).AddTo(this);
            
            // 생성후 무적타임
            unitState = UnitState.NoHit;
            Observable.Timer(System.TimeSpan.FromSeconds(1.5f)).Subscribe(_ =>
            {
                unitState = UnitState.Normal;
            }).AddTo(this);

            // 대사
            var canvas = UIManager.Instance.GetCanvas("Follow").transform;
            var sb = ResourcesManager.Instance.Instantiate("SpeechBubble", canvas).GetComponent<SpeechBubble>();
            sb.SetTalk(damagePivot, LocaleManager.GetLocale(monster.BossSpawnTalk), 3f);
            // 드롭테이블 설정
            deadAction -= DropItem;
            deadAction += DropItem;
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
            }
            
            var atkUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.AtkUp)??0;
            if(atkUpValue > 0)
            {
                atkUp += atkUpValue / 100f;
            }
            
            var hpUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.HpUp)??0;
            if(hpUpValue > 0)
            {
                hpUp += hpUpValue / 100f;
            }

            if (atkUp > 0)
            {
                baseAttack = attack = MyMath.Increase(attack, atkUp);
            }

            if (hpUp > 0)
            {
                health = maxHealth = MyMath.Increase(maxHealth, hpUp);
            }
            
            var superArmourUpValue = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.SuperArmour)??0;
            if(superArmourUpValue > 0)
            {
                knockBackResistance = 10000;
            }
        }
        void StateMachineUpdate(Unit i)
        {
            stateMachine.Tick();
        }
        
        public override UnitBase TakeDamage(InfVal value, HitType hitType)
        {
            if (!isLive) return this;
            if (unitState != UnitState.Normal) return this;
            
            // 데미지 폰트 출력
            DamageHitUI(value, hitType, transform.position);
            health -= value;
            Hp.Value = (float)(health / maxHealth);
            SoundManager.Instance.PlayFX("Hit_Common");
            
            if (health > 0) {
                var mat = spriter.material;
                mat.EnableKeyword("HITEFFECT_ON");
                mat.SetFloat("_HitEffectGlow", 1);
                mat.SetFloat("_HitEffectBlend", 0.1f);
                Sequence hit = DOTween.Sequence();
                hit.Append(mat.DOFloat(6, "_HitEffectGlow", 0.15f))
                    .SetAutoKill(true)
                    // .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        mat.DisableKeyword("HITEFFECT_ON");
                    });
            }
            else
            {
                isLive = false;
                rigid.velocity = Vector2.zero;
                // rigid.simulated = false;
                coll.enabled = false;
               
                
                deadAction?.Invoke();
                BattleManager.Instance.BossEnd();
                disposables.Clear();
                animPlayer.Play("Dead", Dead);
                
                var canvas = UIManager.Instance.GetCanvas("Follow").transform;
                var sb = ResourcesManager.Instance.Instantiate("SpeechBubble", canvas).GetComponent<SpeechBubble>();
                sb.SetTalk(damagePivot, LocaleManager.GetLocale(monster.BossDeadTalk), 3f);
                
                // BattleManager.Instance.kill.Value++;

                // if (GameManager.instance.isLive)
                //     AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
            }

            return this;
        }

      protected override void Dead()
      {
          gameObject.SetActive(false);
      }

      public override void DropItem()
      {
          if(tbBossMonsterGroup.RewardGroupID == 0) return;
          
          var rewardGroups = TableDataManager.Instance.data.RewardGroup.Where(t => t.GroupID == tbBossMonsterGroup.RewardGroupID).ToList();

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
      }
    }
}

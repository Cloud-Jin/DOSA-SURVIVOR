using System;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using UniRx;
using UnityEngine;

namespace ProjectM.Battle
{
    public class Player : PlayerUnitBase, IDamageable
    {
        public override UnitType UnitType { get; set; } = UnitType.Player;
        public bool IsNoDamage = false;
        
        public ReactiveProperty<float> HP;
        public UIFollow UIFollow;
    
        private string[] weapon = { "None", "Weapon_Charm", "Weapon_Sword", "Weapon_Fan", "Weapon_Bow", "Weapon_Stick" };

        private int _criticalRatioGearOption;
        private float _criticalDmgGearOption;
        private float _NormalMobDmgGearOption;
        private float _BossMobDmgGearOption;
        [HideInInspector] public int missStat;
        private ParticleSystem missEffect; 
        private ParticleSystem levelEffect;
        public ParticleSystem hitEffect { get; set; }
        private IDisposable hitDispose;
        public override void Awake()
        {
            base.Awake();
            
            HP = new ReactiveProperty<float>();
            rigid.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            
            var body = transform.Find("Parts/Body");
            if (body)
            {
                BodyParts.Add("Light", body.Find("Light 2D"));
                BodyParts.Add("RecyclingArea", body.Find("RecyclingArea"));
                levelEffect = body.Find("LevelupCylinderYellow").GetComponent<ParticleSystem>();
                ParticleSystem.MainModule mainModule = levelEffect.main;
                mainModule.playOnAwake = false;
                levelEffect.SetActive(true);
            }

            coll.enabled = false;
            /*itemRange = TableDataManager.Instance.GetStageConfig(10).Value / 10f;
            ItemLayer = LayerMask.GetMask("Objects");*/
        }
                
      
        public void SetPlayer()
        {
            /*this.FixedUpdateAsObservable().Where(_=> isLive).Subscribe(FixedGetItem).AddTo(this);
            this.FixedUpdateAsObservable().Where(_=> isLive).Subscribe(FixedMove).AddTo(this);
            this.LateUpdateAsObservable().Where(_ => isLive).Subscribe(LateMove).AddTo(this);
            
            this.OnCollisionEnter2DAsObservable().Where(_ => isRun).Subscribe(OverlapEnterEnemy).AddTo(this);
            this.OnCollisionExit2DAsObservable().Where(_ => isRun).Subscribe(OverlapExitEnemy).AddTo(this);
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
            */
            
            SetAreaSize();
            // BattleManager.Instance.BossSpawn.Subscribe(t => Rigidbody2D.bodyType = RigidbodyType2D.Dynamic).AddTo(this);
            // BattleManager.Instance.BossDespawn.Subscribe(t => Rigidbody2D.bodyType = RigidbodyType2D.Dynamic).AddTo(this);
                
            // Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        
            // 장착 무기생성
            var costumeInfo = BlackBoard.Instance.GetCostume();
            var weaponInfo = BlackBoard.Instance.data.weaponInfo;
            if (costumeInfo.TargetEquipType == 0 || costumeInfo.TargetEquipType != weaponInfo.EquipType)
            {
                var weaponItem = ResourcesManager.Instance.Instantiate(weapon[weaponInfo.EquipType], BodyParts["Weapon_Pivot"]);
                weaponItem.GetComponent<SpriteRenderer>().sprite = ResourcesManager.Instance.GearIcons[weaponInfo.EquipIcon];
            }
            else
            {
                // if (costumeInfo.TargetEquipType == weaponInfo.EquipType)
                {
                    // 코스튬 장비로 변경
                    var weaponCostume = ResourcesManager.Instance.Instantiate(costumeInfo.ChangeEquipIcon, BodyParts["Weapon_Pivot"]);
                    // weaponCostume.GetComponent<SpriteRenderer>().sprite = ResourcesManager.Instance.GearIcons[weaponInfo.EquipIcon];
                }
            }

           
            var canvas = UIManager.Instance.GetCanvas("Follow").transform;
            UIFollow = ResourcesManager.Instance.Instantiate("HealthSkill", canvas).GetComponent<UIFollow>();
            UIFollow.SetFollow(transform, new Vector3(0,-0.4f,0));
            UIFollow.TimeBarHide();
            HP.Subscribe(v => UIFollow.SetHPBar(v)).AddTo(this);
            HP.Value = (float)(health/maxHealth);
            
            _criticalRatioGearOption = (int)(UserDataManager.Instance.gearInfo.GetEquipGearsPower().CriticalRatio * 10000f);
            _criticalDmgGearOption = (UserDataManager.Instance.gearInfo.GetEquipGearsPower().CriticalDmg * 100f);
            _NormalMobDmgGearOption = (UserDataManager.Instance.gearInfo.GetEquipGearsPower().NormalMobDmg * 100f);
            _BossMobDmgGearOption = (UserDataManager.Instance.gearInfo.GetEquipGearsPower().BossMobDmg * 100f);
            SetStat();
            BodyParts["Light"].SetActive(BattleManager.Instance.tbStage.DayNightType == 2);
        }

        private void OnDestroy()
        {
            hitDispose?.Dispose();
        }

        public override void Init(Monster stat)
        {
            base.Init(stat);
            // BlackBoard애서 가져옴
            var data = BlackBoard.Instance.data;
            baseAttack = InfVal.Parse(data.attack);
            baseHealth = InfVal.Parse(data.maxHp);
            baseCriticalRate += _criticalRatioGearOption;
            // 장비스텟 가져옴

            baseSpeed = MyMath.Increase(baseSpeed, UserDataManager.Instance.gearInfo.GetEquipGearsPower().CharMoveSpeed * 100f);
            
            speed = baseSpeed;
            attack = baseAttack;
            health = InfVal.Parse(data.hp);
            maxHealth = baseHealth;
            
            unitID = stat.Index;
            isLive = true;
            SetHpRate(1);
        }
        
        public void LateMove(Vector2 inputVec)
        {
            anim.SetFloat("Speed", inputVec.sqrMagnitude);
            
            if (inputVec.x != 0) 
            {
                FlipX(inputVec.x);
                // FlipX(inputVec.x > 0);
            }
        }
        
        void Hit()
        {
            if (!isLive)
                return;

            if (TutorialManager.Instance.IsTutorialBattle)
            {
                if (health < 0)
                    health = 1;
            }
            
            if (health < 0)
            {
                PlayerManager.Instance.DeadAction();
                // animPlayer.Play("Dead", Dead);
                // isLive = false;
            }
        }

        public void OnLevelUp()
        {
            levelEffect.Play();
        }

        public void SetUIFollow(Transform transform)
        {
            UIFollow.SetFollow(transform, new Vector3(0,0,0));
        }

        public void SetItemRange()
        {
            // to Playerble
            // var baseRange = TableDataManager.Instance.GetStageConfig(10);
            // itemRange = MyMath.Increase(baseRange.Value / 10f, SkillSystem.Instance.incValue.PickUpRange); 
        }

        void SetAreaSize()
        {
            Vector2 size = Vector2.zero;
            Vector2 recyclingSize = Vector2.zero;
            
            switch (BattleManager.Instance.map)
            {
                case Map.Infinite:
                    size = new Vector2(40f, 40f);
                    recyclingSize = new Vector2(15f, 25f);
                    break;
                case Map.Vertical:
                    size = new Vector2(40f, 40f);
                    recyclingSize = new Vector2(45f, 25f);
                    break;
                case Map.Rect:
                    size = new Vector2(40f, 40);
                    recyclingSize = new Vector2(45f, 25f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var area = transform.Find("Parts/Body/Area").GetComponent<BoxCollider2D>();
                area.size = size;
                area.includeLayers = LayerMask.GetMask("Background", "EnemyProjectiles", "PlayerProjectiles");
                area.excludeLayers = ~LayerMask.GetMask("Background", "EnemyProjectiles", "PlayerProjectiles");
            var recyclingArea = transform.Find("Parts/Body/RecyclingArea").GetComponent<BoxCollider2D>();
                recyclingArea.size = recyclingSize;
                recyclingArea.includeLayers = LayerMask.GetMask("Enemy");
                recyclingArea.excludeLayers = ~LayerMask.GetMask("Enemy");
        }

        public void SetRespawnSize(float value)
        {
            transform.Find("Parts/Body/RecyclingArea").localScale = Vector3.one * value;
        }

        public void SetMiss(int per)
        {
            this.missStat = per;
            // effect
            var go = ResourcesManager.Instance.Instantiate("Passive_Shield_00", transform);
            missEffect = go.GetComponent<ParticleSystem>();
        }

        public override void Healing(float rate)
        {
            base.Healing(rate);
            HP.Value = (float)(health/maxHealth);
        }

        public override void SetHpRate(float rate)
        {
            base.SetHpRate(rate);
            HP.Value = (float)(health/maxHealth);
        }

        public override void SetAttackStat()
        {
            base.SetAttackStat();
            int heroAtkIncrease = 0;
            PlayerManager.Instance.GetHeroList().ForEach(t => heroAtkIncrease += t.GetArtifact.BasicAtk);
            attack = MyMath.Increase(baseAttack, SkillSystem.Instance.incValue.damage + (heroAtkIncrease * 0.01f));
        }

        public override void SetHpStat()
        {
            base.SetHpStat();
            // 현재 최대 스탯 X (패시브 스킬 증가 값 + 영웅 소환으로 얻는 증가 값)
            float rate = HP.Value;
            int heroHPIncrease = 0;
            PlayerManager.Instance.GetHeroList().ForEach(t => heroHPIncrease += t.GetArtifact.BasicHP);
            maxHealth = MyMath.Increase(baseHealth, SkillSystem.Instance.incValue.Hp + (heroHPIncrease * 0.01f));     // 체력증가
            SetHpRate(rate);
            // HP.Value = (float)(health/maxHealth);
        }

        public override void SetCriticalRate()
        {
            base.SetCriticalRate();
            
            CriticalRatio += _criticalRatioGearOption;
        }

        public override void SetMoveStat()
        {
            base.SetMoveStat();

            PlayerManager.Instance.playerble.speed = speed;
        }

        public override InfVal CalcCriticalDamage(InfVal value, UnitBase target, out HitType hitType)
        {
            hitType = HitType.Normal;
            
            // 치명타 계산.
            if (MyMath.RandomPer(10000, CriticalRatio))
            {
                hitType = HitType.Fatal;
                value =  MyMath.Increase(value, _criticalDmgGearOption + 100);
            }
            
            // TODO 치명타시 하이퍼 치명타 계산식 추가

            // 일반몹, 보스몹 추가데미지 구현
            switch (target.UnitType)
            {
                case UnitType.Enemy:
                    value = MyMath.Increase(value, _NormalMobDmgGearOption);
                    break;
                case UnitType.Boss:
                    value = MyMath.Increase(value, _BossMobDmgGearOption + SkillSystem.Instance.incValue.BossDamgage);
                    break;
            }
            
            return value;
        }

        public UnitBase TakeDamage(InfVal value, HitType hitType)
        {
            if (IsNoDamage) return this;
            if (unitState != UnitState.Normal) return this;
            if (MyMath.RandomPer(10000, missStat))
            {
                missEffect.Play();
                return this;
            }
                
            
            var cValue = CalcDmgReduce(value); 
            if (cValue < 1) return this;
            
            this.health -= cValue;
            HP.Value = (float)(health/maxHealth);
            
            VibrationManager.Instance.VibratePop();
            hitDispose?.Dispose();
            hitEffect.SetActive(true);
            hitEffect.Play();
            hitDispose = Observable.Timer(TimeSpan.FromSeconds(hitEffect.main.duration)).Subscribe(_ =>
            {
                hitEffect.SetActive(false);
            });
            
            /*var mat = spriter.material;
            mat.EnableKeyword("HITEFFECT_ON");
            mat.SetFloat("_HitEffectGlow", 1);
            mat.SetFloat("_HitEffectBlend", 0.1f);
            Sequence hit = DOTween.Sequence();
            hit.Append(mat.DOFloat(6, "_HitEffectGlow", 0.15f))
                .SetAutoKill(true)
                .OnComplete(() =>
                {
                    mat.DisableKeyword("HITEFFECT_ON");
                    mat.SetFloat("_HitEffectBlend", 0f);
                });*/

            Hit();
            
            return this;
        }

        public void TakeDamageDot(float value, float time)
        {
            
        }

        public override void Revive()
        {
            health = maxHealth;
            SetHpRate(1);
            /*anim.SetFloat("Speed", inputVec.magnitude);*/
            anim.enabled = false;
            anim.enabled = true;
            isLive = true;
            rigid.simulated = true;
            coll.enabled = false;
        }


        protected override void Dead()
        {
           
        }
    }
}
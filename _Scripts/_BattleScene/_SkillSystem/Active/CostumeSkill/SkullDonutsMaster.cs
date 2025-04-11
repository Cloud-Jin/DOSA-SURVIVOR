using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 지옥의 기사가 등장해 휠윈드 상태로 적을 계속 쫒아다닌다.
// (캐릭터 및 보스 몬스터, 원거리 미사일 등 모든 오브젝트와 충돌 X)

namespace ProjectM.Battle
{
    public class SkullDonutsMaster : WeaponSkill
    {
        private int count;
        private float duration;
        private float castingTime;
        private bool timeStandby;
        private Vector3 dir;
        private UnitBase targetUnit;
        private DamageOverTime skull;
        
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
            TimerBar = new ReactiveProperty<float>(0);
            timeStandby = true;
            this.UpdateAsObservable().Where(_ => isRun && timeStandby).Subscribe(Running).AddTo(this);
        }
        
        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, 0);
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed / 10f, skillSystem.incValue.BulletSpeed);
            duration = data.DurationTime / 1000f;


            if (!skull) return;
            skull.SetAttack(damage);
            skull.transform.localScale = GetScale;
        }

        private void Update()
        {
            if (!TargetCheck() && skull)
            {
                targetUnit = PriorityTarget();
                skull.SetAutoTarget(targetUnit, speed);
            }
        }
        
        bool TargetCheck()
        {
            if (targetUnit == null)
                return false;
            
            if (!targetUnit.isLive)
                return false;

            return true;
        }

        // 쿨타임 추적
        // typevalue 범위 생성
        // 타겟 추적 해골발사
        
        public override void Fire()
        {
            count = data.Count;
            castingTime = data.CastingTime;
            timeStandby = false;

            Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
        }
        
        IEnumerator Launch()
        {
            count--;
            // dir = NearestDir();
            
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            player.PlayAnim(data.Ani);
            
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = Quaternion.identity;
                projectile.localScale = GetScale;
                projectile.position = player.BodyParts[data.Pivot].position;

                targetUnit = PriorityTarget();
                var projectileScript = projectile.GetComponent<DamageOverTime>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetSound("Skull_Donuts_M")
                    .SetDamage(damage)
                    .SetTick(data.DamegeTime / 1000f)
                    .SetDuration(data.DurationTime / 1000f, SetTimeAction)
                    .SetSpeed(speed)
                    .SetAutoTarget(targetUnit ,speed)
                    .SetAccDamageFunc(AccDamage)
                    .SetUnit(player)
                    .Build();

                skull = projectileScript;
            }
            
            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
            }
            // else
            // {
            //     timeStandby = true;
            // }
        }
        
        void SetTimeAction()
        {
            timeStandby = true;
            timer = 0;
            skull = null;
        }
    }
}
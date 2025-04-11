using System;
using System.Collections;
using System.Collections.Generic;
using InfiniteValue;
using UniRx;
using UniRx.Triggers;
using UnityEngine;


/*
 *번개 창이 화면 내 최대체력이 가장 높은 적을 향해 발사되며,
  중간에 지나는 적들을 관통하며 데미지를 주고,
  체력이 가장 높은 적에게 닿았을 때 폭발한다.
 */


namespace ProjectM.Battle
{
    public class LightningBallMaster : ActiveSkill
    {
        private float timer;
        private UnitBase target;
        private InfVal ExplosionDamage;

        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 5);
            this.UpdateAsObservable().Where(_ => isRun).Subscribe(Running).AddTo(this);
        }

        public override void Fire()
        {
            base.Fire();
            // 연발
            Observable.FromCoroutine(FireRoutine).Subscribe().AddTo(this);
        }

        public override void DataUpdate()
        {
            base.DataUpdate();

            var skillSystem = SkillSystem.Instance;
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, commonDmg);
            ExplosionDamage = damage;
            // ExplosionDamage = MyMath.CalcDamage(player.attack, data.TypeValue, 0);
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed / 10f, skillSystem.incValue.BulletSpeed);
        }

        void Running(Unit i)
        {
            timer += Time.deltaTime;

            if (timer >= coolTime)
            {
                Fire();
                timer = 0;
            }
        }

        void Shot()
        {
            Vector3 dir = Vector3.zero;
            target = PriorityTarget();
            dir = target == null ? LastDir() : MyMath.GetDirection(transform.position, target.transform.position);

            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            
            Transform projectile = pool.Rent().transform;
            projectile.localPosition = Vector3.zero;
            projectile.localRotation = Quaternion.identity;
            
            projectile.position = transform.position + (dir * 0.5f);
            projectile.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            projectile.transform.localScale = GetScale;

            var projectileScript = projectile.GetComponent<ProjectileExplosion>();
            projectileScript.InitBuilder()
                .SetDamage(damage)
                .SetExplosionDamage(ExplosionDamage)
                .SetPer(per)
                .SetVelocity(dir)
                .SetSpeed(speed)
                .SetPool(pool)
                .SetUnit(player)
                .SetDuration(data.DurationTime *0.001f)
                .SetTarget(target)
                .SetAccDamageFunc(AccDamage)
                .Build();
            
            SoundManager.Instance.PlayFX("RockSpear_M");
        }

        IEnumerator FireRoutine()
        {
            for (int index = 0; index < data.ObjectValue; index++)
            {
                Shot();
            }

            yield return 0;
        }
    }
}
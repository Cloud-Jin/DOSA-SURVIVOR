using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class LightningBall : ActiveSkill
    {
        private float timer;
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

            dir = PriorityDir();
            
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            Transform projectile = pool.Rent().transform;
            projectile.position = transform.position + (dir * 0.5f);
            projectile.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            projectile.transform.localScale = GetScale;

            var projectileScript = projectile.GetComponent<ProjectileExplosion>();
            projectileScript.InitBuilder()
                .SetDamage(0)
                .SetExplosionDamage(damage)
                .SetPer(per)
                .SetVelocity(dir)
                .SetSpeed(speed)
                .SetDuration(data.DurationTime*0.001f)
                .SetPool(pool)
                .SetUnit(player)
                .SetAccDamageFunc(AccDamage)
                .Build();

            SoundManager.Instance.PlayFX("RockSpear");
            // projectile.GetComponent<ProjectileExplosion>().Init(damage, per, dir, speed);
            // projectile.GetComponent<IPoolObject>().Pool = pool;
            // AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
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
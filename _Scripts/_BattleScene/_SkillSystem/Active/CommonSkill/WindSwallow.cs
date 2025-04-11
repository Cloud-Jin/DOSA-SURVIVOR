using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ProjectM.Battle
{
    public class WindSwallow : ActiveSkill
    {
        private List<Transform> bullets = new List<Transform>();
        private float timer;
        private float duration;
        
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
        }
        
        public override void Fire()
        {
            base.Fire();
        }

        public override void Dispose()
        {
            base.Dispose();
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Destroy(bullets[i].gameObject);
            }
        }

        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            for (int index = 0; index < data.ObjectValue; index++) 
            {
                Transform bullet;

                if (index < bullets.Count)
                {
                    bullet = bullets[index];
                }
                else
                {
                    bullet = ResourcesManager.Instance.Instantiate(data.ObjectResource).transform;
                    ShotProjectile(bullet);
                    bullets.Add(bullet);
                }
            }
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, commonDmg);
            coolTime = skillSystem.GetCoolTime(data.CoolTime *0.001f);
            speed = MyMath.Increase(data.Speed / 10f, skillSystem.incValue.BulletSpeed);
            duration = data.DurationTime *0.001f;
            
            for (int index = 0; index < data.ObjectValue; index++) 
            {
                Transform bullet;

                if (index < bullets.Count)
                {
                    bullet = bullets[index];
                    bullet.localScale = GetScale;
                    
                    var bulletScript = bullet.GetComponent<Projectile>();
                    bulletScript.InitBuilder()
                        .SetDamage(damage)
                        .SetPer(per)
                        .SetBounceCount(bound)
                        .SetVelocity(Random.insideUnitCircle.normalized)
                        .SetSpeed(speed)
                        .SetDuration(duration)
                        .SetUnit(player)
                        .SetAccDamageFunc(AccDamage)
                        .Build();

                    bulletScript.AddExplosion(() => Shot(bullet));
                }
            }
        }

        public void Shot(Transform bullet)
        {
            bullet.SetActive(false);
            Observable.Timer(TimeSpan.FromSeconds(coolTime)).Where(t => isRun).Subscribe(t =>
            {
                SoundManager.Instance.PlayFX("WindSwallow");
                ShotProjectile(bullet);
            }).AddTo(this);
        }

        void ShotProjectile(Transform bullet)
        {
            bullet.SetActive(true);
            bullet.position = player.transform.position;
            bullet.localRotation = Quaternion.identity;
            bullet.localScale = GetScale;

            var bulletScript = bullet.GetComponent<Projectile>();
            bulletScript.InitBuilder()
                .SetDamage(damage)
                .SetPer(per)
                .SetBounceCount(bound)
                .SetVelocity(Random.insideUnitCircle.normalized)
                .SetSpeed(speed)
                .SetDuration(duration)
                .SetUnit(player)
                .SetAccDamageFunc(AccDamage)
                .Build();

            bulletScript.AddExplosion(() => Shot(bullet));
            // bullet.GetComponent<WallBounce>().Init(damage, duration, speed, Shot);
        }

    }

}
using System;
using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Unity.Mathematics;

namespace ProjectM.Battle
{
    public class IgnisFatuusMaster : ActiveSkill
    {
        private float timer;
        private float castingTime;
        private bool timeStandby;
        
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 5);
            PoolManager.Instance.CreatePool(TableDataManager.Instance.GetSkillAiData(data.AddExplosionSkillId).ObjectResource, 5);
            PoolManager.Instance.CreatePool("Player_Dosa_Skill_Wisp_Phoenix_Aim", 5);
            timeStandby = true;
            this.UpdateAsObservable().Subscribe(Running).AddTo(this);
        }

        public override void Fire()
        {
            base.Fire();
            // 연발
            castingTime = data.CastingTime;
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
        
        IEnumerator FireRoutine()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            SoundManager.Instance.PlayFX("Ignis_Fatuus_M");
            for (int i = 0; i < data.ObjectValue; i++)
            {

                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = quaternion.identity;
                projectile.transform.localScale = GetScale;

                Vector3[] pos = new Vector3[3] { transform.position, Vector3.one, Vector3.one };
                pos[1] = transform.position + MyMath.RandomDonut((data.Angle / 10f / 2f));
                pos[2] = PriorityTarget(8)?.transform.position ?? player.BodyParts[data.Pivot].position;

                var bulletScript = projectile.GetComponent<ProjectileCurve>();
                bulletScript.InitBuilder()
                    .SetDamage(damage)
                    .SetPool(pool)
                    .SetPosition(pos)
                    .SetPer(-100)
                    .SetSpeed(speed)
                    .SetUnit(player)
                    .SetAimResource("Player_Dosa_Skill_Wisp_Phoenix_Aim")
                    .SetAccDamageFunc(AccDamage)
                    .Build();

                if (data.AddExplosionAble > 0)
                {
                    bulletScript.AddExplosion(() =>
                    {
                        var bullet = ExplosionBullet(data.AddExplosionSkillId);
                        bullet.transform.position = bulletScript.transform.position;
                        //pool.Return(bulletScript);

                    });
                    bulletScript.EndCallback(0);

                    SoundManager.Instance.PlayFX("Explosion_Ignis_Fatuus_M");
                }
            }
            Bullet ExplosionBullet(int skillID)
            {
                var eData = TableDataManager.Instance.GetSkillAiData(skillID);
                var pool = PoolManager.Instance.GetPool(eData.ObjectResource);
             
                var damage = MyMath.CalcDamage(player.attack, eData.DamageRatio, 0);
            
            
                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = Quaternion.identity;
                projectile.localScale = Vector3.one * MyMath.Increase((eData.Scale / 100f), SkillSystem.Instance.incValue.Scale);
            
                var projectileScript = projectile.GetComponent<DamageOverTime>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(damage)
                    .SetUnit(player)
                    .SetDuration(eData.DurationTime/1000f)
                    .SetTick(eData.DamegeTime/1000f)
                    .SetBlockType(eData.ProjectileBlockType, eData.ProjectileBlockedType)
                    .SetAccDamageFunc(AccDamage)
                    .Build();

                return projectileScript;
            }
        }
    }
}
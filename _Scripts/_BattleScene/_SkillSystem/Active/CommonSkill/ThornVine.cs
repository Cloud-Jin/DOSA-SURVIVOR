using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class ThornVine : ActiveSkill
    {
        private float timer;
        private int count;
        private float castingTime;
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 5);
            PoolManager.Instance.CreatePool(TableDataManager.Instance.GetSkillAiData(data.AddExplosionSkillId).ObjectResource, 5);
            this.UpdateAsObservable().Where(_ => isRun).Subscribe(Running).AddTo(this);
        }
        
        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, commonDmg);
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed /10f, skillSystem.incValue.BulletSpeed);
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

        public override void Fire()
        {
            Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
        }

        IEnumerator Launch()
        {
            count--;
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            player.PlayAnim(data.Ani);
            SoundManager.Instance.PlayFX("Thorn_Vine");
            
            float bAngle = UnityEngine.Random.Range(0, 360f); // base 각도
            var gAngle = 360f / data.ObjectValue;         // gap
            
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = Quaternion.identity;
                projectile.localScale = GetScale;
               
                Vector3[] pos = new Vector3[3] { player.BodyParts[data.Pivot].position, Vector3.one, Vector3.one};
                Vector3 temp = MyMath.CalcDonut(data.TypeValue/10f, bAngle + (gAngle*i), 1);
                pos[2] = player.transform.position + temp; 
                pos[1] = player.BodyParts[data.Pivot].position + MyMath.RandomDonut((data.Angle/10f /2f));
                pos[0] = PlayerManager.Instance.playerble.transform.position;
                
                var projectileScript = projectile.GetComponent<ProjectileCurve>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(damage)
                    .SetPosition(pos)
                    .SetSpeed(data.Speed/10f)
                    .SetUnit(player)
                    .Build();

                // 플레이어는 State가 없으므로 여기서 생성
                if (data.AddExplosionAble > 0)
                {
                    projectileScript.AddExplosion(() =>
                    {
                        var bullet = ExplosionBullet(data.AddExplosionSkillId);
                        bullet.transform.position = projectileScript.transform.position;
                        pool.Return(projectileScript);
                    });
                }
            }
            
            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
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
            projectile.localScale = GetDataScale(eData);
           
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
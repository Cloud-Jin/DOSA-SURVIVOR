using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class Fanning : WeaponSkill
    {
        private int count;
        private float castingTime;
        private bool timeStandby;
        private Vector3 dir;
        
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
            TimerBar = new ReactiveProperty<float>(0);
            timeStandby = true;
            this.UpdateAsObservable().Where(_ => isRun && timeStandby).Subscribe(Running).AddTo(this);
        }

        public override void Dispose()
        {
            PoolManager.Instance.ReturnPool(data.ObjectResource);
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, 0);
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed /10f, skillSystem.incValue.BulletSpeed);
        }

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
            SoundManager.Instance.PlayFX("Fanning");
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = Quaternion.identity;
                projectile.localScale = GetScale;
                // projectile.position = player.Rigid.position + (Vector2)(dir * data.TypeValue / 10f);
                projectile.position = PriorityTarget()?.transform.position ?? player.BodyParts[data.Pivot].position; 
                
                var projectileScript = projectile.GetComponent<DamageOverTime>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(damage)
                    .SetTick(data.DamegeTime / 1000f)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetVelocity(dir * speed)
                    .SetAccDamageFunc(AccDamage)
                    .SetUnit(player)
                    .Build();
            }
            
            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
            }
            else
            {
                timeStandby = true;
            }
        }

        void SetTimeAction()
        {
            timeStandby = true;
            timer = 0;
        }
    }
}
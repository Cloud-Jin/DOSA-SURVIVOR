using System;
using System.Collections;
using System.Collections.Generic;
using InfiniteValue;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle 
{
    public class JudgmentMaster : WeaponSkill
    {
        private int count;
        private float castingTime;
        private bool timeStandby;
        private InfVal explosionDamage; //폭파데미지
        
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 5);
            TimerBar = new ReactiveProperty<float>(0);
            timeStandby = true;
            this.UpdateAsObservable().Where(_ => isRun && timeStandby).Subscribe(Running).AddTo(this);
        }
        
        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, 0);
            explosionDamage = MyMath.CalcDamage(player.attack, data.TypeValue, 0);
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
                // projectile.rotation = Quaternion.FromToRotation(Vector3.up, Quaternion.AngleAxis(convertAngle[i], Vector3.forward) * dir);
                var projectileScript = projectile.GetComponent<DamageOverTime>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetSound("Judgment_M")
                    .SetDamage(damage)
                    .SetExplosionDamage(explosionDamage)
                    .SetDuration(data.DurationTime/1000f, SetTimeAction)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(player)
                    .SetAccDamageFunc(AccDamage)
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
            // timeStandby = true;
            // timer = 0;
            SoundManager.Instance.PlayFX("Explosion_Judgment_M");
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class MultipleShot : WeaponSkill
    {
        private int count;
        private float castingTime;
        List<float> convertAngle = new List<float>();
        private bool timeStandby;
        private Vector3 dir;
        
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
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed /10f, skillSystem.incValue.BulletSpeed);
            convertAngle.Clear();
            convertAngle = MyMath.CalcAngleCount(data.Angle, data.ObjectValue);
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
            // dir = LastDir();
            dir = PriorityDir();
            
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            player.PlayAnim(data.Ani);
            SoundManager.Instance.PlayFX("MultipleShot");
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = Quaternion.identity;
                projectile.localScale = GetScale;

                projectile.position = player.BodyParts[data.Pivot].position;
                projectile.rotation = Quaternion.FromToRotation(Vector3.up, Quaternion.AngleAxis(convertAngle[i], Vector3.forward) * dir);
                var projectileScript = projectile.GetComponent<Projectile>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(damage)
                    .SetPer(per)
                    .SetVelocity(Quaternion.AngleAxis(convertAngle[i], Vector3.forward) * dir)
                    .SetSpeed(speed)
                    .SetKnockBack(data.KnockBack / 10f)
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
    }
}
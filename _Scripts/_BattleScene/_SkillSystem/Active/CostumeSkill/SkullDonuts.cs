using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 화면 내 체력이 가장 높으며, 가까운 적에게 빠르게 이동하는 도너츠를 던진다.
// 도너츠는 적을 쫒아가며, 적에게 닿거나(대상이 아니여도),
// 일정 시간 경과 후 폭발한다.
// 폭발 시 범위내 적에게 대미지를 준다.

namespace ProjectM.Battle
{
    public class SkullDonuts : WeaponSkill
    {
        private float timer;
        private int count;
        private float castingTime;
        private bool timeStandby;
        private Vector3 dir;
        private UnitBase target;
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

            target = PriorityTarget();
            // target = player.scanner.GetRandomTarget();
            dir = target == null ? LastDir() : MyMath.GetDirection(transform.position, target.transform.position);
            
            player.PlayAnim(data.Ani);
            
            for (int i = 0; i < data.ObjectValue; i++)
            {
                
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = GetScale;
                
                bullet.position = player.BodyParts[data.Pivot].position;
                bullet.rotation = Quaternion.FromToRotation(Vector3.down, dir);
                var projectileScript = bullet.GetComponent<ProjectileExplosion>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetSound("Skull_Donuts")
                    .SetDamage(0)
                    .SetExplosionDamage(damage)
                    .SetPer(per)
                    .SetVelocity(dir)
                    .SetSpeed(speed)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(player)
                    .SetTarget(target)
                    .SetAutoTarget(target, speed)
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
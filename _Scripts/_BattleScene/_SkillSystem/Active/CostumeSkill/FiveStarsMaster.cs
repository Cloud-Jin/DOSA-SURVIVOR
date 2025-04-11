using System;
using System.Collections;
using System.Collections.Generic;
using InfiniteValue;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 화면 내 가장 가까운 적 위치에 불타는 오망성을 생성
// 오망성에 위치한 적은 일정 간격으로 도트 대미지를 받는다.
// 일정 시간 후 오망성이 폭발하며 추가 대미지를 준다.

namespace ProjectM.Battle
{
    public class FiveStarsMaster : WeaponSkill
    {
        private int count;
        private float castingTime;
        private bool timeStandby;
        private Vector3 dir;
        private InfVal explosionDamage; //폭파데미지
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
            // dir = NearestDir();
            
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            player.PlayAnim(data.Ani);
            // SoundManager.Instance.PlayFX("Fanning");
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
                    .SetSound("Five_Stars_M")
                    .SetDamage(damage)
                    .SetExplosionDamage(explosionDamage)
                    .SetTick(data.DamegeTime / 1000f)
                    .SetDuration(data.DurationTime / 1000f, SetTimeAction)
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
            SoundManager.Instance.PlayFX("Explosion_Five_Stars_M");
        }
    }
}
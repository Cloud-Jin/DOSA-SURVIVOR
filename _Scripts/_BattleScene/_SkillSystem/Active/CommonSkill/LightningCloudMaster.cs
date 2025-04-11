using System.Collections;
using System.Collections.Generic;
using System;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

// 번개구름
// ex 탕탕이 두리안
// 이동하면서 n마다 공격


namespace ProjectM.Battle
{
    public class LightningCloudMaster : ActiveSkill
    {
        public float tick;
        private DamageOverTime damageOverTime;
        private float dist;
        
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            // 이펙트 생성
            var effectResource = ResourcesManager.Instance.Instantiate(data.ObjectResource);
            damageOverTime = effectResource.GetComponent<DamageOverTime>();
            damageOverTime.transform.localPosition = Vector3.zero; 
            damageOverTime.transform.position = player.BodyParts[data.Pivot].position;
            
            damageOverTime.InitBuilder()
                .SetDamage(damage)
                .SetBounceCount(bound)
                .SetVelocity(Random.insideUnitCircle.normalized)
                .SetSpeed(data.Speed/10f)
                .SetSound("","Lightning_Cloud_M")
                .SetTick(data.DamegeTime/1000f)
                .SetAccDamageFunc(AccDamage)
                .SetDamageTime(data.DamegeTime/1000f)
                .SetUnit(player)
                .Build();
            
            
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(5f)).Subscribe(CheckMap).AddTo(gameObject);
            // SoundManager.Instance.PlayFX("LightningWave");
        }

        public override void Fire()
        {
            base.Fire();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            var skillSystem = SkillSystem.Instance;
            // 스킬 데미지 계산
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, commonDmg);
            speed = MyMath.Increase(data.Speed /10f, skillSystem.incValue.BulletSpeed);
            
            if (damageOverTime)
            {
                damageOverTime.SetAttack(damage);
                damageOverTime.SetSpeed(speed);
                damageOverTime.transform.localScale = GetScale;
            }
        }
        
        void CheckMap(long i)
        {
            dist = (player.transform.position - damageOverTime.transform.position).magnitude;
            if (dist > 30)
            {
                damageOverTime.transform.position = player.transform.position;
            }
        }
    }
}
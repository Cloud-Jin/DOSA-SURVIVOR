using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class WaterWave : ActiveSkill
    {
        // private float[] range = new[] { 0.5f, 0.7f, 1f, 1.2f, 1.5f };
        public float tick;
        private DamageOverTime damageOverTime;
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            // 이펙트 생성
            tick = data.DamegeTime / 1000f;
            var effectResource = ResourcesManager.Instance.Instantiate(data.ObjectResource);
            damageOverTime = effectResource.GetComponent<DamageOverTime>();
            damageOverTime.transform.parent = transform;
            damageOverTime.transform.localPosition = Vector3.zero;
            
            damageOverTime.InitBuilder()
                .SetDamage(damage)
                .SetTick(data.DamegeTime / 1000f)
                .SetKnockBack(data.KnockBack / 10f)
                .SetTick(tick)
                .SetUnit(player)
                .SetAccDamageFunc(AccDamage)
                .Build();
            
            SoundManager.Instance.PlayFX("LightningWave");
        }

        public override void Fire()
        {
            base.Fire();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            // 스킬 데미지 계산
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, commonDmg);

            if (damageOverTime)
            {
                damageOverTime.SetAttack(damage);
                damageOverTime.transform.localScale = GetScale;
            }
        }
    }
}

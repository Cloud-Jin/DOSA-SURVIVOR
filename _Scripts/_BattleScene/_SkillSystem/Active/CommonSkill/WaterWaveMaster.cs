using UnityEngine;

namespace ProjectM.Battle
{
    public class WaterWaveMaster : ActiveSkill
    {
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
            
            SoundManager.Instance.PlayFX("LightningWave_M");
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
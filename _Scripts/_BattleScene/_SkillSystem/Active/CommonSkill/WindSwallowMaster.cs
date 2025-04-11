using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

// 타겟 돌진
// 타겟 지속데미지


namespace ProjectM.Battle
{
    public class WindSwallowMaster : ActiveSkill
    {
        private List<Transform> bullets = new List<Transform>();
        
        private float timer;
        private float duration;
        private float durationDelte;
        private UnitBase targetUnit, lastTarget;
        
        private CompositeDisposable disposeAttack = new CompositeDisposable();
        private CompositeDisposable disposeTrace = new CompositeDisposable();
        
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);

            for (int index = 0; index < data.ObjectValue; index++) 
            {
                Transform bullet;

                if (index < bullets.Count)
                {
                    bullet = bullets[index];
                }
                else
                {
                    bullet = ResourcesManager.Instance.Instantiate(data.ObjectResource).transform;
                    ShotProjectile(bullet);
                    bullets.Add(bullet);
                }
            }

            SoundManager.Instance.PlayFX("WindSwallow_M");
            Observable.FromCoroutine(Trace).Subscribe().AddTo(disposeTrace);
        }

        public override void Dispose()
        {
            base.Dispose();
            targetUnit = null;
            disposeAttack.Clear();
            disposeTrace.Clear();
        }

        private void OnDestroy()
        {
            targetUnit = null;
            disposeAttack.Clear();
            disposeTrace.Clear();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, commonDmg);
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed / 10f, skillSystem.incValue.BulletSpeed);
            duration = data.DurationTime / 1000f;
            
            for (int index = 0; index < data.ObjectValue; index++) 
            {
                Transform bullet;

                if (index < bullets.Count)
                {
                    bullet = bullets[index];
                    bullet.localScale = GetScale;
                }
            }
        }

        void ShotProjectile(Transform bullet)
        {
            Vector3 dir = PriorityDir();
            bullet.SetActive(true);
            bullet.position = player.transform.position;
            bullet.localRotation = Quaternion.identity;
            bullet.localScale = GetScale;
            
            bullet.rotation = Quaternion.FromToRotation(Vector3.down, dir);
            var projectileScript = bullet.GetComponent<Projectile>();
            projectileScript.InitBuilder()
                .SetDamage(damage)
                .SetPer(99)
                .SetKnockBack(data.KnockBack/10f)
                .SetUnit(player)
                .SetAccDamageFunc(AccDamage)
                .Build();
        }

        private void Update()
        {
            if (!TargetCheck())
            {
                disposeAttack.Clear();
                Observable.FromCoroutine(Trace).Subscribe().AddTo(disposeTrace);
            }
        }

        IEnumerator Trace()
        {
            // Debug.Log("Trace");
            var projectileScript = bullets[0].GetComponent<Projectile>();
            targetUnit = PriorityTarget();
            if (targetUnit == null && lastTarget)
            {
                // 범위안에 몬스터가 없고 때리던놈 있으면 마저 때림
                targetUnit = lastTarget;
            }

            while (targetUnit == null)
            {
                targetUnit = PriorityTarget();
                projectileScript.SetVelocity(Vector2.zero)
                    .Build();
                yield return new WaitForFixedUpdate();
            }
            
            
            // 타겟한테 이동.
            
            projectileScript.skillParticle.SetActive(true);
            projectileScript.hitParticle.SetActive(false);
            
            // 타겟 거리체크.
            // 생존 체크
            
            projectileScript.InitBuilder()
                .SetDamage(damage)
                .SetPer(99)
                .SetKnockBack(data.KnockBack/10f)
                .SetUnit(player)
                .SetAccDamageFunc(AccDamage)
                .SetAutoTarget(targetUnit, speed)
                .Build();
            
            while (IsDistanceTarget())
            {
                var dir = MyMath.GetDirection(bullets[0].transform.position, targetUnit.transform.position);
                projectileScript.transform.rotation = Quaternion.FromToRotation(Vector3.down, dir);
                yield return new WaitForFixedUpdate();
            }
            
            Observable.FromCoroutine(Attack).Subscribe().AddTo(disposeAttack);
            Observable.FromCoroutine(AttackState).Subscribe().AddTo(disposeAttack);
        }

        IEnumerator AttackState()
        {
            // Debug.Log("AttackState");
            
            var projectileScript = bullets[0].GetComponent<Projectile>();
            
            projectileScript.skillParticle.SetActive(false);
            projectileScript.hitParticle.SetActive(true);

            durationDelte = 0f;
            while (durationDelte < duration)
            {
                durationDelte += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            lastTarget = targetUnit;
            disposeAttack.Clear();
            Observable.FromCoroutine(Trace).Subscribe().AddTo(disposeTrace);
        }
        
        IEnumerator Attack()
        {
            var projectileScript = bullets[0].GetComponent<Projectile>();
            while (true)
            {
                projectileScript.HitAll(data.TypeValue / 10f);
                yield return new WaitForSeconds(data.DamegeTime/1000f);
            }
        }
        
        bool IsDistanceTarget()
        {
            if (targetUnit == null)
                return false;
            
            var collider2Ds = Physics2D.OverlapCircleAll(bullets[0].transform.position, data.Range / 10f);

            return collider2Ds.Count(t => t.transform == targetUnit.transform) == 0;
        }

        bool TargetCheck()
        {
            if (targetUnit == null)
                return false;
            
            if (!targetUnit.isLive)
                return false;

            return true;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle 
{
    public class ShurikenMaster : WeaponSkill
    {
        private int count;
        private float castingTime;
        private bool timeStandby;
        // private Vector3 dir;
        private Vector2[] dir = new[] { new Vector2(0, 1), new Vector2(1,0), new Vector2(0,-1), new Vector2(-1,0) }; 
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
            // convertAngle.Clear();
            // convertAngle = MyMath.CalcAngleCount(data.TypeValue, data.ObjectValue);
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
            SoundManager.Instance.PlayFX("Shuriken_M");
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
                    .SetDamage(damage)
                    .SetSpeed(speed)
                    .SetTick(data.DamegeTime/1000f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(player)
                    .SetAccDamageFunc(AccDamage)
                    .Build();

                var dist = data.Range / 10f;
                Sequence seq = DOTween.Sequence();
                seq.Append(projectileScript.transform.DOLocalMove(dir[i] * dist, dist/speed).SetEase(Ease.Linear).SetRelative());//.SetSpeedBased());
                seq.AppendInterval(data.DurationTime / 1000f);
                seq.Append(projectileScript.transform.DOMoveInTargetLocalSpace(player.transform, Vector3.zero, 0.5f)).OnComplete(
                    () =>
                    {
                        projectileScript.ReturnPool();
                    });
                seq.Play();
                // SoundManager.Instance.PlayFX("Sword_Strength");
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
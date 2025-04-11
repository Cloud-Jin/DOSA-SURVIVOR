using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 데미지타임 간격 추가타

namespace ProjectM.Battle
{
    public class LightningStep : WeaponSkill
    {
        private int count;
        private float castingTime;
        private bool timeStandby;
        private Vector3 dir;
        private float convertAngle;
        private float scaleSize;
         public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
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
            scaleSize = data.Scale / 100f;
            convertAngle = data.TypeValue;
            // convertAngle.Clear();
            // convertAngle = MyMath.CalcAngleCount(data.TypeValue, data.ObjectValue);
        }

        public override void Fire()
        {
            count = data.Count;
            castingTime = data.CastingTime;
           

            Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
        }
        
        IEnumerator Launch()
        {
            count--;
            dir = PriorityDir();
            
            timeStandby = false;
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            player.PlayAnim(data.Ani);
            // SoundManager.Instance.PlayFX("Flash_Stab");
            
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = Quaternion.identity;
                projectile.localScale = GetScale;
                projectile.position = player.BodyParts[data.Pivot].position;
                projectile.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                projectile.transform.SetParent(player.transform);
                
                var projectileScript = projectile.GetComponent<Projectile>();
                projectileScript.InitBuilder()
                    .SetPool(pool)
                    .SetSound("Lightning")
                    // .SetDamage(0)
                    // .SetPer(-100)
                    // .SetTick(data.DamegeTime / 1000f)
                    .SetDuration(data.DurationTime / 1000f)//, SetTimeAction)
                    .SetKnockBack(data.KnockBack / 10f)
                    .SetUnit(player)
                    .SetAccDamageFunc(AccDamage)
                    .Build();
            }
            
            Sequence seq = DOTween.Sequence();
            seq.InsertCallback(0.1f, () => AttackArc(2.5f * scaleSize, convertAngle, dir, LayerMask.GetMask("Enemy"), data.KnockBack / 10f));
            seq.InsertCallback(data.DamegeTime /1000f, () => AttackArc(2.5f * scaleSize, convertAngle, dir, LayerMask.GetMask("Enemy"), data.KnockBack / 10f));

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
            // timer = 0;
        }
    }
}
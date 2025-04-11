using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using System;
using Unit = UniRx.Unit;

namespace ProjectM.Battle
{
    public class RockGuard : ActiveSkill
    {
        private List<Projectile> projectiles = new List<Projectile>();
        public CompositeDisposable disposables = new CompositeDisposable();

        private float baseDist = 1.5f;
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);

            this.UpdateAsObservable().Subscribe(Rotate).AddTo(this);
        }

        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            Fire();
        }

        public override void Dispose()
        {
            base.Dispose();
            disposables.Clear();
        }

        public override void Fire()
        {
            base.Fire();
            
            disposables.Clear();
            projectiles.Clear();
            
            for (int index = 0; index < data.ObjectValue; index++)
            {
                Transform bullet;

                if (index < transform.childCount) 
                {
                    bullet = transform.GetChild(index);
                }
                else
                {
                    bullet = ResourcesManager.Instance.Instantiate(data.ObjectResource).transform;
                    bullet.parent = transform;
                }
                
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;

                Vector3 rotVec = Vector3.forward * 360 * index / data.ObjectValue;
                bullet.Rotate(rotVec);
                bullet.Translate(bullet.up * RockDist, Space.World);
                projectiles.Add(bullet.GetComponent<Projectile>());
            }

            SoundManager.Instance.PlayFX("WaterBall");
            ApplyData();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, commonDmg);
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed /10f, skillSystem.incValue.BulletSpeed);
            ApplyData();
        }

        void ApplyData()
        {
            foreach (var bullet in projectiles)
            {
                bullet.SetActive(true);
                bullet.InitBuilder()
                    .SetDamage(damage)
                    .SetPer(per)
                    .SetVelocity(Vector3.zero)
                    .SetSpeed(speed)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetKnockBack(data.KnockBack / 10f)
                    .SetBlockType(data.ProjectileBlockType, data.ProjectileBlockedType)
                    .SetUnit(player)
                    .SetAccDamageFunc(AccDamage)
                    .Build();

                bullet.transform.localScale = GetScale;
            }


            Observable.Timer(TimeSpan.FromSeconds(coolTime + data.DurationTime / 1000f)).Where(_ => isRun).Subscribe(
                _ =>
                {
                    Fire();
                }).AddTo(disposables);
        }

        void Rotate(Unit i)
        {
            transform.Rotate(Vector3.back * speed * Time.deltaTime);
        }

        float RockDist
        {
            get
            {
                return baseDist;//MyMath.Increase(baseDist, SkillSystem.Instance.incValue.Scale);                
            }
        }
       
    }

}
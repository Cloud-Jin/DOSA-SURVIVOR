using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class FlameCharm : ActiveSkill
    {
        private float timer;
        private int count;
        private float castingTime;
        // List<float> convertAngle = new List<float>();
        private bool timeStandby;
        private UIFollow uiFollow;
        private ReactiveProperty<float> TimerBar;
        private Vector3 dir;
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 5);
            TimerBar = new ReactiveProperty<float>(0);
            timeStandby = true;
            
            
        }

        private void Start()
        {
            uiFollow = PlayerManager.Instance.player.UIFollow;
            // TimerBar.Subscribe(v => uiFollow.SetTimeBar(v)).AddTo(this);
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

        // 레벨만큼 퍼지는 스킬
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
            dir = PriorityDir();
            
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            player.PlayAnim(data.Ani);
            SoundManager.Instance.PlayFX("Charm_Roll");
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
                    .SetDamage(0)
                    .SetExplosionDamage(damage)
                    .SetPer(per)
                    .SetVelocity(dir)
                    .SetSpeed(speed)
                    .SetDuration(data.DurationTime / 1000f)
                    .SetKnockBack(data.KnockBack/10f)
                    .SetUnit(player)
                    .SetAccDamageFunc(AccDamage)
                    // .SetAutoReturn(false)
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
        
        void Running(Unit i)
        {
            timer += Time.deltaTime;
            // 기본공격 타임바
            TimerBar.Value = (timer / coolTime);
            
            if (timer >= coolTime)
            {
                Fire();
                timer = 0;
            }
        }
    }
}
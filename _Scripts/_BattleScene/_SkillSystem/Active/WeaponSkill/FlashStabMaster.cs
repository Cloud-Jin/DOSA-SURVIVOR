using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class FlashStabMaster : WeaponSkill
    {
        // private int count;
        
        private float castingTime;
        private bool timeStandby;
        private Vector3 dir;
        List<float> convertAngle = new List<float>();

        private DamageOverTime _weapon;
        private float _animationLoopTime;
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
            TimerBar = new ReactiveProperty<float>(0);
            
            timeStandby = true;
            _animationLoopTime = data.DamegeTime / 1000f;
        }

        protected override void Start()
        {
            base.Start();
            PlayerManager.Instance.player.UIFollow.TimeBarHide();
            Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
            this.UpdateAsObservable().Where(_ => isRun && !timeStandby).Subscribe(Attack).AddTo(this);
        }


        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, 0);
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed /10f, skillSystem.incValue.BulletSpeed);
            
            convertAngle.Clear();
            convertAngle = MyMath.CalcAngleCount(data.TypeValue, data.ObjectValue);
        }

        public override void Fire()
        {
            // count = data.Count;
            // castingTime = data.CastingTime;
            //
            // Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
        }
        
        IEnumerator Launch()
        {
            // dir = LastDir();
            dir = PriorityDir();
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            player.PlayAnim(data.Ani);
            
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform projectile = pool.Rent().transform;
                projectile.localPosition = Vector3.zero;
                projectile.localRotation = Quaternion.identity;
                projectile.localScale = GetScale;
                projectile.position = player.BodyParts[data.Pivot].position;
                projectile.rotation = Quaternion.FromToRotation(Vector3.up, Quaternion.AngleAxis(convertAngle[i], Vector3.forward) * dir);
                projectile.transform.SetParent(player.transform);
                
                _weapon = projectile.GetComponent<DamageOverTime>();
                _weapon.InitBuilder()
                    .SetPool(pool)
                    .SetDamage(damage)
                    .SetTick(data.DamegeTime / 1000f)
                    .SetAccDamageFunc(AccDamage)
                    .SetKnockBack(data.KnockBack / 10f)
                    .SetUnit(player)
                    .Build();
            }
            
            timeStandby = false;
            // if (count > 0)
            // {
            //     castingTime = data.CountTime / 1000f;
            //     Observable.FromCoroutine(Launch).Subscribe().AddTo(this);
            // }
            // else
            // {
            //     timeStandby = true;
            // }
        }

        void Attack(Unit i)
        {
            // dir = LastDir();
            dir = PriorityDir();
            _weapon.transform.rotation = Quaternion.FromToRotation(Vector3.up, Quaternion.AngleAxis(convertAngle[0], Vector3.forward) * dir);
            
            timer += Time.deltaTime;
            if (timer >= _animationLoopTime)
            {
                player.PlayAnim(data.Ani);
                SoundManager.Instance.PlayFX("Flash_Stab_M");
                
                timer = 0;
            }
        }

        void SetTimeAction()
        {
            timeStandby = true;
            timer = 0;
        }
    }
}
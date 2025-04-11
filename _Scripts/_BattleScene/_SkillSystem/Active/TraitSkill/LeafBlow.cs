using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using System;
using DG.Tweening;
using Sirenix.Utilities;
using Unit = UniRx.Unit;

namespace ProjectM.Battle
{
    public class LeafBlow : ActiveSkill
    {
        private List<Projectile> projectiles = new List<Projectile>();
        CompositeDisposable disposables = new CompositeDisposable();
        
        float circleR = 1;   // 반지름
        float deg;           // 각도
        Vector3 startPos;    // 발사위치
        private bool isRuning;  // 작동중
        public override void Init(Player player, int skillGroupID)
        {
            base.Init(player, skillGroupID);
            Fire();
            this.UpdateAsObservable().Where(t=> isRuning).Subscribe(Rotate).AddTo(this);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            disposables.Clear();
        }

        private void OnDestroy()
        {
            disposables.Clear();
            DOTween.Kill(gameObject);
        }

        public override void Fire()
        {
            base.Fire();
            isRuning = false;
            disposables.Clear();
            projectiles.Clear();

            startPos = PlayerManager.Instance.playerble.transform.position;
            SoundManager.Instance.PlayFX("Leaf_Blow");
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
                
                bullet.localRotation = Quaternion.identity;

                Vector3 rotVec = Vector3.forward * 360 * index / data.ObjectValue;
                bullet.Rotate(rotVec);
                projectiles.Add(bullet.GetComponent<Projectile>());
            }
            
            deg = 0;
            circleR = data.Range / 10f;
            isRuning = true;
            SetObjPos(0);
            DOTween.To(() => circleR, value => circleR = value, data.TypeValue / 10f, data.DurationTime / 1000f)
                .SetEase(Ease.Linear).OnComplete(() =>
                {
                    isRuning = false;
                });
            
            ApplyData();
        }

        public override void DataUpdate()
        {
            base.DataUpdate();
            
            var skillSystem = SkillSystem.Instance;
            damage = MyMath.CalcDamage(player.attack, data.DamageRatio, commonDmg);
            coolTime = skillSystem.GetCoolTime(data.CoolTime / 1000f);
            speed = MyMath.Increase(data.Speed, skillSystem.incValue.BulletSpeed);
            // ApplyData();
        }
        
        
        void ApplyData()
        {
            foreach (var bullet in projectiles)
            {
                bullet.InitBuilder()
                    .SetDamage(damage)
                    .SetPer(per)
                    // .SetVelocity(Vector3.zero)
                    // .SetSpeed(speed)
                    .SetDuration(data.DurationTime / 1000f)
                    // .SetKnockBack(data.KnockBack / 10f)
                    // .SetBlockType(data.ProjectileBlockType, data.ProjectileBlockedType)
                    .SetUnit(player)
                    .SetAccDamageFunc(AccDamage)
                    .Build();

                bullet.transform.localScale = GetScale;
                bullet.SetActive(true);
            }
            
            Observable.Timer(TimeSpan.FromSeconds(coolTime + data.DurationTime / 1000f)).Subscribe(
                _ =>
                {
                    Fire();
                }).AddTo(disposables);
        }

        void Rotate(Unit i)
        {
            deg += Time.deltaTime * speed;
            if (deg < 360)
            {
                SetObjPos(deg);
            }
            else
            {
                deg = 0;
            }
        }

        void SetObjPos(float deg)
        {
            var objSize = data.ObjectValue;
            for (int j = 0; j < objSize; j++)
            {
                var rad = Mathf.Deg2Rad * (deg+(j*(360/objSize)));
                var x = circleR * Mathf.Sin(rad);
                var y = circleR * Mathf.Cos(rad);
                projectiles[j].transform.position = startPos + new Vector3(x, y);
                projectiles[j].transform.rotation = Quaternion.Euler(0, 0, (deg + (j * (360 / objSize))) * -1);
            }
        }
    }
}
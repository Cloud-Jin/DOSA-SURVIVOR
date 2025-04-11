using System;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using Sirenix.Utilities;
using UniRx;
using UnityEngine;

namespace ProjectM.Battle
{
    public class PlayerUnitBase : UnitBase
    {
        public float speed;
        protected int CriticalRatio;                        // 계산된 크리티컬확률 (만분율)
        protected float DmgReduce;                          // 데미지 감소 수치


        public int criticalRatio => CriticalRatio;
        public Rigidbody2D Rigidbody2D
        {
            get { return rigid; }
        }

        public void SetStat()
        {
            SetAttackStat();
            SetHpStat();
            SetCriticalRate();
            SetMoveStat();
            SetDmgReduceStat();
        }
        
        // 최대체력 비례 회복
        public virtual void Healing(float rate)
        {
            this.health += maxHealth * rate;
            if (health > maxHealth)
                health = maxHealth;
        }
        
        // 최대체력 %
        public virtual void SetHpRate(float rate)
        {
            this.health = maxHealth * rate;
            if (health > maxHealth)
                health = maxHealth;
        }
        
        public virtual void SetCriticalRate()
        {
            CriticalRatio = monster.CriticalRatio + SkillSystem.Instance.incValue.CriticalRate;
        }

        public virtual void SetMoveStat()
        {
            speed = MyMath.Increase(baseSpeed, SkillSystem.Instance.incValue.MoveSpeed);
        }
        
        public virtual void SetAttackStat()
        {
            // 현재 최대 스탯 X (패시브 스킬 증가 값 + 영웅 소환으로 얻는 증가 값)
            attack = MyMath.Increase(baseAttack, SkillSystem.Instance.incValue.damage);
        }

        public virtual void SetHpStat()
        {
            // 현재 최대 스탯 X (패시브 스킬 증가 값 + 영웅 소환으로 얻는 증가 값)
            // var prevMaxHealth = maxHealth;
            // var nextMaxHealth = MyMath.Increase(baseHealth, SkillSystem.Instance.incValue.Hp);     // 체력증가
            // var gap  = nextMaxHealth - prevMaxHealth;
            // health += gap;
            // maxHealth = nextMaxHealth;
        }
        
        public virtual void SetDmgReduceStat()
        {
            DmgReduce = SkillSystem.Instance.calcDmgReduce;
        }
        
        

        public InfVal CalcDmgReduce(InfVal damage)
        {
            if (DmgReduce <= 0)
                return damage;
            
            return MyMath.Decrease(damage, DmgReduce);
        }

        public void DaedAnimation()
        {
            animPlayer.Play("Dead", Dead);
            isLive = false;
        }

        public virtual void Revive()
        {
            
        }
    }
}
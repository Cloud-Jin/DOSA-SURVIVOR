using System;
using System.Collections.Generic;
using InfiniteValue;
using UnityEngine;
using UniRx;

namespace ProjectM.AutoBattle
{
    public class AttackState : IState
    {
        AutoUnit caster;
        private AutoBattleSkillAI Data => caster._battleSkillAI;
        public AttackState(AutoUnit unit)
        {
            caster = unit;
        }
        public void Enter()
        {
           caster.AnimatorPlayer.Play(Data.SkillAnimPrefab);
           
           if (string.IsNullOrEmpty(Data.SkilResourcePrefab))
           {
               //근접 공격
               caster.target.DamageHitUI(Damage, HitType.Normal, caster.target.transform.position);
               caster.target.State = AutoUnit.AutoState.Dead;
           }
           else
           {
               // 투사체
               var bullet = ResourcesManager.Instance.Instantiate(Data.SkilResourcePrefab);
               bullet.transform.position = GetPivot().position;
               
               var bulletScript = bullet.GetComponent<AutoProjectile>();
               bulletScript.Shot(caster.target, Data, Damage);
           }

           Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(t =>
           {
               OnAnimationEnd();
           }).AddTo(caster);

        }

        public void Stay()
        {
            
        }

        public void Exit()
        {
              
        }

        void OnAnimationEnd()
        {
            caster.State = AutoUnit.AutoState.Idle;
            caster.target = null;
        }
        
        public InfVal Damage
        {
            get { return MyMath.CalcCoefficient(caster.attack, Data.DamageRatio); }
        }
        
        public Transform GetPivot()
        {
            if(!string.IsNullOrEmpty(Data.Pivot))
            {
                return caster.BodyParts[Data.Pivot];
            }
            else
            {
                return caster.transform;
            }
        }
    }
}
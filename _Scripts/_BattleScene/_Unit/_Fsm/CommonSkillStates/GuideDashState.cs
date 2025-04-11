using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;


// 유도 대쉬

namespace ProjectM.Battle._Fsm
{
    public class GuideDashState : SkillState, IState
    {
        public float remainDistance;
        // private bool isWall;
        private int count;              // 스킬 시전횟수
        private float duration;
        private IDisposable disposable;
        // private LineRenderer lineRenderer;
        private bool isAction;
        private LayerMask targetLayer;
        public GuideDashState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            caster = unit;
            this.stateMachine = stateMachine;
            this.data = data;
            targetLayer = GetTargetLayer();
        }
        public void Enter()
        {
            target = targetUnit.Rigid;
            rigid = caster.Rigid;
            count = data.Count;
            duration = data.DurationTime / 1000f;
            isAction = false;
            castingTime = data.CastingTime / 1000f;
            DashEnter();
            Observable.FromCoroutine(Dash).Subscribe().AddTo(caster.disposables);
            disposable = caster.OnTriggerEnter2DAsObservable().Subscribe(OverlapUnit);
        }

        public void Tick()
        {
            // if (!isAction)
            {
                // var isFlip = targetUnit.Rigid.position.x > rigid.position.x;
                // FlipX(isFlip);
                FlipX();
            }
                
        }
        
        public void Exit()
        {
            // 연계기가 없다면
            // 글로벌 쿨타임 부여
            disposable?.Dispose();
            caster.attack = caster.baseAttack;
            DashEnd();
            stateMachine.SetCoolTime(data);
        }

        private void OverlapUnit(Collider2D col)
        {
            if (targetLayer.Contanis(col.gameObject.layer))
            {
                IDamageable _IDamageable;
                UnitBase _unitBase;
                IsUnitBase(col, out _IDamageable, out _unitBase);
                if (_IDamageable == null) return;
                if (!_unitBase.isLive) return;
                var _attack = MyMath.CalcCoefficient(caster.baseAttack, data.DamageRatio);
                _IDamageable.TakeDamage(_attack, HitType.None);
            }
        }

        IEnumerator Dash()
        {
            MovePause();
            
            count--;
            // 선딜레이
            
            while (castingTime > 0)
            {
                castingTime -= Time.deltaTime;
                yield return 0;
            }
            
            remainDistance = 0;
            float _time = 0;
            isAction = true;
            caster.attack = MyMath.CalcCoefficient(caster.baseAttack, data.DamageRatio);
            caster.PlayAnim(data.Ani);
            // 스피드
            var tempSpeed = caster.baseSpeed;
            caster.baseSpeed = data.Speed;
            while (remainDistance <= data.Range / 10f && _time < duration)
            {
                MoveTo();
                _time += Time.deltaTime;
                yield return 0;
            }

            caster.PlayAnim($"{data.Ani}_END");
            
            caster.baseSpeed = tempSpeed;
            isAction = false;

            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(Dash).Subscribe().AddTo(caster.disposables);
            }
            else if (data.NextSkillId > 0)
            {
                stateMachine.SetNextState(data.NextSkillId);
                disposable?.Dispose();
            }
            else
            {
                stateMachine.SetIdelState();
                disposable?.Dispose();
            }
        }
    }
}
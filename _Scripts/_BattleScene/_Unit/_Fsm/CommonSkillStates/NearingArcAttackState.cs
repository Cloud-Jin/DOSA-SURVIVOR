using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 근접 평타.

namespace ProjectM.Battle._Fsm
{
    public class NearingArcAttackState : SkillState, IState
    {
        private Scanner scanner;
        private float _durationtime;
        private float _damegeTime;
        private float timer;
        private bool running;
        private bool isAction;
        public NearingArcAttackState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            Init();
        }
        
        public void Enter()
        {
            rigid = caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            _durationtime = data.DurationTime / 1000f;
            _damegeTime = data.DamegeTime / 1000f; // n 초 마다 공격

            running = true;
            FindTargetNearest();
            if (IsDistance())
            {
                isStatePlay = true;
                Observable.FromCoroutine(Attack).Subscribe().AddTo(caster.disposables);
                Observable.FromCoroutine(RunTime).Subscribe().AddTo(caster.disposables);    
            }
            else
            {
                isStatePlay = false;
                stateMachine.SetIdelState();
            }
            
        }

        public void Tick()
        { 
        }

        public void Exit()
        {
            caster.Rigid.drag = 0;
            running = false;
            SetCoolTime();
        }

        IEnumerator Attack()
        {
            MovePause(); 
            var dir = MyMath.GetDirection(caster.transform.position, targetUnit.Rigid.position);
            var angle = MyMath.GetAngle(caster.transform.position, targetUnit.transform.position) - 90f;
            // 범위생성
            var indicatorPool = PoolManager.Instance.GetPool("Arc 1 Region");
            var indicator = indicatorPool.Rent().GetComponent<ArcIndicator>();
            indicator.transform.position = caster.Rigid.position;
            indicator.transform.localRotation = Quaternion.identity;
            
            indicator.InitBuilder()
                .SetPool(indicatorPool)
                .SetAngle(-angle)
                .SetArc(data.Angle)
                .SetRadius(data.Scale / 100f)
                .SetDuration(castingTime)
                .Build();

            //castingTime 부채꼴 게이지
            yield return new WaitForSeconds(castingTime);
            
            
            // durationtime 동안 damegeTime 공격
            float durationtime = 0;
            timer = 10000;
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            
            
            while (_durationtime > durationtime)
            {
                durationtime += Time.deltaTime;
                if (timer >= _damegeTime)
                {
                    //attack
                    isAction = true;
                    timer = 0;
                    caster.AnimatorPlayer.PlayMidEnd(data.Ani,() =>
                    {
                        string layer = (caster.UnitType == UnitType.Hero) ? "Enemy" : "Player";
                        AttackArc(data.Scale / 100f, data.TypeValue, dir,LayerMask.GetMask(layer));
                        var _particleBase = pool.Rent().GetComponent<ParticleBase>();
                        _particleBase.transform.position = caster.BodyParts[data.Pivot].position;// + (Vector3)(dir * data.Scale /100f);
                        _particleBase.transform.rotation = Quaternion.FromToRotation(Vector3.up,dir);
                        _particleBase.transform.localScale = Vector3.one * (data.Scale / 100f);
                        _particleBase.SetReturnTime(0.3f, null);
                    }, () =>
                    {
                           
                    });
                }
                yield return null;
            }
            //애니메이션이 끝나야 상태 변경
            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(Attack).Subscribe().AddTo(caster.disposables);
            }
            else if (data.NextSkillId > 0)
            {
                stateMachine.SetNextState(data.NextSkillId);
            }
            else
            {
                stateMachine.SetIdelState();
            }
        }

        IEnumerator RunTime()
        {
            while (running)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
}
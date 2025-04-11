using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// Effect_Skill_Warning_01 
// Boss_Macaroon_Tower_Y_Skill_02_02 충격파

namespace ProjectM.Battle._Fsm
{
    public class JumpState : SkillState, IState
    {
        private int count;              // 스킬 시전횟수
        private bool isAction;
        private float durationTime;
        private Vector3 EndPonsition;
        
        public JumpState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            caster = unit;
            this.stateMachine = stateMachine;
            this.data = data;
            PoolManager.Instance.CreatePool("Effect_Skill_Warning_01b",1);
            PoolManager.Instance.CreatePool("Boss_Macaroon_Tower_Y_Skill_02_02",1);
        }
        
        public void Enter()
        {
            target = targetUnit.Rigid;
            rigid = caster.Rigid;
            count = data.Count;
            isAction = false;
            castingTime = data.CastingTime / 1000f;
            durationTime = data.DurationTime / 1000f;
            Observable.FromCoroutine(JumpUp).Subscribe().AddTo(caster.disposables);
        }

        public void Tick()
        {
            if (!isAction)
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
            caster.attack = caster.baseAttack;
            stateMachine.SetCoolTime(data);
        }

        IEnumerator JumpUp()
        {
            MovePause();
            count--;
            
            while (castingTime > 0)
            {
                castingTime -= Time.deltaTime;
                yield return 0;
            }
            
            isAction = true;
            caster.unitState = UnitState.NoHit;
            // caster.attack = MyMath.CalcCoefficient(caster.baseAttack, data.DamageRatio);
            caster.AnimatorPlayer.Play(data.Ani, () =>
            {
                LayerMask mask = LayerMask.GetMask("PlayerProjectiles", "Player"); 
                caster.Rigid.excludeLayers = mask;
                Observable.FromCoroutine(JumpDown).Subscribe().AddTo(caster.disposables);
            });
        }

        IEnumerator JumpDown()
        {
            yield return new WaitForSeconds(data.TypeValue / 1000f);    // 점프중 기다림.
            EndPonsition = targetUnit.transform.position;

            isAction = false;
            
            var pool = PoolManager.Instance.GetPool("Effect_Skill_Warning_01b");
            var effect = pool.Rent().GetComponent<ParticleBase>();
            effect.transform.position = EndPonsition;
            effect.transform.localScale = Vector3.one * (data.Scale / 100f);
            effect.Pool = pool;
            effect.SetStartLifeTime(durationTime);
            effect.SetReturnTime(durationTime+0.1f, ()=>
            {
                caster.transform.position = EndPonsition;
                caster.Anim.transform.localPosition = Vector3.zero;
                caster.AnimatorPlayer.PlayMidEnd($"{data.Ani}b", () =>
                {
                    Observable.FromCoroutine(JumpEnd).Subscribe().AddTo(caster.disposables);    
                }, null);
            });
            
        }

        IEnumerator JumpEnd()
        {
            var pool = PoolManager.Instance.GetPool("Boss_Macaroon_Tower_Y_Skill_02_02");
            Transform bullet = pool.Rent().transform;
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;
            bullet.localScale = Vector3.one * (data.Scale / 100f);

            // 데이터에 따른 발사 위치
            bullet.position = EndPonsition;
            var projectileScript = bullet.GetComponent<Projectile>();
            projectileScript.InitBuilder()
                .SetPool(pool)
                .SetDamage(Damage)
                .SetPer(99)
                .SetDuration(0.5f)
                .SetUnit(caster)
                .Build();
            
            caster.Rigid.excludeLayers = new LayerMask();
            caster.unitState = UnitState.Normal;
            
            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(JumpUp).Subscribe().AddTo(caster.disposables);
            }
            else if (data.NextSkillId > 0)
            {
                stateMachine.SetNextState(data.NextSkillId);
            }
            else
            {
                stateMachine.SetIdelState();
            }
            
            yield break;
        }
    }
}
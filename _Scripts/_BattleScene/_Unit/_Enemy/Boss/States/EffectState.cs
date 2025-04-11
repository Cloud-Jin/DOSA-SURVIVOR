/*using System;
using System.Collections;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

namespace ProjectM.Battle._Fsm.Boss
{
    public class EffectState : SkillState, IState
    {
        public EffectState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            target = PlayerManager.Instance.PlayerRigidBody2D;
            Rigid =  caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            
            Observable.FromCoroutine(Effect).Subscribe();
        }

        public void Tick()
        {
            // unit.Spriter.flipX = target.position.x > Rigid.position.x;
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
            Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        IEnumerator Effect()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            
            yield return new WaitForSeconds(castingTime);
            
            // var dir = (target.position - Rigid.position).normalized;
            // Start, Weight, End
            caster.PlayAnim(data.Ani);
            for (int i = 0; i < data.TypeValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.SetActive(false);
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                // if (data.SubType == 23) // 23 플레이어 위치
                // {
                //     bullet.position = target.position + MyMath.GetCircleSize(data.SubTypeValue / 10f);
                // } 

                bullet.SetActive(true);
                
                // 버프 타입은 나중에 추가
                bullet.GetComponent<EffectArea>().Init(10, data.DurationTime / 1000f);  // 몇초있다 펑~
                bullet.GetComponent<IPoolObject>().Pool = pool;
                
                yield return new WaitForSeconds(data.CountTime / 1000f);
            }

            stateMachine.SetIdelState();
        }
    }
}*/
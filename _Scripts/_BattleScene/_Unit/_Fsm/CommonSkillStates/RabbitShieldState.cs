using System;
using System.Collections;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;
// 23번
namespace ProjectM.Battle._Fsm
{
    public class RabbitShieldState : SkillState, IState
    {
        public RabbitShieldState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
        }
        
        public void Enter()
        {
            target = PlayerManager.Instance.playerble.Rigid;
            rigid =  caster.Rigid;
            castingTime = data.CastingTime / 1000f;

            Observable.FromCoroutine(RabbitShield).Subscribe().AddTo(caster.disposables);
        }

        public void Tick()
        {
            // unit.Spriter.flipX = target.position.x > Rigid.position.x;
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
        }
        
        IEnumerator RabbitShield()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            
            yield return new WaitForSeconds(castingTime);
            
            // Start, mid, End
            caster.PlayAnim(data.Ani);
           
            Transform bullet = pool.Rent().transform;
            bullet.SetActive(false);
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;
            bullet.localScale = GetScale();
            bullet.position = target.position;
            bullet.SetActive(true);
            
            // 버프 타입은 나중에 추가
            bullet.GetComponent<EffectArea>().Init(10, data.DurationTime / 1000f);  // 몇초있다 펑~
            bullet.GetComponent<IPoolObject>().Pool = pool;
            
            // yield return new WaitForSeconds(data.CountTime / 1000f);
            

            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(RabbitShield).Subscribe().AddTo(caster.disposables);
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
    }
}
/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


namespace ProjectM.Battle._Fsm.Boss
{
    [Serializable]
    public class TrapState : SkillState, IState
    {
        public TrapState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
        }

        public void Enter()
        {
            Observable.FromCoroutine(Trap).Subscribe().AddTo(caster);
        }

        public void Tick()
        {
            
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        IEnumerator Trap()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            yield return new WaitForSeconds(castingTime);
            
            for (int i = 0; i < data.Count; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);
                // if (data.SubType == 51)  // 51 사용자 기준 N범위 함정
                // {
                //     bullet.position = caster.Rigid.position + MyMath.GetCircleSize(data.SubTypeValue / 10f);
                //     bullet.GetComponent<DamageOverTime>().Init(Damage, data.DamegeTime / 1000f, data.DurationTime / 1000f);
                // }

                bullet.GetComponent<IPoolObject>().Pool = pool;
                caster.PlayAnim(data.Ani);
                yield return new WaitForSeconds(data.CountTime / 1000f);
            }

            stateMachine.SetIdelState();
        }
    }
}*/
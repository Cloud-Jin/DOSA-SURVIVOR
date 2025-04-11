/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


namespace ProjectM.Battle._Fsm.Boss
{
    [Serializable]
    public class SummonState : SkillState, IState
    {
        public SummonState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
        }

        public void Enter()
        {
            Observable.FromCoroutine(Summon).Subscribe().AddTo(caster);
        }

        public void Tick()
        {
            
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        IEnumerator Summon()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            yield return new WaitForSeconds(castingTime);
            
            for (int i = 0; i < data.TypeValue; i++)
            {
                Transform unit = pool.Rent().transform;
                unit.localPosition = Vector3.zero;
                unit.localRotation = Quaternion.identity;
                unit.localScale = Vector3.one * (data.Scale / 100f);
                unit.GetComponent<IPoolObject>().Pool = pool;
                unit.position = caster.Rigid.position + MyMath.GetCircleSize(30 / 10f);
                // if (data.SubType == 62)  // 62 몬스터 ID 소환
                // {
                //     var tm = TableDataManager.Instance.data.Monster;
                //     var hero = unit.GetComponent<Hero>();
                //     hero.Init(tm.Single(t=> t.Index == data.SubTypeValue));
                //     hero.Return(data.DurationTime / 1000f);
                // }

                caster.PlayAnim(data.Ani);
                yield return new WaitForSeconds(data.CountTime / 1000f);
            }

            stateMachine.SetIdelState();
        }
    }
}*/
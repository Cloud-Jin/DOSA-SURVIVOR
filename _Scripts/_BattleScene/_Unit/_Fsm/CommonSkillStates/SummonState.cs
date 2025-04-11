using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle._Fsm
{
    public class SummonState : SkillState, IState
    {
        private string poolName;
        public SummonState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;


            var monster = TableDataManager.Instance.data.Monster.Single(t => t.Index == data.TypeValue);
            poolName = monster.Resource;
            PoolManager.Instance.CreatePool(monster.Resource, 1);
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
            // caster.Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        IEnumerator Summon()
        {
            var pool = PoolManager.Instance.GetPool(poolName);
            // caster.Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            MovePause();
            yield return new WaitForSeconds(castingTime);
            
            // caster.PlayAnim(data.Ani);
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform unit = pool.Rent().transform;
                unit.localPosition = Vector3.zero;
                unit.localRotation = Quaternion.identity;
                unit.localScale = Vector3.one * (data.Scale / 100f);
                unit.GetComponent<IPoolObject>().Pool = pool;
                unit.position = caster.Rigid.position + MyMath.GetCircleSize(30 / 10f);
           
                // 차후 영웅말고 다른개체 소환시 체크.
                var tm = TableDataManager.Instance.data.Monster.Single(p => p.Index == data.TypeValue);
                switch (tm.Type)
                {
                    case 2:
                    {
                        var heroOrigin = caster as Hero;
                        if (heroOrigin)
                        {
                            var hero = unit.GetComponent<Hero>();
                            hero.Init(tm, Damage, heroOrigin.AccDamageFunc);
                            hero.Return(data.DurationTime * 0.001f);
                        }

                        break;
                    }
                    case 5:
                    {
                        var baseRatio = InfVal.Parse(BattleManager.Instance.tbStage.MonsterEnhanceRatio);
                        var spawnData = BattleManager.Instance.spawner.SpawnList;
                        var enemyNormal = unit.GetComponent<EnemyNormal>();
                        enemyNormal.Pool = pool;
                        enemyNormal.SetType(UnitType.Enemy, EnemyType.Summon);
                        enemyNormal.Init(tm);
                        enemyNormal.SetSpawnData(baseRatio,spawnData.MonsterHPIncrease,spawnData.MonsterAttackIncrease,0);
                        enemyNormal.SetCoefficient(data.DamageRatio);
                        break;
                    }
                }
            }

            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(Summon).Subscribe().AddTo(caster.disposables);
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
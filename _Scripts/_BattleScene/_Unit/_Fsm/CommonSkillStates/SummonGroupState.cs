using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;


namespace ProjectM.Battle._Fsm
{
    public class SummonGroupState : SkillState, IState
    {
        private float duration;
        private Monster[] monsters;
        private string effectSummon = "Effect_Skill_Warning_01";
        
        public SummonGroupState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            // Group 몬스터 등록.
            monsters = TableDataManager.Instance.GetNormalMonster(data.TypeValue);
            
            for (int idx = 0; idx < monsters.Length; idx++)
            {
                PoolManager.Instance.CreatePoolSummonEnemy(monsters[idx].Resource, 1);
            }
            
            PoolManager.Instance.CreatePool(data.ObjectResource, 5);
            PoolManager.Instance.CreatePool(effectSummon, 5);
        }

        public void Enter()
        {
            duration = data.DurationTime / 1000f;
            
            Observable.FromCoroutine(Summon).Subscribe().AddTo(caster);
            
        }

        public void Tick()
        {
            
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
            
        }
        
        IEnumerator Summon()
        {
            MovePause();
            yield return new WaitForSeconds(castingTime);
            var _spawnEffectPool = PoolManager.Instance.GetPool(data.ObjectResource);
            var _effectPool = PoolManager.Instance.GetPool(effectSummon);
            caster.PlayAnim(data.Ani);
            
            for (int i = 0; i < data.ObjectValue; i++)
            {
                var _position = Vector3.zero;

                for (int j = 0; j < 100; j++)
                {
                    var pos = MyMath.GetCircleSize(data.Range / 10f);
                    var newPos = caster.Rigid.position + pos;
                    if (BattleManager.Instance.MapOverlapPoint(newPos))
                    {
                        _position = newPos;
                        break;
                    }
                }
                
                var _effect = _effectPool.Rent().GetComponent<ParticleBase>();
                _effect.Pool = _effectPool;
                _effect.transform.localScale = Vector3.one * (data.Scale / 100f);
                _effect.transform.position = _position;
                _effect.SetReturnTime(duration, () =>
                {
                    // 소환로직
                    var monster = monsters[Random.Range(0, monsters.Length)];

                    var pool = PoolManager.Instance.GetPool($"Summon_{monster.Resource}");
                    Transform unit = pool.Rent().transform;
                    unit.localPosition = Vector3.zero;
                    unit.localRotation = Quaternion.identity;
                    unit.localScale = Vector3.one * (data.Scale / 100f);
                    unit.GetComponent<IPoolObject>().Pool = pool;
                    // 체크 포지션
                    unit.position = _position;
                    var spawneffect = _spawnEffectPool.Rent().GetComponent<ParticleBase>();
                    spawneffect.transform.position = _position;
                    spawneffect.SetReturnTime(1f, null);
                    
                    switch (monster.Type)
                    {
                        case 5:
                        {
                            var baseRatio = InfVal.Parse(BattleManager.Instance.tbStage.MonsterEnhanceRatio);
                            var spawnData = BattleManager.Instance.spawner.SpawnList;
                            var enemyNormal = unit.GetComponent<EnemyNormal>();
                            enemyNormal.Pool = pool;
                            enemyNormal.SetType(UnitType.Enemy, EnemyType.Summon);
                            enemyNormal.Init(monster);
                            enemyNormal.SetSpawnData(baseRatio,spawnData.MonsterHPIncrease,spawnData.MonsterAttackIncrease,0);
                            enemyNormal.SetCoefficient(data.DamageRatio);
                            break;
                        }
                    } 
                });
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
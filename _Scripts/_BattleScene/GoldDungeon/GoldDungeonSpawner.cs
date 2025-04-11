using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gbros.UniRx;
using InfiniteValue;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using Unit = UniRx.Unit;

namespace ProjectM.Battle
{
    public class GoldDungeonSpawner : Spawner
    {
        private InfVal monsterEnhanceDungeonRatio;                 // 던전 레벨 강화 계수
        protected override void Init()
        {
            base.Init();
            
            var bm = BattleManager.Instance;
            var bb = BlackBoard.Instance.data;
            map = bm.map;
            
            bm.LoopRange.Subscribe(t => loopRangeScale = t).AddTo(this);
            
            monsterEnhanceBaseRatio = InfVal.Parse(bm.tbStage.MonsterEnhanceRatio);
            normalMonsters = bm.NormalMonster;
            normalMonsterGroups = bm.NormalMonsterGroups;
            monsterEnhanceDungeonRatio = InfVal.Parse(TableDataManager.Instance.data.DungeonLevel.Single(t => t.Level == bb.dungeonLevel).MonsterEnhanceRatio);
                
            stageSpawnLists = TableDataManager.Instance.GetstageSpawnLists(BattleManager.Instance.tbStage.SpawnGroupID);
            maxEnemyCount = TableDataManager.Instance.GetStageConfig(101).Value;
            
            PoolManager.Instance.AddEnemy();
            
            this.UpdateAsObservable().Where(_ => isOnlyRun && waveType == WaveType.Wave).Subscribe(SpawnEnemyWave).AddTo(this); // 몬스터 웨이브 스폰
            
            SetEnemyWaveSpawnEvent();           // 몬스터 웨이브 스폰 이벤트
        }
        
        #region 던전 웨이브

        Vector3 GetSpawnWavePosition(int line)
        {
            // 1열 2열
            float x, y;
            Vector3 position;
            Vector3 returnPos;
            
            switch (map)
            {
                case Map.Infinite:
                    x = (line == 1) ? 5f : 5.7f;
                    y = (line == 1) ? 7.5f : 8f;
                    position = MyMath.GetRandomPosition(map, x, y);
                    returnPos = position * loopRangeScale;
                    returnPos += _player.transform.position;
                    break;
                case Map.Vertical:
                    x = 3.2f;
                    y = (line == 1) ? 9f : 10f;
                    position = MyMath.GetRandomPosition(map, x, y);
                    returnPos = position;
                    returnPos.y *= loopRangeScale;
                    returnPos.y += _player.transform.position.y;
                    break;
                case Map.Rect:
                    x = (line == 1) ? 5f : 6f;
                    y = (line == 1) ? 4.5f : 5.5f;
                    position = MyMath.GetRandomPosition(map, x, y);
                    returnPos = position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(map), map, null);
            }
            
            return returnPos;
        }
        
        void SpawnEnemyWave(Unit j)
        {
            // 시간체크
            timer += Time.deltaTime;
            // 스폰 딜레이
            
            if (BattleManager.Instance.enemyCount >= maxEnemyCount)
            {
                return;
            }
            
            
            if (timer > spawnDelay)
            {
                timer = 0;
                // 시간당 n 마리 생성
                // 포지션은 Player로 부터 받아오자.
                var monster = normalSpawnMonsterSelection[0];
                var monsterID = normalMonsterGroups.Where(t => t.MonsterID == monster.Index).First();
                
                for (int i = 0; i < monsterID.WaveSpawnCount; i++)
                {
                    var pool = PoolManager.Instance.GetPool(monster.Resource);
                    // enemy normal.
                    var position = GetSpawnWavePosition(1);

                    var enemy = pool.Rent();
                    var enemyscript = enemy.GetOrAddComponent<EnemyNormal>();
                    enemyscript.Pool = pool;
                    enemyscript.SetType(UnitType.Enemy, EnemyType.Normal);
                    enemyscript.Init(monster);
                    enemyscript.SetDungeonSpawnData(monsterEnhanceBaseRatio, monsterEnhanceDungeonRatio);
                    enemyscript.transform.position = position;
                    enemyscript.transform.localScale = Vector3.one;
                    BattleManager.Instance.enemyCount++;
                }
                
                monster = normalSpawnMonsterSelection[1];
                monsterID = normalMonsterGroups.Where(t => t.MonsterID == monster.Index).First();
                
                for (int i = 0; i < monsterID.WaveSpawnCount; i++)
                {
                    var pool = PoolManager.Instance.GetPool(monster.Resource);
                    // enemy normal.
                    var position = GetSpawnWavePosition(2);

                    var enemy = pool.Rent();
                    var enemyscript = enemy.GetOrAddComponent<EnemyNormal>();
                    enemyscript.Pool = pool;
                    enemyscript.SetType(UnitType.Enemy, EnemyType.Normal);
                    enemyscript.Init(monster);
                    enemyscript.SetDungeonSpawnData(monsterEnhanceBaseRatio, monsterEnhanceDungeonRatio);
                    enemyscript.transform.position = position;
                    enemyscript.transform.localScale = Vector3.one;
                    BattleManager.Instance.enemyCount++;
                }
                
                spawnCount--;
                if (spawnCount == 0)
                {
                    // 웨이브 횟수 종료
                    waveType = WaveType.Normal;
                    spawnLevel.SetValueAndForceNotify(spawnLevel.Value);
                }
            }
        }
        
        void SetWaveData(int index)
        {
            var data = stageSpawnLists.Where(t => t.Index == index && t.SpawnType == 5).Single();
            spawnDelay = data.SpawnDelay / 1000f;
            spawnCount = data.SpawnCount;                           // 웨이브 스폰 횟수
            
            monsterHPIncrease = data.MonsterHPIncrease;
            monsterAttackIncrease = data.MonsterAttackIncrease;
            monsterExpMultiple = data.MonsterExpMultiple;
            // 몬스터 랜덤 2마리 배정
            var numberOfRandomSelections = 2;

            normalSpawnMonsterSelection = Enumerable.Range(0, normalMonsters.Length)
                .OrderBy(i => Random.value)
                .Select(i => normalMonsters[i])
                .Take(numberOfRandomSelections)
                .ToArray();
            
        }
        void SetEnemyWaveSpawnEvent()
        {
            var bm = BattleManager.Instance;

            // 몬스터 웨이브
            var data = stageSpawnLists.Where(t => t.SpawnType == 5).ToList();
            
            // 다음 웨아브 정보
            // nextWaveSpawnList = data[0];
            
            // 이벤트 등록
            for (int i = 0; i < data.Count; i++)
            {
                int idx = i;
                bm.kill.Where(kill => kill == data[idx].SpawnKillCount).Subscribe(x =>
                {
                    waveType = WaveType.Wave;
                    SetWaveData(data[idx].Index);
                }).AddTo(this);
            }
        }

        #endregion
    }
}
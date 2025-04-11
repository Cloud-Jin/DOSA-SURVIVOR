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
    public class StageSpawner : Spawner
    {
        // # 균열
        private InfVal crackEnhanceBaseRatio;                      // 스테이지 기본 강화 계수
        protected IDisposable crackdisposable;
        
        // # 몬스터
        private bool isKillCondition;
        
        protected override void Init()
        {
            base.Init();
            
            var bm = BattleManager.Instance;

            map = bm.map;
            
            bm.LoopRange.Subscribe(t => loopRangeScale = t).AddTo(this);

            monsterEnhanceBaseRatio = InfVal.Parse(bm.tbStage.MonsterEnhanceRatio);
            normalMonsters = bm.NormalMonster;
            normalMonsterGroups = bm.NormalMonsterGroups;
            
            stageSpawnLists = TableDataManager.Instance.GetstageSpawnLists(BattleManager.Instance.tbStage.SpawnGroupID);
            maxEnemyCount = TableDataManager.Instance.GetStageConfig(101).Value;
            
            PoolManager.Instance.AddEnemy();
            // # 균열
            crackEnhanceBaseRatio = InfVal.Parse(bm.tbStage.DimensionHPRatio);
            bm.CrackSpawn.Subscribe(CrackSpawn).AddTo(this);
            bm.CrackDespawn.Subscribe(CrackDespawn).AddTo(this);
            SetCrackSpawnEvent();               // 균열 생성 이벤트
            
            this.UpdateAsObservable().Where(_ => isOnlyRun && waveType == WaveType.Normal).Subscribe(Spawn).AddTo(this); // 몬스터 스폰
            this.UpdateAsObservable().Where(_ => isOnlyRun && waveType == WaveType.Wave).Subscribe(SpawnEnemyWave).AddTo(this); // 몬스터 웨이브 스폰
            
            SetEnemyWaveSpawnEvent();           // 몬스터 웨이브 스폰 이벤트
            SetEnemyEliteSpawnEvent();          // 엘리트 몬스터 스폰 이벤트
            
            // # 보스
            InitBoss();                         // 킬카운트 보스 스폰 이벤트

            spawnLevel.Subscribe(SetSpawnData).AddTo(this);
        }
        
        #region 차원균열

        void SetCrackSpawnEvent()
        {
            // 차원균열 타이머
            // 보스등장시 타이머 멈춤
            // 일정시간(고정) 에 따라 스폰함
            // 일정횟수(3번) 까지 스폰.

            if (BattleManager.Instance.crackOrder > 3)
            {
                Debug.Log("최대횟수 등장");
                return;
            }

            var config = TableDataManager.Instance;
            float minTime = config.GetStageConfig(22).Value;
            float maxTime = config.GetStageConfig(23).Value;
            var ranTime = Random.Range(minTime, maxTime);
            if (TutorialManager.Instance.IsTutorialBattle) ranTime = 30f;
            
            crackdisposable = PowerObservable.Countdown(BattleManager.Instance.timerPause, ranTime)
                .Select(t=> (int)t.TotalSeconds)
                .Subscribe((f) => {}, ()=> BattleManager.Instance.CrackSpawn.OnNext(1)).AddTo(this);

            Observable.Amb(BattleManager.Instance.CrackSpawn).Subscribe(CrackSpawnTimeComplete).AddTo(this);

            // Observable.Amb(  bm.bossSpawnTime,  bm.bossSpawnKill, bm.BossSpawn).Subscribe(BossSpawnEventComplete).AddTo(this);
        }

        void CrackSpawnTimeComplete(uint i) 
        {
            // BattleManager.Instance.CrackStart();
            // sendBM Event
        }

        void CrackSpawn(uint i)
        {
            crackdisposable.Dispose();
            if (BattleManager.Instance.crackOrder > 3)
                return;

            var data = TableDataManager.Instance.data.Monster.Single(t => t.Index == 30001);
            var spwandata = stageSpawnLists.Single(t => t.SpawnType == 4 && t.Order == BattleManager.Instance.crackOrder);
            
            var pool = PoolManager.Instance.GetPool("Crack");
            var crackTransform = pool.Rent().transform;
            var crackScript = crackTransform.GetComponent<Crack>();
            crackScript.Pool = pool;
            crackScript.Init(data);
            crackScript.SetSpawnData(crackEnhanceBaseRatio, spwandata.MonsterHPIncrease);
            BattleManager.Instance.Crack = crackScript;
            
            
            if (TutorialManager.Instance.IsTutorialBattle)
            {
                if(UserDataManager.Instance.clientInfo.ClearGroupID.Count(t=> t == 11) == 0)
                    crackScript.unitState = UnitState.NoHit;
                
                for (int j = 0; j < 100; j++)
                {
                    var pos = _player.transform.position + MyMath.RandomCirclePoint(3);
                    if (BattleManager.Instance.MapOverlapPoint(pos))
                    {
                        crackTransform.position = pos;
                        break;
                    }
                }    
            }
            else
            {
                crackTransform.position = MyMath.GetCrackSpawnPosition(map, PlayerManager.Instance.player.transform.position);    
            }
            
            
            
            BattleManager.Instance.crackOrder++;
        }

        // sendBM Event
        void CrackDespawn(bool isKill)
        {
            if (isKill)
            {
                // 보상
            }
            
            // 종료처리
            // 타이머 재설정
            SetCrackSpawnEvent();
        }

        #endregion

        #region 일반몬스터

        Vector3 GetSpawnPosition()
        {
            float x, y;
            Vector3 position;
            Vector3 returnPos;
            
            switch (map)
            {
                case Map.Infinite:
                    x = 5.7f;
                    y = 8f;
                    position = MyMath.GetRandomPosition(map, x, y);
                    returnPos = position * loopRangeScale;
                    returnPos += _player.transform.position;
                    break;
                case Map.Vertical:
                    x = 3.2f;
                    y = 9f;
                    position = MyMath.GetRandomPosition(map, x, y);
                    returnPos = position;
                    returnPos.y *= loopRangeScale;
                    returnPos.y += _player.transform.position.y ;
                    break;
                case Map.Rect:
                    x = 6f;
                    y = 5.5f;
                    position = MyMath.GetRandomPosition(map, x, y);
                    returnPos = position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(map), map, null);
            }
            
            return returnPos;
        }
        
        void Spawn(Unit i)
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
                int remainCount = SpawnLevel();
                EnemySpawn(remainCount);
            }
        }


        int SpawnLevel()
        {
            if (nextSpawnList.Index == 0) 
                return 10000;
            // 시간 & 킬에 따라 되는 시간, 종류, 수량 달라짐.
            var killScore = BattleManager.Instance.kill.Value;
            // var GameTime = BattleManager.Instance.gameTime.Value;
            
            isKillCondition = (killScore >= nextSpawnList.SpawnKillCount && nextSpawnList.SpawnKillCount > 0) ? true : false;
            // isTimeCondition = GameTime > nextSpawnList.SpawnTime;
            // 킬, 시간 조건에 따라 다음 스폰리스트 변경
            if (isKillCondition)//|| isTimeCondition)
            {
                // 다음 스폰리스트로 변경   
                spawnLevel.Value++;
                
                Debug.Log($"스폰레벨 :{spawnLevel.Value}");
                BlackBoard.Instance.SaveBattleData();
                return nextSpawnList.SpawnKillCount;
            }

            return 1000;
        }
        
        void SetSpawnData(int level)
        {
            var data = stageSpawnLists.Where(t => t.Order == level && t.SpawnType == 1).Single();
            currentSpawnList = data;
            spawnDelay = data.SpawnDelay / 1000f;
            spawnCount = data.SpawnCount;
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


            nextSpawnList = stageSpawnLists.Where(t => t.Order == level + 1 && t.SpawnType == 1).SingleOrDefault();
        }
        void EnemySpawn(int remainCount)
        {
            // 시간당 n 마리 생성
            // 포지션은 Player로 부터 받아오자.
            int _spawnCount = Mathf.Min(remainCount, spawnCount);
            
            for (int i = 0; i < _spawnCount; i++)
            {
                var monster = normalSpawnMonsterSelection[Random.Range(0, 2)];
                var pool = PoolManager.Instance.GetPool(monster.Resource);
                // enemy normal.
                var position = GetSpawnPosition();

                var enemy = pool.Rent();
                var enemyscript = enemy.GetOrAddComponent<EnemyNormal>();
                enemyscript.Pool = pool;
                enemyscript.SetType(UnitType.Enemy, EnemyType.Normal);
                enemyscript.Init(monster);
                enemyscript.SetSpawnData(monsterEnhanceBaseRatio, monsterHPIncrease, monsterAttackIncrease, monsterExpMultiple);
                enemyscript.transform.position = position;
                enemyscript.transform.localScale = Vector3.one;
                BattleManager.Instance.enemyCount++;
            }
        }

            #endregion
        
        #region 몬스터 웨이브

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
                    enemyscript.SetSpawnData(monsterEnhanceBaseRatio, monsterHPIncrease, monsterAttackIncrease, monsterExpMultiple);
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
                    enemyscript.SetSpawnData(monsterEnhanceBaseRatio, monsterHPIncrease, monsterAttackIncrease, monsterExpMultiple);
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
            var data = stageSpawnLists.Where(t => t.Index == index && t.SpawnType == 2).Single();
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
            var data = stageSpawnLists.Where(t => t.SpawnType == 2).ToList();
            
            // 다음 웨아브 정보
            // nextWaveSpawnList = data[0];
            
            // 이벤트 등록
            for (int i = 0; i < data.Count; i++)
            {
                int idx = i;
                bm.kill.Where(kill => kill == data[idx].SpawnKillCount).Subscribe(x =>
                {
                    BattleManager.Instance.SetCameraZoom(idx +1);
                    BlackBoard.Instance.SaveBattleData();
                    // waveType = WaveType.NoSpawn;
                    timer = -10000;
                    var popup = UIManager.Instance.Get(PopupName.Battle_AlarmEnemyWave) as AlarmEnemyWave;
                    popup.Show();
                    popup.HideCallback(() =>
                    {
                        waveType = WaveType.Wave;
                        SetWaveData(data[idx].Index);
                        // nextWaveSpawnList = data.Where(t => t.Order == data[idx].Order + 1).SingleOrDefault();
                        timer = 10000f;
                    });
                }).AddTo(this);
            }
        }

        #endregion

        #region 엘리트 몬스터

        void SetEnemyEliteSpawnEvent()
        {
            var bm = BattleManager.Instance;

            var data = stageSpawnLists.Where(t => t.SpawnType == 3).ToList();
            
            for (int i = 0; i < data.Count; i++)
            {
                int idx = i;
                bm.kill.Where(kill => kill == data[idx].SpawnKillCount).Subscribe(x =>
                {
                    var spawnData = data[idx];
                    int monsterHPIncrease = spawnData.MonsterHPIncrease;
                    int monsterAttackIncrease = spawnData.MonsterAttackIncrease;
                    int monsterExpMultiple = spawnData.MonsterExpMultiple;
                    
                    var monster = normalMonsters[Random.Range(0, normalMonsters.Length)];
                    var pool = PoolManager.Instance.GetPool($"{monster.Resource}");
                    // enemy Elite.
                    var position = GetSpawnPosition();
                
                    var enemy = pool.Rent();
                    var enemyscript = enemy.GetOrAddComponent<EnemyNormal>();
                    enemyscript.Pool = pool;
                    enemyscript.SetType(UnitType.EnemyElite, EnemyType.Elite);
                    enemyscript.Init(monster);
                    enemyscript.SetSpawnData(monsterEnhanceBaseRatio, monsterHPIncrease, monsterAttackIncrease, monsterExpMultiple);
                    enemyscript.transform.position = position;
                    enemyscript.transform.localScale = Vector3.one * monster.EliteScale / 100f;
                    enemyscript.deadAction -= enemyscript.DropItem;
                    enemyscript.deadAction += enemyscript.DropItem;
                }).AddTo(this);
            }
            Debug.Log("엘리트 몬스터 셋");
        }

        #endregion
        
        #region 보스
        public void InitBoss()
        {
            var bm = BattleManager.Instance;
            bm.BossSpawn.Subscribe(BossSpawn).AddTo(this);
            bm.BossDespawn.Subscribe(BossDespawn).AddTo(this);
            Observable.Amb(  bm.bossSpawnTime,  bm.bossSpawnKill, bm.BossSpawn).Subscribe(BossSpawnEventComplete).AddTo(this);
            SetBossSpawnEvent();
        }
        
        void SetBossSpawnEvent()
        {
            // 보스 타이머
            // 보스는 정해진 시간, 킬 카운트에 의해 스폰 이벤트 발생함
            var bm = BattleManager.Instance;
            if (!bm.IsBossRemain())
                return;
            
            // bm.gameTime.Where(time => time == tbCurrentBoss.SpawnTime).Subscribe(t =>
            // {
            //     bm.bossSpawnTime.OnNext(1);
            // }).AddTo(this);
            bm.kill.Where(kill => kill ==  bm.GetBoss[0].SpawnKillCount).Subscribe(x =>
            {
                bm.bossSpawnKill.OnNext(1);
            }).AddTo(this);
            
        }
        
        void BossSpawnEventComplete(int i)
        {
            // sendBM Event
            // Debug.Log("Boss Spawn Event!!");
            BattleManager.Instance.BossStart();
        }
        
        void BossSpawn(int j)
        {
            // Debug.Log("리얼 보스 등장");
            // var enemyBossTb = bossMonsterGroup[BattleManager.Instance.bossOrder];
            var bm = BattleManager.Instance;
            
            Vector3 position = MyMath.GetBossSpawnPosition(PlayerManager.Instance.PlayerRigidBody2D.transform, map);
            // 보스등장마크, 바리게이트 설치
            var mark = ResourcesManager.Instance.Instantiate("Effect_BossStart_00");
            mark.transform.position = position;
            BattleManager.Instance.CreateBarricade(position);

            var seq = DOTween.Sequence();
            seq.AppendInterval(2);
            seq.AppendCallback(() =>
            {
                Destroy(mark);
                // 보스 등장, 보스마크 삭제.
                // PlayerManager.Instance.GetHeroList().ForEach(hero =>
                // {
                //     hero.Teleport();
                // });
                
                
                for (int i = 0; i < bm.GetBoss.Count; i++)
                {
                    int idx = i;
                    var enemyBossTb = TableDataManager.Instance.data.Monster.Single(t => t.Index == bm.GetBoss[idx].MonsterID);
                    var enemyBossResources = ResourcesManager.Instance.GetResources<GameObject>(enemyBossTb.Resource);
                    var enemyBoss = Instantiate(enemyBossResources);
                    enemyBoss.transform.position = position;
                    enemyBoss.GetOrAddComponent<EnemyBoss>().Init(enemyBossTb);
                    enemyBoss.GetOrAddComponent<EnemyBoss>().SetSpawnData(bm.GetBoss[idx], monsterEnhanceBaseRatio, idx);
                    
                }
                
               
            });

        }
        
        void BossDespawn(uint i)
        {
            SetBossSpawnEvent();
        }

        #endregion
    }
}
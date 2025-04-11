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

// 유닛 스폰
// 이벤트에 따라 스폰만 담당함
// 시간, 스폰 딜레이, 등장수, 웨이브여부.
// 2종류씩 나옴.
namespace ProjectM.Battle
{
    public class Spawner : MonoBehaviour
    {
        public WaveType waveType;
        protected Map map;
        protected Player _player;
        // # 몬스터 
        protected int maxEnemyCount;                            // 최대스폰 몬스터
        protected float timer;                                  // 스폰 간격 타임
        protected float spawnDelay = 1f;                        // 몬스터 스폰 딜레이타임
        protected int spawnCount = 3;                           // 몬스터 스폰 숫자
        
        protected int monsterHPIncrease;                        // 시간별 몬스터 체력강화 비율
        protected int monsterAttackIncrease;                    // 시간별 몬스터 공격력강화 비율
        protected InfVal monsterEnhanceBaseRatio;               // 스테이지 기본 강화 계수
        protected int monsterExpMultiple;                       // 시간별 몬스터 경험치 가중치
        protected Monster[] normalMonsters;                     // 스테이지에 등장하는 일반 몬스터
        protected Monster[] normalSpawnMonsterSelection;        // 스폰마다 나오는 랜덤 몬스터 2종
        protected NormalMonsterGroup[] normalMonsterGroups;     // 웨이브에 등장하는 일반 몬스터
        
        protected StageSpawnList currentSpawnList;              // 현재 웨이브 정보
        protected StageSpawnList[] stageSpawnLists;             // 웨이브 데이터
        protected IntReactiveProperty spawnLevel;               // 현재 웨이브 레벨 ( order )
        protected StageSpawnList nextSpawnList;                 // 다음 스폰 웨이브 정보
        
        
        
        protected bool isOnlyRun;   // Run state
        protected bool isRun;       // Run || Boss state
        protected float loopRangeScale;

        public StageSpawnList SpawnList => currentSpawnList;
        private void Awake()
        {
            // Data Set?
            spawnLevel = new IntReactiveProperty(1);
            var bm = BattleManager.Instance;
            bm.InitReady.Where(t=> t).Subscribe(t => Init()).AddTo(this);
            
        }

        protected virtual void Init()
        {
            _player = PlayerManager.Instance.player;
            var bb = BlackBoard.Instance.data;
            waveType = bb.WaveType;
            
            BattleManager.Instance.BattleState.Subscribe(t =>
            {
                isOnlyRun = t == BattleState.Run;
                isRun = t is BattleState.Run or BattleState.Boss;
            }).AddTo(this);
        }

        public List<StageSpawnList> GetSpawnList(int type)
        {
            var list = stageSpawnLists.Where(t => t.SpawnType == type).ToList();
            return list;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM.Battle;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

namespace ProjectM.AutoBattle
{
    public class AutoBattleManager : Singleton<AutoBattleManager>
    {
        [Header("# Game Object")]
        public Transform mapTransform;
        public BoxCollider2D mapCollider;
        
        Vector2[] returnPos = new[]
        {
            new Vector2(0, -1f), 
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(-1, -2),
            new Vector2(1, -2),
        };
        // 챕터 맵
        // Slot 관리 (1~5)
        //  ㄴ 랜덤 아티펙트
        //  ㄴ 슬롯배치
        //  ㄴ 리소스 관리 
        
        // 챕터 변경
        private ReactiveProperty<int> Chapter = new ReactiveProperty<int>();
        public List<AutoUnit> player;
        
        public ReactiveProperty<int> EnemyCount;
        public Subject<int> OnPlayerUpdate;
        protected override void Init()
        {
            Debug.Log("Auto Battle Manager Init");
            EnemyCount = new ReactiveProperty<int>();
            OnPlayerUpdate = new Subject<int>();
            
            EnemyCount.Where(c=> c == 0).Subscribe(t =>
            {
                Observable.Timer(TimeSpan.FromSeconds(2f), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                {
                    SpawnEnemy();
                }).AddTo(this);
            }).AddTo(this);
            
            // Observable.Interval(TimeSpan.FromSeconds(5f)).Subscribe(t =>
            // {
            //     // 현재 스테이지
            //     // 노말 몬스터 그룹
            //     // 랜덤.
            //     SpawnEnemy();
            // }).AddTo(this);
        }

        void Start()
        {
            
            // player[0].Init(TableDataManager.Instance.data.Monster.Single(t => t.Index == 1));
            List<int> randList = new List<int> { 101, 102, 103, 104, 105, 106, 107 };
            for (int i = 0; i < 4; i++)
            {
                int rand = Random.Range(0, randList.Count);
                AddHero(randList[rand]);
                randList.RemoveAt(rand);
            }
            
            
            int j = 0;
            foreach (var unit in player)
            {
                unit.SetRetrunPos(returnPos[j++]);
            }
            
            // 챕터 1 맵 세팅
            UserDataManager.Instance.stageInfo.SelectStage.Subscribe(idx =>
            {
                var _ChapterID = TableDataManager.Instance.data.Stage.Single(t => t.Index == idx).ChapterID;
                Chapter.Value = _ChapterID;
            }).AddTo(this);

            Chapter.Subscribe(chID =>
            {
                var tbChapter = TableDataManager.Instance.data.ChapterType.Single(t => t.Index == chID);
                for (int i = mapTransform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(mapTransform.GetChild(i).gameObject);
                }

                ResourcesManager.Instance.Instantiate(tbChapter.BgResource, mapTransform);
            }).AddTo(this);
            
            // 캐릭터 세팅
            OnPlayerUpdate.Subscribe(t =>
                {
                    var bb = BlackBoard.Instance;
                    bb.data.weaponInfo = UserDataManager.Instance.gearInfo.GetEquipWeaponInfo();
                    bb.data.costumeInfo = UserDataManager.Instance.gearInfo.GetEquipCostumeInfo();
                    ((AutoUnitPlayer)player[0]).SetPlayer();
                    
                    player[0].Init(TableDataManager.Instance.data.Monster.Single(t => t.Index == 1));
                    DataUpdate();
                }
            ).AddTo(this);
            OnPlayerUpdate.OnNext(1);
        }


        public void AddHero(int index)
        {
            var heroDB = TableDataManager.Instance.data.Monster.First(t => t.Index == index);
            
            var hero = ResourcesManager.Instance.Instantiate(heroDB.Resource, mapCollider.transform);
            var heroScript = hero.AddComponent<AutoUnitHero>();
            heroScript.Init(heroDB);
            
            var heroSkill = TableDataManager.Instance.data.AutoBattleSkillAI.First(t => t.TypeID == index);
            heroScript.SetSkillData(heroSkill);
            // 능력치 ?
            player.Add(heroScript);
        }

        public void DataUpdate()
        {
            player.ForEach(unit =>
            {
                var tbData = TableDataManager.Instance.data.Monster.First(t => t.Index == unit.unitId);
                unit.Init(tbData);
            });
        }

        [Button]
        public void SpawnEnemy()
        {
            for (int i = 0; i < player.Count; i++)
            {
                var stageIndex = UserDataManager.Instance.stageInfo.PlayStage;
                var tbStage = TableDataManager.Instance.GetStageData(stageIndex);
                var normalMonster = TableDataManager.Instance.GetNormalMonster(tbStage.NormalMonsterGroupID);
                string monId = normalMonster[Random.Range(0, normalMonster.Length)].Resource;
            
                var enemy = ResourcesManager.Instance.Instantiate(monId, mapCollider.transform);
                enemy.transform.position = Return_RandomPosition(mapCollider);
                enemy.transform.localScale = Vector3.one;
                var enemyScript = enemy.GetOrAddComponent<AutoUnit>();
                var targetUnit = NearestTarget(enemyScript);
                if (targetUnit == null || enemyScript == null)
                {
                    enemy.SetActive(false);
                    continue;
                }
                
                enemyScript.target = targetUnit;
                targetUnit.target = enemyScript;
                enemyScript.DeadAction = () => EnemyCount.Value--;

                EnemyCount.Value++;
            }
            
        }
        
        Vector3 Return_RandomPosition(BoxCollider2D rangeCollider)
        {
            // Vector3 originPosition = rangeObject.transform.position;
            Vector3 originPosition = rangeCollider.transform.position;
            float f = Random.value > 0.5f ? -1f : 1f;
            // 콜라이더의 사이즈를 가져오는 bound.size 사용
            float range_X = rangeCollider.bounds.size.x;
            float range_Y = rangeCollider.bounds.size.y;
            
            range_X = (range_X / 2) * f;
            range_Y = Random.Range( (range_Y / 2) * -1, range_Y / 2);
            Vector3 RandomPostion = new Vector3(range_X, range_Y, 0f);

            Vector3 respawnPosition = originPosition + RandomPostion;
            return respawnPosition;
        }

        public AutoUnit NearestTarget(AutoUnit unit)
        {
            AutoUnit nearestTarget = null;
            float minDist = Mathf.Infinity;

            foreach (var autoUnit in player)
            {
                AutoUnit temp = autoUnit;
                if (temp.isLive && !temp.target) // 같은편 체크?
                {
                    float dist = Vector3.Distance(unit.transform.position, temp.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestTarget = temp;
                    }
                }
            }

            return nearestTarget;
        }
        
    }
}
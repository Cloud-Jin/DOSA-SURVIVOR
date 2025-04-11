using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;

// battle Scene pool manager

namespace ProjectM.Battle
{
    public class PoolManager : Singleton<PoolManager>
    {
        [System.Serializable]
        private class ObjectInfo
        {
            // 오브젝트 이름
            public string objectName;

            // 오브젝트 풀에서 관리할 오브젝트
            public ObjectBase perfab;

            // 몇개를 미리 생성 해놓을건지
            public int count = 10;
        }

        
        [SerializeField] private ObjectInfo[] objectInfos;      // 테이블 연동전 임시
        public Transform attackIndicator;
        private Dictionary<string, ObjectPooling<ObjectBase>> objectPoolDic = new Dictionary<string, ObjectPooling<ObjectBase>>();
        protected override void Init()
        {
           
        }

        private void OnDestroy()
        {
            Debug.Log("ClearPool!");
        }

        private void Start()
        {  
            // AddEnemy();
            // AddEnemyBoss();
            InitPool();
            // preload
            CreatePool("Arc 1 Region", 1, attackIndicator);
            CreatePool("Effect_Skill_Revive_00", 1);
        }

        // battlemanaer? scene? invoke
        public void InitPool()
        {
            for (int idx = 0; idx < objectInfos.Length; idx++)
            {
                var prefab = new ObjectPooling<ObjectBase>(transform, objectInfos[idx].perfab);
                prefab.PreloadAsync(objectInfos[idx].count, 1).Subscribe();  // count만큼 미리 생성
                objectPoolDic.Add(objectInfos[idx].objectName, prefab);             // pool 정보 등록
            }
        }

        public void CreatePool(string Resource, int preloadCount)
        {
            if(objectPoolDic.ContainsKey(Resource)) return;
            
            var monsterResource = ResourcesManager.Instance.GetResources<GameObject>(Resource);
            var prefab = new ObjectPooling<ObjectBase>(transform,monsterResource.GetComponent<ObjectBase>());
            prefab.PreloadAsync(preloadCount, 1).Subscribe();    // count만큼 미리 생성
            objectPoolDic.Add(Resource, prefab);                        // pool 정보 등록
        }
        
        public void CreatePool(string Resource, int preloadCount, Transform parentTransform)
        {
            if(objectPoolDic.ContainsKey(Resource)) return;
            
            var monsterResource = ResourcesManager.Instance.GetResources<GameObject>(Resource);
            var prefab = new ObjectPooling<ObjectBase>(parentTransform,monsterResource.GetComponent<ObjectBase>());
            prefab.PreloadAsync(preloadCount, 1).Subscribe();    // count만큼 미리 생성
            objectPoolDic.Add(Resource, prefab);                        // pool 정보 등록


        }

        public void CreatePoolSummonEnemy(string resource, int preloadCount)
        {
            if (objectPoolDic.ContainsKey($"Summon_{resource}"))
                return;
                
            var monsterResource = ResourcesManager.Instance.Instantiate(resource, transform);
            monsterResource.AddComponent<EnemyNormal>();
            monsterResource.SetActive(false);
            var prefab = new ObjectPooling<ObjectBase>(transform,monsterResource.GetComponent<ObjectBase>());
            prefab.PreloadAsync(preloadCount, 1).Subscribe();     // count만큼 미리 생성
                
            objectPoolDic.Add($"Summon_{resource}", prefab);            // pool 정보 등록
        }

        public void AddEnemy()
        {
            var monsters = BattleManager.Instance.NormalMonster;
            
            // 일반 몬스터 프리팹
            for (int idx = 0; idx < monsters.Length; idx++)
            {
                if(objectPoolDic.ContainsKey(monsters[idx].Resource))
                    continue;
                
                // var monsterResource = ResourcesManager.Instance.GetResources<GameObject>(monsters[idx].Resource);
                var monsterResource = ResourcesManager.Instance.Instantiate(monsters[idx].Resource, transform);
                monsterResource.name = monsters[idx].Resource;
                monsterResource.GetOrAddComponent<EnemyNormal>();
                monsterResource.GetOrAddComponent<Reposition>();
                monsterResource.SetActive(false);
                var prefab = new ObjectPooling<ObjectBase>(transform,monsterResource.GetComponent<ObjectBase>());
                prefab.PreloadAsync(10, 1).Subscribe();     // count만큼 미리 생성
                
                objectPoolDic.Add(monsters[idx].Resource, prefab);            // pool 정보 등록
                
            }
        }

        void AddEnemyBoss()
        {
            
        }

        public ObjectPooling<ObjectBase> GetPool(string name)
        {
            return objectPoolDic[name];
        }
        
        public void ReturnPool(string name)
        {
            var pool = GetPool(name);
            for (int i = pool.objects.Count - 1; i >= 0; i--)
            {
                try
                {
                    pool.objects[i].GetComponent<IPoolObject>().ReleaseObject(pool.objects[i]);
                }
                catch
                {
                    // Debug.Log(e);
                    // throw;
                    continue;
                }
                
            }
        }
        
        // pool resource Destorey
        public void ClearPool(string poolName)
        {
            var pool = GetPool(poolName);
            pool.Clear();
        }

        public void ReleaseAllEnemyNormalPool()
        {
            // 애니메이터 Active 로그.
            var monsters = BattleManager.Instance.NormalMonster;
            for (int i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                ReturnPool(monster.Resource);
            }

            var skills = objectPoolDic.Where(t => t.Key.Contains("Monster_")).ToList();
            
            for (int i = 0; i < skills.Count; i++)
            {
                ReturnPool(skills[i].Key);
            }
        }
        
        public void ReleaseAllEnemyPool()
        {
            ReleaseAllEnemyNormalPool();
        }
    }
}
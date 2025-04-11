using System.Collections;
using System.Collections.Generic;
using DTT.Utils.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UniRx;
using UniRx.Triggers;

namespace ProjectM
{
    public class ResourcesManager : Singleton<ResourcesManager>
    {
        public Dictionary<int, Sprite> GearRarityBgs = new();
        public Dictionary<int, Sprite> GoodsTypeBgs = new();
        public Dictionary<int, Sprite> GoodsTypeIcon = new();
        public Dictionary<string, Sprite> GearIcons = new();
        public List<GameObject> gearOptionEffect = new();
        public List<GameObject> gearLevelUpEffect = new();
        public GameObject summonResultContent;
        
        protected override void Init()
        {
            Debug.Log("ResourcesManager Init");

            // var a = Addressables.LoadAssetsAsync<Material>("Mtrl", material =>
            // {
            //     Debug.Log(material.name);
            //     material.SetFloat("_Alpha", 0);
            // });
            //
            // var go = a.WaitForCompletion();
        }
        
        

        public void PreLoadResources()
        {
            gearOptionEffect.Clear();
            GearRarityBgs.Clear();
            GearIcons.Clear();
            GoodsTypeBgs.Clear();
            gearLevelUpEffect.Clear();
            GoodsTypeIcon.Clear();
            
            foreach (var rarityType in TableDataManager.Instance.data.RarityType)
                GearRarityBgs.TryAdd(rarityType.Index, GetAtlas(MyAtlas.Equipment_Frame).GetSprite(rarityType.RarityColor));
            
            foreach (var equipment in TableDataManager.Instance.data.Equipment)
                GearIcons.TryAdd(equipment.EquipIcon, GetAtlas(MyAtlas.Common_Equipment).GetSprite(equipment.EquipIcon));

            foreach (var goodsType in TableDataManager.Instance.data.GoodsType)
            {
                if (goodsType.IconBackground.IsNullOrEmpty() == false)
                {
                    GoodsTypeBgs.TryAdd(goodsType.TypeID,
                        GetAtlas(MyAtlas.Common_UISlot).GetSprite(goodsType.IconBackground));
                }

                if (goodsType.TypeID == 13 || goodsType.TypeID == 14)
                    GoodsTypeIcon.TryAdd(goodsType.TypeID, GetAtlas(MyAtlas.Summon_Shop).GetSprite(goodsType.Icon));
                else
                    GoodsTypeIcon.TryAdd(goodsType.TypeID, GetAtlas(MyAtlas.Common_Goods).GetSprite(goodsType.Icon));
            }

            for (int i = 0; i < 6; ++i)
            {
                string index = string.Format("{0:00}", i);
                 
                GameObject effectObject = GetResources<GameObject>($"UI_Effect_ItemSellect_OptionSlot{index}");
                gearOptionEffect.Add(effectObject);

                effectObject = GetResources<GameObject>($"UI_Effect_PlayerGearSlot_{index}");
                gearLevelUpEffect.Add(effectObject);
            }

            summonResultContent = GetResources<GameObject>($"SummonResultContent");
        }

        public GameObject GetResources(string Path)
        {
            return Resources.Load<GameObject>(Path);
        }

        public T GetResources<T>(string Path) where T : Object
        {
            var op = Addressables.LoadAssetAsync<T>(Path);
            T go = op.WaitForCompletion();

            // Addressables.Release(op);
            return go;
            //Do work...
        }

        public GameObject Instantiate(string path, Transform parent = null)
        {
            var prefab = Addressables.InstantiateAsync(path, parent).WaitForCompletion();
            return prefab;
        }

        public SpriteAtlas GetAtlas(string Path)
        {
            return Addressables.LoadAssetAsync<SpriteAtlas>(Path).WaitForCompletion();
        }

        // 어드레서블 동기 로드
        public void A()
        {
            var op = Addressables.LoadAssetAsync<GameObject>("myGameObjectKey");
            GameObject go = op.WaitForCompletion();

            //Do work...
            
            Addressables.Release(op);
            
            
            // 바로 Instantiate할때도 사용가능
            // prefab = Addressables.InstantiateAsync(path, parent).WaitForCompletion();
        }
        
        public IEnumerator InitAddressables()
        {
            var Handle = Addressables.InitializeAsync();
            yield return Handle;

            Debug.Log("Addressables 초기화");
            yield return UpdateCatalogCoro();
            
            Debug.Log("카탈로그 업데이트 체크 완료");
            initComplete = true;
        }
        
        IEnumerator UpdateCatalogCoro()
        {
            List<string> catalogsToUpdate = new List<string>();
            var checkCatalogHandle = Addressables.CheckForCatalogUpdates(false);
            yield return checkCatalogHandle;
            Debug.Log("카탈로그 업데이트 체크");
            if (checkCatalogHandle.Status == AsyncOperationStatus.Succeeded)
                catalogsToUpdate = checkCatalogHandle.Result;

            if (catalogsToUpdate.Count > 0)
            {
                Debug.Log($"카탈로그 업데이트 카운트 {catalogsToUpdate.Count}");
                var updateCatalogHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
                yield return updateCatalogHandle;
                Addressables.Release(updateCatalogHandle); 
            }

            Addressables.Release(checkCatalogHandle);
        }
    }
}
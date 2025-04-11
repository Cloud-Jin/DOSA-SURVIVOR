using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;


namespace ProjectM.Battle
{
    public class BattleItemManager : Singleton<BattleItemManager>
    {
        private List<ItemBase> Potion;
        private List<ItemBase> ExpGem;
        private List<ItemBase> Bomb;
        private List<ItemBase> Magnet;

        protected override void Init()
        {
            Potion = new List<ItemBase>();
            ExpGem = new List<ItemBase>();
            Bomb = new List<ItemBase>();
            Magnet = new List<ItemBase>();
        }

        public ItemBase DropItem(int idx, Vector3 position)
        {
            var data = TableDataManager.Instance.data.BattleItem.Single(t => t.Idx == idx);
            var pool = PoolManager.Instance.GetPool(data.Resource);
            var item = pool.Rent().GetComponent<ItemBase>();
            item.transform.position = position;
            item.Pool = pool;
            item.Init(idx);

            switch (data.TypeID)
            {
                case 1:
                    Potion.Add(item);
                    break;
                case 2:
                    Magnet.Add(item);
                    break;
                case 3:
                    Bomb.Add(item);
                    break;
                case 12:
                    ExpGem.Add(item);
                    break;

            }
            // IDX => 아이템 드롭.생성

            return item;
        }

        public void UseItem(ItemBase item)
        {
            switch (item.item.TypeID)
            {
                case 1:
                    Potion.Remove(item);
                    break;
                case 2:
                    Magnet.Remove(item);
                    break;
                case 3:
                    Bomb.Remove(item);
                    break;
                case 12:
                    ExpGem.Remove(item);
                    break;
            }
        }

        [Button]
        public void MagnetExpItem()
        {
            SoundManager.Instance.PlayFX("Stage_Item_Magnet");
            foreach (var itemBase in ExpGem)
            {
                itemBase.Magnet();
            }
        }
    }
}
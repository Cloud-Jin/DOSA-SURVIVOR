using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

// 연출 끝나고 아이템 효과 발동.
namespace ProjectM.Battle
{
    public abstract class ItemBase : ObjectBase, IPoolObject
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }
        public BattleItem item;
        
        protected Transform PlayerTransform;
        protected Collider2D col;
        protected SpriteRenderer spriteRenderer;
        public override void Awake()
        {
            PlayerTransform = PlayerManager.Instance.player.transform;
            col = GetComponent<Collider2D>();
            col.enabled = false;
            col.isTrigger = true;
            spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sortingLayerName = MyLayer.Item;
        }

        public void Init(int idx)
        {
            item = TableDataManager.Instance.data.BattleItem.Single(t => t.Idx == idx);
            col.enabled = true;
        }

        public void GetItem()
        {
            Pool.Return(this);
            // switch (type.TypeID)
            // {
            //     case BattleItemType.Coin:
            //     case BattleItemType.Gem:
            //     case BattleItemType.Magnet:
            //     case BattleItemType.Bomb:
            //     case BattleItemType.Ice:
            //         UseItem();
            //         break;
            // }
            UseItem();
        }

        protected abstract void UseItem();

        public virtual void Magnet()
        {
            col.enabled = false;
            float distance = Vector2.Distance(transform.position, PlayerTransform.position);

            var duration = distance * 0.15f;
            if (duration >= 1)
                duration = 1f;

            transform.DOMoveInTargetLocalSpace(PlayerTransform, Vector3.zero, duration)
                .SetEase(Ease.InCubic)
                .SetAutoKill(true)
                .OnComplete(() => { GetItem(); });
        }
    }
}
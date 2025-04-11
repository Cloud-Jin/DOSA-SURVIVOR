using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;


namespace ProjectM.Battle
{
    public class ExpItem : ItemBase
    {
        private IDisposable _disposable;
        public override void Awake()
        {
            base.Awake();
            _disposable = this.UpdateAsObservable().Subscribe(t => Distance());
        }

        protected override void UseItem()
        {
            BattleManager.Instance.GetExp(item.Value);
            BattleItemManager.Instance.UseItem(this);
            //AudioManager.instance.PlaySfx(AudioManager.Sfx.ExpGet);
        }

        private void Distance()
        {
            var distance = (transform.position - PlayerTransform.position).sqrMagnitude;
            col.enabled = distance < 3;
        }

        public override void Magnet()
        {
            _disposable.Dispose();
            base.Magnet();
            
        }
    }
}

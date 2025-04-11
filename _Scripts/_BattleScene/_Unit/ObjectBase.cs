using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class ObjectBase : MonoBehaviour
    {

        public virtual void Awake()
        {
            // BattleManager.Instance.BattleState.Subscribe(t =>
            // {
            //     isOnlyRun = t == BattleState.Run;
            //     isRun = t is BattleState.Run or BattleState.Boss;
            // }).AddTo(this);
        }
    }
}

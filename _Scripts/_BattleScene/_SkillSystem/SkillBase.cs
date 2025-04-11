using System;
using System.Collections;
using System.Collections.Generic;
using InfiniteValue;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;
using UnityEngine.Serialization;


namespace ProjectM.Battle
{
    public class SkillBase : MonoBehaviour
    {
        public int idx;         // 스킬 Idx
        public int level;

        protected Player player;
        protected SkillAI data;
        
        protected bool isOnlyRun;   // Run state
        protected bool isRun;       // Run || Boss state

        public virtual void Init(Player player, int skillGroupID)
        {
            this.player = player;
            // Level = 1;
            idx = skillGroupID;
            // Basic Set
            name = "Skill " + idx;
            
            // transform.parent = player.transform;
            transform.parent = PlayerManager.Instance.playerble.slot;
            transform.localPosition = Vector3.zero;

            SetLevel(1);
            BattleManager.Instance.BattleState.Subscribe(t =>
            {
                isOnlyRun = t == BattleState.Run;
                isRun = t is BattleState.Run or BattleState.Boss;
            }).AddTo(this);
        }

        public virtual void SetLevel(int Lv)
        {
            level = Lv;
            if (level > 5)
                level = 5;
        }

        

        public virtual void Fire()
        {
            
        }

        public virtual void DataUpdate()
        {
            data = GetSkillTable();
        }
        
        public SkillAI GetSkillTable()
        {
            return TableDataManager.Instance.GetSkillAiData(idx, level);
        }
    }
}
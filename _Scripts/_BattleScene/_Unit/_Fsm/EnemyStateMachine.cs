using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectM.Battle._Fsm.Enemy
{
    public class EnemyStateMachine : StateMachine, IState
    {
        private UnitBase unit;
        // reference to the state objects
        public IdleState idleState;
        public List<int> Patterns = new List<int>();
        public float GlobalCoolTime;
        // private Player player;
        private Playerble playerble;
        public EnemyStateMachine(UnitBase unit) : base(unit)
        {
            this.unit = unit;
            // this.idleState = new IdleState(unit, this);
            unit.deadAction = null;
            Patterns.Clear();
            // player = PlayerManager.Instance.player;
            playerble = PlayerManager.Instance.playerble;
        }

        public void IdleState(IdleState state)
        {
            this.idleState = state;
        }

        public void AddPattern(int skillId)
        {
            Patterns.Add(skillId);
        }

        // Main Type

        public SkillState GetSkillState(int groupID)
        {
            if (SkillStates.Count(t => t.data.GroupID == groupID && (t.data.AddExplosionSkillId > 0 || t.data.NextSkillId > 0)) > 0) 
                return SkillStates.Single(t => t.data.GroupID == groupID && (t.data.AddExplosionSkillId > 0 || t.data.NextSkillId > 0));
            
            return SkillStates.Single(t => t.data.GroupID == groupID);
        }

        public override void StateMachineUpdate()
        {
            base.StateMachineUpdate();
            GlobalCoolTime -= Time.deltaTime;
        }
        

        public override void NextState()
        {
            base.NextState();
            if (Patterns.Count == 0) return;
            
            if(GlobalCoolTime < 0)
            {
                var pattern = Patterns[Random.Range(0, Patterns.Count)];
                var state = GetSkillState(pattern);
                state.SetTarget(playerble);
                TransitionTo(state);
            }
        }

        public override void SetIdelState()
        {
            base.SetIdelState();
            TransitionTo(idleState);
        }

        public override void SetCoolTime(SkillAI data)
        {
            base.SetCoolTime(data);
            GlobalCoolTime = data.CoolTime / 1000f;
        }

        public override void SetNextState(int index)
        {
            base.SetNextState(index);
            var state = SkillStates.Single(t => t.data.Index == index);
            state.SetTarget(playerble);
            TransitionTo(state);
        }
    }
}
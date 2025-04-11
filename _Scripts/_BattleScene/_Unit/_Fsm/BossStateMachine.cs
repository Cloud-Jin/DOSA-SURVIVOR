using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectM.Battle._Fsm.Boss
{
    public class BossStateMachine : StateMachine, IState
    {
        private UnitBase unit;
        // reference to the state objects
        public IdleState idleState;
        public List<int> Patterns = new List<int>();
        public float GlobalCoolTime;
        private Playerble playerble;
        public BossStateMachine(UnitBase unit) : base(unit)
        {
            this.unit = unit;
            idleState = new IdleState(unit, this);
            Patterns.Clear();
            playerble = PlayerManager.Instance.playerble;
        }

        public void AddPattern(int skillId)
        {
            Patterns.Add(skillId);
        }

        // Main Type

        public SkillState GetSkillState(int groupID)
        {
            if (SkillStates.Count(t => t.data.GroupID == groupID && (t.data.AddExplosionSkillId > 0 || t.data.NextSkillId > 0)) > 0)
                return SkillStates.First(t => t.data.GroupID == groupID);
            
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
            if (Patterns.Count == 0)
                return;

            if(GlobalCoolTime < 0)
            {
                var pattern = Patterns[Random.Range(0, Patterns.Count)];
                // if (pattern == 1) return;
                // var state = GetSkillState(4);
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
            if (data.CoolTime == 0) return;
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
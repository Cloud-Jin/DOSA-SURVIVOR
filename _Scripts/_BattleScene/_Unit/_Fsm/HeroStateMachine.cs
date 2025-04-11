using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectM.Battle._Fsm
{
    public class HeroStateMachine : StateMachine
    {
        private UnitBase unit;
        // reference to the state objects
        public IdleState idleState;
        public TeleportState teleportState;
        
       
        public List<float> coolTimes = new List<float>();
        public HeroStateMachine(UnitBase unit) : base(unit)
        {
            this.unit = unit;
            
            idleState = new IdleState(unit, this);
            teleportState = new TeleportState(unit);
        }

        public override void AddSkillState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            base.AddSkillState(unit, data, stateMachine);
            
            coolTimes.Add(data.CoolTime / 1000f);
            SetTimeTarget();
        }

        public override void StateMachineUpdate()
        {
            base.StateMachineUpdate();
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < coolTimes.Count; i++)
            {
                coolTimes[i] -= deltaTime;
            }
        }
        

        public override void NextState()
        {
            base.NextState();
        }

        public override void SetCoolTime(SkillAI data)
        {
            base.SetCoolTime(data);
            int index = SkillStates.FindIndex(t => t.data.GroupID == data.GroupID);
            coolTimes[index] = data.CoolTime / 1000f;

            if (data.SkillGroup != 9)
                SetTimeTarget();
        }

        public void ResetCoolTime()
        {
            for (int i = 0; i < coolTimes.Count; i++)
            {
                coolTimes[i] = 0f;
            }

            SetTimeTarget();
        }

        public override void SetIdelState()
        {
            base.SetIdelState();
            
            unit.disposables.Clear();
            TransitionTo(idleState);
        }
        
        public void ChangeSkillData(int level)
        {
            // 평타제외하고 스킬레벨업 데이터로 변경.
            SkillStates.Where(t => t.data.SkillGroup != 9).ToList().ForEach(t =>
            {
                var newData = TableDataManager.Instance.GetSkillAiData(t.data.GroupID, level);
                t.SetData(newData);
            });
        }

        // 쿨타임 게이지 바 세팅
        void SetTimeTarget()
        {
            if (coolTimes.Count == 1) return;
            
            idleState.value = coolTimes.Select((elem, index) => new { elem, index })
                .Where(t => t.index > 0)
                .Min(arg => arg.elem); //.Min(); //minValue
            idleState.TargetIndex = coolTimes.IndexOf(idleState.value); //index
        }
    }
}
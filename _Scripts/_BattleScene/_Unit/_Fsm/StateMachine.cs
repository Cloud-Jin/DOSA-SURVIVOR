using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectM.Battle._Fsm
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }
        public List<SkillState> SkillStates = new List<SkillState>();
        // event to notify other objects of the state change
        public event Action<IState> stateChanged;
        
        // reference to the state objects

        public StateMachine(UnitBase unit)
        {
            
        }
        
        public void Initialize(IState state)
        {
            CurrentState = state;
            state.Enter();
	

            // notify other objects that state has changed
            stateChanged?.Invoke(state);
        }
        
        // exit this state and enter another
        public void TransitionTo(IState nextState)
        {
            CurrentState.Exit();
            CurrentState = nextState;
            nextState.Enter();
	

            // notify other objects that state has changed
            stateChanged?.Invoke(nextState);
        }

        public virtual void AddSkillState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            switch (data.Type)
            {
                case 1: // 바운드 미사일
                    SkillStates.Add(new BounceProjectileState(unit, data, stateMachine));
                    break;
                case 2: // 일반 미사일
                    SkillStates.Add(new ProjectileState(unit, data, stateMachine));
                    break;
                case 3:
                    SkillStates.Add(new PierceProjectileState(unit, data, stateMachine));
                    break;
                case 4:
                    SkillStates.Add(new MultiAngleProjectileState(unit, data, stateMachine));
                    break;
                case 5:
                    SkillStates.Add(new RadiationProjectileState(unit, data, stateMachine));
                    break;
                case 6:
                    SkillStates.Add(new GuideProjectileState(unit, data, stateMachine));
                    break;
                case 7: // 범위 폭파 탄환
                    SkillStates.Add(new ProjectileExplosionState(unit, data, stateMachine));
                    break;
                case 9:
                    SkillStates.Add(new CrossProjectileState(unit, data, stateMachine));
                    break;
                case 10:
                    SkillStates.Add(new GuideRadarProjectileState(unit, data, stateMachine));
                    break;
                // case 13: // 부채꼴 관통
                //     SkillStates.Add(new GuideRadarProjectileState(unit, data, stateMachine));
                //     break;
                case 17: //축구공
                    SkillStates.Add(new BounceProjectileBallState(unit, data, stateMachine));
                    break;
                case 18:
                    SkillStates.Add(new RadiationBounceProjectileState(unit, data, stateMachine));
                    break;
                case 23:
                    SkillStates.Add(new RabbitShieldState(unit, data, stateMachine));
                    break;
                case 24:
                    SkillStates.Add(new DeadEffectState(unit, data, stateMachine));
                    break;
                case 26: // 부채꼴 폭파
                    SkillStates.Add(new MultiProjectileExplosionState(unit, data, stateMachine));
                    break;
                case 31:
                    SkillStates.Add(new JumpState(unit, data, stateMachine));
                    break;
                case 32:
                    SkillStates.Add(new DashState(unit, data, stateMachine));
                    break;
                case 33:
                    SkillStates.Add(new GuideDashState(unit, data, stateMachine));
                    break;
                case 34:
                    SkillStates.Add(new DashShockWaveState(unit, data, stateMachine));
                    break;
                case 35:
                    SkillStates.Add(new JumpResetState(unit, data, stateMachine));
                    break;
                case 41:
                    SkillStates.Add(new BombState(unit, data, stateMachine));
                    break;
                case 42:
                    SkillStates.Add(new TargetBombState(unit, data, stateMachine));
                    break;
                case 43:
                    SkillStates.Add(new TargetTraceBombState(unit, data, stateMachine));
                    break;
                case 51:
                    SkillStates.Add(new TrapState(unit, data, stateMachine));
                    break;
                case 52:
                    SkillStates.Add(new TargetTrapState(unit, data, stateMachine));
                    break;
                case 53:
                    SkillStates.Add(new GridTrapState(unit, data, stateMachine));
                    break;
                case 58:
                    SkillStates.Add(new BounceProjectilePassState(unit, data, stateMachine));
                    break;
                case 61:
                    SkillStates.Add(new SummonGroupState(unit, data, stateMachine));
                    break;
                case 62:
                    SkillStates.Add(new SummonState(unit, data, stateMachine));
                    break;
                case 71:
                    SkillStates.Add(new BombLaunchState(unit, data, stateMachine));
                    break;
                case 81:
                    SkillStates.Add(new NearingAttackState(unit, data, stateMachine));
                    break;
                case 82:
                    SkillStates.Add(new NearingArcAttackState(unit, data, stateMachine));
                    break;
                case 84:
                    SkillStates.Add(new NearingStraightAttackState(unit, data, stateMachine));
                    break;
                case 85:
                    SkillStates.Add(new NearingCircleAttackState(unit, data, stateMachine));
                    break;

                default:
                    SkillStates.Add(new EmptyState(unit, data, stateMachine));
                    break;
            }
        }
        public void Tick()
        {
            if (CurrentState != null)
            {
                CurrentState.Tick();
            }
        }
        
        public virtual void StateMachineUpdate()
        {
            // 쿨타임관리
        }

        public virtual void SetIdelState()
        {
            // set Idel
        }
        
        public virtual void SetNextState(int index)
        {
          // 다음연계기 지정
        }
        
        public virtual void NextState()
        {
            // state 랜덤 or 쿨타임 선택
        }

        public virtual void SetCoolTime(SkillAI data)
        {
            // 쿨타임 지정
        }
        
        public SkillState GetState(int index)
        {
            var state = SkillStates.Single(t => t.data.Index == index);
            return state;
        }
        
        
        
    }
}
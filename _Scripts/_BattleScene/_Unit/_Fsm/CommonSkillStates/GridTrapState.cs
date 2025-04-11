using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEditor.Rendering;

namespace ProjectM.Battle._Fsm
{
    public class GridTrapState : SkillState, IState
    {
        private int _count;
        private int RowSize;
        private float CubeSize;
        
        public GridTrapState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
        }

        public void Enter()
        {
            castingTime = data.CastingTime / 1000f;
            count = data.Count;

            _count = 0;
            RowSize = data.TypeValue;
            CubeSize = data.Range / 10f;

            isStatePlay = true;
            Observable.FromCoroutine(Trap).Subscribe().AddTo(caster);
        }

        public void Tick()
        {
            
        }

        public void Exit()
        {
            SetCoolTime();
        }

        IEnumerator Trap()
        {
            --count;
            
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            
            yield return new WaitForSeconds(castingTime);
            caster.PlayAnim(data.Ani);
            MovePause();
            var dir = (targetUnit.transform.position - caster.transform.position).normalized * 2f;
            var basePos = caster.transform.position + dir;
            
            var angle = MyMath.GetAngle(caster.transform.position, targetUnit.transform.position) - 90f;

            for (int j = 0; j < data.TypeValue; j++)
            {
                for (int i = 0; i < data.ObjectValue; i++)
                {
                    Transform bullet = pool.Rent().transform;
                    bullet.localPosition = Vector3.zero;
                    bullet.localRotation = Quaternion.identity;
                    bullet.localScale = Vector3.one * (data.Scale / 100f);
                    bullet.position = ( basePos + GetNextPos());
                    bullet.RotateAround(basePos, Vector3.forward, angle);
                    
                    var projectileScript = bullet.GetComponent<ProjectileTrap>();
                    projectileScript.InitBuilder()
                        .SetPool(pool)
                        .SetPer(Per)
                        .SetDamage(Damage)
                        .SetDuration(data.DurationTime / 1000f)
                        .SetUnit(caster)
                        .Build();
                }
                
                yield return new WaitForSeconds(0.25f);
            }

            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(Trap).Subscribe().AddTo(caster.disposables);
            }
            else if (data.NextSkillId > 0)
            {
                stateMachine.SetNextState(data.NextSkillId);
            }
            else
            {
                stateMachine.SetIdelState();
            }
        }
        
        public Vector3 GetNextPos()
        {
            int x = _count % RowSize;
            int y = (_count / RowSize) % RowSize;

            ++_count;
            if(_count >= Mathf.Pow(RowSize, 2))
                _count = 0;

            Vector3 vector3 = new Vector3(x, y, 0) * CubeSize + new Vector3(-CubeSize, 0, 0);
            
            // Debug.Log($"Vector = {vector3}");

            return vector3;
        }
    }
}
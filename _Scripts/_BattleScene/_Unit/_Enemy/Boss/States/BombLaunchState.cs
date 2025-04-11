/*using System;
using System.Collections;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

// 폭격
namespace ProjectM.Battle._Fsm.Boss
{
    [Serializable]
    public class BombLaunchState : SkillState, IState
    {
        public BombLaunchState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.unit = unit.GetComponent<EnemyBoss>();
            this.data = data;
            this.stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            target = PlayerManager.Instance.PlayerRigidBody2D;
            Rigid = unit.Rigid;
            castingTime = data.CastingTime / 1000f;
            Observable.FromCoroutine(BombLaunch).Subscribe();
        }

        public void Tick()
        {
            unit.Spriter.flipX = target.position.x > Rigid.position.x;
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
            Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        IEnumerator BombLaunch()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            yield return new WaitForSeconds(castingTime);
            
            // var dir = (target.position - Rigid.position).normalized;
            // Start, Weight, End
            unit.PlayAnim(data.Ani);
            for (int i = 0; i < data.TypeValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.SetActive(false);
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;

                bullet.position = unit.weaponM.position;
                Vector3[] pos = new Vector3[3] { unit.weaponM.position, Vector3.one, Vector3.one};
                pos[1] = unit.weaponM.position + Vector3.up * 3;
                // pos[2] = target.position + MyMath.GetCircleSize(data.SubTypeValue / 10f);
                bullet.SetActive(true);
                bullet.GetComponent<ProjectileCurve>().Init(Damage, 0, pos, data.Speed / 10f);
                bullet.GetComponent<IPoolObject>().Pool = pool;
                
                yield return new WaitForSeconds(data.CountTime / 1000f);
            }

            stateMachine.SetIdelState();
        }

        // 해당 각도로 던짐
        public Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }
    }
}*/
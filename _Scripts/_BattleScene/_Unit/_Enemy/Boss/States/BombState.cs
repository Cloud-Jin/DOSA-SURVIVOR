/*using System;
using System.Collections;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

namespace ProjectM.Battle._Fsm.Boss
{
    public class BombState : SkillState, IState
    {
        public BombState(UnitBase unit, SkillAI data, StateMachine stateMachine)
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
            Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            
            Observable.FromCoroutine(Bomb).Subscribe();
        }

        public void Tick()
        {
            // unit.Spriter.flipX = target.position.x > Rigid.position.x;
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
            Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        IEnumerator Bomb()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            
            yield return new WaitForSeconds(castingTime);
            
            // var dir = (target.position - Rigid.position).normalized;
            // Start, Weight, End
            unit.PlayAnim(data.Ani);
            for (int i = 0; i < data.TypeValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.SetActive(false);
                bullet.parent = unit.transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.position = Rigid.position + MyMath.GetCircleSize(data.TypeValue / 10f);
                bullet.SetActive(true);
                
                bullet.GetComponent<ProjectileExplosion>().Init(10, 2f, null);  // 몇초있다 펑~
                bullet.GetComponent<IPoolObject>().Pool = pool;
                
                yield return new WaitForSeconds(data.CountTime / 1000f);
            }
            
            stateMachine.SetIdelState();
        }
    }
}*/
/*using System;
using System.Collections;
using UnityEngine;
using UniRx;


/*
1 = 튕김(관통)(n회 튕길 때까지)
2 = 단일
3 = 관통
4 = 사용자 중앙 기준 바라보는 방향(n도) 부채꼴 발사
5 = 사용자 중앙 기준 바라보는 방향 방사
6 = 유도형
#1#


namespace ProjectM.Battle._Fsm.Boss
{
    [Serializable]
    public class LaunchState : SkillState, IState
    {
        private int count;
        public LaunchState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
        }
        public void Enter()
        {
            if (targetUnit == null)
            {
                var scanner = caster.GetComponent<Scanner>();
                if (scanner.nearestTarget == null)
                {
                    stateMachine.SetIdelState();
                    return;
                }

                targetUnit = scanner.nearestTarget.GetComponent<UnitBase>();
            }
            Rigid = caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            count = data.Count;
            // switch (data.SubType)
            // {
            //     // case 1:
            //     //     Observable.FromCoroutine(Launch).Subscribe();
            //     //     break;
            //     case 5: // 방사 패턴
            //         Observable.FromCoroutine(BombLaunch).Subscribe();
            //         break;
            //     default:
            //         Observable.FromCoroutine(Launch).Subscribe();
            //         break;
            // }
            
            
        }

        public void Tick()
        {
            if(targetUnit)
                caster.Spriter.flipX = targetUnit.Rigid.position.x > caster.Rigid.position.x;
        }

        public void Exit()
        {
            stateMachine.SetCoolTime(data);
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        IEnumerator Launch()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            yield return new WaitForSeconds(castingTime);
            
            for (int i = 0; i < data.Count; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;
                bullet.localScale = Vector3.one * (data.Scale / 100f);
                
                
                bullet.position = (caster.Spriter.flipX) ? caster.weaponR.position : caster.weaponL.position;
                var dir = (targetUnit.Rigid.position - new Vector2(bullet.position.x, bullet.position.y)).normalized;
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                bullet.GetComponent<IPoolObject>().Pool = pool;
                
                // if(data.SubType == 1)
                //     bullet.GetComponent<WallBounce>().Init(Damage, dir, 0, data.SubTypeValue, data.Speed / 10f);
                // if(data.SubType == 3)
                //     bullet.GetComponent<Projectile>().Init(Damage, -100, dir, data.Speed / 10f);

                caster.PlayAnim(data.Ani);
                yield return new WaitForSeconds(data.CountTime / 1000f);
            }

            stateMachine.SetIdelState();
        }
        
        IEnumerator BombLaunch()
        {
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            yield return new WaitForSeconds(castingTime);
            count--;
            var dir = (targetUnit.Rigid.position - caster.Rigid.position).normalized;
            caster.PlayAnim(data.Ani);
            for (int i = 0; i < data.TypeValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;

                bullet.position = caster.weaponM.position;
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                bullet.GetComponent<Projectile>().Init(Damage, 1, dir, data.Speed / 10f);
                bullet.GetComponent<IPoolObject>().Pool = pool;
                
                //yield return new WaitForSeconds(data.CountTime / 1000f);
                dir = Quaternion.AngleAxis(360f/data.TypeValue, Vector3.forward) * dir;
            }

            if (count > 0)
            {
                yield return new WaitForSeconds(data.CountTime / 1000f);
                Observable.FromCoroutine(BombLaunch).Subscribe();
                
            }
            else
            {
               stateMachine.SetIdelState();
            }
        }

        // 해당 각도로 던짐
        public Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }
    }
}*/
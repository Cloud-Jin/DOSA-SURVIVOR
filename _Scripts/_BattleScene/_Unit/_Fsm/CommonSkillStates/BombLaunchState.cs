using System;
using System.Collections;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

// 폭격스킬 71
// 2차 폭발

namespace ProjectM.Battle._Fsm
{
    public class BombLaunchState : SkillState, IState
    {
        public BombLaunchState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            this.caster = unit;
            this.data = data;
            this.stateMachine = stateMachine;
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
        }
        
        public void Enter()
        {
            rigid = caster.Rigid;
            castingTime = data.CastingTime / 1000f;
            count = data.Count;
            
            FindTargetNearest();
            if (IsDistance())
            {
                isStatePlay = true;
                Observable.FromCoroutine(BombLaunch).Subscribe().AddTo(caster.disposables);    
            }
            else
            {
                isStatePlay = false;
                stateMachine.SetIdelState();
            }
        }

        public void Tick()
        {
            
        }

        public void Exit()
        {
            SetCoolTime();
        }

        IEnumerator BombLaunch()
        {
            --count;
            var pool = PoolManager.Instance.GetPool(data.ObjectResource);
            yield return new WaitForSeconds(castingTime);
            
            // var dir = (target.position - Rigid.position).normalized;
            // Start, Weight, End
            caster.PlayAnim(data.Ani);
            MovePause();
            FlipX();
            for (int i = 0; i < data.ObjectValue; i++)
            {
                Transform bullet = pool.Rent().transform;
                bullet.SetActive(false);
                bullet.localPosition = Vector3.zero;
                bullet.localRotation = Quaternion.identity;

                bullet.position = GetPivot().position;// caster.BodyParts[data.Pivot].position;
                Vector3[] pos = new Vector3[3] { bullet.position, Vector3.one, Vector3.one};
                pos[1] = bullet.position + (Vector3.up * data.Angle / 10f);
                pos[2] = targetUnit.Rigid.position + MyMath.GetCircleSize(data.TypeValue / 10f);
                bullet.SetActive(true);
                
                
                var bulletScript = bullet.GetComponent<ProjectileCurve>();
                bulletScript.InitBuilder()
                    .SetDamage(Damage)
                    .SetPer(Per)
                    .SetPool(pool)
                    .SetPosition(pos)
                    .SetSpeed(data.Speed / 10f)
                    .SetUnit(caster)
                    .SetAimResource("Effect_Skill_Warning_01")
                    .Build();
                
                if (data.AddExplosionAble > 0)
                {
                    bulletScript.AddExplosion(() =>
                    {
                        var _data = TableDataManager.Instance.GetSkillAiData(data.AddExplosionSkillId);
                        // var _damage =  MyMath.CalcCoefficient(caster.attack, _data.DamageRatio);
                        // bulletScript.HitCallback(_data, _damage);
                    
                        switch (_data.Type)
                        {
                            case 41:
                                var bulletBomb = ExplosionBulletBomb(data.AddExplosionSkillId);
                                bulletBomb.transform.position = bulletScript.transform.position;
                                pool.Return(bulletScript);
                                break;
                            case 51:
                                var bulletTrap = ExplosionBulletTrap(data.AddExplosionSkillId);
                                bulletTrap.transform.position = bulletScript.transform.position;
                                pool.Return(bulletScript);
                                break;
                        }    
                    });
                }
                else
                {
                    bulletScript.EndCallback(0);
                }
            }

            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(BombLaunch).Subscribe().AddTo(caster.disposables);
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

        // 해당 각도로 던짐
        public Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }

        Bullet ExplosionBulletTrap(int skillID)
        {
            var eData = TableDataManager.Instance.GetSkillAiData(skillID);
            var damage = MyMath.CalcDamage(caster.attack, eData.DamageRatio, 0);

            PoolManager.Instance.CreatePool(eData.ObjectResource, 1);
            var pool = PoolManager.Instance.GetPool(eData.ObjectResource);
            var obj = pool.Rent();

            var bulletScript = obj.GetComponent<DamageOverTime>();
            bulletScript.InitBuilder()
                .SetPool(pool)
                .SetDamage(damage)
                .SetTick(eData.DamegeTime / 1000f)
                .SetDuration(eData.DurationTime / 1000f)
                .SetUnit(caster)
                .Build();

            return bulletScript;
        }
        
        Bullet ExplosionBulletBomb(int skillID)
        {
            var eData = TableDataManager.Instance.GetSkillAiData(skillID);
            var damage = MyMath.CalcDamage(caster.attack, eData.DamageRatio, 0);

            PoolManager.Instance.CreatePool(eData.ObjectResource, 1);
            var pool = PoolManager.Instance.GetPool(eData.ObjectResource);
            var obj = pool.Rent();
            
            
            var bulletScript = obj.GetComponent<ProjectileTrap>();
            bulletScript.InitBuilder()
                .SetPool(pool)
                .SetDamage(damage)
                .SetPer(1)
                .SetbombScale(eData.Scale / 100f)
                .SetDuration(eData.DurationTime *0.001f)
                .SetKnockBack(eData.KnockBack / 10f)
                .SetUnit(caster)
                .Build();

            return bulletScript;
        }
    }
}
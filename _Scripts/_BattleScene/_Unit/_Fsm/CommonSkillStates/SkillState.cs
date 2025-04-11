
using System;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectM.Battle._Fsm
{
    public class SkillState : IState
    {
        protected Rigidbody2D target;
        protected Rigidbody2D rigid;

        // 통합
        public SkillAI data;
        protected bool isStatePlay;     // 스킬시전여부
        protected UnitBase caster;      // 시전자
        protected UnitBase targetUnit;
        protected StateMachine stateMachine;
        protected float castingTime;
        public Queue<Transform> Pivot = new Queue<Transform>();
        protected int count;
        protected Action<InfVal> AccDamageFunc; // 영웅전용 누적 데미지;
        
        public void SetTarget(UnitBase unit)
        {
            targetUnit = unit;
        }
        
        protected InfVal Damage
        {
            get { return MyMath.CalcCoefficient(caster.attack, data.DamageRatio); }
        }

        protected int Per => data.Penetration;
        protected int BounceCount => data.Bounce;
        

        protected void Init()
        {
            var hero = caster as Hero;
            if (hero)
            {
                AccDamageFunc = hero.AccDamageFunc;
            }
            
            if(!string.IsNullOrEmpty(data.ObjectResource))
                PoolManager.Instance.CreatePool(data.ObjectResource, 1);
        }

        protected void FindTargetNearest(bool reset = false)
        {
            if (reset)
                targetUnit = null;
            if (targetUnit)
                return;
            
            if (targetUnit == null)
            {
                var scanner = caster.GetComponent<Scanner>();
                if(scanner == null)
                {
                    isStatePlay = false;
                    stateMachine.SetIdelState();
                    return;
                }
                if (scanner.nearestTarget == null)
                {
                    isStatePlay = false;
                    stateMachine.SetIdelState();
                    return;
                }

                targetUnit = scanner.nearestTarget.GetComponent<UnitBase>();
            }
        }
        
        /// <summary>
        /// true dir Right
        /// </summary>
        public void FlipX(bool isFlip)
        {
            var dot = targetUnit.transform.position.x - caster.transform.position.x;

            if (Math.Abs(dot) < 0.01f) return;
            
            var scale = caster.BodyParts["Parts"].transform.localScale;
            scale.x = (isFlip) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            caster.Spriter.transform.localScale = scale;
            // caster.BodyParts["Parts"].transform.localScale = scale;
        }
        
        public void FlipX()
        {
            if (targetUnit == null) return;
            var dot = targetUnit.transform.position.x - caster.transform.position.x;
            
            if (Math.Abs(dot) < 0.1f) return;
            
            var value = targetUnit.transform.position.x - caster.transform.position.x;
            // var scale = caster.Spriter.transform.localScale;
            var scale = caster.BodyParts["Parts"].transform.localScale;
            scale.x = (value > 0 ) ? -Math.Abs(scale.x) : Math.Abs(scale.x);
            
            // caster.Spriter.transform.localScale = scale;
            caster.BodyParts["Parts"].transform.localScale = scale;
        }

        public void MoveTo()
        {
            if(caster.isKnockBack) return;
            FindTargetNearest();
            if (targetUnit == null) return;
            
            Vector2 dirVec = targetUnit.Rigid.position - caster.Rigid.position;
            caster.Rigid.velocity = dirVec.normalized * (caster.baseSpeed / 10f);
            caster.Rigid.drag = 0;
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            if(caster.UnitType == UnitType.Boss)
                caster.Anim.SetFloat("Speed", caster.Rigid.velocity.sqrMagnitude);
        }

        public void MovePause()
        {
            caster.Anim.SetFloat("Speed", 0);
            caster.Rigid.drag = 1000f;
            caster.isHitAniPlayEnable = false;
            if (caster.UnitType == UnitType.Boss)
            {
                caster.Rigid.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        public void MoveResume()
        {
            caster.Anim.SetFloat("Speed", 0);
            // caster.Rigid.velocity = Vector2.zero;
            caster.Rigid.drag = 0;
            caster.isHitAniPlayEnable = true;
            caster.Rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        protected void DashEnter()
        {
            if (caster.UnitType == UnitType.Boss)
                caster.Rigid.mass = 10;
        }
        
        protected void DashEnd()
        {
            if (caster.UnitType == UnitType.Boss)
                caster.Rigid.mass = 1;
        }

        public bool IsDistance()
        {
            if (targetUnit == null)
                return false;
            
            var Distance = (targetUnit.Rigid.position - caster.Rigid.position).magnitude;
            if (Distance <= data.Range / 10f)
                return true;

            return false;
        }

        public bool IsDistanceTarget()
        {
            var collider2Ds = Physics2D.OverlapCircleAll(caster.transform.position, data.Range / 10f);

            return collider2Ds.Count(t => t.transform == targetUnit.transform) == 0;
        }

        public void  SetData(SkillAI newData)
        {
            data = newData;
        }

        public Transform GetPivot()
        {
            if (Pivot.Count > 0)
            {
                return Pivot.Peek();
            }
            else if(!string.IsNullOrEmpty(data.Pivot))
            {
                return caster.BodyParts[data.Pivot];
            }
            else
            {
                return caster.transform;
            }
        }
        
        public UnitBase PriorityTarget(float range = 0)
        {
            UnitBase target = null;
            switch (data.PriorityTarget)
            {
                case 0:
                    target = PlayerManager.Instance.playerble;
                    break;
                // case 2:
                //     target = NearestTargetInWindow();
                //     break;
                // case 3:
                //     target = MaxHpTarget();
                //     break;
                // case 4:
                //     target = NearestUnitTypeTarget(range);
                //     break;
                case 5:
                    target = UnitTypeTargetRandom(range);
                    break;
            }

            return target;
        }
        
        UnitBase UnitTypeTargetRandom(float range)
        {
            Vector2 pointA = new Vector2(-4, 6) + caster.Rigid.position;
            Vector2 pointB = new Vector2(4, -7) + caster.Rigid.position;
            float _range = range > 0 ? range : data.Range / 10f;
            var _list = Physics2D.OverlapAreaAll(pointA, pointB, LayerMask.GetMask("Enemy"));
            
            var unitList = _list.Select(t => t.GetComponent<UnitBase>())
                .Where(t=> Vector2.Distance(t.transform.position,caster.transform.position) <= _range).ToList();

            // Debug.Log($"Count = {unitList.Count}");
            // 스킬 사거리
            // 특수 몬스터 
            // 거리


            var target = unitList.OrderByDescending(t => t.UnitType)
                .ThenBy(t => Random.value)
                .FirstOrDefault();

            return target;

        }

        public Vector3 GetScale()
        {
            var hero = caster as Hero;
            if (hero)
            {
                var _value = MyMath.Increase((data.Scale / 100f), SkillSystem.Instance.incValue.Scale);
                return Vector3.one * _value;    
            }
            else
            {
                return Vector3.one * (data.Scale / 100f);
            }
        }
        
        // 부채꼴 데미지 판정
        public void AttackArc(float radius, float angleRange, Vector3 dir, LayerMask targetLayer)
        {
            var targets = Physics2D.OverlapCircleAll(caster.transform.position, radius, targetLayer);
            
            foreach (var target in targets)
            {
                IDamageable _IDamageable;
                UnitBase _unitBase;
                IsUnitBase(target, out _IDamageable, out _unitBase);
                if(_IDamageable == null) continue;
                
                var topPos = target.bounds.center + new Vector3(0, target.bounds.extents.y, 0);
                float topDegree = MyMath.GetDegree(caster.transform.position, topPos, dir);
                var bottomPos = target.bounds.center + new Vector3(0, -target.bounds.extents.y, 0);
                float bottomDegree = MyMath.GetDegree(caster.transform.position, bottomPos, dir);

                // Debug.DrawLine(caster.transform.position, topPos, Color.yellow);
                // Debug.DrawLine(caster.transform.position, bottomPos, Color.yellow);

                // 시야각 판별
                if (topDegree <= angleRange / 2f || bottomDegree <= angleRange / 2f)
                {
                    _IDamageable.TakeDamage(Damage, HitType.Normal);
                    // IN
                }
            }
        }

        protected void SetCoolTime()
        {
            // 실행하고 상태가 변경되면 쿨타임 적용
            if(isStatePlay)
                stateMachine.SetCoolTime(data);
        }
        
        public void IsUnitBase(Collider2D col, out IDamageable IDamageable, out UnitBase unitBase)
        {
            if (col.attachedRigidbody)
            {
                IDamageable = col.attachedRigidbody.GetComponent<IDamageable>();
                unitBase = col.attachedRigidbody.GetComponent<UnitBase>();
            }
            else
            {
                IDamageable = col.GetComponent<IDamageable>();
                unitBase = col.GetComponent<UnitBase>();
            }
        }

        public LayerMask GetTargetLayer()
        {
            LayerMask mask;
            switch (caster.UnitType)
            {
                case UnitType.Enemy:
                    mask = LayerMask.GetMask("Player");
                    break;
                case UnitType.Boss:
                    mask = LayerMask.GetMask("Player");
                    break;
                case UnitType.Hero:
                    mask = LayerMask.GetMask("Enemy");
                    break;
                default:
                    mask = LayerMask.GetMask("Default");;
                    Debug.Log("Set Mask");
                    break;
            }

            return mask;
        }
    }
}
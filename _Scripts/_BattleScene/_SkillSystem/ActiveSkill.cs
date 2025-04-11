using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectM.Battle
{
    public class ActiveSkill : SkillBase
    {
        public InfVal damage;
        public float speed;
        public float coolTime;
        public InfVal accDamage;

        public void AccDamage(InfVal val)
        {
            accDamage += val;
        }
        public override void SetLevel(int lv)
        {
            base.SetLevel(lv);
            
            DataUpdate();
        }

        protected int per => data.Penetration;
        protected int bound => data.Bounce;

        public virtual void Dispose()
        {
            
        }
        public float commonDmg => SkillSystem.Instance.common;
        public Vector2 LastDir()
        {
           var dir = PlayerManager.Instance.LastDir;

           return dir;
        }

        public UnitBase MaxHpTarget()
        {
            Vector2 pointA = new Vector2(-4, 6) + player.Rigid.position;
            Vector2 pointB = new Vector2(4, -7) + player.Rigid.position;
            var _list = Physics2D.OverlapAreaAll(pointA, pointB, LayerMask.GetMask("Enemy"));
            var unitList = _list.Select(t => t.GetComponent<UnitBase>()).ToList();
            
            // Debug.Log($"Count = {unitList.Count}");
            return unitList.OrderByDescending(t => t.maxHealth).FirstOrDefault();
        }
        
        public UnitBase PriorityTarget(float range = 0)
        {
            UnitBase target = null;
            switch (data.PriorityTarget)
            {
                case 2:
                    target = NearestTargetInWindow();
                    break;
                case 3:
                    target = MaxHpTarget();
                    break;
                case 4:
                    target = NearestUnitTypeTarget(range);
                    break;
                case 5:
                    target = UnitTypeTargetRandom(range);
                    break;
            }

            return target;
        }
        
        public Vector3 PriorityDir(float range = 0)
        {
            UnitBase target = null;
            switch (data.PriorityTarget)
            {
                case 1:
                    return LastDir();
                case 2:
                    target = NearestTargetInWindow();
                    break;
                case 3:
                    target = MaxHpTarget();
                    break;
                case 4:
                    target = NearestUnitTypeTarget(range);
                    break;
                case 5:
                    target = UnitTypeTargetRandom(range);
                    break;
            }

            Vector3 dir = Vector3.up;
            
            if (target)
            {
                dir = MyMath.GetDirection(player.transform.position, target.transform.position);
            }
            else
            {
                dir = PlayerManager.Instance.LastDir;
            }
            
            if(dir == Vector3.zero)
                dir = Vector3.up;
            
            return dir;
        }

        UnitBase NearestUnitTypeTarget(float range)
        {
            Vector2 pointA = new Vector2(-4, 6) + player.Rigid.position;
            Vector2 pointB = new Vector2(4, -7) + player.Rigid.position;
            var _list = Physics2D.OverlapAreaAll(pointA, pointB, LayerMask.GetMask("Enemy"));
            float _range = range > 0 ? range : data.Range / 10f;
            var unitList = _list.Select(t => t.GetComponent<UnitBase>())
                .Where(t=> Vector2.Distance(t.transform.position,player.transform.position) <= _range).ToList();

            // Debug.Log($"Count = {unitList.Count}");
            // 스킬 사거리
            // 특수 몬스터 
            // 거리
            var target = unitList.OrderByDescending(t=> t.UnitType)
                .ThenBy(t => Vector2.Distance(t.transform.position, player.transform.position))
                .FirstOrDefault();
            return target;
        }
        
        UnitBase UnitTypeTargetRandom(float range)
        {
            Vector2 pointA = new Vector2(-4, 6) + player.Rigid.position;
            Vector2 pointB = new Vector2(4, -7) + player.Rigid.position;
            float _range = range > 0 ? range : data.Range / 10f;
            var _list = Physics2D.OverlapAreaAll(pointA, pointB, LayerMask.GetMask("Enemy"));
            
            var unitList = _list.Select(t => t.GetComponent<UnitBase>())
                .Where(t=> Vector2.Distance(t.transform.position,player.transform.position) <= _range).ToList();

            // Debug.Log($"Count = {unitList.Count}");
            // 스킬 사거리
            // 특수 몬스터 
            // 거리


            var target = unitList.OrderByDescending(t => t.UnitType)
                .ThenBy(t => Random.value)
                .FirstOrDefault();

            return target;

        }
        
        UnitBase NearestTargetInWindow()
        {
            Vector2 pointA = new Vector2(-4, 6) + player.Rigid.position;
            Vector2 pointB = new Vector2(4, -7) + player.Rigid.position;
            var _list = Physics2D.OverlapAreaAll(pointA, pointB, LayerMask.GetMask("Enemy"));
            var unitList = _list.Select(t => t.GetComponent<UnitBase>()).ToList();
            
            return unitList.OrderBy(t => Vector2.Distance(t.transform.position, player.transform.position)).FirstOrDefault();;
        }

        public Vector3 GetScale
        {
            get
            {
                var _value = MyMath.Increase((data.Scale / 100f), SkillSystem.Instance.incValue.Scale);
                var challengeBattleManager = ChallengeBattleManager.Instance as ChallengeBattleManager;
                var skillSize = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.SkillSize)??0;
                _value = MyMath.Decrease(_value, skillSize/100f);
                
                return Vector3.one * _value;
            }
        }

        public Vector3 GetDataScale(SkillAI data)
        {
            var _value = MyMath.Increase((data.Scale / 100f), SkillSystem.Instance.incValue.Scale);
            var challengeBattleManager = ChallengeBattleManager.Instance as ChallengeBattleManager;
            var skillSize = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.SkillSize)??0;
            _value = MyMath.Decrease(_value, skillSize/100f);
                
            return Vector3.one * _value;
        }
        
        // 부채꼴 데미지 판정
        public void AttackArc(float radius, float angleRange, Vector3 dir, LayerMask targetLayer, float knockBackDist)
        {
            var targets = Physics2D.OverlapCircleAll(player.transform.position, radius, targetLayer);

            foreach (var target in targets)
            {
                if(target.GetComponent<IDamageable>() == null) continue;
                
                var topPos = target.bounds.center + new Vector3(0, target.bounds.extents.y, 0);
                float topDegree = MyMath.GetDegree(player.transform.position, topPos, dir);
                var bottomPos = target.bounds.center + new Vector3(0, -target.bounds.extents.y, 0);
                float bottomDegree = MyMath.GetDegree(player.transform.position, bottomPos, dir);

                // Debug.DrawLine(caster.transform.position, topPos, Color.yellow);
                // Debug.DrawLine(caster.transform.position, bottomPos, Color.yellow);

                // 시야각 판별
                if (topDegree <= angleRange / 2f || bottomDegree <= angleRange / 2f)
                {
                    // IN
                    var _attack = player.CalcCriticalDamage(damage, target.GetComponent<UnitBase>(), out var hitType);
                    target.GetComponent<IDamageable>().TakeDamage(_attack, hitType);
                    AccDamage(_attack);
                    if (knockBackDist > 0)
                    {
                        var _dir = MyMath.GetDirection(player.transform.position, target.transform.position);
                        target.GetComponent<IDamageable>().TakeKnockBack(_dir, knockBackDist);
                    }
                }
            }
        }
    }
}
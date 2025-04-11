// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 폭탄
// 범위 공격 등 활용

namespace ProjectM.Battle
{
    public class Scanner : MonoBehaviour
    {
        public float scanRange;
        public LayerMask targetLayer;
        public RaycastHit2D[] targets;
        public Transform nearestTarget;
        // public Transform randomTarget;

        public void Init(float scanRange, LayerMask mask)
        {
            this.scanRange = scanRange;
            targetLayer = mask;
        }
        
        void FixedUpdate()
        {
            targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
            // 태그보단 레이어로 체크 하는게 성능상 이득
            
            nearestTarget = GetNearest();
        }

        // 에디터 확인용 코드
#if UNITY_EDITOR
        public Color editorCircleColor = Color.green; //circle colour for editor
        //Draw gizmo wire disc (circle) when GameObject is selected.
        //If you want it to always draw, then you can use void OnDrawGizmos() instead
        void OnDrawGizmosSelected()
        {
            //set gizmo colour.
            UnityEditor.Handles.color = editorCircleColor;
            //draw wire circle (disc in unity lingo) based on radius variable you set.
            UnityEditor.Handles.DrawWireDisc(this.transform.position, this.transform.forward, scanRange);
        }
#endif

        Transform GetNearest()
        {
            Transform result = null;
            float diff = 100;

            foreach (RaycastHit2D target in targets)
            {
                Vector3 myPos = transform.position;
                Vector3 targetPos = target.transform.position;
                float curDiff = Vector3.Distance(myPos, targetPos);

                if (curDiff < diff)
                {
                    diff = curDiff;
                    result = target.transform;
                }
            }

            return result;
        }
        
        public UnitBase MaxHpTarget()
        {
            var unitList = GetList().Select(t => t.GetComponent<UnitBase>()).ToList();
            
            // Debug.Log($"Count = {unitList.Count}");
            return unitList.OrderByDescending(t => t.maxHealth).FirstOrDefault();
        }

        public UnitBase GetRandomTarget()
        {
            UnitBase result = null;
            var unitList = GetList().Select(t => t.GetComponent<UnitBase>()).ToList();
            if(unitList.Count > 0)
                result = unitList[Random.Range(0, unitList.Count)];

            return result;
        }

        public List<Transform> GetList()
        {
            return targets.Select(t=> t.transform).ToList();
        }


    }
}
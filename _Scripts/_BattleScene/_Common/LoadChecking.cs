using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class LoadChecking : MonoBehaviour
    {
        public List<CompositeCollider2D> _collider2D;

        public bool OverlapPoint(Vector2 point)
        {
            foreach (var t in _collider2D)
            {
                var check = t.OverlapPoint(point);
                if (check)
                    return true;
            }

            return false;
        }
    }
}
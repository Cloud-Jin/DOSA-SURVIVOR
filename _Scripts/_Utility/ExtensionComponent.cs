﻿using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace ProjectM
{
    public static class ExtensionComponent
    {
        public static void SetActive(this Component component, bool value)
        {
            component.gameObject.SetActive(value);
        }
        
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveInTargetLocalSpace(this Transform transform, Transform target, Vector3 targetLocalEndPosition, float duration)
        {
            var t = DOTween.To(
                () => transform.position - target.transform.position, // Value getter
                x => transform.position = x + target.transform.position, // Value setter
                targetLocalEndPosition, 
                duration);
            t.SetTarget(transform);
            return t;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 타겟을 추적해서 UI 표기
namespace ProjectM.Battle
{
    public class Follow : MonoBehaviour
    {
        [Tooltip("World space object to follow")]
        public Transform target;
        public Vector3 offset;
        [Tooltip("World space camera that renders the target")]
        public Camera worldCamera;
        [Tooltip("Canvas set in Screen Space Camera mode")]
        public Canvas canvas;
        RectTransform rect;

        public float y;
        void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvas = UIManager.Instance.GetCanvas("Follow").GetComponent<Canvas>();
        }

        private void Start()
        {
            worldCamera = Camera.main;
            // canvas = UIManager.Instance.GetCanvas("Follow").GetComponent<Canvas>();
            FollowFixedUpdate();
            // Observable.
            this.FixedUpdateAsObservable().Subscribe(t =>
            {
                FollowFixedUpdate();
            }).AddTo(this);
        }

        private void FollowFixedUpdate()
        {
            RectTransform parent = (RectTransform)rect.parent;
            var vp = worldCamera.WorldToViewportPoint(target.transform.position + offset);
            var sp = canvas.worldCamera.ViewportToScreenPoint(vp);

            //if (sp.y > 1920)      // 보정  1080x1920

            Vector3 worldPoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, sp, canvas.worldCamera, out worldPoint);
            rect.position = worldPoint;
        }
    }
}

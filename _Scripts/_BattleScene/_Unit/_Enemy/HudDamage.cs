using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;

namespace ProjectM.Battle
{
    public class HudDamage : ObjectBase, IPoolObject
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }
        
        RectTransform rect;
        TMP_Text myText;
        Camera worldCamera;
        Canvas canvas;
        private Transform target;
        void Awake()
        {
            rect = GetComponent<RectTransform>();
            myText = GetComponent<TMP_Text>();
            canvas = UIManager.Instance.GetCanvas("Follow").GetComponent<Canvas>();
            transform.SetParent(canvas.transform, false);
            worldCamera = Camera.main;
        }
        
        public void Init(Transform targetTr, int damage)
        {
            target = targetTr;
            myText.SetText(damage.ToString());
            
            transform.localScale = Vector3.one;
            Invoke("Hide", 1f);
            transform.SetParent(canvas.transform, false);
        }

        private void FixedUpdate()
        {
            
            RectTransform parent = (RectTransform)rect.parent;
            var vp = worldCamera.WorldToViewportPoint(target.position /*+ offset*/);
            var sp = canvas.worldCamera.ViewportToScreenPoint(vp);

            //if (sp.y > 1920)      // 보정  1080x1920

            Vector3 worldPoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, sp, canvas.worldCamera, out worldPoint);
            rect.position = worldPoint;
        }

        void Hide()
        {
            Pool.Return(this);
        }

        
    }

}
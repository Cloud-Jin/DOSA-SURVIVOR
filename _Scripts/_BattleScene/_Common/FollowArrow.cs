using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class FollowArrow : MonoBehaviour
    {
        [Tooltip("World space object to follow")]
        public Transform target;
        public Transform Panel, Pivot;
        // public Vector3 offset;
        [Tooltip("World space camera that renders the target")]
        Camera worldCamera;
        [Tooltip("Canvas set in Screen Space Camera mode")] 
        Canvas canvas;
        RectTransform rect;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        private void Start()
        {
            worldCamera = Camera.main;
            canvas = UIManager.Instance.GetCanvas("Follow").GetComponent<Canvas>();
            
        }

        private void FixedUpdate()
        {
            RectTransform parent = (RectTransform)rect;
            var vp = worldCamera.WorldToViewportPoint(target.transform.position);// + offset);
            var sp = canvas.worldCamera.ViewportToScreenPoint(vp);

            // 보정  1080x1920
           
            var a = MyMath.Compare(sp.x, 0, Screen.width);
            var b = MyMath.Compare(sp.y, 0, Screen.height);

            if (a && b)
            {
                Panel.SetActive(false);
            }
            else
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, sp, canvas.worldCamera, out var originPosition);
                
                var convertX = Mathf.Clamp(sp.x, Screen.width * 0.1f, Screen.width * 0.9f);
                var convertY = Mathf.Clamp(sp.y, Screen.height * 0.1f, Screen.height * 0.82f);

                sp.x = convertX;
                sp.y = convertY;
                
                RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, sp, canvas.worldCamera, out var worldPoint);
                rect.position = worldPoint;

                var dirPosition = target.position;
                var dir = MyMath.GetDirection(dirPosition, originPosition);
                Pivot.rotation = Quaternion.FromToRotation(Vector3.up, dir);

                Panel.SetActive(true);
            }
        }
        
        public void SetFollow(Transform target, Vector3 offset) 
        {
            this.target = target;
            // this.offset = offset;
        }
    }
}
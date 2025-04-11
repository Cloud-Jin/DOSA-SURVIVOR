using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectM
{
    public class SceneBase : MonoBehaviour
    {
        public Camera camera;

        private void Awake()
        {
            Init();
        }

        public virtual void Init()
        {
            // camera overlay 
            UIManager.Instance.UIScreen();
            var cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Clear();
            cameraData.cameraStack.Add(UIManager.Instance.UICamera);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace ProjectM.Battle
{
    public class ScreenColliders : MonoBehaviour
    {
        public void ApplyCollider()
        {
            if (Camera.main == null)
            {
                Debug.LogError("Camera.main not found, failed to create edge colliders");
                return;
            }
            
            var cam = Camera.main;
            if (!cam.orthographic)
            {
                Debug.LogError("Camera.main is not Orthographic, failed to create edge colliders");
                return;
            }

            var basePosition = new Vector3(0, 0, -10);
            cam.transform.position = basePosition;
            
            var bottomLeft = (Vector2)cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            var topLeft = (Vector2)cam.ScreenToWorldPoint(new Vector3(0, cam.pixelHeight * 0.9f, cam.nearClipPlane));
            var topRight =
                (Vector2)cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight * 0.9f, cam.nearClipPlane));
            var bottomRight = (Vector2)cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, 0, cam.nearClipPlane));

            // add or use existing EdgeCollider2D
            var edge = GetComponent<EdgeCollider2D>() == null
                ? gameObject.AddComponent<EdgeCollider2D>()
                : GetComponent<EdgeCollider2D>();

            var edgePoints = new[] { bottomLeft, topLeft, topRight, bottomRight, bottomLeft };
            edge.points = edgePoints;
        }
    }

}
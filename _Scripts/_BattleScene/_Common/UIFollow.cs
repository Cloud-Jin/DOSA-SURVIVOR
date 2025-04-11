using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public class UIFollow : MonoBehaviour
    {
        private Follow follow;
        private UIHealth uiHealth;

        private void Awake()
        {
            follow = GetComponent<Follow>();
            uiHealth = GetComponent<UIHealth>();
        }

        public void SetFollow(Transform target, Vector3 offset)
        {
            follow.target = target;
            follow.offset = offset;
        }

        public void SetHPBar(float v)
        {
            uiHealth.HpBar.value = v;
        }

        public void SetTimeBar(float v)
        {
            uiHealth.TimeBar.value = v;
        }

        public void SetSize(float value)
        {
            // 80 -> 110
            GetComponent<RectTransform>().sizeDelta = new Vector2(value, 80);
        }

        public void TimeBarHide()
        {
            uiHealth.TimeBar.gameObject.SetActive(false);
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}

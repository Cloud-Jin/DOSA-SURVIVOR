using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ProjectM.Battle
{
    public class SpeechBubble : MonoBehaviour
    {
        [Tooltip("World space object to follow")]
        public Transform target;

        public Vector3 offset;

        [Tooltip("World space camera that renders the target")]
        [HideInInspector] public Camera worldCamera;

        [Tooltip("Canvas set in Screen Space Camera mode")]
        [HideInInspector] public Canvas canvas;

        RectTransform rect;
        public TMP_Text talk;
        
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
            if (this.target == null) return;
            
            RectTransform parent = (RectTransform)rect.parent;
            var vp = worldCamera.WorldToViewportPoint(target.transform.position + offset);
            var sp = canvas.worldCamera.ViewportToScreenPoint(vp);
            Vector3 worldPoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, sp, canvas.worldCamera, out worldPoint);
            rect.position = worldPoint;
        }

        public void SetTalk(Transform _target, string _msg, float _duration)
        {
            this.target = _target;
            talk.SetText(_msg);

            Observable.Timer(System.TimeSpan.FromSeconds(_duration)).Subscribe(_ =>
            {
                Destroy(gameObject);
            }).AddTo(this);
        }

        // public void SetTalkTouch(Transform _target, string _msg, Action touchAction)
        // {
        //     this.target = _target;
        //     talk.SetText(_msg);
        //     
        //     var clickStream = this.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0));
        //     clickStream.ThrottleFirst(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
        //     {
        //         touchAction?.Invoke();
        //         Destroy(gameObject);
        //     }).AddTo(this);
        // }
        
        public void SetTalkTouch(SpeechBubbleData data)
        {
            this.target = data.target;
            talk.SetText(data.msgKey.Locale());

            switch (data.type)
            {
                case 1:
                    var clickStream = this.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0));
                    clickStream.ThrottleFirst(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
                    {
                        data.touchAction?.Invoke();
                        Destroy(gameObject);
                    }).AddTo(this);
                    break;
                case 20:// type 20 2초이후 화면터치
                    Observable.Timer(TimeSpan.FromSeconds(2), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                    {
                        var clickStream = this.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0));
                        // var clickStream = this.UpdateAsObservable().Where(_ => Input.GetMouseButton(0));
                        clickStream.ThrottleFirst(TimeSpan.FromSeconds(0.5f)).Subscribe(_ =>
                        {
                            data.touchAction?.Invoke();
                            Destroy(gameObject);
                        }).AddTo(this);
                    }).AddTo(this);
                    
                    break;
                case 21:// type 21 2초이후 동작실행
                    Observable.Timer(TimeSpan.FromSeconds(2f), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                    {
                        data.touchAction?.Invoke();
                        Destroy(gameObject);
                    }).AddTo(this);
                    break;
              
            }    
        }

        public class SpeechBubbleData
        {
            public Transform target;
            public string msgKey;
            public int type;
            public Action touchAction;
        }
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class Reposition : MonoBehaviour
    {
        Collider2D coll;
        private CompositeDisposable disposable;
        private Player player;
        private Map map;
        float valueX = 20;
        float valueY = 20;

        private float scale;
        void Awake()
        {
            coll = GetComponent<Collider2D>();
            disposable = new CompositeDisposable();
        }

        private void Start()
        {
            player = PlayerManager.Instance.player;
            map = BattleManager.Instance.map;
            BattleManager.Instance.LoopRange.Subscribe(v => scale = v).AddTo(this);
        }

        private void OnDestroy()
        {
            EventClear();
        }

        private void OnEnable()
        {
            this.OnTriggerExit2DAsObservable().Where(t=> t.CompareTag("Respawn")).Subscribe(ExitArea).AddTo(disposable);
        }

        private void OnDisable()
        {
            EventClear();
        }

        public void EventClear()
        {
            disposable?.Clear();
        }

        void ExitArea(Collider2D col)
        {
            // if ( !(col.CompareTag("Area") || !col.CompareTag("Ground")) )
            //     return;

            // if(!gameObject.activeSelf) return;
            if(!player) return;
            if (!gameObject.activeSelf) return;
            
            Vector3 playerPos = player.transform.position;
            Vector3 myPos = transform.position;

            
            float diffX = playerPos.x - myPos.x;
            float diffY = playerPos.y - myPos.y;
            float dirX = diffX < 0 ? -1 : 1;
            float dirY = diffY < 0 ? -1 : 1;
            diffX = Mathf.Abs(diffX);
            diffY = Mathf.Abs(diffY);
            switch (transform.tag) 
            {
                case "Ground":
                    switch (map)
                    {
                        case Map.Infinite:
                            if (diffX > diffY)
                            {
                                transform.Translate(Vector3.right * dirX * 32 * 2);
                            }
                            else if (diffX < diffY)
                            {
                                transform.Translate(Vector3.up * dirY * 32 * 2);
                            }

                            break;
                        case Map.Vertical:
                            if (diffX < diffY)
                                transform.parent.parent.Translate(Vector3.up * dirY * 32 * 2);
                            break;
                        case Map.Rect:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case "Enemy":
                    switch (map)
                    {
                        case Map.Infinite:
                            // gameObject.SetActive(false);
                            var a = playerPos - transform.position;
                            transform.Translate(a*1.95f);
                            // gameObject.SetActive(true);
                            // if (diffX > diffY)
                            // {
                            //     transform.Translate(Vector3.right * dirX * (valueX * scale));
                            // }
                            // else if (diffX < diffY)
                            // {
                            //     transform.Translate(Vector3.up * dirY * (valueY * scale));
                            // }

                            break;
                        case Map.Vertical:
                            if (diffX < diffY)
                            {
                                // gameObject.SetActive(false);
                                transform.Translate(Vector3.up * dirY * (valueY * scale));
                                // gameObject.SetActive(true);
                            }
                                
                            break;
                        case Map.Rect:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
            }
        }

        
    }

}
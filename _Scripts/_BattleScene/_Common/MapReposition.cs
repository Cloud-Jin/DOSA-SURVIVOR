using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ProjectM.Battle
{
    public class MapReposition : MonoBehaviour
    {
        Collider2D coll;
        private CompositeDisposable disposable;
        private Player player;
        private Map map;
        void Awake()
        {
            coll = GetComponent<Collider2D>();
            disposable = new CompositeDisposable();
        }

        private void Start()
        {
            player = PlayerManager.Instance.player;
            map = BattleManager.Instance.map;
        }

        private void OnDestroy()
        {
            EventClear();
        }

        private void OnEnable()
        {
            this.OnTriggerExit2DAsObservable().Where(t=> t.CompareTag("Area")).Subscribe(ExitArea).AddTo(disposable);
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
                        // 8 * %
                        case Map.Infinite:
                            if (diffX > diffY)
                            {
                                transform.Translate(Vector3.right * dirX * 32);
                            }
                            else if (diffX < diffY)
                            {
                                transform.Translate(Vector3.up * dirY * 32);
                            }

                            break;
                        case Map.Vertical:
                            if (diffX < diffY)
                                transform.Translate(Vector3.up * dirY * 32);
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
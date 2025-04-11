using System;
using System.Collections.Generic;
using System.Linq;
using InfiniteValue;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using UniRx;
using UniRx.Triggers;
using UnityEngine.EventSystems;
using DG;
using DG.Tweening;

// player Manager
namespace ProjectM.Battle
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        public Player player;
        public Playerble playerble;
        
        [SerializeField] private Vector2 JoystickSize = new Vector2(200, 200);
        public JoyStick Joystick;
        public Vector2 MovementAmount;
        public Vector2 LastDir;                 // 플레이어 최종 방향
        private Finger MovementFinger;
        private Transform _rader;
        private List<Hero> heroList = new List<Hero>();
        private bool isLive;
        public ReactiveProperty<bool> InitReady;                         // 초기화 이벤트
        //[Header("# Player Info")] 

        public int PlayerCount => heroList.Count + 1;
        protected override void Init()
        {
            InitReady = new ReactiveProperty<bool>();
            // state에 따라서 조이스틱 상태 변경
            LastDir = new Vector2(0, 1);
            
            BattleManager.Instance.InitReady.Where(t=> t).Subscribe(_ => StartSet()).AddTo(this);
        }

        void StartSet()
        {
            PoolManager.Instance.CreatePool("Effect_Revive_01", 1);
            BattleManager.Instance.gameObjectRef.enemyCleaner.transform.SetParent(player.transform);
            _rader = ResourcesManager.Instance.Instantiate("Effect_Radar_00").transform;
            _rader.transform.localPosition = Vector3.zero;
            _rader.transform.localScale = Vector3.one;
            _rader.transform.SetParent(player.transform);
            
            
            
            player.Init(TableDataManager.Instance.data.Monster.Single(t => t.Index == 1));
            player.SetPlayer();
            
            playerble.SetPlayer();
            playerble.transform.position = Vector3.zero;
            SetPlayerble();
            
            var heroDatas = BlackBoard.Instance.data.HeroDatas;
            foreach (var data in heroDatas)
            {
                var hero = AddHero(data.GroupID);
                hero.accDamage = new InfVal(data.accDamage);
            }
            
            this.UpdateAsObservable().Where(t=> isLive).Subscribe(_ => UpdateInput()).AddTo(this);
            
            isLive = true;
            InitReady.Value = true;
            
        }

        public Rigidbody2D PlayerRigidBody2D
        {
            get { return player.Rigidbody2D; }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            ETouch.Touch.onFingerDown += HandleFingerDown;
            ETouch.Touch.onFingerUp += HandleLoseFinger;
            ETouch.Touch.onFingerMove += HandleFingerMove;
        }
        
        private void OnDisable()
        {
            ETouch.Touch.onFingerDown -= HandleFingerDown;
            ETouch.Touch.onFingerUp -= HandleLoseFinger;
            ETouch.Touch.onFingerMove -= HandleFingerMove;
            EnhancedTouchSupport.Disable();
        }

        private void HandleFingerMove(Finger movedFinger)
        {
            if (movedFinger == MovementFinger && EventSystem.current.currentSelectedGameObject == null)
            {
                Vector2 knobPosition;
                float maxMovement = JoystickSize.x / 2f;
                ETouch.Touch currentTouch = movedFinger.currentTouch;

                if (Vector2.Distance(currentTouch.screenPosition, Joystick.joyStickObj.anchoredPosition) > maxMovement)
                {
                    knobPosition = (currentTouch.screenPosition - Joystick.joyStickObj.anchoredPosition).normalized * maxMovement;
                }
                else
                {
                    knobPosition = currentTouch.screenPosition - Joystick.joyStickObj.anchoredPosition;
                }

                Joystick.Knob.anchoredPosition = knobPosition;
                MovementAmount = knobPosition / maxMovement;
                LastDir = (currentTouch.screenPosition - Joystick.joyStickObj.anchoredPosition).normalized;
                if(LastDir == Vector2.zero)
                    LastDir = Vector2.up;
            }

        }

        private void HandleFingerDown(Finger touchedFinger)
        {
            if (MovementFinger == null && touchedFinger.screenPosition.x <= Screen.width)
            {
                MovementFinger = touchedFinger;
                Joystick.joyStickObj.anchoredPosition = ClampStartPosition(touchedFinger.screenPosition);
                JoystickReset();
            }
        }
        private void HandleLoseFinger(Finger lostFinger)
        {
            if (lostFinger == MovementFinger)
            {
                MovementFinger = null;
                Joystick.joyStickObj.anchoredPosition = new Vector2(UnityEngine.Device.Screen.width / 2f, UnityEngine.Device.Screen.height/5f);
                Joystick.Knob.anchoredPosition = Vector2.zero;
                MovementAmount = Vector2.zero;
            }
        }

        private Vector2 ClampStartPosition(Vector2 startPosition)
        {
            if (startPosition.x < JoystickSize.x / 2)
            {
                startPosition.x = JoystickSize.x / 2;
            }

            if (startPosition.y < JoystickSize.y / 2)
            {
                startPosition.y = JoystickSize.y / 2;
            }
            else if (startPosition.y > Screen.height - JoystickSize.y / 2)
            {
                startPosition.y = Screen.height - JoystickSize.y / 2;
            }

            return startPosition;
        }
        public void JoystickReset()
        {
            MovementAmount = Vector2.zero;
            Joystick.gameObject.SetActive(true);
            Joystick.joyStickObj.sizeDelta = JoystickSize;
        }

        void UpdateInput()
        {
            Vector3 scaledMovement = new Vector3(MovementAmount.x, MovementAmount.y, 0);
            playerble.inputVec = scaledMovement;
            _rader.rotation = Quaternion.FromToRotation(Vector3.up, LastDir);
            // Debug.Log(MovementAmount);

            // print("MovementAmount.x: " + MovementAmount.x);
            // print("MovementAmount.y: " + MovementAmount.y);
        }

        [Button]
        public Hero AddHero(int index)
        {
            var heroDB = TableDataManager.Instance.data.Monster.First(t => t.Index == index);

            if (heroList.Count(t => t.unitID == heroDB.Index) > 0)
            {
                return null;
            }
            
            var hero = ResourcesManager.Instance.Instantiate(heroDB.Resource);
            var heroScript = hero.AddComponent<Hero>();
            heroScript.Init(heroDB);
            heroList.Add(heroScript);
            BlackBoard.Instance.AddHeroData(index);
            
            hero.transform.SetParent(playerble.transform);
            // heroScript.Teleport();
            
            SetAttackDamageUp();
            SetHpUp();
            
            var pool = PoolManager.Instance.GetPool("Effect_Revive_01");
            var effect = pool.Rent() as ParticleBase;
            effect.transform.parent = hero.transform;
            effect.transform.localPosition = Vector3.zero;
            effect.SetReturnTime(1f, SetPlayerble);
            // player Set 
            return heroScript;
        }

        public List<Hero> GetHeroList()
        {
            return heroList;
        }

        void SetPlayerble()
        {
            int count = heroList.Count+1;
            if (playerble.hitRanges.Count < count)
            {
                Debug.Log("MAX Hero");
                return;
            }
            
            // Set pos
            var pos = playerble.hitRanges[count-1].pos;
            
            player.transform.DOLocalMove(pos[0].localPosition, 1f);
            for (int i = 1; i < count; i++)
            {
                heroList[i-1].transform.DOLocalMove(pos[i].localPosition, 1f);
            }
            
            // Set HitBox
            playerble.SetHitBox(count);
            player.SetUIFollow(playerble.hitRanges[count - 1].pivot);
            float size = 70 + (PlayerCount * 10);
            player.UIFollow.SetSize(size);

        }
        

        public void SetHpUp()
        {
            player.SetHpStat();
            foreach (var hero in heroList)
            {
                hero.SetHpStat();
            }
        }

        public void SetMoveSpeedUp()
        {
            player.SetMoveStat();
            foreach (var hero in heroList)
            {
                hero.SetMoveStat();
            }
        }

        public void SetAttackDamageUp()
        {
            player.SetAttackStat();
            foreach (var hero in heroList)
            {
                hero.SetAttackStat();
            }
        }
        
        public void OnHealing(float rate)
        {
            var challengeBattleManager = ChallengeBattleManager.Instance as ChallengeBattleManager;
            var value = challengeBattleManager?.GetPenaltyValue(ChallengePenalty.Hp)??0;
            if (value == 1)
            {
                return;
            }
            
            player.Healing(rate);
            foreach (var hero in heroList)
            {
                hero.Healing(rate);
            }
        }
        
        public void SetCriticalRate()
        {
            player.SetCriticalRate();
            foreach (var hero in heroList)
            {
                hero.SetCriticalRate();
            }
        }
        
        public void SetDamageReduce()
        {
            player.SetDmgReduceStat();
            foreach (var hero in heroList)
            {
                hero.SetDmgReduceStat();
            }
        }
        
        public void LateMove(Vector2 inputVec)
        {
            player.LateMove(inputVec);
            heroList.ForEach(t => t.LateMove(inputVec));

            // 영웅들도
        }

        public void DeadAction()
        {
            player.DaedAnimation();
            foreach (var hero in heroList)
            {
                hero.DaedAnimation();
            }
            
            playerble.OnDead();
            
            isLive = false;
            
            Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(_ =>
            {
                if (BattleManager.Instance.reviveCount > 0)
                {
                    var popup = UIManager.Instance.Get(PopupName.Battle_Revive).GetComponent<PopupRevive>();
                    popup.revive = () =>
                    {
                        ReviveAction();
                    };
                
                    popup.Show();
                }
                else
                    BattleManager.Instance.Lose();
            }).AddTo(this);
        }

        public void ReviveAction()
        {
            BattleManager.Instance.reviveCount--;
            BattleManager.Instance.Bomb();
            // n당 실행
            // 부활
            // TODO 3초 무적 적용
            string powerEffectKey = $"Effect_Skill_Revive_0{PlayerCount}";
            var powerEffect = ResourcesManager.Instance.Instantiate(powerEffectKey, transform);
            powerEffect.transform.SetParent(playerble.transform);
            powerEffect.transform.localPosition = Vector3.zero;
                    
            player.unitState = UnitState.NoHit;
            playerble.Rigid.excludeLayers = LayerMask.GetMask("Enemy");
            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(_ =>
            {
                player.unitState = UnitState.Normal;
                playerble.Rigid.excludeLayers = new LayerMask();
                Destroy(powerEffect);
            }).AddTo(this);
            
            // var pool = PoolManager.Instance.GetPool("Effect_Revive_01");
            // var effect = pool.Rent() as ParticleBase;
            // effect.transform.parent = hero.transform;
            // effect.transform.localPosition = Vector3.zero;
            // effect.SetReturnTime(1f, SetPlayerble);

            Sequence seq = DOTween.Sequence();
            ReviveEffect(player.transform, null);
            player.Revive();
            playerble.OnRevive();
            
            for (int i = 0; i < heroList.Count; i++)
            {
                int index = i;
                Hero hero = heroList[index];
                seq.AppendInterval(0.3f).AppendCallback(() =>
                    {
                        ReviveEffect(hero.transform, null);
                        hero.Revive();
                    }
                );
            }

            seq.Play();
            
            isLive = true;
        }

        public void ReviveEffect(Transform tr, Action endAction)
        {
            var pool = PoolManager.Instance.GetPool("Effect_Revive_01");
            var effect = pool.Rent() as ParticleBase;
            effect.transform.parent = tr;
            effect.transform.localPosition = Vector3.zero;
            effect.SetReturnTime(1f, endAction);
        }
    }
}

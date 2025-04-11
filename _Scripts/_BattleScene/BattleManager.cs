using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Logging;
using Cinemachine;
using DG.Tweening;
using Gbros.UniRx;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;


namespace ProjectM.Battle
{
    public enum BattleState
    {
        Init, Run, Boss, Pause, Victory, Lose
    }

    public enum Map
    {
        Infinite = 1, Vertical, Rect
    }
    public class BattleManager : Singleton<BattleManager>
    {
        public ReactiveProperty<BattleState> BattleState = new ReactiveProperty<BattleState>();
        [HideInInspector] public Map map;
        
        [Header("# Game Control")]
        public int EnemyUnitID = 0;
        [HideInInspector] public Monster[] NormalMonster;
        [HideInInspector] public NormalMonsterGroup[] NormalMonsterGroups;

        [Header("# Player Info")] 
        [HideInInspector] public int[] nextExp;
        
        public GameObjectRef gameObjectRef;
        public Spawner spawner;
        public int bossOrder;
        public int crackOrder;
        public int zoomIndex;
        public bool skipLevelUpUI;
        private List<LoadChecking> loadList = new List<LoadChecking>();
        private List<LoadChecking> barricadeLoadList = new List<LoadChecking>();
        private BattleState prevState;
        private int bossCount;
        private float[] LensSize = new float[4];
        private float[] LoopRangeSize = new float[4];
        private GameObject barricade;
        private bool isBooster;         // 던전 부스터
        private CompositeDisposable disposables = new CompositeDisposable();
        
        public IntReactiveProperty gameTime, kill, level, levelUpCount;
        [HideInInspector] public ReactiveProperty<int> CrackKill, GoldToadKill;
        public FloatReactiveProperty LoopRange;     // 몬스터 재사용 범위
        public BoolReactiveProperty timerPause;
        public BoolReactiveProperty battleTimerPause;
       

        public Subject<int> bossSpawnTime, bossSpawnKill, BossSpawn;
        public Subject<uint> CrackSpawn, BossDespawn;
        public Subject<bool> CrackDespawn;
        public Subject<float> ExpAdd;                          // 획득경험치 이벤트
        public Subject<LevelUpData> OnLevelUp;                 // 레벨업 이벤트
        public ReactiveProperty<bool> InitReady;               // 초기화 이벤트
        public int reviveCount;
        public float exp;
        public int enemyCount;                                 // 현재 스테이지 몬스터 수
        public Stage tbStage;
        public MapType tbMapType;
        public List<List<BossMonsterGroup>> bossGroups;
        public List<BossMonsterGroup> GetBoss => bossGroups[bossOrder];
        public Crack Crack { get; set; }
        
        public List<ScrollDataModel> default_rewards = new();
        public List<ScrollDataModel> add_rewards = new();

        public void SetGameObjectRef(GameObjectRef gameObjectRef)
        {
            this.gameObjectRef = gameObjectRef;
        }
        protected override void Init()
        {
            Debug.Log("배틀 매니저 Init");
            spawner = GetComponent<Spawner>();
            gameTime = new IntReactiveProperty(0);
            kill = new IntReactiveProperty(0);
            timerPause = new BoolReactiveProperty(false);
            battleTimerPause = new BoolReactiveProperty(false); 
            level = new IntReactiveProperty(1);
            levelUpCount = new IntReactiveProperty(0);          // 요청한 레벨업 횟수
            LoopRange = new FloatReactiveProperty();
            CrackKill = new ReactiveProperty<int>(0);
            GoldToadKill = new ReactiveProperty<int>(0);
            
            CrackSpawn = new Subject<uint>();
            CrackDespawn = new Subject<bool>();

            BossSpawn = new Subject<int>();
            BossDespawn = new Subject<uint>();
            bossSpawnTime = new Subject<int>();
            bossSpawnKill = new Subject<int>();
            OnLevelUp = new Subject<LevelUpData>();
            ExpAdd = new Subject<float>();
            InitReady = new ReactiveProperty<bool>();
            
            nextExp = TableDataManager.Instance.LevelData();
            tbStage = TableDataManager.Instance.GetStageData(BlackBoard.Instance.data.mapIndex);
            tbMapType = TableDataManager.Instance.data.MapType.Single(t => t.Type == tbStage.MapType);
            
            bossGroups = TableDataManager.Instance.GetBossMonsterGroups(tbStage.BossMonsterGroupID);
            NormalMonster = TableDataManager.Instance.GetNormalMonster(tbStage.NormalMonsterGroupID);
            NormalMonsterGroups = TableDataManager.Instance.GetNormalMonsterGroup(tbStage.NormalMonsterGroupID);
            
            
            LensSize[0] = tbMapType.CameraZoom / 100f;
            LensSize[1] = tbMapType.WaveCameraZoom1st / 100f;
            LensSize[2] = tbMapType.WaveCameraZoom2nd / 100f;
            LensSize[3] = tbMapType.WaveCameraZoom3rd / 100f;
            LoopRangeSize[0] = tbMapType.MonsterLocationLoopRange / 100f;
            LoopRangeSize[1] = tbMapType.MonsterLocationLoopRange1st / 100f;
            LoopRangeSize[2] = tbMapType.MonsterLocationLoopRange2nd / 100f;
            LoopRangeSize[3] = tbMapType.MonsterLocationLoopRange3rd / 100f;
            
            
            BattleState.Subscribe(t =>
            {
                timerPause.SetValueAndForceNotify(t != Battle.BattleState.Run);
            }).AddTo(this);

            UIManager.Instance.popups.ObserveCountChanged().Subscribe(num =>
            {
                if(num == 0)
                    Resume();
                else if(num > 0 && BattleState.Value != Battle.BattleState.Pause)
                    Pause();
            }).AddTo(this);
            
            // BlackBoard Data Set Here
            var bb = BlackBoard.Instance.data;
            level.Value = bb.level;
            kill.Value = bb.killScore;
            exp = bb.exp;
            CrackKill.Value = bb.crackScore;
            reviveCount = bb.reviveCount;
            bossOrder = bb.bossOrder;
            crackOrder = bb.crackOrder;
            zoomIndex = bb.zoom;
            BattleState.Value = Battle.BattleState.Init;
            isBooster = bb.booster;
        }

        private void Start()
        {
            GameStart();
            InitReady.Value = true;
        }


        private void OnApplicationPause(bool pauseStatus)
        {
#if UNITY_EDITOR
            return;
#endif
        if (pauseStatus)
            {
                if (UIManager.Instance.popups.Count == 0)
                {
                    var popup = UIManager.Instance.Get(PopupName.Battle_Pause);
                    popup.Show();
                }
            }
        }

        private void OnDestroy()
        {
            DOTween.KillAll();
            var damagePool = GameObject.Find("Damage Number Pool");
            Destroy(damagePool);
        }

        protected virtual void GameStart()
        {
            CreatePlayer();
            CreateMap();
            CreateBattleItem();
        }

        protected virtual void CreatePlayer()
        {
            // 캐릭터 생성
            var costumeInfo = BlackBoard.Instance.GetCostume();
            var _player = ResourcesManager.Instance.Instantiate(costumeInfo.Resource);
            _player.transform.SetParent(gameObjectRef.playerble.transform);
            _player.transform.localPosition = Vector3.zero;
            PlayerManager.Instance.player = _player.AddComponent<Player>();
            PlayerManager.Instance.playerble = gameObjectRef.playerble;

            gameObjectRef.cam.Follow = _player.transform;
        }

        protected virtual void CreateMap()
        {
            // Map Load
            map = (Map)tbStage.MapType;
            for (int i = gameObjectRef.mapTransform.childCount -1; i >= 0; i--)
            {
                DestroyImmediate(gameObjectRef.mapTransform.GetChild(i).gameObject);
            }
            
            var _mapObj = ResourcesManager.Instance.Instantiate(tbStage.Resource, gameObjectRef.mapTransform);
            var _LoadCheck = _mapObj.GetComponent<LoadChecking>();
            if(_LoadCheck)
                loadList.Add(_LoadCheck);
            
            if(!ResourcesManager.Instance.initComplete)
                ResourcesManager.Instance.PreLoadResources();
            
            Debug.Log($"스테이지 Index{tbStage.Index}");
        }
        
        protected virtual void CreateBattleItem()
        {
            foreach (var battleItem in TableDataManager.Instance.data.BattleItem)
            {
                PoolManager.Instance.CreatePool(battleItem.Resource, 1);
            }
        }

        protected void TimerStart()
        {
            // 게임진행방식 변경
            BattleState.Value = Battle.BattleState.Run;
            var bb = BlackBoard.Instance.data;
            gameTime.Value = bb.gameTime;
            
            PowerObservable.Countdown(battleTimerPause, bb.gameTime)
                .Select(t => (int)t.TotalSeconds)
                .Subscribe(MainTimeSet, 
                    () =>
                    {
                        gameTime.Value = 0;
                        Lose();
                    }).AddTo(this);
        }

        protected void SpawnEXP()
        {
            var itemType = TableDataManager.Instance.GetStageConfig(11);
            var itemCount = TableDataManager.Instance.GetStageConfig(12);
            
            for (int i = 0; i < itemCount.Value; i++)
            {
                var pos = Vector3.zero + MyMath.RandomDonut(2);
                BattleItemManager.Instance.DropItem(itemType.Value, pos);
            }
        }
        
        // 메인게임 시간
        void MainTimeSet(int sec)
        {
            gameTime.Value = sec;
        }

        [Button]
        public void BossStart()
        {
            if (BattleState.Value == Battle.BattleState.Boss) return;
            
            // 1. 보스 등장 알람
            var popup = UIManager.Instance.Get(PopupName.Battle_AlarmBoss);
            popup.Show();
            
            BattleState.Value = Battle.BattleState.Boss;

            BlackBoard.Instance.SaveBattleData();
            // 3초 뒤 일반 몬스터, 균열 삭제
            PoolManager.Instance.ReleaseAllEnemyPool();
            enemyCount = 0;
            bossCount = GetBoss.Count;
            CrackDespawn.OnNext(false);
            
            Sequence a = DOTween.Sequence();
            a.AppendInterval(3f);
            a.AppendCallback(() =>
            {
                var popup = UIManager.Instance.Get(PopupName.Battle_BossName) as PopupBossName;
                popup.Show();
                
                // map type 울타리 생성
                BossSpawn.OnNext(1);
            });
        }
        public void BossEnd()
        {
            bossCount--;
            if (bossCount > 0) return;
            // 보스 카운팅
            bossOrder++;
            BossDespawn.OnNext(1);

            // map type 울타리 제거
            if (barricade)
            {
                Destroy(barricade);
                barricadeLoadList.Clear();
            }
            
            if (!IsBossRemain())
            {
                if (gameTime.Value <= 0) return;
                Debug.Log("보스 다 잡음");
                battleTimerPause.SetValueAndForceNotify(true);
                Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(_ => Victory()).AddTo(this);
            }
            else
            {
                BlackBoard.Instance.SaveBattleData();
                
                Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
                {
                    BattleState.SetValueAndForceNotify(Battle.BattleState.Run);
                }).AddTo(this);
            }
            
            // 미사일 제거
            LayerMask mask = LayerMask.GetMask("EnemyProjectiles");
            var _collider2D = Physics2D.OverlapCircleAll(PlayerManager.Instance.PlayerRigidBody2D.position, 10, mask);

            foreach (var col in _collider2D)
            {
                if (col.transform.TryGetComponent(out Bullet bullet))
                {
                    bullet.Pool.Return(bullet);
                }
            }
            
            LayerMask _enemyLayer = LayerMask.GetMask("Enemy");
            var _enemyCol = Physics2D.OverlapCircleAll(PlayerManager.Instance.PlayerRigidBody2D.position, 10, _enemyLayer);

            foreach (var col in _enemyCol)
            {
                if (col.transform.TryGetComponent(out EnemyNormal enemy))
                {
                    enemy.Pool.Return(enemy);
                }
            }
            
           
        }
        
        [Button]
        public void CrackStart()
        {
            // 맵 상에 균열이 있으면 스폰 하면 안된다.
            if (PoolManager.Instance.GetPool("Crack").objects.Count > 0)
            {
                return;
            }
            
            CrackSpawn.OnNext(1);
        }

        [Button]
        public void Bomb()
        {
            gameObjectRef.enemyCleaner.SetActive(true);
        }
        [Button]
        public void LevelUp()
        {
            {
                level.Value++;
                 
                Action ShowLevelUp = () =>
                {
                    PlayerManager.Instance.player.OnLevelUp();
                    
                    if (skipLevelUpUI) return;
                    var popup = UIManager.Instance.Get(PopupName.Battle_LevelUp);
                    popup.Show();
                    OnLevelUp.OnNext(new LevelUpData()
                    {
                        popup = popup as PopupLevelUp,
                        level = level.Value,
                    });
                };

                // 자석 레벨업 팝업 닫히는 중에 잠시 텀 중에 호출되는것 같음
                // Pause 체크로직 수정필요.
                
                if (BattleState.Value == Battle.BattleState.Pause || levelUpCount.Value > 0)
                {
                    UIManager.Instance.ReservePopup(ShowLevelUp);
                }
                else
                {
                    ShowLevelUp.Invoke();
                }
            }
            levelUpCount.Value++;
        }

        [Button]
        void HidePopup()
        {
            UIManager.Instance.HidePopup();
        }
        
        public virtual void GetExp(int addExp)
        {
            var calcExp = MyMath.Increase(addExp, SkillSystem.Instance.incValue.Exp);
            exp += calcExp;
            ExpAdd.OnNext(calcExp);
            
            SoundManager.Instance.PlayFX("Get_Exp");
        }

        protected virtual void Victory()
        {
            UIManager.Instance.PendingPopupClear();
            BattleState.Value = Battle.BattleState.Victory;
            
            Debug.Log("생존완료!");
            var tbStage = TableDataManager.Instance.data.Stage.Single(t => t.Index == BlackBoard.Instance.data.mapIndex);
            
            JObject data = new JObject();
            data.Add("type", 1);
            data.Add("t", gameTime.Value);
            data.Add("ge", GoldToadKill.Value);
            // 완료시만
            data.Add("c", 1);
            data.Add("k", kill.Value);
            data.Add("skill", JToken.FromObject(SkillSystem.Instance.ConvertData()));
            
            var payload = new Dictionary<string, object> { { "type", 1 }, { "chapter_id", tbStage.ChapterID }, { "level", tbStage.StageLevel },
                { "data", data } };
            
            BlackBoard.Instance.ResetData();
            APIRepository.RequestStageEnd(payload, data =>
            {
                default_rewards.Clear();
                add_rewards.Clear();

                // 기본 보상
                var rewardList = APIRepository.ConvertReward(data.default_rewards);
                rewardList.AddRange(APIRepository.ConvertReward(data.clear_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.benefit_default_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.benefit_clear_rewards));
                default_rewards = rewardList;
                
                // 추가보상
                var addRewardList = APIRepository.ConvertReward(data.vip_rewards);
                addRewardList.AddRange(APIRepository.ConvertReward(data.first_clear_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.gold_elite_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.breach_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.benefit_breach_rewards));
                add_rewards = addRewardList;

                var popup = UIManager.Instance.Get(PopupName.Battle_ResultVictory) as PopupResultVictory;
                popup.SetRewardData(default_rewards);
                popup.SetAddRewardData(add_rewards);
                popup.Show();
                popup.SetTitle(0);
              
                
                // 레벨업 보상
                int prevLv = UserDataManager.Instance.userInfo.PlayerData.player.level;
                UserDataManager.Instance.userInfo.SetPlayerData(data.player);
                if (prevLv < data.player.level)
                {
                    Action ShowLevelUp = () =>
                    {
                        var levelUpRewardList = APIRepository.ConvertReward(data.level_up_rewards);
                        
                        var levelUp = UIManager.Instance.Get(PopupName.Lobby_AccountLevelUp) as PopupAccountLevelUp;
                        levelUp.SetLevelData(prevLv,data.player.level);
                        levelUp.SetRewardData(levelUpRewardList);
                        levelUp.Show();
                        
                        UserDataManager.Instance.currencyInfo.SetItem(data.level_reward_currencies);
                        
                        if (LobbyMain.Instance)
                            LobbyMain.Instance.ReloadLobbyRedDot();
                    };
                    
                    UIManager.Instance.ReservePopup(ShowLevelUp);
                }
                
                // 연출
                int i = UserDataManager.Instance.stageInfo.Stage(data.stage);
                UserDataManager.Instance.stageInfo.StageData = data.stage; // 결과창 사후체크.
                
                if (i > 0)  // 최초 클리어.
                {
                    UserDataManager.Instance.stageInfo.PlayStage = i;

                    if (i == 8) // Next 8
                    {
                        // 리뷰요청
                        PlayerPrefs.SetInt(MyPlayerPrefsKey.Review, 1);
                        PlayerPrefs.Save();
                    }
                }
                
                // 리뷰 체크.
                var value = PlayerPrefs.GetInt(MyPlayerPrefsKey.Review, 0);
                if (value == 1)
                {
                    // 스토어 *정책 일년에 최대 세번 요청 가능
                    PlayerPrefs.SetInt(MyPlayerPrefsKey.Review, 0);
                    PlayerPrefs.Save();
                    
                    Action ShowReview = () =>
                    {
                        #if UNITY_ANDROID
                        var popupConfirm = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                        popupConfirm.InitBuilder()
                            .SetTitle(LocaleManager.GetLocale("UI_Key_012"))
                            .SetMessage(LocaleManager.GetLocale("go_review"))
                            .SetYesButton(GlobalManager.Instance.OnStoreReview, LocaleManager.GetLocale("Beta_Survey_Agree"))
                            .SetNoButton(null, LocaleManager.GetLocale("Cancel"))
                            .SetHideOverlay(true)
                            .Build();
                        #elif UNITY_IOS
                            GlobalManager.Instance.OnStoreReview();
                        #endif
                    };
                    
                  
                    UIManager.Instance.ReservePopup(ShowReview);
                }
                
                //
            }, (key) =>
            {
                var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage(key.Locale())
                    .SetYesButton(()=>
                    {
                        SceneLoadManager.Instance.LoadScene("Lobby");
                    }, "UI_Key_011".Locale())
                    .Build();
                Debug.Log(key);
            });
        }
        
        [Button]
        public virtual void Lose()
        {
            UIManager.Instance.PendingPopupClear();
            BattleState.Value = Battle.BattleState.Lose;
            var tbStage = TableDataManager.Instance.data.Stage.Single(t => t.Index == BlackBoard.Instance.data.mapIndex);
            
            JObject data = new JObject();
            data.Add("type", 1);
            data.Add("t", gameTime.Value);
            data.Add("ge", GoldToadKill.Value);
            // 완료시만
            // data.Add("c", 1);
            // data.Add("k", kill.Value);
            
            var payload = new Dictionary<string, object> { { "type", 1 }, { "chapter_id", tbStage.ChapterID }, { "level", tbStage.StageLevel },
                { "data", data } };

            BlackBoard.Instance.ResetData();
            APIRepository.RequestStageEnd(payload, data =>
            {
                default_rewards.Clear();
                add_rewards.Clear();
                
                // 기본 보상
                var rewardList = APIRepository.ConvertReward(data.default_rewards);
                rewardList.AddRange(APIRepository.ConvertReward(data.clear_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.benefit_default_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.benefit_clear_rewards));
                default_rewards = rewardList;
                
                // 추가보상
                var addRewardList = APIRepository.ConvertReward(data.vip_rewards); 
                addRewardList.AddRange(APIRepository.ConvertReward(data.gold_elite_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.breach_rewards));
                addRewardList.AddRange(APIRepository.ConvertReward(data.benefit_breach_rewards));
                add_rewards = addRewardList;

                var popup = UIManager.Instance.Get(PopupName.Battle_ResultVictory) as PopupResultVictory;
                popup.SetRewardData(default_rewards);
                popup.SetAddRewardData(add_rewards);
                popup.Show();
                popup.SetTitle(1);

                // 연출
                
                // 레벨업 보상
                int prevLv = UserDataManager.Instance.userInfo.PlayerData.player.level;
                UserDataManager.Instance.userInfo.SetPlayerData(data.player);
                if (prevLv < data.player.level)
                {
                    Action ShowLevelUp = () =>
                    {
                        var levelUpRewardList = APIRepository.ConvertReward(data.level_up_rewards);
                        
                        var levelUp = UIManager.Instance.Get(PopupName.Lobby_AccountLevelUp) as PopupAccountLevelUp;
                        levelUp.SetLevelData(prevLv,data.player.level);
                        levelUp.SetRewardData(levelUpRewardList);
                        levelUp.Show();
                        
                        UserDataManager.Instance.currencyInfo.SetItem(data.level_reward_currencies);
                        
                        if (LobbyMain.Instance)
                            LobbyMain.Instance.ReloadLobbyRedDot();
                    };
                    
                    UIManager.Instance.ReservePopup(ShowLevelUp);
                }
            }, (key) =>
            {
                var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage(key.Locale())
                    .SetYesButton(()=>
                    {
                        SceneLoadManager.Instance.LoadScene("Lobby");
                    }, "UI_Key_011".Locale())
                    .Build();
                Debug.Log(key);
            });
            
         
        }

        public void Pause()
        {
            Time.timeScale = 0;
            prevState = BattleState.Value;
            BattleState.SetValueAndForceNotify(Battle.BattleState.Pause);
            UIManager.Instance.GetCanvas("JoyStick").gameObject.SetActive(false);
        }

        public void Resume()
        {
            Time.timeScale = 1;
            BattleState.SetValueAndForceNotify(prevState);
            UIManager.Instance.GetCanvas("JoyStick").gameObject.SetActive(true);
        }

        public bool IsBossRemain()
        {
            // return bossOrder < tbBoss.Length;
            return bossOrder < bossGroups.Count(t => t.Any());
        }

        public void EnemyKill()
        {
            if (battleTimerPause.Value == true)
                return;
            
            kill.Value += isBooster ? 2 : 1;
        }

        [Button]
        public void CreateBarricade(Vector3 position)
        {
            if (map == Map.Rect)
                return;
            
            string resName = "";
            switch (map)
            {
                case Map.Infinite:
                    resName = "Item_Barricade_Square_7";
                    break;
                case Map.Vertical:
                    resName = "Item_Barricade_Line_7";
                    break;
                case Map.Rect:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            barricade = ResourcesManager.Instance.Instantiate(resName);
            var temp = position;
            if (map == Map.Vertical)
                temp.x = 0;
            
            barricadeLoadList.Add(barricade.GetComponent<LoadChecking>());
            barricade.transform.position = temp;
        }
        
        
        [Button]
        public void SetCameraZoom(int idx)
        {
            zoomIndex = idx;
            float duration = idx == 0 ? 0f : 2f;
            // Smooth
            DOTween.To(() => gameObjectRef.cam.m_Lens.OrthographicSize, value => gameObjectRef.cam.m_Lens.OrthographicSize = value, LensSize[idx],
                    duration)
                .SetEase(Ease.Linear).OnComplete(() =>
                {
                    gameObjectRef.ScreenColliders.ApplyCollider();
                });
            
            // Player Area Set
            PlayerManager.Instance.player.SetRespawnSize(LoopRangeSize[idx]);
            LoopRange.Value = LoopRangeSize[idx];
        }
        
        public void SetCameraZoom(float value)
        {
            // Smooth
            DOTween.To(() => gameObjectRef.cam.m_Lens.OrthographicSize, value => gameObjectRef.cam.m_Lens.OrthographicSize = value, value,
                    2)
                .SetEase(Ease.Linear).OnComplete(() =>
                {
                    gameObjectRef.ScreenColliders.ApplyCollider();
                });
            
            // Player Area Set
            // PlayerManager.Instance.player.SetRespawnSize(LoopRangeSize[idx]);
            // LoopRange.Value = LoopRangeSize[idx];
        }

        public bool MapOverlapPoint(Vector2 point)
        {
            bool inPoint = false;
            foreach (var t in loadList)
            {
                var check = t.OverlapPoint(point);
                if (check)
                    inPoint = true;
            }

            if (loadList.Count == 0)
                inPoint = true;

            foreach (var t in barricadeLoadList)
            {
                var check = t.OverlapPoint(point);
                if (check && inPoint)
                    return true;
            }
            
            if (barricadeLoadList.Count == 0)
                return inPoint;

            return false;
        }
        
        public int GetBonusRate()
        {
            // 1 스테이지 , 2 골드던전, 3 심연
            switch (tbStage.StageType)
            {
                case 2:
                    return TableDataManager.Instance.GetDungeonConfig(3).Value;
                // case 3:
                //     Debug.LogError("Error Config");
                //     return 0;
                default:
                    return TableDataManager.Instance.GetStageConfig(13).Value;
            }
        }
        
        [Serializable]
        public class GameObjectRef
        {
            public Transform mapTransform;
            public GameObject enemyCleaner;
            public CinemachineVirtualCamera cam;
            public ScreenColliders ScreenColliders;
            public Transform screenViewRange;
            public Playerble playerble;
            // public Spawner Spawner;
        }

        public class PenaltyData
        {
            public ChallengeEffectGroup EffectGroup;
            public ChallengePenaltyType PenaltyType;
        }

        public class LevelUpData
        {
            public PopupLevelUp popup;
            public int level;
        }
    }
}

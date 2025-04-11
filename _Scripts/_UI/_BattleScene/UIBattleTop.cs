using System;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Gbros.UniRx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace ProjectM.Battle
{
    public class UIBattleTop : View
    {
        public override ViewName ID { get; set; } = ViewName.Battle_Top;
        
        public TMP_Text time, gold, kill, Level, boss, CrackKill, GoldToadKill;
        public Image expBar;
        public UIButton PauseBtn;
        public GameObject BossUI, BoostUI;
        public Image bossImage;
        public Sprite[] bossIconSprite;
        public HeroRespawnPanel[] heroRespawnPanels;
        [Header("GoldDungeon")] 
        public GameObject goGoldDungeonUi;
        public TMP_Text step, timeCount, dungeonDesc;
        
        
        private int level;
        private int bossSpawnKillCount, bossOrder;

        protected override void Init()
        {
            foreach (var heroRespawnPanel in heroRespawnPanels)
            {
                heroRespawnPanel.SetActive(false);
            }
            
            goGoldDungeonUi.SetActive(false);
        }

        private void Start()
        {
            SetBossUI();
            SetCrackUI();
            // 보스가 나오면 보스 UI로 바뀜.
            SetBoostUI();
            SetExp(0);
            BattleManager.Instance.gameTime.Subscribe(SetTime).AddTo(this);
            BattleManager.Instance.ExpAdd.Subscribe(SetExp).AddTo(this);
            BattleManager.Instance.GoldToadKill.Subscribe(x=>SetText(GoldToadKill,x)).AddTo(this);
            BattleManager.Instance.level.Subscribe(x=>SetText(Level,x)).AddTo(this);
            BattleManager.Instance.kill.Subscribe(x=>SetBossKill(boss,x)).AddTo(this);
            
            PauseBtn.AddEvent(()=>
            {
                var popup = UIManager.Instance.Get(PopupName.Battle_Pause);
                popup.Show();
            });

            BattleManager.Instance.BossSpawn.Subscribe(t =>
            {
                BossUI.SetActive(false);
                BoostUI.SetActive(false);
            }).AddTo(this);
            
            BattleManager.Instance.BossDespawn.Subscribe(t =>
            {
                SetBossUI();
                SetBoostUI();
            }).AddTo(this);

            
        }

        void SetTime(int gametime)
        {
            float remainTime = gametime;
            int min = Mathf.FloorToInt(remainTime / 60);
            int sec = Mathf.FloorToInt(remainTime % 60);
            time.SetText(string.Format("{0:D2}:{1:D2}", min, sec));
        }

        void SetExp(float exp)
        {
            float maxExp = BattleManager.Instance.nextExp[Mathf.Min(BattleManager.Instance.level.Value, BattleManager.Instance.nextExp.Length - 1)];
            
            if (BattleManager.Instance.exp >= maxExp)
            {
                BattleManager.Instance.exp -= maxExp;
                BattleManager.Instance.LevelUp();
            }
            
            // Debug.LogFormat($"{exp} / {maxExp} = {exp/maxExp}");
            expBar.fillAmount =  BattleManager.Instance.exp / maxExp;
        }

        void SetText(TMP_Text text, int value)
        {
            text.SetText(value.ToString());
        }

        void SetBossKill(TMP_Text text, int value)
        {
            text.SetText($"{value}/{bossSpawnKillCount}");
        }

        void SetBossUI()
        {
            var bm = BattleManager.Instance;

            if (!bm.IsBossRemain())
            {
                BossUI.SetActive(false);
                return;
            }
            bossSpawnKillCount = bm.GetBoss[0].SpawnKillCount;
            bossOrder = bm.GetBoss[0].Order;
            
            bossImage.sprite = bossIconSprite[bossOrder - 1];
            BossUI.SetActive(true);
        }

        void SetCrackUI()
        {
            var bm = BattleManager.Instance;
            var crackList = bm.spawner.GetSpawnList(4);
            if (crackList.Any())
            {
                CrackKill.transform.parent.SetActive(true);
                BattleManager.Instance.CrackKill.Subscribe(x=>SetText(CrackKill,x)).AddTo(this);
            }
            else
            {
                CrackKill.transform.parent.SetActive(false);
            }
        }

        public void SetGoldBattleUI(Action complete)
        {
            goGoldDungeonUi.SetActive(true);
            var bb = BlackBoard.Instance.data;
            bossSpawnKillCount = TableDataManager.Instance.GetDungeonConfig(1).Value;
            boss.SetText($"{0}/{bossSpawnKillCount}");
            bossImage.sprite = bossIconSprite[0];
            BossUI.SetActive(true);
            
            step.SetText("Gold_Dungeon_Stage".Locale(bb.dungeonLevel));
            PowerObservable.Countdown(3)
                .Select(t => (int)t.TotalSeconds)
                .Subscribe(t=> timeCount.SetText($"{t}"),
                    () =>
                    {
                        dungeonDesc.transform.parent.SetActive(false);
                        timeCount.transform.parent.SetActive(false);
                        complete.Invoke();
                    }).AddTo(this);
        }

        public void SetBoostUI()
        {
            var bb = BlackBoard.Instance.data;
            BoostUI.SetActive(bb.booster);
        }

        public void SetHeroRespawn(int heroIndex, float respawnTime, Action OnComplete)
        {
            var panel = heroRespawnPanels.First(go => !go.gameObject.activeSelf);
            panel.SetRespawn(heroIndex, respawnTime, OnComplete);
        }
        
    }
}
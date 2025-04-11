using System;
using System.Collections.Generic;
using System.Linq;
using AssetKits.ParticleImage;
using Doozy.Runtime.UIManager.Components;
using ProjectM.AutoBattle;
using ProjectM.Battle;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;

namespace ProjectM
{
    public class UILobby : View
    {
        public static UILobby Instance;
        
        public override ViewName ID { get; set; } = ViewName.Lobby_Main;
        public UILobbyGnb LobbyGnb;
        public UIButton button, boosterButton, FastBattleBtn, AutoBattleBtn, helpBoosterBtn;
        public UIToggle boosterToggle;
        public List<GameObject> Contents;
        public UIToggleGroup uiToggleGroup;
        public List<LockContent> LockContents;
        public ParticleImage CoinParticle;
        public TMP_Text saveAutobattleTime;
        int maxMin; // 최대누적 시간
        List<int> _rewardCollectTime;
        public List<GameObject> rewardBoxlist;
        public Action StartAction;
        [Space(20)]
        public List<Transform> redDotList;
        [Space(20)]
        public List<UITab> categoryTabList;
        
        protected override void Init()
        {
            Instance = this;
            
            // LobbyGnb.Init();
            button.GetOrAddComponent<TouchTypeObject>().TouchType = 16;
            FastBattleBtn.GetOrAddComponent<TouchTypeObject>().TouchType = 13;
            AutoBattleBtn.GetOrAddComponent<TouchTypeObject>().TouchType = 11;
            Contents.ForEach(t=> t.SetActive(true));
            button.AddEvent(() => BattlePlay());
            boosterButton.AddEvent(() => BattlePlay(1));
            FastBattleBtn.AddEvent(FastBattle);
            AutoBattleBtn.AddEvent(AutoBattle);
            helpBoosterBtn.AddEvent(OnHelpBooster);
            // boosterToggle
            // TODO 메인 & 자동전투 기능 분리예정.
            maxMin = TableDataManager.Instance.data.AutoBattleConfig.Single(t => t.Index == 1).Value; // 1440
            _rewardCollectTime = TableDataManager.Instance.data.TreasureBox.Select(t => t.RewardCollectTime).ToList();
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1f)).Subscribe(SaveAutoBattleTime).AddTo(this);
            var boosterValue = PlayerPrefs.GetInt(MyPlayerPrefsKey.BattleBooster, 0) == 1;
            boosterToggle.isOn = boosterValue;
            boosterToggle.ObserveEveryValueChanged(x => x.isOn).Subscribe(t =>
            {
                button.SetActive(!t);
                boosterButton.SetActive(t);
                PlayerPrefs.SetInt(MyPlayerPrefsKey.BattleBooster, t ? 1 : 0);
                PlayerPrefs.Save();
            }).AddTo(this);
        }

        private void Start()
        {
            AutoBattleManager.Instance.EnemyCount.Where(c=> c == 0).Subscribe(t =>
            {
                CoinParticle.Play();
            }).AddTo(this);

            StartAction?.Invoke();
            InitLockContents();

            redDotList.ForEach(r => r.SetActive(false));

            SetRedDot(LobbyTap.Character, UserDataManager.Instance.Gears.Any(u => u.Value.IsShowRedDot()));
            
            categoryTabList.ForEach(c => c.AddBehavioursPointerClick());
        }
        
        [Button]
        void TutorialTest(int groupID)
        {
           TutorialManager.Instance.SetSequence(groupID);
           TutorialManager.Instance.PlaySequence();
        }
        
        [Button]
        void TutorialReset()
        {
            UserDataManager.Instance.clientInfo.Reset();
        }

        [Button]
        void AddUnlock(int type)
        {
            UserDataManager.Instance.clientInfo.AddShowUnlockPopup(type);
        }
        
        private void OnHelpBooster()
        {
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale("Help"))
                    .SetMessage("Booster_Kill_Desc".Locale())
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("Common_Ok_Btn"))
                    .Build();
            }
        }

        void InitLockContents()
        {
            LockContents[0].SetLockEventUIToggle(1);        // 특성
            LockContents[1].SetLockEventUIToggle(2);        // 던전
            LockContents[2].SetLockEventUIButton(6);        // 랭킹
            LockContents[3].SetLockEventUIButton(7);        // 신수
            LockContents[4].SetLockEventUIButton(9);        // 킬부스터
        }

        void SaveAutoBattleTime(long i)
        {
            // 타임
            // var maxMin = autoBattleConfig.Single(t => t.Index == 1).Value; // 1440
            var timeSpan = DateTime.UtcNow - UserDataManager.Instance.stageInfo.StageData.afk_reward_at;
            var maxTimeSpan = TimeSpan.FromMinutes(maxMin);
            var _time = (timeSpan.TotalMinutes > maxMin) ? maxTimeSpan.TotalSeconds : timeSpan.TotalSeconds;
            
            int _hour = (int)(_time / 3600);
            int _min = (int)(_time % 3600 / 60);
            int _sec = (int)(_time % 3600 % 60);
            
            string saveTime = String.Format("{0:D2} : {1:D2} : {2:D2}",_hour,_min,_sec);
            
            saveAutobattleTime.SetText(saveTime);
            AutoBattleBtn.interactable = timeSpan.TotalMinutes >= 1;

            // var value = _rewardCollectTime.OrderBy(x => timeSpan.TotalMinutes - x).First();
            var idx = _rewardCollectTime.FindLastIndex(t => timeSpan.TotalMinutes - t >= 0);
            if (idx == -1) idx = 0;
            if (idx == 4) idx = 3;
            
            for (int j = 0; j < rewardBoxlist.Count; j++)
            {
                rewardBoxlist[j].SetActive(j == idx);
            }
            // Debug.LogFormat("{0} 분", value);
        }

        void BattlePlay(int booster = 0)
        {
            var stageIdx = UserDataManager.Instance.stageInfo.PlayStage;
            var tbStage = TableDataManager.Instance.data.Stage.Single(t => t.Index == stageIdx);
            var payload = new Dictionary<string, object> { { "type", 1 }, { "chapter_id", tbStage.ChapterID}, { "level", tbStage.StageLevel } ,{"boost", booster}};

            // 에너지 재화 체크
            bool enough = UserDataManager.Instance.currencyInfo.ValidGoods((CurrencyType)tbStage.ConsumeType, tbStage.ConsumeCount);
            if (!enough)
            {
                var alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale("Not_Enough_Energy_Msg"))
                    .Build();
                return;
            }

            Action stageAPI = () =>
            {
                APIRepository.RequestStageStart(payload, data =>
                {
                    // Debug.Log(data);
                    var bb = BlackBoard.Instance;
                    bb.ClearData();

                    bb.data.gameTime = tbStage.StageTime;
                    bb.data.mapIndex = tbStage.Index;
                    bb.data.attack = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Attack}";
                    bb.data.hp = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Hp}";
                    bb.data.maxHp = $"{UserDataManager.Instance.gearInfo.GetEquipGearsPower().Hp}";
                    bb.data.weaponInfo = UserDataManager.Instance.gearInfo.GetEquipWeaponInfo();
                    bb.data.costumeInfo = UserDataManager.Instance.gearInfo.GetEquipCostumeInfo();
                    bb.data.RewardDatas = data.breach_rewards;
                    bb.data.BenefitRewardDatas = data.benefit_breach_rewards;
                    bb.data.benefit_ids = data.benefit_ids;
                    bb.data.benefitEffect = UserDataManager.Instance.payInfo.IncreaseStageClearBaseReward;
                    bb.data.booster = booster == 1;
                    var tbSet = bb.GetSkillSet();
                    bb.SetSkillData("player", tbSet.ChangeSkillID, 1);
                    bb.Save();
                    // data.stage.etc
                    // send
                    AppsFlyerManager.Instance.SendLevel(stageIdx);
                    SceneLoadManager.Instance.LoadScene("BattleBase");
                });
            };
            
            if(booster == 0)
                stageAPI.Invoke();
            else
            {
                AdMobManager.Instance.ShowAD(() =>
                    {
                        stageAPI.Invoke();
                    },
                    () =>
                    {
                        Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                        alarm.InitBuilder()
                            .SetMessage(LocaleManager.GetLocale("No_Ad"))
                            .Build();
                    },
                    () =>
                    {
                        Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                        alarm.InitBuilder()
                            .SetMessage(LocaleManager.GetLocale("Ad_Not_Completed"))
                            .Build();
                    });
            }
        }

        void AutoBattle()
        {
            var payload = new Dictionary<string, object>
            {
                { "type", 1}
            };
            APIRepository.RequestAutoBattleRoll(payload, (data =>
            {
                var rewardList = APIRepository.ConvertReward(data.default_rewards);
                rewardList.AddRange(APIRepository.ConvertReward(data.currency_box_rewards));
                rewardList.AddRange(APIRepository.ConvertReward(data.gear_box_rewards));
                
                var popup = UIManager.Instance.Get(PopupName.AutoBattle_BattleAuto) as PopupBattleAuto;
                popup.SetRewardData(rewardList);
                popup.Show();
            }));
        }

        void FastBattle()
        {
            var popup = UIManager.Instance.Get(PopupName.AutoBattle_BattleAutoFast);
            popup.Show();
        }

        public void SetSelectContentsTabs(LobbyTap tap)
        {
            var index = (int)tap;
            if (index < 0 || uiToggleGroup.toggles.Count <= index) return;

            uiToggleGroup.toggles[index].isOn = true;
        }
        
        public UIToggle GetContentsTab(LobbyTap tap)
        {
            var index = (int)tap;
            // if (index < 0 || uiToggleGroup.toggles.Count <= index) return;

            return uiToggleGroup.toggles[index];
        }

        public void SetRedDot(LobbyTap tap, bool state)
        {
            var index = (int)tap;
            if (index < 0 || redDotList.Count <= index)
                return;
            
            redDotList[index].SetActive(state);
        }
    }
}
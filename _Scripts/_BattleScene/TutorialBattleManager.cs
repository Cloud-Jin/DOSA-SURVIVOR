using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;


namespace ProjectM.Battle
{
    public class TutorialBattleManager : BattleManager
    {
        protected override void Init()
        {
            base.Init();
            BlackBoard.Instance.LobbyTap = LobbyTap.Lobby;
        }

        protected override void GameStart()
        {
            base.GameStart();
            SetCameraZoom(0);
            var player = PlayerManager.Instance.player;
            player.SetActive(true);
            
            TutorialManager.Instance.PlaySequence();
            CrackSpawnCallback();
            CrackDeSpawnCallback();
            VictoryCallback();
        }

        public void BattleTimeStart()
        {
            TimerStart();
        }

        public GameObject PlayerSpeechBubble(SpeechBubble.SpeechBubbleData data)
        {
            var player = PlayerManager.Instance.player;
            data.target = player.transform;
            
            var canvas = UIManager.Instance.GetCanvas("Follow").transform;
            var sb = ResourcesManager.Instance.Instantiate("SpeechBubble", canvas).GetComponent<SpeechBubble>();
            sb.SetTalkTouch(data);
            return sb.gameObject;
        }

        public void FingerHideCallback()
        { 
            var player = PlayerManager.Instance.player;
            SpawnEXP();

            var canvas = UIManager.Instance.GetCanvas("Follow").transform;
            var sb = ResourcesManager.Instance.Instantiate("SpeechBubble", canvas).GetComponent<SpeechBubble>();
            sb.SetTalk(player.transform, "Tutorial_Talk_InGame_1".Locale(), 3f);
            TutorialManager.Instance.PlaySequence();
        }

        public void FirstLevelUpCallback()
        {
            IDisposable levelUp = null;
            levelUp = OnLevelUp.Subscribe(t =>
            {
                levelUp.Dispose();
                Debug.Log("레벨업");
                TimerStart();
                // TutorialManager.Instance.PlaySequence();
            }).AddTo(this);
        }

        public void PlayerMovePause(bool pause)
        {
            PlayerManager.Instance.playerble.MovePause = pause;
        }

        public void SetMasterSkillTutorial()
        {
            IDisposable levelUp = null;
            PopupLevelUp.StatedData statedData = new PopupLevelUp.StatedData()
            {
                SkillIdx = new List<int>(){204},
                Bonus = new List<int>(){2,1,1}
            };
            
            IDisposable onBattleLevelUp = null;
            onBattleLevelUp = UIManager.Instance.popups.ObserveAdd().Subscribe(t =>
            {
                if (t.Value.ID == PopupName.Battle_LevelUp)
                {
                    onBattleLevelUp.Dispose();
                    var popup = t.Value as PopupLevelUp;
                    popup.SetRoll(statedData);
                    // popup.slots[0].levelUpBtn.AddComponent<TouchTypeObject>().TouchType = 19;
                    TutorialManager.Instance.SetSequence(13);
                    TutorialManager.Instance.PlaySequence();
                }
            }).AddTo(this);
            
            // levelUp = OnLevelUp.Subscribe(t =>
            // {
            //     levelUp.Dispose();
            //     t.popup.SetRoll(statedData);
            //     t.popup.slots[0].levelUpBtn.GetOrAddComponent<TouchTypeObject>().TouchType = 19;
            //     TutorialManager.Instance.SetSequence(13);
            //     TutorialManager.Instance.PlaySequence();
            // }).AddTo(this);
            
        }

        public void CrackSpawnCallback()
        {
            IDisposable OnCrackSpawn = null;
            OnCrackSpawn = CrackSpawn.Subscribe(t =>
            {
                OnCrackSpawn.Dispose();
                Observable.Timer(TimeSpan.FromSeconds(0.3f)).Subscribe(_ =>
                {
                    // if (UserDataManager.Instance.clientInfo.ClearGroupID.Count(x => x == 11) == 0)
                    {
                        TutorialManager.Instance.SetSequence(11);
                        TutorialManager.Instance.PlaySequence();
                    }
                });


            }).AddTo(this);
        }
        
        public void CrackDeSpawnCallback()
        {
            // 균열 팝업 튜토리얼
            IDisposable onDimensionRiftOpen = null;
            onDimensionRiftOpen = UIManager.Instance.popups.ObserveAdd().Subscribe(t =>
            {
                if (t.Value.ID == PopupName.Battle_DimensionRift)
                {
                    onDimensionRiftOpen.Dispose();
                    TutorialManager.Instance.PlaySequence();
                }
            }).AddTo(this);
            
            // 팝업체크
            
            // IDisposable OnCrackDeSpawn = null;
            // OnCrackDeSpawn = CrackDespawn.Subscribe(t =>
            // {
            //     if (t)
            //     {
            //         OnCrackDeSpawn.Dispose();        
            //         TutorialManager.Instance.PlaySequence();
            //     }
            // }).AddTo(this);
        }

        public void VictoryCallback()
        {
            BattleState.Where(t => t == Battle.BattleState.Victory).Subscribe(_ =>
            {
                TutorialManager.Instance.SetSequence(12);
                TutorialManager.Instance.PlaySequence();
            }).AddTo(this);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using ProjectM.Battle;
using UnityEngine;

namespace ProjectM
{
    public class BattleBaseScene : SceneBase
    {
        public CinemachineConfiner2D confiner2D;
        public Collider2D Vertical, Rect;

        [Header("# Game Object")] 
        public BattleManager.GameObjectRef GameObjectRef;

        private string[] manager = new[]
        {
            "TutorialBattleManager", 
            "BattleManager", 
            "GoldDungeonBattleManager",
            "ChallengeBattleManager",
        };
        public override void Init()
        {
            base.Init();
            UIManager.Instance.PendingPopupClear();
            
            var bb = BlackBoard.Instance;
            var stageData =  TableDataManager.Instance.GetStageData(bb.data.mapIndex);
            // Set Battle manager
            var managerResources = ConvertBattleManager(stageData);
            var manager = ResourcesManager.Instance.Instantiate(managerResources).GetComponent<BattleManager>();
            manager.SetGameObjectRef(GameObjectRef);
        }

        private void Start()
        {
            SoundManager.Instance.PlayBGM("BGM_Battle");
            UIManager.Instance.Get(ViewName.Battle_Top);
            UIManager.Instance.GetCanvas("JoyStick").GetComponent<Canvas>().worldCamera = UIManager.Instance.UICamera;
            UIManager.Instance.GetCanvas("Follow")  .GetComponent<Canvas>().worldCamera = UIManager.Instance.UICamera;
            SetConfiner2D();
            if(AppConfig.Instance.Development)
                UIManager.Instance.Get(ViewName.Battle_Cheat);
        }

        void SetConfiner2D()
        {
            Collider2D collider2D = null;
            switch (BattleManager.Instance.map)
            {
                case Map.Infinite:
                    break;
                case Map.Vertical:
                    collider2D = Vertical;
                    break;
                case Map.Rect:
                    collider2D = Rect;
                    break;
            }

            confiner2D.m_BoundingShape2D = collider2D;
        }

        string ConvertBattleManager(Stage data)
        {
            if (data.ChapterID == 0 && data.StageLevel == 0)
            {
                return manager[0];
            }
            else
            {
                return manager[data.StageType];
            }
        }
    }
}
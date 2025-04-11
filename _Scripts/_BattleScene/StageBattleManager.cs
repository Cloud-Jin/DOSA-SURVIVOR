using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using Gbros.UniRx;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;

namespace ProjectM.Battle
{
    public class StageBattleManager : BattleManager
    {
        protected override void Init()
        {
            base.Init();

            BlackBoard.Instance.LobbyTap = LobbyTap.Lobby;
        }

        protected override void GameStart()
        {
            base.GameStart();
            PlayerManager.Instance.player.SetActive(true);
            SetCameraZoom(zoomIndex);
            TimerStart();
           
            if(level.Value == 1)
                SpawnEXP(); // 초기 경험치 조각 드랍
        }
    }
}
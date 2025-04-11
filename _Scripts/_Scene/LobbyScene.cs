using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM.Battle;
using UnityEngine;

namespace ProjectM
{
    public class LobbyScene : SceneBase
    {
        private void Start()
        {
            SoundManager.Instance.PlayBGM("BGM_Main");
            UIManager.Instance.UIScreen();
            NotificationsManager.Instance.NotificationsRegister();

            var main = UIManager.Instance.Get(ViewName.Lobby_Main) as UILobby;
            var loadingView = UIManager.Instance.Get(ViewName.Common_Loading);
            main.StartAction = () =>
            {
                // 1
                BattleContinue();
                TutorialLogic();
                UnLockPopup();
                SetAttendancePopup();
                SetAutoBattleGoldPopup();
                
                main.SetSelectContentsTabs(BlackBoard.Instance.LobbyTap);
                UIManager.Instance.GetPendingPopups();
                loadingView.Hide();
            };
            
            float size = camera.aspect > 0.5f ? 5.5f : 6.67f;
            camera.orthographicSize = size;
        }

        void UnLockPopup()
        {
            if (TutorialManager.Instance.IsTutorial)
            {
                TutorialManager.Instance.tutoEnd += UnLockPopup;
                return;
            }
            
            var unlockTypes = TableDataManager.Instance.data.UnlockType;
            unlockTypes = unlockTypes.OrderBy(t => t.Order).ToArray();
            
            foreach (var unlockType in unlockTypes)
            {
                UnlockCondition condition = TableDataManager.Instance.GetUnlockCondition(unlockType.UnlockConditionID);
                bool isCondition = false;
                switch (condition.ConditionType)
                {
                    case 1:         // 스테이지 클리어
                        StageData stageData = UserDataManager.Instance.stageInfo.StageData;
                        var clearStage = TableDataManager.Instance.GetStageData(stageData.chapter_id_cap, stageData.level_cap, 1);
                        isCondition = condition.ConditionValue <= clearStage.Index;
                        break;
                    case 2:         // 계정 레벨
                        break;
                }

                if (!isCondition) continue;
                
                // 노출확인 여부 체크
                var isShow = UserDataManager.Instance.clientInfo.GetUnlockData(unlockType.Type);
                if (isShow) 
                    continue;
                if (unlockType.UnlockPopupDisplay == 0) // 팝업 노출여부
                {
                    UserDataManager.Instance.clientInfo.AddShowUnlockPopup(unlockType.Type);
                    continue;
                }

                Action showUnlockPopup = () => 
                {
                    //Debug.Log("등록");
                    var newContentPopup = UIManager.Instance.Get(PopupName.Common_NewContent) as PopupNewContent;
                    newContentPopup.SetNewContent(unlockType);
                    newContentPopup.Show();
                };
                    
                UIManager.Instance.ReservePopup(showUnlockPopup);
            }
        }

        void BattleContinue()
        {
            var bb = BlackBoard.Instance;
            if (bb.Load())
            {
                // 데이터 타입 확인.
                Action battleContinue = () =>
                {
                    string message;
                    if (bb.data.mapIndex == 10001) // 골드던전 이어하기
                    {
                        // 이름, 레벨
                        var tbStage = TableDataManager.Instance.GetStageData(10001);
                        message = "Dungeon_Continue".Locale(tbStage.Name.Locale(), bb.data.dungeonLevel);
                    }
                    else
                    {
                        message = "Popup_Battle_Continue_01".Locale();
                    }
                    
                    //전투 팝업
                    var c = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                    c.InitBuilder()
                        .SetTitle("UI_Key_012".Locale())
                        .SetMessage(message)
                        .SetNoButton(() => { bb.ResetData(); }, "UI_Key_007".Locale()) //리셋하고 튜토리얼 시작?
                        .SetYesButton(() =>
                        {
                            UIManager.Instance.PendingPopupClear();
                            SceneLoadManager.Instance.LoadScene("BattleBase");
                        }, "UI_Key_008".Locale())
                        .Build();
                };
                
                
                
                UIManager.Instance.ReservePopup(battleContinue);
            }
        }

        void TutorialLogic()
        {
            TutorialManager.Instance.tutoEnd = () => { };
            var groupID = UserDataManager.Instance.clientInfo.NextGroupID();
            if (groupID > 0)
            {
                TutorialManager.Instance.IsTutorial = true;
                Action tutorial = () => 
                {
                    TutorialManager.Instance.SetSequence(groupID, true);
                    TutorialManager.Instance.PlaySequence();
                };

                UIManager.Instance.ReservePopup(tutorial);
            }
            else
                TutorialManager.Instance.IsTutorial = false;
        }

        void SetAttendancePopup()
        {
            if (TutorialManager.Instance.IsTutorial)
            {
                TutorialManager.Instance.tutoEnd += SetAttendancePopup;
                return;
            }
                
            UserDataManager.Instance.missionInfo.AttendanceList.ForEach(u =>
            {
                if (u.type == 1 && u.is_check == false)
                {
                    Action showAttendancePopup = () => 
                    {
                        AttendancePopup attendancePopup = UIManager.Instance.Get(PopupName.Attendance) as AttendancePopup;
                        attendancePopup.SetShowPendingPopup(false);
                        attendancePopup.Show();
                    };
                    
                    UIManager.Instance.ReservePopup(showAttendancePopup);
                }
            });
        }

        void SetAutoBattleGoldPopup()
        {
            if (TutorialManager.Instance.IsTutorial)
            {
                TutorialManager.Instance.tutoEnd += SetAutoBattleGoldPopup;
                return;
            }
            
            if (UserDataManager.Instance.isShowAutoBattleReward == false)
            {
                UserDataManager.Instance.isShowAutoBattleReward = true;
                
                var timeSpan = DateTime.UtcNow - UserDataManager.Instance.stageInfo.StageData.afk_reward_at;

                if (timeSpan.TotalMinutes >= 1)
                {
                    Action showAutoBattleGoldPopup = () =>
                    {
                        var payload = new Dictionary<string, object>
                        {
                            { "type", 1 }
                        };

                        APIRepository.RequestAutoBattleRoll(payload, data =>
                        {
                            var rewardList = APIRepository.ConvertReward(data.default_rewards);
                            rewardList.AddRange(APIRepository.ConvertReward(data.currency_box_rewards));
                            rewardList.AddRange(APIRepository.ConvertReward(data.gear_box_rewards));

                            var popup = UIManager.Instance.Get(PopupName.AutoBattle_BattleAuto) as PopupBattleAuto;
                            popup.SetShowPendingPopup(false);
                            popup.SetRewardData(rewardList);
                            popup.Show();
                        });
                    };
                    
                    UIManager.Instance.ReservePopup(showAutoBattleGoldPopup);
                }
            }
        }
    }
}
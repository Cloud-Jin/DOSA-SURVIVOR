using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Best.WebSockets;
using Doozy.Runtime.UIManager.Containers;
using DTT.Utils.Extensions;
using Phoenix;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UniRx;
using ProjectM.AutoBattle;
using UnityEngine.Purchasing.MiniJSON;
using ILogger = Phoenix.ILogger;

namespace ProjectM
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        private Socket socket;
        private Channel userChannel;
        public string ServerUrl => GlobalManager.Instance.ServerUrl;
        private string lastErrorCode;

        private List<String> apiEventList = new();
        public Subject<bool> onConnect = new Subject<bool>();       // 소켓 연결성공시
        protected override void Init()
        {
            // OnConnect();
        }
        
        public IEnumerator InitNetwork()
        {
            userChannel?.Leave();
            socket?.Disconnect();
            OnConnect();
            yield return null;
        }

        void OnDestroy()
        {
            DisConnect();
            socket?.Disconnect();
            socket = null;
        }

        void DisConnect()
        {
            userChannel?.Leave();
            userChannel = null;
            initComplete = false;
        }

        bool IsConnected()
        {
            if (!userChannel.IsJoined()) return false;
            if (!socket.IsConnected()) return false;

            return true;
       }

        public void OnConnect()
        {
            // Create the WebSocket instance
            var socketOptions = new Socket.Options(new JsonMessageSerializer())
            {
                HeartbeatInterval = TimeSpan.FromMinutes(15),                // 핑 15분 -> 30분 TimeOut
                // ReconnectAfter = _ => TimeSpan.FromMilliseconds(200),
                // Logger = new BasicLogger()       // log option
            };
            
            var socketFactory = new BestHTTPWebsocketFactory();
            Debug.Log($"Entry Server {AppConfig.Instance.Server}");

            string address = ServerUrl;
            socket = new Socket(address,null, socketFactory, socketOptions);

            socket.OnOpen += OnOpenCallback;
            socket.OnError += OnErrorCallback;
            socket.OnClose += OnCloseCallback;
            socket.Connect();
        }
        

        public void JoinUserChannel(Dictionary<string, object> payload, Action<JsonBox> onResponse, Action<Reply> onError = null)
        {
            //initialize a channel with topic and parameters
            userChannel = socket.Channel("user:play", payload);
            
            Log("Request","JoinUserChannel");
            userChannel.Join()
                .Receive(ReplyStatus.Ok, rep =>
                {
                    Log("Response OK", "JoinUserChannel");
                    // 클라이언트 인포.
                    ClearLoadingPopup();
                    SetOnSubscriptionAPI();
                    onResponse?.Invoke(rep.Response.Unbox<JsonBox>());
                })
                .Receive(ReplyStatus.Error, rep =>
                {
                    ClearLoadingPopup();
                    
                    LogError("Response Error", "JoinUserChannel", rep.Response.Unbox<JsonBox>().Element.ToString());
                    LeaveChannel(null);
                    onError.Invoke(rep);
                });
        }

        public void LeaveChannel(Action StatusOk)
        {
            userChannel.Leave().Receive(ReplyStatus.Ok, reply =>
            {
                StatusOk?.Invoke();  
            })
            .Receive(ReplyStatus.Close, reply =>
            {
                StatusOk?.Invoke();
            });
        }

        // payload 없는 API 
        public void PushUserChannel<T>(string _event, Action<T> onResponse, bool requestCheck, Action<Reply> onError = null)
        {
            if (!IsConnected())
            {
                ShowErrorPopup("Network Connected fail");
                return;
            }

            if (requestCheck)
            {
                if (apiEventList.Contains(_event)) return;
                apiEventList.Add(_event);
                
                UIManager.Instance.ShowLoadingPopup(() =>
                {
                    PushUserChannelAction(_event, onResponse, requestCheck, onError);
                });
            }
            else
            {
                PushUserChannelAction(_event, onResponse, requestCheck, onError);
            }
        }

        private void PushUserChannelAction<T>(string _event, Action<T> onResponse, bool requestCheck, Action<Reply> onError = null)
        {
            Log("Request", _event);
            
            userChannel.Push(_event).Receive(ReplyStatus.Ok, rep => 
            {
                HideLoadingPopup(_event);
                Log("Response Ok", _event, rep.Response.Unbox<JObject>());
                onResponse?.Invoke(rep.Response.Unbox<T>());
            })
            .Receive(ReplyStatus.Error, rep =>
            {
                HideLoadingPopup(_event);
                
                LogError("Response Error", _event, rep.Response.Unbox<JsonBox>().Element.ToString());
                onError?.Invoke(rep);
            })
            .Receive(ReplyStatus.Timeout, reply =>
            {
                HideLoadingPopup(_event);
                
                ShowTimeOutPopup(() =>
                {
                    PushUserChannel<T>(_event, onResponse, requestCheck, onError);
                });
            });
        }
        
        public void PushUserChannel<T>(string _event, object payload, Action<T> onResponse,  bool requestCheck, Action<Reply> onError = null)
        {
            if (!IsConnected())
            {
                ShowErrorPopup("Network Connected fail");
                return;
            }
            if (requestCheck)
            {
                if (apiEventList.Contains(_event)) return;
                apiEventList.Add(_event);
                
                UIManager.Instance.ShowLoadingPopup(() =>
                {
                    PushUserChannelAction(_event, payload, onResponse, requestCheck, onError);
                });
            }
            else
            {
                PushUserChannelAction(_event, payload, onResponse, requestCheck, onError);
            }
        }

        private void PushUserChannelAction<T>(string _event, object payload, Action<T> onResponse, bool requestCheck, Action<Reply> onError = null)
        {
            Log("Request", _event);
            var _payload = payload as Dictionary<string, object>;
            Debug.Log($"{_event} params {_payload.toJson()}");
            
            userChannel.Push(_event, payload).Receive(ReplyStatus.Ok, rep =>
            {
                HideLoadingPopup(_event);

                Log("Response Ok", _event, rep.Response.Unbox<JObject>());
                onResponse?.Invoke(rep.Response.Unbox<T>());
            })
            .Receive(ReplyStatus.Error, rep =>
            {
                HideLoadingPopup(_event);
                
                LogError("Response Error", _event, rep.Response.Unbox<JsonBox>().Element.ToString());
                onError?.Invoke(rep);
            })
            .Receive(ReplyStatus.Timeout, reply =>
            {
                HideLoadingPopup(_event);
                
                ShowTimeOutPopup(() =>
                {
                    PushUserChannel<T>(_event, payload, onResponse, requestCheck, onError);
                });
            });
        }
        
        public void PushUserChannel<T>(string _event, object payload, Action<T> onResponse, Action<string> onError, bool requestCheck)
        {
            if (!IsConnected())
            {
                ShowErrorPopup("Network Connected fail");
                return;
            }
            
            if (requestCheck)
            {
                if (apiEventList.Contains(_event)) return;
                apiEventList.Add(_event);
                
                UIManager.Instance.ShowLoadingPopup(() =>
                {
                    PushUserChannelAction(_event, payload, onResponse, onError, requestCheck);
                });
            }
            else
            {
                PushUserChannelAction(_event, payload, onResponse, onError, requestCheck);
            }
        }

        private void PushUserChannelAction<T>(string _event, object payload, Action<T> onResponse, Action<string> onError, bool requestCheck)
        {
            Log("Request", _event);
            var _payload = payload as Dictionary<string, object>;
            Debug.Log($"{_event} params {_payload.toJson()}");
            
            userChannel.Push(_event, payload).Receive(ReplyStatus.Ok, rep => 
            { 
                HideLoadingPopup(_event);

                Log("Response Ok", _event, rep.Response.Unbox<JObject>());
                onResponse?.Invoke(rep.Response.Unbox<T>());
            })
            .Receive(ReplyStatus.Error, rep =>
            {
                HideLoadingPopup(_event);
            
                LogError("Response Error", _event, rep.Response.Unbox<JsonBox>().Element.ToString());
                var errorKey = rep.Response.Unbox<JsonBox>().Element["reason"]?.ToString();
                onError?.Invoke(errorKey);
                // onError?.Invoke(rep);
            })
            .Receive(ReplyStatus.Timeout, reply =>
            {
                HideLoadingPopup(_event);
                
                ShowTimeOutPopup(() =>
                {
                    PushUserChannel<T>(_event, payload, onResponse, onError, requestCheck);
                });
            });
        }
        
        public void SubscriptionUserChannel<T>(string _event, Action<T> onResponse)
        {
            userChannel.Off(_event);
            
            Log("Subscription", _event);
            userChannel.On(_event, rep =>
            {
                Log("Response", _event, rep.Payload.Unbox<JObject>());
                onResponse.Invoke(rep.Payload.Unbox<T>());
            });
        }

        public void DisposeEvent(string _event)
        {
            userChannel.Off(_event);
        }
        
        #region WebSocket Event Handlers
        
        public sealed class BasicLogger : ILogger
        {
            public void Log(LogLevel level, string source, string message)
            {
                Debug.LogFormat("[{0}]: {1} - {2}", level, source, message);
            }
        }
        void OnOpenCallback()
        {
            initComplete = true;
            onConnect.OnNext(true);
            Debug.Log("소켓 연결완료");
        }

        void OnErrorCallback(string message)
        {
            ClearLoadingPopup();
                
            Debug.Log($"Error = {message}");
            ShowErrorPopup(message);
        }

        void OnCloseCallback(WebSocketStatusCodes code, string message)
        {
            ClearLoadingPopup();
            return;
            
            Debug.Log($"Close = {code} : {message}");
            if (message == "Connection closed unexpectedly!")
                return;
            
            ShowErrorPopup(message);
        }
        void OnMessageCallback(Message message)
        {
            var payload = message.Payload.Unbox<JObject>();
            Debug.LogFormat("{0} - {1}: {2}", message.Topic, message.Event, payload);
        }

        void Log(string prefix, string eventName, JObject json=null)
        {
            Debug.Log($"<color=#00FF22>{prefix} {eventName} </color>{json}");

        }
        
        void LogError(string prefix, string eventName, string msg)
        {
            Debug.Log($"<color=#FF3954>{prefix} {eventName} </color>{msg}");

        }
        #endregion

        void ShowErrorPopup(string errorKey)
        { 
            if (lastErrorCode == errorKey)// || string.IsNullOrEmpty(errorKey))
            {
                return;
            }
            
            Debug.Log($"Error = {errorKey}");
            lastErrorCode = errorKey;
            DisConnect();
            var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.uiPopup.canvas.sortingLayerName = "UI";
                popup.uiPopup.SetOverrideSorting(true, false);

                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage("Server_Disconnected_Msg_2".Locale())
                    .SetYesButton(() =>
                    {
                        lastErrorCode = String.Empty;
                        SceneLoadManager.Instance.LoadScene("Start");
                    }, "Restart_Btn".Locale())
                    .Build();
            }
        }
        
        void ShowTimeOutPopup(Action retry)
        {
            var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.uiPopup.canvas.sortingLayerName = "UI";
                popup.uiPopup.SetOverrideSorting(true, false);

                popup.InitBuilder()
                    .SetTitle("UI_Key_012".Locale())
                    .SetMessage("No_response_server".Locale())
                    .SetYesButton(() =>
                    {
                        retry?.Invoke();
                    }, "Retry".Locale())
                    .SetNoButton(() =>
                    {
                        DisConnect();
                        SceneLoadManager.Instance.LoadScene("Start");
                    }, "Restart_Btn".Locale())
                    .Build();
            }
        }

        private void HideLoadingPopup(string eventKey)
        {
            if (eventKey.IsNullOrEmpty() == false)
                apiEventList.Remove(eventKey);
            
            if (apiEventList.IsNullOrEmpty())
                UIManager.Instance.HideLoadingPopup();
        }

        private void ClearLoadingPopup()
        {
            apiEventList.Clear();
            UIManager.Instance.HideLoadingPopup();
        }
        
        private void SetOnSubscriptionAPI()
        {
            if (TimerManager.Instance) TimerManager.Instance.InitTimerManager();
            
            SubscriptionPostPoxes();
            SubscriptionMemberships();
            SubscriptionOrderHistories();
            SubscriptionMission();
            SubscriptionAchievement();
            SubscriptionAttendance();
            SubscriptionAdBuff();
            SubscriptionStage();
            SubscriptionDownTime();
            SubscriptionDisconnect();
            SubscriptionClientInfo();
            SubscriptionTrait();
            SubscriptionPortrait();
            SubscriptionGuardian();
            SubscriptionExploration();
        }
        
        private void SubscriptionPostPoxes()
        {
            if (UserDataManager.Instance.postBoxInfo.PostBoxes.IsNullOrEmpty() == false)
                UserDataManager.Instance.postBoxInfo.PostBoxes.Clear();
            
            Action<CommonApiResultData> resp = data => {
                APIRepository.SubscriptionPostPoxes(data);
                Debug.Log("On PostPoxes");
            };

            SubscriptionUserChannel("post_boxes", resp);
        }

        private void SubscriptionOrderHistories()
        {
            if (UserDataManager.Instance.payInfo.OrderHistories.IsNullOrEmpty() == false)
                UserDataManager.Instance.payInfo.OrderHistories.Clear();
            
            Action<CommonApiResultData> resp = data =>
            {
                APIRepository.SubscriptionOrderHistories(data);
                Debug.Log("On OrderHistories");
            };
            
            SubscriptionUserChannel("order_histories", resp);
        }

        private void SubscriptionMemberships()
        {
            if (UserDataManager.Instance.payInfo.Memberships.IsNullOrEmpty() == false)
                UserDataManager.Instance.payInfo.Memberships.Clear();
            
            Action<CommonApiResultData> resp = data =>
            {
                data.SetData();
                
                APIRepository.SubscriptionMemberships();
                Debug.Log("On Memberships");
            };
            
            SubscriptionUserChannel("memberships", resp);
        }
        
        private void SubscriptionMission()
        {
            if (UserDataManager.Instance.missionInfo.MissionDataList.IsNullOrEmpty() == false)
                UserDataManager.Instance.missionInfo.MissionDataList.Clear();
            
            Action<CommonApiResultData> resp = data =>
            {
                APIRepository.SubscriptionMission(data);
                Debug.Log("On SubscriptionMission");
            };
            
            SubscriptionUserChannel("missions", resp);
        }

        private void SubscriptionAchievement()
        {
            if (UserDataManager.Instance.missionInfo.AchievementInfoList.IsNullOrEmpty() == false)
                UserDataManager.Instance.missionInfo.AchievementInfoList.Clear();
            
            Action<AchievementData> resp = data =>
            {
                APIRepository.SubscriptionAchievement(data);
                Debug.Log("On SubscriptionAchievement");
            };
            
            SubscriptionUserChannel("achievements", resp);
        }

        private void SubscriptionAttendance()
        {
            if (UserDataManager.Instance.missionInfo.AttendanceList.IsNullOrEmpty() == false)
                UserDataManager.Instance.missionInfo.AttendanceList.Clear();
            
            Action<AttendanceData> resp = data =>
            {
                APIRepository.SubscriptionAttendance(data);
                Debug.Log("On SubscriptionAttendance");
            };
            
            SubscriptionUserChannel("attendances", resp);
        }

        public void SubscriptionAdBuff()
        {
            if (UserDataManager.Instance.payInfo.AdBuffs.IsNullOrEmpty() == false)
                UserDataManager.Instance.payInfo.AdBuffs.Clear();
            
            Action<AdBuffData> resp = data =>
            {
                if (data.ad_Buff != null)
                    UserDataManager.Instance.payInfo.AddADBuff(data.ad_Buff);
                
                if (data.ad_buffs.IsNullOrEmpty() == false)
                    UserDataManager.Instance.payInfo.AddADBuff(data.ad_buffs);

                APIRepository.SubscriptionAdBuff();
                Debug.Log("On SubscriptionAdBuff");
            };
            
            SubscriptionUserChannel("ad_buffs", resp);
        }

        private void SubscriptionStage()
        {
            Action<JObject> resp = data =>
            {
                if (data["stages"].Any())
                {
                    UserDataManager.Instance.stageInfo.Data = data["stages"].ToObject<List<StageData>>();
                }

                Debug.Log("On SubscriptionStage");
            };
            
            SubscriptionUserChannel("stages", resp);
        }
        
        // 점검
        private void SubscriptionDownTime()
        {
            Action<JObject> resp = data => {
              
                var obj = data["down_time"];
                // down_type 점검 타입 정기, 임시
                if (string.IsNullOrEmpty(obj["down_type"].ToString()))
                {
                    return;
                }

                string sat = String.Empty;
                string eat = String.Empty;
                TimeSpan startCap = TimeSpan.Zero;
                TimeSpan endCap = TimeSpan.Zero;
                
                if (DateTime.TryParse(obj["start_at"].ToString(), out DateTime _sat))
                {
                    startCap = DateTime.UtcNow - _sat;
                    sat = _sat.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                }
                
                if (DateTime.TryParse(obj["end_at"].ToString(), out DateTime _eat))
                {
                    endCap = DateTime.UtcNow - _eat;
                    eat = _eat.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                }
                

                if (startCap.TotalSeconds < 0 )
                {
                    // 점검 n분전 Server_Maintenance_Toast_1
                    int c = startCap.TotalMinutes < -1 ? 1 : 2; 
                    string msg = $"Server_Maintenance_Toast_{c}";
                    var time = c == 1 ? $"{-startCap.TotalMinutes:N0}" : $"{-startCap.TotalSeconds:N0}";
                    var popup = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    popup.InitBuilder()
                        .SetMessage(msg.Locale(sat,eat,time))
                        .SetDuration(5f)
                        .Build();
                }
            };

            SubscriptionUserChannel("down_time", resp);
        }
        
        // 연결종료 from 서버
        private void SubscriptionDisconnect()
        {
            Action<JObject> resp = data =>
            {
                var errorKey = data["reason"].ToString();
                ShowErrorPopup(errorKey);
            };

            SubscriptionUserChannel("disconnect", resp);
        }
        
        private void SubscriptionClientInfo()
        {
            // 최초접속때 한번만 줌.
            
            Action<JObject> resp = data => {
              
                var obj = data["client_infos"];

                if (obj.IsNullOrEmpty())
                    Debug.Log("비어있음");
                else
                {
                    var parse = JObject.Parse(obj[0]["data"].ToString()).ToObject<UserDataManager.ClientInfo>();
                    UserDataManager.Instance.clientInfo.LinkTutorialData(parse);
                }
                    
            };

            SubscriptionUserChannel("client_infos", resp);
        }
        
        private void SubscriptionTrait()
        {
            // return;
            
            Action<TraitInfoData> resp = data =>
            {
                UserDataManager.Instance.userTraitInfo.SetData(data);
                UserDataManager.Instance.userTraitInfo.apMax.Value = data.trait_point_max;
                if(data.trait_point_use != 0)
                    UserDataManager.Instance.userTraitInfo.apUse.Value = data.trait_point_use;
                // if (data.traits.IsNullOrEmpty() == false) UserDataManager.Instance.userTraitInfo.SetData(data.traits);
                Debug.Log("On SubscriptionTrait");
            };

            SubscriptionUserChannel("traits", resp);
        }
        
        private void SubscriptionPortrait()
        {
            if (UserDataManager.Instance.userProfileInfo.portraitList.IsNullOrEmpty() == false)
                UserDataManager.Instance.userProfileInfo.portraitList.Clear();
            
            Action<CommonApiResultData> resp = data =>
            {
                data.SetData();
                APIRepository.SubscriptionPortraits();
            };
            
            SubscriptionUserChannel("portraits", resp);
        }
        
        public void SubscriptionGuardian()
        {
            if (UserDataManager.Instance.gearInfo.GuardianList.IsNullOrEmpty() == false)
                UserDataManager.Instance.gearInfo.GuardianList.Clear();
            
            Action<GuardianData> resp = data =>
            {
                data.SetData();
                
                GuardianPopup guardianPopup = UIManager.Instance.GetPopup(PopupName.Guardian) as GuardianPopup;
                if (guardianPopup) guardianPopup.SetData();
                
                UserDataManager.Instance.gearInfo.SetGuardianRedDot();

                if (ContainerInventory.Instance)
                    ContainerInventory.Instance.playerStateInfo.SetPowerInfo();
                
                if (LobbyMain.Instance)
                    AutoBattleManager.Instance.DataUpdate();

                Debug.Log("On SubscriptionGuardian");
            };
            
            SubscriptionUserChannel("guardians", resp);
        }
        
        public void SubscriptionExploration()
        {
            UserDataManager.Instance.gearInfo.explorationData = null;

            Action<ExplorationData> resp = data =>
            {
                data.SetData();
                
                GuardianPopup guardianPopup = UIManager.Instance.GetPopup(PopupName.Guardian) as GuardianPopup;
                if (guardianPopup) guardianPopup.SetData();
                
                Debug.Log("On SubscriptionExploration");
            };
                
            SubscriptionUserChannel("explore", resp);
        }
    }
}

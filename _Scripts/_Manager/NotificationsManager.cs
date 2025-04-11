using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Notifications;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

// 노티 메시지 관리

namespace ProjectM
{
    public class NotificationsManager : Singleton<NotificationsManager>
    {
        private bool _isAlarmAccept = true;               // 푸시권한
        //private bool _isNightAlarmAccept = true;        // 야간푸시 권한
        // 야간푸시
        private int nightStartHour = 21;
        private int nightEndHour = 8;
        private int apiLevel;
        private int platform;                             // aos 1   ios 2
        string CHANNEL_ID = "myChannel";
        protected override void Init()
        {
            // 시스템 초기화
            _isAlarmAccept = OptionSettingManager.Instance.AlarmAccept;
            
            var args = NotificationCenterArgs.Default;
            args.AndroidChannelId = "notifications";
            args.AndroidChannelName = "Notifications";
            args.AndroidChannelDescription = "Game notifications";
            
            NotificationCenter.Initialize(args);
        }

        private void Start()
        {
            platform = GlobalManager.Instance._platform;
            if(platform == 1)
                InitializeAndroidLocalPush();
        }

        public IEnumerator InitNotification()
        {
            var request = NotificationCenter.RequestPermission();
            if (request.Status == NotificationsPermissionStatus.RequestPending)
                yield return request;
            Debug.Log("Permission result: " + request.Status);
            initComplete = true;
        }
        
        public void InitializeAndroidLocalPush()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
              // 디바이스의 안드로이드 api level 얻기
            string androidInfo = SystemInfo.operatingSystem;
            Debug.Log("androidInfo: " + androidInfo);
            apiLevel = int.Parse(androidInfo.Substring(androidInfo.IndexOf("-") + 1, 2));
            Debug.Log("apiLevel: " + apiLevel);

            // 디바이스의 api level이 33 이상이라면 퍼미션 요청
            if (apiLevel >= 33 &&
                !Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }

            // 디바이스의 api level이 26 이상이라면 알림 채널 설정
            if (apiLevel >= 26)
            {
                var channel = new AndroidNotificationChannel()
                {
                    Id = CHANNEL_ID,
                    Name = "pubSdk",
                    Importance = Importance.High, // 아래 참고
                    Description = "for test",
                };
                AndroidNotificationCenter.RegisterNotificationChannel(channel);
            }
#endif
        }

        
        public void SendNotification(string title, string text, DateTime deliveryTime, int index)
        {
            // 알람푸쉬 권한 체크
            if (!_isAlarmAccept)
                return;
        
            // 야간푸쉬 권한 체크
            // if(!_isNightAlarmAccept)
            // {
            //     bool isNight = CheckNightTime(deliveryTime);
            //     if (isNight)
            //         return;
            // }

            var time = deliveryTime.ToLocalTime();
            Debug.LogFormat($" {index} Send Push {time}");
            var remainTime = time - DateTime.Now;
            if (remainTime.TotalSeconds < 0)
            {
                Debug.Log("지나간 시간");
                return;
            }
            if (platform == 1)
            {
#if UNITY_ANDROID
                var _notification = new AndroidNotification();
                _notification.Title = title;
                _notification.Text = text;
                _notification.FireTime = deliveryTime;
                _notification.ShowInForeground = false;
                _notification.ShowTimestamp = true;
                // 디바이스의 api level이 26 이상이라면 알림 표시
                if (apiLevel >= 26)
                {

                    AndroidNotificationCenter.SendNotificationWithExplicitID(_notification, CHANNEL_ID, index);

                }
#endif
            }
            else
            {
                var notification = new Notification()
                {
                    Title = title,
                    Text = text,
                    ShowInForeground = false,
                    Identifier = index,
                };
            
                NotificationCenter.ScheduleNotification(notification, new NotificationDateTimeSchedule(deliveryTime));
                // NotificationCenter.ScheduleNotification(notification, new NotificationIntervalSchedule(TimeSpan.FromHours(3)));    
            }
        }

        public void CancelAllNotifications()
        {
            if (platform == 1)
            {
#if UNITY_ANDROID
                AndroidNotificationCenter.CancelAllNotifications();
                AndroidNotificationCenter.CancelAllScheduledNotifications();
#endif
                    
            }
            else
            {
                NotificationCenter.CancelAllScheduledNotifications();
                NotificationCenter.CancelAllDeliveredNotifications();
            }
        }
        
        public void CancelNotifications(int id)
        {
            if (platform == 1)
            {
#if UNITY_ANDROID
                AndroidNotificationCenter.CancelNotification(id);
                AndroidNotificationCenter.CancelScheduledNotification(id);
#endif
                
            }
            else
            {
                NotificationCenter.CancelScheduledNotification(id);
                NotificationCenter.CancelDeliveredNotification(id);
            }
        }
        
        private bool CheckNightTime(DateTime checkTime)
        {
            bool isNight = false;

            int hour = int.Parse(checkTime.ToString("HH"));

            if (hour < nightEndHour || hour >= nightStartHour)
                isNight = true;

            return isNight;
        }

        public void NotificationsRegister()
        {
            var pushInfos = TableDataManager.Instance.data.PushInfo;

            foreach (var pushInfo in pushInfos)
            {
                SetNotificationType(pushInfo);
            }
        }

        public void SetNotificationType(int idx)
        {
            var pushInfo = TableDataManager.Instance.data.PushInfo.Single(t => t.Index == idx);
            SetNotificationType(pushInfo);
        }

        void SetNotificationType(PushInfo info)
        {
            string _title = info.PushTitle.Locale();
            string _desc = info.PushDescription.Locale();
            DateTime deliveryTime = DateTime.MinValue;
            CancelNotifications(info.Index);        // 기존메시지 제거
            
            switch (info.Type)
            {
                case 1:
                {
                    // 자동전투 누적보상
                   deliveryTime = UserDataManager.Instance.stageInfo.StageData.afk_reward_at.ToLocalTime().AddSeconds(info.TypeValue);
                }
                    break;
                case 2:
                    // 에너지 자동 충전
                    var userData = UserDataManager.Instance;
                    var _maxEnergy = info.TypeValue + UserDataManager.Instance.payInfo.IncreaseMaxEnergy;
                    //var payinfo = userData.payInfo.Memberships.SingleOrDefault(t => t.package_id == 14); // 월정액
                    userData.currencyInfo.data.TryGetValue(CurrencyType.Energy, out var energy);
                    int count = _maxEnergy - (int)energy.Value; //count * 10분 예약
                    if (count > 0)
                    {
                        var rechargeGap = TimeSpan.FromSeconds(TableDataManager.Instance.GetConfig(21).Value);
                        var rechargeSec = rechargeGap.TotalSeconds * (count -1);
                        var timeSpan = DateTime.UtcNow - UserDataManager.Instance.userInfo.PlayerData.player.energy_at;
                        if (timeSpan.TotalMinutes > 10)
                        {
                            timeSpan = timeSpan - TimeSpan.FromMinutes((int)timeSpan.TotalMinutes - (int)timeSpan.TotalMinutes % 10);
                        }
                        rechargeSec += (rechargeGap - timeSpan).TotalSeconds;
                        deliveryTime = DateTime.UtcNow.AddSeconds(rechargeSec);
                    }
                    break;
                default:
                    break;
            }
            
            SendNotification(_title, _desc, deliveryTime, info.Index);
        }
    }
}

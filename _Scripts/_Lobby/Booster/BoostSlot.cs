using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Reactor;
using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Timer = TimerTool.Timer;

namespace ProjectM
{
    public class BoostSlot : MonoBehaviour
    {
        public TMP_Text boostNameTxt;
        public TMP_Text boostLevelTxt;
        public TMP_Text boostEffectTxt;
        public TMP_Text boostCountTxt;
        public TMP_Text boostTimerTxt;
        [Space(20)]
        public Progressor levelProgress;
        [Space(20)]
        public UIButton adBtn;
        [Space(20)]
        public Image boostIconImg;

        private ADBuffType _adBuffType;
        private AdBuff _adBuff;
        
        private List<ADBuffInfo> _adBuffInfoList;

        private Coroutine _adBuffExpiry;

        private void Start()
        {
            adBtn.AddEvent(OnShowAD);
        }

        private void OnShowAD()
        {
            AdMobManager.Instance.ShowAD(() =>
            {
                var payload = new Dictionary<string, object>
                {
                    { "type", _adBuffType.Index }
                };

                APIRepository.RequestAdBuff(payload, data =>
                {
                    var popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
                    popup.InitBuilder()
                        .SetTitle("UI_Key_012".Locale())
                        .SetMessage("Ad_Reward_Buff_Desc".Locale())
                        .SetYesButton(popup.Hide, "Common_Ok_Btn".Locale())
                        .Build();
                    
                }, reply =>
                {
                    Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                    alarm.InitBuilder()
                        .SetMessage(LocaleManager.GetLocale("No_response_server"))
                        .Build();
                });
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

        public void SetData(ADBuffType adBuffType)
        {
            _adBuffType = adBuffType;

            _adBuffInfoList = TableDataManager.Instance.data.ADBuffInfo
                .Where(t => t.BuffID == _adBuffType.Index).OrderBy(t => t.BuffLevel).ToList();

            int maxLevel = _adBuffInfoList.Select(a => a.BuffLevel).Max();
            
            boostNameTxt.SetText(LocaleManager.GetLocale(_adBuffType.ADBuffName));
            boostIconImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods)
                .GetSprite(_adBuffType.ADBuffIcon);

            _adBuff = UserDataManager.Instance.payInfo.AdBuffs
                .SingleOrDefault(u => u.type == _adBuffType.Index);

            int currentLevel = _adBuff?.level ?? 1;
            
            ADBuffInfo adBuffInfo = _adBuffInfoList.Single(a => a.BuffLevel == currentLevel);
            OptionType optionType = TableDataManager.Instance.data.OptionType
                .Single(t => t.AddOptionType == _adBuffType.OptionType);
            
            boostLevelTxt.SetText(LocaleManager.GetLocale("Common_Level", currentLevel));
            
            switch (optionType.AddOptionType)
            {
                case 2:
                case 4:
                case 11:
                case 13:
                {
                    InfVal value = adBuffInfo.OptionValue;
                    boostEffectTxt.SetText(LocaleManager.GetLocale(optionType.AddOptionValueName, value.ToParseString()));
                    break;
                }
                case 5:
                case 6:
                case 14:
                {
                    if (0 < adBuffInfo.OptionValue % 100)
                    {
                        float addOptionValue =
                            MathF.Round(adBuffInfo.OptionValue / 100f, 1, MidpointRounding.AwayFromZero);
                        boostEffectTxt.SetText(LocaleManager.GetLocale(optionType.AddOptionValueName, addOptionValue));
                    }
                    else
                    {
                        int addOptionValue = adBuffInfo.OptionValue / 100;
                        boostEffectTxt.SetText(LocaleManager.GetLocale(optionType.AddOptionValueName, addOptionValue));
                    }

                    break;
                }
                default:
                {
                    int addOptionValue = Mathf.RoundToInt(adBuffInfo.OptionValue / 100f);
                    boostEffectTxt.SetText(LocaleManager.GetLocale(optionType.AddOptionValueName, addOptionValue));
                    break;
                }
            }

            if (maxLevel <= currentLevel)
            {
                boostCountTxt.SetText(LocaleManager.GetLocale(
                    "Common_Count", adBuffInfo.NeedLevelCount, adBuffInfo.NeedLevelCount));
                levelProgress.SetProgressAt(1);
            }
            else
            {
                boostCountTxt.SetText(LocaleManager.GetLocale(
                    "Common_Count", _adBuff?.count ?? 0, adBuffInfo.NeedLevelCount));
                levelProgress.SetProgressAt((_adBuff?.count ?? 0) / (float)adBuffInfo.NeedLevelCount);
            }

            Timer timer = TimerManager.Instance.GetTimer(_adBuff?.timer_id ?? 0);

            if (timer != null && timer.IsFinished == false)
            {
                adBtn.SetActive(false);
                boostTimerTxt.SetActive(true);

                if (_adBuffExpiry != null)
                    StopCoroutine(_adBuffExpiry);

                _adBuffExpiry = StartCoroutine(AdBuffExpiryTimer());
            }
            else
            {
                adBtn.SetActive(true);
                boostTimerTxt.SetActive(false);
            }
        }
        
        private IEnumerator AdBuffExpiryTimer()
        {
            Timer timer;
            
            do
            {
                timer = TimerManager.Instance.GetTimer(_adBuff.timer_id);

                if (timer != null)
                {
                    TimeSpan remainTime = TimeSpan.FromSeconds(timer.GetTimeRemaining());

                    if (1 <= remainTime.Days)
                    {
                        boostTimerTxt.SetText($"{remainTime.Days:D2}D " +
                                              $"{remainTime.Hours:D2}:{remainTime.Minutes:D2}");
                    }
                    else if (0 < remainTime.Hours)
                    {
                        boostTimerTxt.SetText($"{remainTime.Hours:D2}:" +
                                              $"{remainTime.Minutes:D2}:" + $"{remainTime.Seconds:D2}");
                    }
                    else
                    {
                        boostTimerTxt.SetText($"{remainTime.Minutes:D2}:" + $"{remainTime.Seconds:D2}");
                    }
                }

                yield return null;
                
            } while (timer != null);
            
            StopCoroutine(_adBuffExpiry);
            
            SetData(_adBuffType);
        }
    }
}
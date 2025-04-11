using I2.Loc;
using UniRx;
using UnityEngine;

namespace ProjectM
{
    public class OptionSettingManager : Singleton<OptionSettingManager>
    {
        public ReactiveProperty<string> curLangCode = new();
        
        protected override void Init()
        {
            initComplete = true;
        }
        
        public void SetBgmVolume(float value)
        {
            SoundManager.Instance.SetBgmVolume(value);
            
            if (value <= 0.0f)
                PlayerPrefs.SetInt("BgmMute", 1);
            else
                PlayerPrefs.SetInt("BgmMute", 0);
            
            PlayerPrefs.SetFloat("BgmVolume", value);
            PlayerPrefs.Save();
        }

        public float GetBgmVolume()
        {
            return PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        }

        public bool GetBgmMute()
        {
            int value = PlayerPrefs.GetInt("BgmMute", 0);
            return value == 1;
        }

        public void SetEffectVolume(float value)
        {
            SoundManager.Instance.SetEffectVolume(value);
            
            if (value <= 0.0f)
                PlayerPrefs.SetInt("EffectMute", 1);
            else
                PlayerPrefs.SetInt("EffectMute", 0);

            PlayerPrefs.SetFloat("EffectVolume", value);
            PlayerPrefs.Save();
        }
        
        public float GetEffectVolume()
        {
            return PlayerPrefs.GetFloat("EffectVolume", 1.0f);
        }

        public bool GetEffectMute()
        {
            int value = PlayerPrefs.GetInt("EffectMute", 0);
            return value == 1;
        }
        
        public void SetVibration(int state)
        {
            if (state == 1)
                Vibration.VibratePop();

            PlayerPrefs.SetInt("VibrationState", state);
            PlayerPrefs.Save();
        }
        
        public bool GetVibration()
        {
            int value = PlayerPrefs.GetInt("VibrationState", 1);

            if (value == 1)
                return true;

            return false;
        }
        
        public void SetLang(string langCode)
        {
            if (LocalizationManager.HasLanguage(langCode))
            {
                LocalizationManager.CurrentLanguage = langCode;
                curLangCode.Value = langCode;
            }
        }

        public void SetSoundMute(int state)
        {
            SoundManager.Instance.SetSoundMute(state);
            
            PlayerPrefs.SetInt("BgmMute", state);
            PlayerPrefs.SetInt("EffectMute", state);
            PlayerPrefs.Save();
        }

        public bool AlarmAccept
        {
            get
            {
                var value = PlayerPrefs.GetInt(MyPlayerPrefsKey.AlarmAccept, 1);
                return value == 1;
            }

            set
            {
                PlayerPrefs.SetInt(MyPlayerPrefsKey.AlarmAccept, (value) ? 1 : 0);
                if (value)
                {
                    NotificationsManager.Instance.NotificationsRegister();  // 등록
                }
                else
                {
                    NotificationsManager.Instance.CancelAllNotifications(); // 제거
                }
            }
        }
    }
}

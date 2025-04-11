using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using DarkTonic.MasterAudio;

namespace ProjectM
{
    public class SoundManager : Singleton<SoundManager>
    {
        protected override void Init()
        {
            if (OptionSettingManager.Instance.GetBgmMute())
                PersistentAudioSettings.MusicMuted = true;
            else
                PersistentAudioSettings.MusicVolume = OptionSettingManager.Instance.GetBgmVolume();

            if (OptionSettingManager.Instance.GetEffectMute())
                PersistentAudioSettings.MixerMuted = true;
            else
                PersistentAudioSettings.MixerVolume = OptionSettingManager.Instance.GetEffectVolume();

            initComplete = true;
        }

        public void PlayFXUIButton()
        {
            MasterAudio.PlaySoundAndForget("Select");
        }
        
        public void PlayFX(string name)
        {
            MasterAudio.PlaySoundAndForget(name);
        }

        public void PlayBGM(string name)
        {
            if (MasterAudio.OnlyPlaylistController != null)
            {
                if (MasterAudio.OnlyPlaylistController.PlaylistName.Equals(name))
                    return;
            }

            MasterAudio.StartPlaylist(name);
        }
        
        public void SetBgmVolume(float bgmVolumeValue)
        {
            PersistentAudioSettings.MusicVolume = bgmVolumeValue;

            if (bgmVolumeValue <= 0.0f)
                PersistentAudioSettings.MusicMuted = true;
            else
                PersistentAudioSettings.MusicMuted = false;
        }

        public void SetEffectVolume(float effectVolumeValue)
        {
            PersistentAudioSettings.MixerVolume = effectVolumeValue;
            
            if (effectVolumeValue <= 0.0f)
                PersistentAudioSettings.MixerMuted = true;
            else
                PersistentAudioSettings.MixerMuted = false;
        }

        public float GetBgmVolume()
        {
            return PersistentAudioSettings.MusicVolume ?? 1;
        }
        
        public float GetEffectVolume()
        {
            return PersistentAudioSettings.MixerVolume ?? 1;
        }

        public void SetSoundMute(int state)
        {
            if (state == 1)
            {
                PersistentAudioSettings.MusicMuted = true;
                PersistentAudioSettings.MixerMuted = true;
            }
            else
            {
                PersistentAudioSettings.MusicMuted = false;
                PersistentAudioSettings.MixerMuted = false;
            }
        }
    }
}
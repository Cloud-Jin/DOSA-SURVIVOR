using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM
{
    public class VibrationManager : Singleton<VibrationManager>
    {
        protected override void Init()
        {
            Vibration.Init();
            
            initComplete = true;
        }

        public void VibratePop()
        {
            if (OptionSettingManager.Instance.GetVibration())
                Vibration.VibratePop();
        }
    }
}

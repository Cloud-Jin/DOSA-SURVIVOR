using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UniRx;
using UnityEngine;

namespace ProjectM
{
    public class LocaleManager : Singleton<LocaleManager>
    {
        public ReactiveProperty<int> onUpdate;
        protected override void Init()
        {
            onUpdate = new ReactiveProperty<int>();
            initComplete = true;
        }

        private void Start()
        {
            LocalizationManager.OnLocalizeEvent += OnLocalize;
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLocalizeEvent -= OnLocalize;
        }

        void OnLocalize()
        {
            onUpdate.SetValueAndForceNotify(1);
        }

        public static string GetLocale(string key)
        {
            bool v = LocalizationManager.TryGetTranslation(key, out var locale);
            if (!v)
            {
                Debug.LogWarning($"Error Locale key : {key}");
                return key;
            }
            locale = LocalizationManager.GetTranslation(key) ?? throw new ArgumentNullException("I2.Loc.LocalizationManager.GetTranslation(key)");
            return locale;
        }
        
        public static string GetLocale(string key, params object[] arg)
        {
            string locale = LocalizationManager.GetTranslation(key) ?? throw new ArgumentNullException("I2.Loc.LocalizationManager.GetTranslation(key)");
            var retLocale = string.Format(locale, arg);
            return retLocale;
        }
        
    }

}
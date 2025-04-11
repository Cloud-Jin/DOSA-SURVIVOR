using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using I2.Loc;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace ProjectM
{
    public class LangSelectPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Lang_Select;
        [Space(20)]
        public UIButton[] langBtns;
        [Space(20)]
        public TMP_Text titleTxt;

        private Action _changeLang;

        protected override void Init()
        {
            
        }

        private void Start()
        {
            titleTxt.SetText(LocaleManager.GetLocale("Language_Title"));

            langBtns.ForEach(d => d.SetActive(false));
            
            List<string> allLang = LocalizationManager.GetAllLanguages();
            
            for (int i = 0; i < allLang.Count; ++i)
            {
                langBtns[i].SetActive(true);
                langBtns[i].transform.Find("Label").GetComponent<TMP_Text>()
                    .SetText(LocaleManager.GetLocale($"Language_{allLang[i]}"));

                int index = i;
                langBtns[i].AddEvent(() => OnChangeLangauge(allLang[index]));
            }
        }

        public void SetData(Action setText)
        {
            _changeLang = setText;
        }

        private void OnChangeLangauge(string langCode)
        {
            OptionSettingManager.Instance.SetLang(langCode);
            _changeLang.Invoke();
            Hide();
        }
    }
}

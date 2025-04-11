using System;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class ProfilePortraitSlot : MonoBehaviour
    {
        public Image iconImg;
        [Space(20)]
        public Transform selectEffect;
        public Transform redDot;
        public Transform noHaveLock;
        public Transform equip;
        [Space(20)]
        public UIButton slotBtn;

        private Portrait _portrait;
        private Portrait _selectPortrait;
        private Action<Portrait> _selectAction;

        private void Start()
        {
            slotBtn.AddEvent(OnSelect);
        }

        public void SetData(Portrait portrait, Portrait selectPortrait, Action<Portrait> selectAction)
        {
            _portrait = portrait;
            _selectPortrait = selectPortrait;
            _selectAction = selectAction;
            
            equip.SetActive(false);
            selectEffect.SetActive(false);
            noHaveLock.SetActive(false);
            
            iconImg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(portrait.Icon);

            int equipPortraitId = UserDataManager.Instance.userInfo.PlayerData.player.portrait_id;
            equip.SetActive(equipPortraitId == portrait.Index);
            
            bool isHave = UserDataManager.Instance.userProfileInfo.portraitList.All(u => u != portrait.Index);
            noHaveLock.SetActive(isHave);

            if (_selectPortrait != null)
                selectEffect.SetActive(portrait.Index == _selectPortrait.Index);
            else
                selectEffect.SetActive(equipPortraitId == portrait.Index);

            SetPortraitRedDot();
        }

        private void SetPortraitRedDot()
        {
            redDot.SetActive(false);
            
            if (PlayerPrefs.GetInt(_portrait.Icon) == 1)
                redDot.SetActive(true);
        }

        private void OnSelect()
        {
            PlayerPrefs.SetInt(_portrait.Icon, 2);
            PlayerPrefs.Save();
            
            redDot.SetActive(false);
            
            if (_portrait.Equals(_selectPortrait)) return;
            
            _selectAction?.Invoke(_portrait);
        }
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class RewardViewer : MonoBehaviour
    {
        private int DataIndex { get; set; }
        private Image[] _iconImgs;
        private Transform[] _gearIcons;

        public List<Transform> grades;
        [Space(20)]
        public Transform Container;
        public Transform Icon;
        [Space(20)]
        public TMP_Text Count;
        public TMP_Text Level;
        [Space(20)]
        public Image Background;

        private void Awake()
        {
            int childCount = Icon.childCount;
            
            _gearIcons = new Transform[childCount];
            _iconImgs = new Image[childCount];
            
            for (int i = 0; i < Icon.childCount; ++i)
            {
                _gearIcons[i] = Icon.GetChild(i);
                _iconImgs[i] = _gearIcons[i].GetComponentInChildren<Image>();
            
                _gearIcons[i].SetActive(false);
            }
        }

        public void SetData(List<ScrollDataModel> data, int dataIndex)
        {
            // link the data to the cell view
            Container.SetActive(-1 < dataIndex);
            if (dataIndex <= -1 || data.Count <= dataIndex) return;

            DataIndex = dataIndex;
            var _data = data[dataIndex];
            
            foreach (var gearIcon in _gearIcons)
                gearIcon.SetActive(false);
            
            // 0번 재화 아이콘 1~10 Gear 아이콘
            _gearIcons[_data.IconType].SetActive(true);
            _iconImgs[_data.IconType].sprite = _data.IconSprite;  // 아이콘

            if (Background)
                Background.sprite = _data.BackgroundSprite;           // BG
            
            grades.ForEach(d => d.SetActive(false));
            
            if (1 <= _data.IconType && _data.IconType <= 10)
            {
                for (int i = 0; i <= 4 - _data.Grade; ++i)
                    grades[i].SetActive(true);
            }

            if (_data.Count <= 0)
            {
                Count.SetActive(false);
            }
            else
            {
                Count.SetActive(true);
                
                if (_data.GoodsType == Numerals.Comma) // Count
                    Count.SetText($"{_data.Count.ToGoodsString()}");
                else if (_data.GoodsType == Numerals.KMGT)
                    Count.SetText($"{_data.Count.ToGoldString()}");
            }
        }
    }
}
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class PopupProfileCharInfoCellView : EnhancedScrollerCellView
    {
        public TMP_Text optionName;
        public TMP_Text optionValue;
        public Image bgImg;

        public void SetData(string name, string value, string colorCode)
        {
            optionName.SetText(name);
            optionValue.SetText(value);

            Color color;
            ColorUtility.TryParseHtmlString(colorCode, out color);
            bgImg.color = color;
        }
    }
}

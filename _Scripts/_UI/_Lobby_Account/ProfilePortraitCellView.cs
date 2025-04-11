using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class ProfilePortraitCellView : EnhancedScrollerCellView
    {
        public List<ProfilePortraitSlot> slotList;

        public void SetData(List<Portrait> portraitList, int startIndex, Portrait selectPortrait, Action<Portrait> selectAction)
        {
            slotList.ForEach(s => s.SetActive(false));

            for (int i = 0; i < slotList.Count; ++i)
            {
                int dataIndex = startIndex + i;
                if (portraitList.Count <= dataIndex) break;
                
                slotList[i].SetData(portraitList[dataIndex], selectPortrait, selectAction);
                slotList[i].SetActive(true);
            }
        }
    }
}
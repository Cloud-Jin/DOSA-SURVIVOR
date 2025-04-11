using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;

namespace ProjectM
{
    public class PopupAccountLevelUpCellView : EnhancedScrollerCellView
    {
        public List<ItemSlot> itemSlotList;
        
        private List<ScrollDataModel> _scrollDataModelList = new();

        public void SetData(List<ScrollDataModel> scrollDataModelList, int startIndex)
        {
            _scrollDataModelList.Clear();

            for (int i = 0; i < itemSlotList.Count; ++i)
            {
                int dataIndex = startIndex + i;
                if (scrollDataModelList.Count <= dataIndex) break;
                
                ScrollDataModel scrollDataModel = scrollDataModelList[dataIndex];
                _scrollDataModelList.Add(scrollDataModel);
            }
            
            itemSlotList.ForEach(d => d.SetActive(false));

            for (int i = 0; i < _scrollDataModelList.Count; ++i)
            {
                itemSlotList[i].SetActive(true);
                itemSlotList[i].SetDataScrollDataModel(_scrollDataModelList[i]);
            }
        }
    }
}

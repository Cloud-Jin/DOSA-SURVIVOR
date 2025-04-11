using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;

namespace ProjectM
{
    public class CommonRewardCellView : EnhancedScrollerCellView
    {
        public List<ItemSlot> _itemSlotList;
        
        private List<ScrollDataModel> _scrollDataModelList = new();

        public void SetData(List<ScrollDataModel> scrollDataModelList, int startIndex)
        {
            _scrollDataModelList.Clear();

            for (int i = 0; i < _itemSlotList.Count; ++i)
            {
                int dataIndex = startIndex + i;
                if (scrollDataModelList.Count <= dataIndex) break;
                
                ScrollDataModel scrollDataModel = scrollDataModelList[dataIndex];
                _scrollDataModelList.Add(scrollDataModel);
            }
            
            _itemSlotList.ForEach(d => d.SetActive(false));

            for (int i = 0; i < _scrollDataModelList.Count; ++i)
            {
                _itemSlotList[i].SetActive(true);
                _itemSlotList[i].SetDataScrollDataModel(_scrollDataModelList[i]);
            }
        }
    }
}

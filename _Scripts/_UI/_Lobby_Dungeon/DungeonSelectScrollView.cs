using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class DungeonSelectScrollView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private DungeonSelectScrollCellView prefabCellView;
        [Space(20)]
        public EnhancedScroller enhancedScroller;
        [Space(20)]
        public Vector2 prefabCellSize;
        [Space(20)]
        public int numberOfCellsPerRow = 5;
        
        private List<List<ScrollDataModel>> _rewardDataList = new();

        private void Start()
        {
            enhancedScroller.Delegate = this;
        }

        // list
        public void SetData(List<List<ScrollDataModel>> rewardDataList)
        {
            _rewardDataList = rewardDataList;
        }
        
        public void ReloadScrollView(bool isResetPosition = true)
        {
            if (isResetPosition == false)
            {
                float position = enhancedScroller._scrollPosition;
                enhancedScroller.ReloadData();
                enhancedScroller.SetScrollPositionImmediately(position);
            }
            else
            {
                enhancedScroller.ReloadData();
            }
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            // if (_rewardDataList.IsNullOrEmpty())
            //     return 0;

            return _rewardDataList.Count;
            // return Mathf.CeilToInt(_rewardDataList.Count / (float)numberOfCellsPerRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal)
                return prefabCellSize.x;
                
            return prefabCellSize.y;
        }

        // SetCell
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            DungeonSelectScrollCellView cellView = scroller.GetCellView(prefabCellView) as DungeonSelectScrollCellView;
            
            if (cellView)
            {
                int startIndex = dataIndex;// * numberOfCellsPerRow;
                
                cellView.SetData(_rewardDataList, startIndex);
                return cellView;
            }
            
            return null;
        }
        
        private void SetCellViewSize(EnhancedScrollerCellView cell)
        {
            if (cell.isActiveAndEnabled == false) return;
            
            LayoutElement layout = cell.transform.GetComponent<LayoutElement>();
            layout.ignoreLayout = true;
            
            if (layout)
            {
                if (0 < prefabCellSize.x && 0 < prefabCellSize.y)
                {
                    layout.minWidth = layout.preferredWidth = prefabCellSize.x;
                    layout.minHeight = layout.preferredHeight = prefabCellSize.y;
                }
                else if (0 < prefabCellSize.x)
                {
                    layout.minWidth = layout.preferredWidth = prefabCellSize.x;
                    layout.minHeight = layout.preferredHeight = prefabCellSize.x;
                }
                else if (0 < prefabCellSize.y)
                {
                    layout.minWidth = layout.preferredWidth = prefabCellSize.y;
                    layout.minHeight = layout.preferredHeight = prefabCellSize.y;
                }
                else
                {
                    if (0 < layout.preferredWidth && 0 < layout.preferredHeight)
                    {
                        layout.minWidth = layout.preferredWidth;
                        layout.minHeight = layout.preferredHeight;
                    }
                    else
                    {
                        if (0 < layout.minWidth)
                            layout.preferredWidth = layout.preferredHeight = layout.minWidth;
                        else if (0 < layout.minHeight)
                            layout.preferredWidth = layout.preferredHeight = layout.minHeight;
                    }
                }
            }
        }
    }
}
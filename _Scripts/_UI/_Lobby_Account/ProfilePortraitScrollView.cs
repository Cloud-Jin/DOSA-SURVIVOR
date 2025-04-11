using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class ProfilePortraitScrollView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private ProfilePortraitCellView prefabCellView;
        public EnhancedScroller enhancedScroller;
        [Space(20)]
        public Vector2 prefabCellSize;
        [Space(20)]
        public int numberOfCellsPerRow = 5;
        
        private List<Portrait> _portraitList = new ();
        private Portrait _selectPortrait;
        private Action<Portrait> _selectAction;
        
        private void Start()
        {
            enhancedScroller.Delegate = this;
            
            SetData();
            ReloadScrollView();
        }

        public void SetData()
        {
            _portraitList = TableDataManager.Instance.data.Portrait.ToList();
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

        public void SetSelectPortraitData(Portrait selectPortrait, Action<Portrait> selectAction)
        {
            _selectPortrait = selectPortrait;
            _selectAction = selectAction;
            
            SetData();
            ReloadScrollView(false);
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.CeilToInt(_portraitList.Count / (float)numberOfCellsPerRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal)
                return prefabCellSize.x;

            return prefabCellSize.y;
        }
        
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            ProfilePortraitCellView cellView = scroller.GetCellView(prefabCellView) as ProfilePortraitCellView;

            if (cellView)
            {
                int startIndex = dataIndex * numberOfCellsPerRow;

                cellView.SetData(_portraitList, startIndex, _selectPortrait, _selectAction);
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
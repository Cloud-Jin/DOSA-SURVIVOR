using System.Collections;
using System.Collections.Generic;
using Castle.Core.Internal;
using Doozy.Runtime.UIManager.Components;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class StageSelectScrollView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        private List<Stage> _stages;

        public EnhancedScroller enhancedScroller;
        [Space(20)]
        public StageSelectScrollCellView prefabCellView;
        [Space(20)]
        public Vector2 prefabCellSize;

        public void SetData(List<Stage> stages)
        {
            _stages = stages;
        }

        public void ReloadScrollView()
        {
            enhancedScroller.Delegate = this;
            enhancedScroller.ReloadData();
        }
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (_stages.IsNullOrEmpty())
                return 0;

            return _stages.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal)
                return prefabCellSize.x;
                
            return prefabCellSize.y;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            StageSelectScrollCellView cellView = scroller.GetCellView(prefabCellView) as StageSelectScrollCellView;
            if (cellView)
            {
                cellView.SetData(_stages[dataIndex]);
                return cellView;
            }
            
            return null;
        }

        private void SetCellViewSize(EnhancedScrollerCellView cell)
        {
            if (cell.isActiveAndEnabled == false) return;

            LayoutElement layout = cell.transform.GetComponent<LayoutElement>();
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
        
        public UIButton GetTutorialCellViewBtn()
        {
            UIButton tutorialBtn = null;

            for (int i = 0; i < _stages.Count; ++i)
            {
                StageSelectScrollCellView cellView = enhancedScroller.GetCellViewAtDataIndex(i) as StageSelectScrollCellView;

                if (cellView)
                {
                    tutorialBtn = cellView.GetTutorialBtn();
                    if (tutorialBtn) break;
                }
            }

            return tutorialBtn;
        }
    }
}

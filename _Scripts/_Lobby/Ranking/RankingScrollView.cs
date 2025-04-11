using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class RankingScrollView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private RankingScrollCellView prefabCellView;
        [Space(20)]
        public EnhancedScroller enhancedScroller;
        [Space(20)]
        public Vector2 prefabCellSize;

        private List<PlayerRank> _playerRankList = new(); 
        
        private int _selectTabIndex;
        private float _scrollPosition;
        
        private void Start()
        {
            enhancedScroller.Delegate = this;
        }

        public void SetData(int selectTabIndex, float scrollPosition)
        {
            _selectTabIndex = selectTabIndex;
            _scrollPosition = scrollPosition;

            _playerRankList = UserDataManager.Instance.playerRankInfo.rankDataList[_selectTabIndex].player_ranks;
            
            enhancedScroller._scrollPosition = _scrollPosition;
            
            ReloadScrollView(false);
        }
        
        private void ReloadScrollView(bool isResetPosition = true)
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
            return _playerRankList?.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal)
                return prefabCellSize.x;

            return prefabCellSize.y;
        }
        
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            RankingScrollCellView cellView = scroller.GetCellView(prefabCellView) as RankingScrollCellView;
            
            if (cellView)
            {
                cellView.SetData(_playerRankList[dataIndex], _selectTabIndex, this);
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

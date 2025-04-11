using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class HardDungeonSelectScrollView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private HardDungeonSelectCellView prefabCellView;
        
        public EnhancedScroller enhancedScroller;
        [Space(20)]
        public Vector2 prefabCellSize;

        private List<Stage> _stageList = new();
        private StageData _stageData;
        private Stage _stage;

        private void Start()
        {
            enhancedScroller.Delegate = this;

            _stageList = TableDataManager.Instance.data.Stage.Where(t => 
                t.StageType == 3).ToList().Where(t => t.ChapterID != 0 || t.StageLevel != 0).ToList();
        }

        public void SetData()
        {
            _stageData = UserDataManager.Instance.stageInfo.Data.SingleOrDefault(u => u.type == 3);
            
            if (_stageData == null)
            {
                _stage = _stageList.Single(s => s.ChapterID == 1 && s.StageLevel == 1);
            }
            else
            {
                _stage = _stageList.SingleOrDefault(s => s.ChapterID == _stageData.chapter_id_cap
                                                         && s.StageLevel == _stageData.level_cap);

                if (_stage == null)
                    _stage = _stageList.Single(s => s.ChapterID == 1 && s.StageLevel == 1);
                else
                    _stage = 0 < _stage.NextStage ? _stageList.Single(s => s.Index == _stage.NextStage) : null;
            }
            
            ReloadScrollView();
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
            return _stageList.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal)
                return prefabCellSize.x;

            return prefabCellSize.y;
        }
        
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            HardDungeonSelectCellView cellView = scroller.GetCellView(prefabCellView) as HardDungeonSelectCellView;

            if (cellView)
            {
                cellView.SetData(_stageList[dataIndex], _stage);
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

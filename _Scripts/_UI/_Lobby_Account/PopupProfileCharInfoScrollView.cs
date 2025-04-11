using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using Facebook.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class PopupProfileCharInfoScrollView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private PopupProfileCharInfoCellView prefabCellView;
        public EnhancedScroller enhancedScroller;
        [Space(20)]
        public Vector2 prefabCellSize;

        private List<ProfileInfo> _profileInfoList = new();
        
        private enum OptionType
        {
            ATK = 1,
            HP,
            MOVE_SPEED,
            CHI_RATIO,
            CHI_DMG,
            SKILL_COOL,
            NORMAL_MOB_DMG,
            BOSS_MOB_DMG,
            COMMON_ACTIVE_SKILL_DMG,
            DECRASE_DMG,
            IGNORE_DMG
        }

        private void Start()
        {
            enhancedScroller.Delegate = this;

            _profileInfoList = TableDataManager.Instance.data.ProfileInfo.ToList();
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
            return _profileInfoList.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal)
                return prefabCellSize.x;

            return prefabCellSize.y;
        }
        
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            PopupProfileCharInfoCellView cellView = scroller.GetCellView(prefabCellView) as PopupProfileCharInfoCellView;

            if (cellView)
            {
                string optionValue = "";

                ProfileInfo profileInfo = _profileInfoList[dataIndex];
                GearPower equipGearPower = UserDataManager.Instance.gearInfo.GetEquipGearsPower();
                
                switch ((OptionType)profileInfo.Index)
                {
                    case OptionType.ATK:
                    {
                        optionValue = equipGearPower.Attack.ToParseString();
                        break;
                    }
                    case OptionType.HP:
                    {
                        optionValue = equipGearPower.Hp.ToParseString();
                        break;
                    }
                    case OptionType.MOVE_SPEED:
                    {
                        Monster monster = TableDataManager.Instance.data.Monster.Single(t => t.Index == 1);
                        optionValue = $"{(monster.Speed * equipGearPower.CharMoveSpeed) + monster.Speed}";
                        break;
                    }
                    case OptionType.CHI_RATIO:
                    {
                        optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio", equipGearPower.CriticalRatio * 100f);
                        break;
                    }
                    case OptionType.CHI_DMG:
                    {
                        optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio", equipGearPower.CriticalDmg * 100f);
                        break;
                    }
                    case OptionType.SKILL_COOL:
                    {
                        int charAttackSpeed = (int)(equipGearPower.CharAttackSpeed * 10000);
                        if (0 < charAttackSpeed)
                            optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio", (10000 - charAttackSpeed) / 100f);
                        else
                            optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio", 0);
                        break;
                    }
                    case OptionType.NORMAL_MOB_DMG:
                    {
                        optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio", equipGearPower.NormalMobDmg * 100f);
                        break;
                    }
                    case OptionType.BOSS_MOB_DMG:
                    {
                        optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio", equipGearPower.BossMobDmg * 100f);
                        break;
                    }
                    case OptionType.COMMON_ACTIVE_SKILL_DMG:
                    {
                        optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio", equipGearPower.CommonActiveSkillDmg * 100f);
                        break;
                    }
                    case OptionType.DECRASE_DMG:
                    {
                        int decreaseDmg = (int)(equipGearPower.DecreaseDmg * 10000); 
                        if (0 < decreaseDmg)
                            optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio",(10000 - decreaseDmg) / 100f);
                        else
                            optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio",0);
                        break;
                    }
                    case OptionType.IGNORE_DMG:
                    {
                        optionValue = LocaleManager.GetLocale("Equip_Add_Option_Ratio", equipGearPower.IgnoreDmg * 100f);
                        break;
                    }
                }

                cellView.SetData(LocaleManager.GetLocale(profileInfo.Name), optionValue, profileInfo.BgColor);
                
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

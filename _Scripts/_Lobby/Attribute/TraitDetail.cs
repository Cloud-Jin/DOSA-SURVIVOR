using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace ProjectM
{
    public class TraitDetail : MonoBehaviour
    {
        public UIButton button;
        public Image icon;
        public TMP_Text level, description01, description02,costTMP,btnTMP;
        public GameObject arrow, desc02, goCost, goRedDot;
        public int Index;

        private Trait tbTarit;
        private ContainerTrait PopupTrait;
        private string Description;
        private int optionType;
        private int maxLevel;
        private int levelNum;

        private void Awake()
        {
            button.AddEvent(OnClickTraitUp);
            UserDataManager.Instance.userTraitInfo.apMax.Subscribe(i => SetLevel()).AddTo(this);
        }

        public void SetTarit(int taritID, ContainerTrait popupTrait)
        {
            this.PopupTrait = popupTrait;
            this.tbTarit = TableDataManager.Instance.data.Trait.Single(t => t.Index == taritID);
            Index = tbTarit.Index;
            var traitType = TableDataManager.Instance.data.TraitType.Single(t => t.Type == tbTarit.Type);
            Description = traitType.Description;
            optionType = traitType.OptionType;
            icon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(traitType.Icon);
            maxLevel = TableDataManager.Instance.data.TraitLevel.Count(t => t.GroupID == tbTarit.TraitLevelGroupID);
            costTMP.SetText($"{tbTarit.ConsumePoint}");
            SetLevel();
        }

        public void SetLevel()
        {
            if (Index == 0) return;
            
            var userData = UserDataManager.Instance.userTraitInfo;
            levelNum = userData.GetTraitLevel(tbTarit.Index);
            level.text = $"{levelNum}/{maxLevel}";
            var _level = levelNum == 0 ? 1 : levelNum;
            bool isMaxLevel = levelNum == maxLevel;
            bool isCost = userData.TraitNum.Value >= tbTarit.ConsumePoint;
            var prevTrait = TableDataManager.Instance.data.TraitTree.Where(t => t.GroupID == tbTarit.TraitTreeGroupID).ToList();
            bool isBridge = prevTrait.Count == 0;   // 조건없으면 true
            for (int i = 0; i < prevTrait.Count; i++)
            {
                isBridge = userData.IsTraitMaxLevel(prevTrait[i].PrevTraitIndex);
                if (isBridge)
                    break;
            }
            var btnString = isMaxLevel ? "Max_Level" : levelNum == 0 ? "Trait_Learn" : "Trait_Reinforce";
            btnTMP.SetText(btnString.Locale());//습득하기
            costTMP.color = isCost ? Color.white : Color.red;
            button.interactable = !isMaxLevel && isCost && isBridge;
            goRedDot.SetActive(!isMaxLevel && isCost && isBridge);
            goCost.SetActive(!isMaxLevel);
            LocaleManager.Instance.onUpdate.Subscribe(i =>
            {
//desc
                double value01 = TableDataManager.Instance.data.TraitLevel.Single(t => t.GroupID == tbTarit.TraitLevelGroupID && t.Level == _level).Value;
                var valueType = TableDataManager.Instance.data.OptionType.Single(t => t.AddOptionType == optionType).AddOptionValueType;
                if (optionType == 18) // 스킬
                {
                    var skillName = TableDataManager.Instance.GetSkillAiData((int)value01, 1).Name;
                    description01.SetText(Description.Locale(skillName.Locale()));
                }
                else
                {
                
                    string _value01 = valueType == 1 ? InfVal.Parse($"{value01}").ToParseString() : $"{value01 * 0.01}";
                    description01.SetText(Description.Locale(_value01));    
                }
            
                //0 or max는 하나만.
                if (levelNum == 0 || isMaxLevel)
                {
                    desc02.SetActive(false);
                    arrow.SetActive(false);
                    return;
                }
            
                desc02.SetActive(true);
                arrow.SetActive(true);
                double value02 = TableDataManager.Instance.data.TraitLevel.Single(t => t.GroupID == tbTarit.TraitLevelGroupID && t.Level == levelNum+1).Value;
                //value02 = valueType == 1 ? value02 : value02*0.01f;
                string _value02 = valueType == 1 ? InfVal.Parse($"{value02}").ToParseString() : $"{value02 * 0.01}";
                description02.SetText(Description.Locale(_value02));
            }).AddTo(this);
        }

        void OnClickTraitUp()
        {
            PopupTrait.RequestTraitUp(tbTarit.Tab, tbTarit.Index);
        }
    }
}
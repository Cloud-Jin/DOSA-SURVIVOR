using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetKits.ParticleImage;
using Doozy.Runtime.UIManager.Components;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace ProjectM
{
   public class TraitIcon : MonoBehaviour
   {
      public UIButton button;
      public Image icon;
      public TMP_Text level;
      public Transform blackIcon,blackIcon2, selectUI;
      public GameObject goRedDot;
      
      private Trait tbTarit;
      private ContainerTrait PopupTrait;
      private List<TraitTree> prevTrait;

      public List<TraitBridge> _traitBridges;
      public int Index;
      public int levelNum;
      public int Tier;
      public bool enableReddot;
      public ParticleImage levelUpEffect;
      private int maxLevel;
      private void Awake()
      {
         button.AddEvent(OnClickIcon);
         levelUpEffect.onLastParticleFinish.AddListener(() => levelUpEffect.SetActive(false));
      }

      public void SetTarit(Trait tbTarit, ContainerTrait popupTrait)
      {
         this.tbTarit = tbTarit;
         this.PopupTrait = popupTrait;
         
         Index = tbTarit.Index;
         Tier = tbTarit.Row;
         var traitType = TableDataManager.Instance.data.TraitType.Single(t => t.Type == tbTarit.Type);
         // Description = traitType.Description;
         icon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleSkill).GetSprite(traitType.Icon);
         maxLevel = TableDataManager.Instance.GetTraitMaxLevel(tbTarit.TraitLevelGroupID);
         prevTrait = TableDataManager.Instance.data.TraitTree.Where(t => t.GroupID == tbTarit.TraitTreeGroupID).ToList();
         _traitBridges = transform.parent.GetComponentsInChildren<TraitBridge>().ToList();
         SetLevel();

         popupTrait.selectIdx.Subscribe(OnSelected).AddTo(this);
      }

      public void SetLevel()
      {
         var userData = UserDataManager.Instance.userTraitInfo;
         levelNum = UserDataManager.Instance.userTraitInfo.GetTraitLevel(tbTarit.Index);
         level.text = $"{levelNum}/{maxLevel}";
         

         // 이전레벨꺼 다 배움 & 나도 1레벨이상 배움.
         {
            for (int i = 0; i < prevTrait.Count; i++)
            {
               bool isBridge = userData.IsTraitMaxLevel(prevTrait[i].PrevTraitIndex);
               _traitBridges[i].SetBridge(isBridge);
            }   
         }
         
         
         blackIcon.SetActive(levelNum == 0);
         bool isBridgeCondition = prevTrait.Count == 0 || _traitBridges.Any(t=> t.isOn);
         blackIcon2.SetActive(!isBridgeCondition);
         // redDot
         bool isMaxLevel = levelNum == maxLevel;
         bool isCost = userData.TraitNum.Value >= tbTarit.ConsumePoint;
         enableReddot = !isMaxLevel && isCost && isBridgeCondition;
         goRedDot.SetActive(enableReddot);
         
      }

      public void OnClickIcon()
      {
         PopupTrait.selectIdx.Value = Index;
      }

      void OnSelected(int i)
      {
         selectUI.SetActive(Index == i);
      }

      public void PlayEffect()
      {
         levelUpEffect.SetActive(true);
         levelUpEffect.Play();
         SoundManager.Instance.PlayFX("Summon_Normal");
      }
   }
}

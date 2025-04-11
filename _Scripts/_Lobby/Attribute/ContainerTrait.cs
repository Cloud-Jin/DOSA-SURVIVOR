using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Animators;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using ProjectM.AutoBattle;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class ContainerTrait : MonoBehaviour
    {
        private UIContainer uiContainer;
        public ScrollRect weaponScrollRect, commonScrollRect;
        public TraitDetail traitDetail;
        public List<TraitIcon> weaponTarit;
        public List<TraitIcon> commonTarit;
        public UIButton questionBtn, resetBtn;
        public Transform weaponTr, commonTr;
        public TMP_Text apPoint, weaponPoint, commonPoint;
        public ReactiveProperty<int> selectIdx;
        public bool isTraitRedDot;
        private int tab;

        public UIContainerUIAnimator weaponTabAnimator, commonTabAnimator;

        private void Awake()
        {
            uiContainer = GetComponent<UIContainer>();
        }

        void Start()
        {
            selectIdx = new ReactiveProperty<int>();
            
            weaponTarit = new List<TraitIcon>();
            // commonTarit = new List<TraitIcon>();

            weaponTarit.AddRange(weaponTr.GetComponentsInChildren<TraitIcon>());
            // commonTarit.AddRange(commonTr.GetComponentsInChildren<TraitIcon>());

            questionBtn.AddEvent(OnDesc);
            // closeBtn.AddEvent(OnClose);
            resetBtn.AddEvent(TraitReset);
            
            var traits = TableDataManager.Instance.data.Trait.ToList();

            var weaponTaritData = traits.Where(t => t.Tab == 1).ToList();
        
            for (int i = 0; i < weaponTarit.Count; i++)
            {
                weaponTarit[i].SetTarit(weaponTaritData[i], this);
            }
            
            // var commonTaritData = traits.Where(t => t.Tab == 2).ToList();
            //
            // for (int i = 0; i < commonTarit.Count; i++)
            // {
            //     commonTarit[i].SetTarit(commonTaritData[i], this);
            // }

            selectIdx.Where(t=> t > 0).Subscribe(OnClickTrait).AddTo(this);
            UserDataManager.Instance.userTraitInfo.TraitNum.Subscribe(i => apPoint.SetText($"{i}")).AddTo(this);
            // var resetIdx = TableDataManager.Instance.data.TraitConfig.Single(t => t.Index == 1).Value;
            // UserDataManager.Instance.currencyInfo.SubscribeItemRx((CurrencyType)resetIdx, val => resetCost.SetText(val.ToGoldString())).AddTo(this);
            SetDefault();
            weaponTabAnimator.showAnimation.OnFinishCallback.AddListener(() =>
            {
                var icon = weaponTarit.Where(t => t.levelNum > 0)
                    .OrderByDescending(t => t.Tier)
                    .ThenBy(t=> t.Index)
                    .FirstOrDefault();
                if (icon)
                {
                    icon.OnClickIcon();
                    SetScrollRectPosition(icon.Tier, weaponScrollRect);
                }
                else
                {
                    weaponTarit[0].OnClickIcon();
                    SetScrollRectPosition(0, weaponScrollRect);
                }
                tab = 1;
            });
            
            commonTabAnimator.showAnimation.OnFinishCallback.AddListener(() =>
            {
                var icon = commonTarit.Where(t => t.levelNum > 0)
                    .OrderByDescending(t => t.Tier)
                    .ThenBy(t=> t.Index)
                    .FirstOrDefault();
                if (icon)
                {
                    icon.OnClickIcon();
                    SetScrollRectPosition(icon.Tier, commonScrollRect);
                }
                else
                {
                    commonTarit[0].OnClickIcon();
                    SetScrollRectPosition(0, commonScrollRect);
                }
                tab = 2;
            });

            CalcTraitPoint();
            RedDotCheck();
            UserDataManager.Instance.userTraitInfo.apUse.Subscribe(t => resetBtn.SetActive(t > 0)).AddTo(this);
            UserDataManager.Instance.userTraitInfo.OnTraitRefresh.Subscribe(t=>
            {
                CalcTraitPoint();
            }).AddTo(this);
            
            uiContainer.OnShowCallback.Event.AddListener(OnShowTrait);
        }
        
        void OnShowTrait()
        {
            SetDefault();
        }

        // [Button]
        // public void SetTeir(int i)
        // {
        //     SetScrollRectPosition(i, weaponScrollRect);
        // }
        
        void SetDefault()
        {
            var icon = weaponTarit.Where(t => t.levelNum > 0)
                .OrderByDescending(t => t.Tier)
                .ThenBy(t=> t.Index)
                .FirstOrDefault();
            if (icon)
            {
                icon.OnClickIcon();
                SetScrollRectPosition(icon.Tier, weaponScrollRect);
            }
            else
            {
                weaponTarit[0].OnClickIcon();
                SetScrollRectPosition(0, weaponScrollRect);
            }
        }
        
        void OnDesc()
        {
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale("Help"))
                    .SetMessage("Trait_Desc".Locale())
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("Common_Ok_Btn"))
                    .Build();
            }
        }

        void RefrashAll()
        {
            for (int i = 0; i < weaponTarit.Count; i++)
            {
                weaponTarit[i].SetLevel();
            }

            for (int i = 0; i < commonTarit.Count; i++)
            {
                commonTarit[i].SetLevel();
            }
            AutoBattleManager.Instance.OnPlayerUpdate.OnNext(1);
            RedDotCheck();
        }

        void CalcTraitPoint()
        {
            var traitDatas = UserDataManager.Instance.userTraitInfo.GetActiveTraits();
            var weaponTarits = traitDatas.Where(t => t.tab == 1).ToList();
            int wPoint = 0;
            foreach (var _tarit in weaponTarits)
            {
                wPoint += TableDataManager.Instance.data.Trait
                    .Single(t => t.Index == _tarit.trait_id && t.Tab == _tarit.tab).ConsumePoint * _tarit.level;
            }
            weaponPoint.SetText($"{wPoint}");
            
            var commonTarits = traitDatas.Where(t => t.tab == 2).ToList();
            int cPoint = 0;
            foreach (var _tarit in commonTarits)
            {
                cPoint += TableDataManager.Instance.data.Trait
                    .Single(t => t.Index == _tarit.trait_id && t.Tab == _tarit.tab).ConsumePoint * _tarit.level;
            }
            commonPoint.SetText($"{cPoint}");
        }
        
        public void RedDotCheck()
        {
            var c = weaponTarit.Count(t => t.enableReddot) + commonTarit.Count(t => t.enableReddot);
            
            isTraitRedDot = c > 0;
            
            if (UILobby.Instance)
                UILobby.Instance.SetRedDot(LobbyTap.Trait, isTraitRedDot);
        }

        void OnClose()
        {
            // Hide();
        }

        void TraitReset()
        {
            var config = TableDataManager.Instance.data.TraitConfig;
            var idx = config.Single(t => t.Index == 1).Value;
            var cost = UserDataManager.Instance.userTraitInfo.GetResetCost();
            var valid = UserDataManager.Instance.currencyInfo.ValidGoods((CurrencyType)idx, cost);
            
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            if (popup)
            {
                popup.InitBuilder()
                    .SetTitle("Trait_Reset".Locale())
                    .SetMessage("Trait_Reset_Desc".Locale())
                    .SetCloseButton(popup.Hide)
                    .SetBuyCurrencyButton(RequestTraitReset, "Reset".Locale(), ResourcesManager.Instance.GoodsTypeIcon[idx],cost.ToGoldString(), valid)
                    .Build();
            }
        }

        void RequestTraitReset()
        {
            APIRepository.RequestTraitReset(o =>
            {
                RefrashAll();
                SetDefault();
                traitDetail.SetLevel();
            }, reply =>
            {

            });
        }

        public void RequestTraitUp(int tab, int index)
        {
            // 이전트리
            // 최대레벨
            // 포인트 체크
            var payload = new Dictionary<string, object>
            {
                { "tab", tab},
                {"index", index}
            };
            APIRepository.RequestTraitLevelUp(payload, o =>
            {
                RefrashAll();
                traitDetail.SetLevel();
                weaponTarit.Single(t => t.Index == index).PlayEffect();

            }, reply =>
            {

            });
        }

        void OnClickTrait(int i)
        {
            traitDetail.SetTarit(i, this);
        }

        void SetScrollRectPosition(int tier, ScrollRect scrollRect)
        {
            float value = 1 - ((tier-3) * 0.038f);
            if (tier < 4)
                value = 1;
            else if (tier > 27)
                value = 0;
            // float value = (tier * 100) / (scrollRect.content.rect.height - scrollRect.GetComponent<RectTransform>().rect.height);
            // scrollRect.content.anchoredPosition = new Vector2(0,value);
            scrollRect.verticalNormalizedPosition = value;
        }
    }
}

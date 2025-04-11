using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Reactor.Animators;
using Doozy.Runtime.UIManager.Animators;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class PopupTrait : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Lobby_Trait;
        public ScrollRect weaponScrollRect, commonScrollRect;
        public TraitDetail traitDetail;
        public List<TraitIcon> weaponTarit;
        public List<TraitIcon> commonTarit;
        public UIButton questionBtn, closeBtn, resetBtn;
        public Transform weaponTr, commonTr;
        public TMP_Text apPoint, resetCost;
        public ReactiveProperty<int> selectIdx;
        
        private int tab;

        public UIContainerUIAnimator weaponTabAnimator, commonTabAnimator;

        protected override void Init()
        {
            selectIdx = new ReactiveProperty<int>();
            
            weaponTarit = new List<TraitIcon>();
            commonTarit = new List<TraitIcon>();

            weaponTarit.AddRange(weaponTr.GetComponentsInChildren<TraitIcon>());
            commonTarit.AddRange(commonTr.GetComponentsInChildren<TraitIcon>());

            questionBtn.AddEvent(OnDesc);
            closeBtn.AddEvent(OnClose);
            resetBtn.AddEvent(TraitReset);
            
            var traits = TableDataManager.Instance.data.Trait.ToList();

            var weaponTaritData = traits.Where(t => t.Tab == 1).ToList();
        
            for (int i = 0; i < weaponTarit.Count; i++)
            {
                // weaponTarit[i].SetTarit(weaponTaritData[i], this);
            }
            
            var commonTaritData = traits.Where(t => t.Tab == 2).ToList();

            for (int i = 0; i < commonTarit.Count; i++)
            {
                // commonTarit[i].SetTarit(commonTaritData[i], this);
            }

            selectIdx.Where(t=> t > 0).Subscribe(OnClickTrait).AddTo(this);
            UserDataManager.Instance.userTraitInfo.TraitNum.Subscribe(i => apPoint.SetText($"{i}")).AddTo(this);
            var resetIdx = TableDataManager.Instance.data.TraitConfig.Single(t => t.Index == 1).Value;
            UserDataManager.Instance.currencyInfo.SubscribeItemRx((CurrencyType)resetIdx, val => resetCost.SetText(val.ToGoldString())).AddTo(this);
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

            UserDataManager.Instance.userTraitInfo.apUse.Subscribe(t => resetBtn.SetActive(t > 0)).AddTo(this);
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
        }

        void OnClose()
        {
            Hide();
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
            }, reply =>
            {

            });
        }

        void OnClickTrait(int i)
        {
            // traitDetail.SetTarit(i, this);
            // Debug.Log($"Click {i}");
            // 세팅
            // Level up
        }

        void SetScrollRectPosition(int tier, ScrollRect scrollRect)
        {
            float value = (tier *180f) / (scrollRect.content.rect.height - scrollRect.preferredHeight);
            scrollRect.content.anchoredPosition = new Vector2(0,value);
        }
    }
}
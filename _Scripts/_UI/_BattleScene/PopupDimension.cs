using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace ProjectM.Battle
{
    public class PopupDimension : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_DimensionRift;

        public UIButton[] Slots;
        public UIButton rerollBtn;
        protected override void Init()
        {
            ParticleImageUnscaled();
            rerollBtn.AddEvent(OnClickReroll);
            SetData();
            
            SoundManager.Instance.PlayFX("DimensionPopup");
        }

        void SetData()
        {
            var bb = BlackBoard.Instance.data;
            foreach (var slot in Slots)
            {
                for (int i = slot.transform.childCount -1; i >= 0; i--)
                {
                    DestroyImmediate(slot.transform.GetChild(i).gameObject);
                }
                
                slot.ClearEvent();
            }
            
            foreach (var item in bb.RewardDatas.Select((value, index) => (value, index))) 
            {
                var data = item.value;
                int i = item.index;
                int idx = 0;
                
                switch (data.t)
                {
                    case 1:
                        var rewardData = new RewardData();// { itemIdx = idx, itemCount = InfVal.Parse(data.c) }
                        idx = TableDataManager.Instance.data.GoodsType.Single(t => t.TypeID == data.i).TypeID;
                        rewardData.itemIdx = idx;
                        rewardData.itemCount = InfVal.Parse(data.c);
                        if (bb.benefitEffect > 0)
                        {
                            rewardData.AddBenefit = InfVal.Parse(bb.BenefitRewardDatas[i].c);
                            rewardData.BenefitEffect = bb.benefitEffect;
                        }
                        
                        ResourcesManager.Instance.Instantiate("UI_Popup_Dimension_03", Slots[i].transform)
                            .GetComponent<DimensionItemReward>()
                            .SetData(rewardData);
                            
                        
                        Slots[i].AddEvent(()=>
                        {
                            var retcount = rewardData.itemCount + rewardData.AddBenefit;
                            SendData(i, () => AddItem(idx, retcount));
                        });
                        break;
                    case 2://장비 미계획
                        // idx = TableDataManager.Instance.data.GoodsType.Single(t => t.TypeID == data.i).TypeID;
                        // ResourcesManager.Instance.Instantiate("UI_Popup_Dimension_03", Slots[i].transform)
                        //     .GetComponent<DimensionItemReward>()
                        //     .SetData(new RewradData() { itemIdx = idx, itemCount = data.c });
                        //
                        // Slots[i].AddEvent(()=>
                        // {
                        //     AddItem(idx, data.c);
                        //     SendData(i);
                        // });
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 10:
                        break;
                    case 100:
                        break;
                    case 101:
                        idx = TableDataManager.Instance.data.Card.Single(t => t.Index == data.i).HeroCharacterID;
                        ResourcesManager.Instance.Instantiate("UI_Popup_Dimension_01", Slots[i].transform)
                            .GetComponent<DimensionHeroReward>()
                            .SetData(new RewardData() { heroIdx = idx });
                        
                        Slots[i].AddEvent(()=>
                        {
                            SendData(i, ()=> AddHero(idx));
                        });
                        break;
                }
                // 서버로 선택지 날린다.
            }
            
            uiPopup.Container.SetActive(false);
            uiPopup.Container.SetActive(true);

            //"보상" 타입
            // 1 = 재화
            // 2 = 장비
            // 3 = 카드
            // 4 = 성물
            // 10 = 계정 경험치
            // 100 = 전투 아이템(서버 사용X)
            // 101 = 균열 영웅(서버 사용X)
        }

        void OnClickSlot()
        {
            Hide();
        }

        void OnClickReroll()
        {
            // var payload = new Dictionary<string, object> { /*{ "step", 0 }*/ };

            AdMobManager.Instance.ShowAD(() =>
            {
                rerollBtn.SetActive(false);

                APIRepository.RequestStageCrackRoll(null, (r) =>
                {
                    SetData();
                    BlackBoard.Instance.Save();
                    Time.timeScale = 0;
                });
            },
            () =>
            {
                Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale("No_Ad"))
                    .Build();
            },
            () =>
            {
                Alarm alarm = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                alarm.InitBuilder()
                    .SetMessage(LocaleManager.GetLocale("Ad_Not_Completed"))
                    .Build();
            });
        }

        void AddHero(int idx)
        {
            if (PlayerManager.Instance.GetHeroList().Count(t => t.unitID == idx) > 0)
                return;

            var hero = PlayerManager.Instance.AddHero(idx);
            hero.SetStat();
            
            AddHideCallback(() =>
            {
                var popup = UIManager.Instance.Get(PopupName.Battle_GetHero) as PopupGetHero;
                popup.Show();
                popup.SetHero(idx);
            });
            Hide();
        }
        
        void AddItem(int idx, InfVal count)
        {
            AddHideCallback(() =>
            {
                var popup = UIManager.Instance.Get(PopupName.Battle_GetItem) as PopupGetItem;
                popup.Show();
                popup.SetItem(idx, count);
            });
            Hide();
        }

        [Button]
        public void SendData(int i, Action success)
        {
            // step: 몇번째 0~3, index: 선택지 0~2
            var payload = new Dictionary<string, object> { { "index", i }};
                
            APIRepository.RequestStageCrack(payload, data =>
            {
                success.Invoke();
                BlackBoard.Instance.SaveBattleData();
            });
        }
        
        public class RewardData
        {
            public int heroIdx;
            public int itemIdx;
            public InfVal itemCount;
            public int BenefitEffect;   // 멤버쉽 추가 효과
            public InfVal AddBenefit;   // 추가 재화
        }
    }
}
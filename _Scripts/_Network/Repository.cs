using System.Collections.Generic;
using System.Linq;
using InfiniteValue;

namespace ProjectM
{
    public partial class APIRepository
    { 
       public static NetworkManager Net => NetworkManager.Instance;

       public static void DisposeEvent(string _event)
       {
           Net.DisposeEvent(_event);
       }

       public static List<RewardData> SortRewardData(List<RewardData> rewardDataList)
       {
           List<RewardData> sortRewardDataList = rewardDataList.OrderBy(r => r.o).ToList();
           return sortRewardDataList;
       }
       
       public static List<ScrollDataModel>ConvertReward(List<RewardData> datas)
       {
           var list = new List<ScrollDataModel>();

           if (datas == null)
               return list;

           foreach (var item in datas.Select((value, index) => (value, index)))
           {
               var data = item.value;
               
               switch (data.t)
               {
                   case 6:  // 코스츔
                   {
                       Costume costume = TableDataManager.Instance.data.Costume.Single(t => t.Index == data.i);
                       CostumeRarityType costumeRarityType = TableDataManager.Instance.data.CostumeRarityType
                           .Single(t => t.Grade == costume.Grade);
                       
                       list.Add(new ScrollDataModel()
                       {
                           Index = costume.Index,
                           IconSprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Summon_Shop).GetSprite(costume.Icon),
                           BackgroundSprite = ResourcesManager.Instance
                               .GetAtlas(MyAtlas.Equipment_Frame).GetSprite(costumeRarityType.IconBackground),
                           Grade = costume.Grade,
                           RewardType = data.t,
                           bonusIconType = data.b,
                           Count = InfVal.Parse(data.c)
                       });
                       break;
                   }
                   case 5: // 골드
                   {
                       List<Stage> stageList =
                           TableDataManager.Instance.data.Stage.Where(t => t.StageType == 1).ToList();
                       
                       Stage clearStage = stageList.SingleOrDefault(t => 
                           t.ChapterID == UserDataManager.Instance.stageInfo.StageData.chapter_id_cap
                           && t.StageLevel == UserDataManager.Instance.stageInfo.StageData.level_cap);

                       int nextStageIndex;
                       if (clearStage != null)
                           nextStageIndex = clearStage.NextStage <= 0 ? clearStage.Index : clearStage.NextStage;
                       else
                           nextStageIndex = 1;

                       Stage nextStage = TableDataManager.Instance.data.Stage.SingleOrDefault(t => t.Index == nextStageIndex);

                       InfVal rewardGold = nextStage?.RewardGold ?? 0;

                       AutoBattleConfig stageAfkRewardGoldPercent = TableDataManager.Instance.data.AutoBattleConfig
                           .Single(t => t.Index == 9);
                       AutoBattleConfig stageAfkRewardGoldInterval = TableDataManager.Instance.data.AutoBattleConfig
                           .Single(t => t.Index == 15);

                       ScrollDataModel scrollDataModel = new ScrollDataModel();
                       var tbGoods = TableDataManager.Instance.data.GoodsType.Single(t => t.TypeID == 11);
                       scrollDataModel.Index = tbGoods.TypeID;

                       InfVal dataC = InfVal.Parse(data.c);
                       scrollDataModel.Count = rewardGold * stageAfkRewardGoldPercent.Value *
                           (dataC / stageAfkRewardGoldInterval.Value) / 10000; 
                       
                       scrollDataModel.GoodsType = Numerals.KMGT;
                       scrollDataModel.IconSprite = ResourcesManager.Instance.GoodsTypeIcon[tbGoods.TypeID];
                       scrollDataModel.BackgroundSprite = ResourcesManager.Instance.GoodsTypeBgs[tbGoods.TypeID];
                       scrollDataModel.RewardType = data.t;
                       scrollDataModel.bonusIconType = data.b;
                       list.Add(scrollDataModel);
                       break;
                   }
                   case 1: // 재화
                   {
                       var tbGoods = TableDataManager.Instance.data.GoodsType.Single(t => t.TypeID == data.i);
                       
                       ScrollDataModel scrollDataModel = new ScrollDataModel();
                       scrollDataModel.Index = tbGoods.TypeID;
                       scrollDataModel.IconType = 0;
                       scrollDataModel.BackgroundSprite = ResourcesManager.Instance.GoodsTypeBgs[tbGoods.TypeID];
                       scrollDataModel.Count = InfVal.Parse(data.c);
                       scrollDataModel.GoodsType = tbGoods.TypeID == 11 ? Numerals.KMGT : Numerals.Comma;
                       scrollDataModel.IconSprite = ResourcesManager.Instance.GoodsTypeIcon[tbGoods.TypeID];
                       scrollDataModel.RewardType = data.t;
                       scrollDataModel.bonusIconType = data.b;
                       list.Add(scrollDataModel);
                       break;
                   }
                   case 2:  // 장비
                       var tbGear = TableDataManager.Instance.data.Equipment.Single(t => t.Index == data.i);
                       
                       list.Add(new ScrollDataModel()
                       {
                           Index = tbGear.Index,
                           IconType = tbGear.EquipType,
                           IconSprite = ResourcesManager.Instance.GearIcons[tbGear.EquipIcon],
                           BackgroundSprite = ResourcesManager.Instance.GearRarityBgs[tbGear.EquipRarity],
                           Grade = tbGear.EquipGrade,
                           Count = InfVal.Parse(data.c),
                           RewardType = data.t,
                           bonusIconType = data.b,
                       });
                       break;
                   case 3:  // 카드
                       break;
                   case 4:  // 성물
                       break;
                   case 9: // 특성 포인트
                       list.Add(new ScrollDataModel()
                       {
                           Index = data.i,
                           IconType = 0,
                           IconSprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite("Icon_item_Trait"),
                           BackgroundSprite = ResourcesManager.Instance.GoodsTypeBgs[11],
                           Count = InfVal.Parse(data.c),
                           GoodsType = Numerals.Comma,
                           RewardType = data.t,
                           bonusIconType = data.b,
                       });
                       break;
                   case 10: // 계정경험치
                       list.Add(new ScrollDataModel()
                       {
                           Index = data.i,
                           IconType = 0,
                           IconSprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Common_Goods).GetSprite("Icon_item_Exp"),
                           BackgroundSprite = ResourcesManager.Instance.GoodsTypeBgs[11],
                           Count = InfVal.Parse(data.c),
                           GoodsType = Numerals.KMGT,
                           RewardType = data.t,
                           bonusIconType = data.b,
                       });
                       break;
                   case 100: // 전투 아이템
                       break;
                   case 101: // 균열 영웅
                       // idx = TableDataManager.Instance.data.Artifact.Single(t => t.Index == data.i).HeroCharacterID;
                       // ResourcesManager.Instance.Instantiate("UI_Popup_Dimension_01", Slots[i].transform)
                       //     .GetComponent<DimensionHeroReward>()
                       //     .SetData(new PopupDimension.RewradData() { heroIdx = idx });
                       break;
               }
               // 서버로 선택지 날린다.
           }

           return list;
       }
    }
}
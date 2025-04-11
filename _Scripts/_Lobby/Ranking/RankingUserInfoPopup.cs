using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using ProjectM.Battle;
using Sirenix.Utilities;
using TMPro;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM
{
    public class RankingUserInfoPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.RankingUserInfo;

        public UIButton infoBtn;
        public UIButton closeBtn;
        [Space(20)]
        public Transform costumeParent;
        public Transform scrollView;
        public List<Transform> activeSkillGradeList;
        public List<Transform> passiveSkillGradeList;
        [Space(20)]
        public TMP_Text levelTxt;
        public TMP_Text nameTxt;
        public TMP_Text attTxt;
        public TMP_Text hpTxt;
        public TMP_Text stageTxt;
        public TMP_Text timeTxt;
        public TMP_Text noRankTxt;
        [Space(20)]
        public List<ItemSlot> equipGearList;
        public List<Image> activeSkillList;
        public List<Image> passiveSkillList;
        public List<Image> heroList;
        public List<Image> heroGradeBgList;
        [Space(20)]
        public Image flagImg;
            
        private GameObject _costumeObj;
        private PlayerRank _playerRank;
        
        private List<Transform> _weaponList = new();

        public int _selectTabIndex;
        public float _scrollPosition;
        
        protected override void Init()
        {
        }

        private void Start()
        {
            infoBtn.AddEvent(ShowInfo);
            closeBtn.AddEvent(HidePopup);
            
            AddShowCallback(() =>
            {
                Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ =>
                {
                    costumeParent.SetActive(true);
                    scrollView.SetActive(true);
                    ReloadData();
                }).AddTo(this);
            });
            
            uiPopup.OnHideCallback.Event.AddListener(() =>
            {
                Observable.Timer(TimeSpan.FromSeconds(0.05f)).Subscribe(_ =>
                {
                    costumeParent.SetActive(false);
                }).AddTo(this);
                
                HidePopup();
            });
        }
        
        public void SetData(PlayerRank playerRank, int selectTabIndex, float scrollPosition)
        {
            _selectTabIndex = selectTabIndex;
            _scrollPosition = scrollPosition;
            _playerRank = playerRank;
        }
        
        private void ReloadData()
        {
            Costume costume;
            PlayerRankStatus playerRankStatus = null;
                
            if (_playerRank == null)
            {
                EquipCostume equipCostume = UserDataManager.Instance.EquipCostumes[0];
                costume = TableDataManager.Instance.data.Costume
                    .SingleOrDefault(t => t.Index == equipCostume.item_id);
            }
            else
            {
                playerRankStatus = _playerRank.p;
                costume = TableDataManager.Instance.data.Costume
                    .SingleOrDefault(t => t.Index == playerRankStatus.c);
            }
            
            string costumePrefab = "Icon_" + costume.Resource + "_idle";
            _costumeObj = Instantiate(ResourcesManager.Instance
                .GetResources<GameObject>(costumePrefab), costumeParent);
            
            Transform weaponPivot = _costumeObj.transform.Find("Weapon").Find("Weapon_Pivot");

            for (int i = 0; i < weaponPivot.childCount; ++i)
                _weaponList.Add(weaponPivot.GetChild(i));            
            
            if (playerRankStatus == null)
            {
                EquipGear equipGear;
                
                if (UserDataManager.Instance.EquipGears.TryGetValue(1, out equipGear))
                    SetPlayerWeapon(new DictionaryAddEvent<int, EquipGear>(1, equipGear), costume);
                
                GearPower gearPower = UserDataManager.Instance.gearInfo.GetEquipGearsPower();
                attTxt.SetText(gearPower.Attack.ToParseString());
                hpTxt.SetText(gearPower.Hp.ToParseString());
                levelTxt.SetText(LocaleManager.GetLocale("Common_Level", UserDataManager.Instance.userInfo.PlayerData.player.level));
                nameTxt.SetText(UserDataManager.Instance.userInfo.PlayerData.player.name);

                List<int> keyList = UserDataManager.Instance.gearInfo.EquipGears.Keys.ToList();
                keyList.Sort();

                for (int i = 0; i < keyList.Count; ++i)
                {
                    UserDataManager.Instance.gearInfo.EquipGears.TryGetValue(keyList[i], out equipGear);
                    UserDataManager.Instance.Gears.TryGetValue(equipGear.item_id, out var gear);                   

                    equipGearList[i].SetDataRankingEquipment(gear.GetGearEquipment(), gear.level);
                    equipGearList[i].SetActive(true);
                }
                
                passiveSkillList.ForEach(p => p.SetActive(false));
                activeSkillList.ForEach(a => a.SetActive(false));
                heroList.ForEach(h => h.SetActive(false));
                
                stageTxt.SetActive(false);
                timeTxt.SetActive(false);
                flagImg.SetActive(false);
                noRankTxt.SetActive(true);
                
                return;
            }

            for (int i = 0; i < playerRankStatus.g.Count; ++i)
            {
                Equipment equipWeapon = TableDataManager.Instance.data.Equipment
                    .Single(t => t.Index == playerRankStatus.g[i].i);
                EquipType equipType = TableDataManager.Instance.data.EquipType
                    .Single(t => t.Index == equipWeapon.EquipType);

                if (equipType.Slot == 1)
                {
                    EquipGear equipGear = new();
                    equipGear.equip_type = equipType.Slot;
                    equipGear.item_id = equipWeapon.Index;
                        
                    SetPlayerWeapon(new DictionaryAddEvent<int, EquipGear>(1, equipGear), costume);
                    break;
                }
            }
            
            attTxt.SetText(InfVal.Parse(playerRankStatus.d).ToParseString());
            hpTxt.SetText(InfVal.Parse(playerRankStatus.hp).ToParseString());
            levelTxt.SetText(LocaleManager.GetLocale("Common_Level", playerRankStatus.lv));
            nameTxt.SetText(playerRankStatus.n);
            
            Stage clearStage = TableDataManager.Instance.data.Stage.Single(t => t.Index == _playerRank.i);
            TimeSpan clearTime = TimeSpan.FromSeconds(_playerRank.t); 
        
            stageTxt.SetActive(true);
            timeTxt.SetActive(true);
            flagImg.SetActive(true);
            noRankTxt.SetActive(false);

            stageTxt.SetText(LocaleManager.GetLocale(clearStage.Name));
            timeTxt.SetText(LocaleManager.GetLocale("Remain_Time", clearTime.Minutes, clearTime.Seconds));
            
            equipGearList.ForEach(e => e.SetActive(false));

            List<PlayerRankGear> playerRankGears = playerRankStatus.g.OrderBy(p => p.i).ToList();

            for (int i = 0; i < playerRankGears.Count; i++)
            {
                Equipment equipment = TableDataManager.Instance.data.Equipment
                    .Single(t => t.Index == playerRankGears[i].i);
                
                equipGearList[i].SetDataRankingEquipment(equipment, playerRankGears[i].l);
                equipGearList[i].SetActive(true);
            }

            int pIndex = 0;
            int aIndex = 0;
            
            passiveSkillList.ForEach(p => p.SetActive(false));
            activeSkillList.ForEach(a => a.SetActive(false));

            for (int i = 0; i < playerRankStatus.s.Count; ++i)
            {
                SkillAI skillAI = TableDataManager.Instance.data.SkillAI
                    .Where(t => t.GroupID == playerRankStatus.s[i].i).Single(t => t.Level == playerRankStatus.s[i].l);
                SkillTypeGroup skillType = TableDataManager.Instance.data.SkillTypeGroup
                        .Single(t => t.SkillGroup == skillAI.SkillGroup);

                if (skillType.Type == 2)
                {
                    if (passiveSkillList.Count <= pIndex)
                        continue;
                        
                    passiveSkillList[pIndex].sprite = ResourcesManager.Instance
                        .GetAtlas(MyAtlas.BattleSkill).GetSprite(skillAI.Icon);
                    passiveSkillList[pIndex].SetActive(true);
                    
                    for (int j = 0; j < skillAI.Level; ++j)
                        passiveSkillGradeList[pIndex].GetChild(j).SetActive(true);
                    
                    pIndex++;
                }
                else
                {
                    if (activeSkillList.Count <= aIndex)
                        continue;
                    
                    activeSkillList[aIndex].sprite = ResourcesManager.Instance
                        .GetAtlas(MyAtlas.BattleSkill).GetSprite(skillAI.Icon);
                    activeSkillList[aIndex].SetActive(true);

                    if (skillAI.SkillGroup == 6)
                    {
                        activeSkillGradeList[aIndex].GetChild(activeSkillGradeList[aIndex].childCount - 1)
                            .SetActive(true);
                    }
                    else
                    {
                        for (int j = 0; j < skillAI.Level; ++j)
                            activeSkillGradeList[aIndex].GetChild(j).SetActive(true);
                    }
                    
                    aIndex++;
                }
            }

            heroList.ForEach(h => h.SetActive(false));
            heroGradeBgList.ForEach(h => h.SetActive(false));

            for (int i = 0; i < playerRankStatus.h.Count; ++i)
            {
                var artifact = TableDataManager.Instance.data.Card
                    .Single(t => t.Index == playerRankStatus.h[i]);
                var artifactRarityType = TableDataManager.Instance.data.CardRarityType
                    .Single(t => t.Index == artifact.CardRarity);

                heroList[i].sprite = ResourcesManager.Instance
                    .GetAtlas(MyAtlas.BattleIcon).GetSprite(artifact.HeroCharacterIcon);
                heroList[i].SetActive(true);

                heroGradeBgList[i].sprite =
                    ResourcesManager.Instance.GetAtlas(MyAtlas.Equipment_Frame).GetSprite(artifactRarityType.RankingRarityColor);
                heroGradeBgList[i].SetActive(true);
            }
        }
        
        private void SetPlayerWeapon(DictionaryAddEvent<int, EquipGear> equipGear, Costume costume)
        {
            if (equipGear.Key != 1 || _weaponList.IsNullOrEmpty()) return;
            
            _weaponList.ForEach(d => d.SetActive(false));

             Equipment equipment = equipGear.Value.GetEquipment();

            if (costume.TargetEquipType == equipment.EquipType)
            {
                _weaponList[_weaponList.Count - 1].SetActive(true);
            }
            else
            {
                _weaponList[equipment.EquipType - 1].SetActive(true);
                _weaponList[equipment.EquipType - 1].GetComponent<SpriteRenderer>().sprite
                    = ResourcesManager.Instance.GearIcons.GetValueOrDefault(equipment.EquipIcon);
            }
        }

        private void ShowInfo()
        {
            PopupConfirm popup = UIManager.Instance.Get(PopupName.Common_Confirm) as PopupConfirm;
            
            if (popup)
            {
                popup.SetOverrideSorting(10005);
                popup.InitBuilder()
                    .SetTitle(LocaleManager.GetLocale("UserInfo_Guide_Title"))
                    .SetMessage(LocaleManager.GetLocale("UserInfo_Guide_Desc"))
                    .SetCloseButton(popup.Hide)
                    .SetYesButton(popup.Hide, LocaleManager.GetLocale("UI_Key_011"))
                    .Build();
            }
        }
        
        private void HidePopup()
        {
            RankingPopup rankingPopup = UIManager.Instance.GetPopup(PopupName.Ranking) as RankingPopup;
            if (rankingPopup == false)
                rankingPopup = UIManager.Instance.Get(PopupName.Ranking) as RankingPopup;

            rankingPopup.SetData(_selectTabIndex, _scrollPosition);
            rankingPopup.Show();

            Hide();
        }
    }
}
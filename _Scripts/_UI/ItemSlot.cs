using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Reactor;
using ProjectM;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public List<RectTransform> gradeList;
    [Space(20)]
    public Transform icon;
    public Transform bonusIcon;
    public Transform costumeGrade;
    public Transform redDot;
    [Space(20)]
    public Progressor countProgressBar;
    [Space(20)]
    public Image defaultBg;
    public Image rarityBg;
    public Image progressBg;
    public Image progressFill;
    public Image costumeGradeBg;
    [Space(20)]
    public TMP_Text countTxt;
    public TMP_Text levelUpperTxt;
    public TMP_Text levelLowerTxt;
    public TMP_Text countProgressTxt;
    public TMP_Text costumeGradeTxt; 
    
    private Image[] _iconImgs;
    private Transform[] _gearIcons;
    private Gear _gear;
    
    private void Awake()
    {
        // int childCount = icon.childCount;
        //
        // _gearIcons = new Transform[childCount];
        // _iconImgs = new Image[childCount];
        //
        // for (int i = 0; i < icon.childCount; ++i)
        // {
        //     _gearIcons[i] = icon.GetChild(i);
        //     _iconImgs[i] = _gearIcons[i].GetComponentInChildren<Image>();
        //
        //     _gearIcons[i].SetActive(false);
        // }

        SetData();
    }

    public void SetData()
    {
        int childCount = icon.childCount;

        if (_gearIcons != null && _iconImgs != null) return;
        
        _gearIcons = new Transform[childCount];
        _iconImgs = new Image[childCount];

        for (int i = 0; i < icon.childCount; ++i)
        {
            _gearIcons[i] = icon.GetChild(i);
            _iconImgs[i] = _gearIcons[i].GetComponentInChildren<Image>();

            _gearIcons[i].SetActive(false);
        }
    }

    public void SetGearRedDot()
    {
        if (_gear == null)
            redDot.SetActive(false);
        else
            redDot.SetActive(_gear.IsShowRedDot());
    }

    public void SetDataRankingEquipment(Equipment equipment, int level)
    {
        _gearIcons.ForEach(g => g.SetActive(false));
        _gearIcons[equipment.EquipType].SetActive(true);

        _iconImgs[equipment.EquipType].sprite =
            ResourcesManager.Instance.GearIcons.GetValueOrDefault(equipment.EquipIcon);
        
        rarityBg.sprite = ResourcesManager.Instance.GearRarityBgs
            .GetValueOrDefault(equipment.EquipRarity);

        gradeList.ForEach(d => d.SetActive(false));
        for (int i = 0; i <= 4 - equipment.EquipGrade; ++i)
            gradeList[i].SetActive(true);
        
        levelLowerTxt.SetActive(true);
        levelLowerTxt.SetText(LocaleManager.GetLocale("Common_Level", level));
    }

    public void SetDataEquipment(Equipment equipment, Gear composeAllNewGear = null)
    {
        if (UserDataManager.Instance.Gears.TryGetValue(equipment.Index, out var gear))
            _gear = gear;
        else
            _gear = null;
        
        _gearIcons.ForEach(g => g.SetActive(false));
        _gearIcons[equipment.EquipType].SetActive(true);

        _iconImgs[equipment.EquipType].sprite =
            ResourcesManager.Instance.GearIcons.GetValueOrDefault(equipment.EquipIcon);
        
        rarityBg.sprite = ResourcesManager.Instance.GearRarityBgs
            .GetValueOrDefault(equipment.EquipRarity);

        gradeList.ForEach(d => d.SetActive(false));
        for (int i = 0; i <= 4 - equipment.EquipGrade; ++i)
            gradeList[i].SetActive(true);

        int maxRarity = TableDataManager.Instance.data.RarityType.Max(t => t.Index);
        int maxGrade = TableDataManager.Instance.data.Equipment
            .Select(t => t.EquipGrade).Distinct().Min();

        if (composeAllNewGear != null)
            _gear = composeAllNewGear;

        if (_gear == null)
        {
            _iconImgs[equipment.EquipType].color = new Color(50 / 255f, 50 / 255f, 50 / 255f);

            levelLowerTxt.SetActive(false);

            countProgressTxt.SetText(LocaleManager.GetLocale(
                "Common_Count", 0, equipment.MergeMaterialValue));
            countProgressBar.SetProgressAt(0);

            if (maxRarity <= equipment.EquipRarity && equipment.EquipGrade <= maxGrade)
                countProgressTxt.SetText("0");
        }
        else
        {
            _iconImgs[equipment.EquipType].color = new Color(255 / 255f, 255 / 255f, 255 / 255f);

            levelLowerTxt.SetActive(true);
            levelLowerTxt.SetText(LocaleManager.GetLocale("Common_Level", _gear.level));

            countProgressTxt.SetText(LocaleManager.GetLocale(
                "Common_Count", 
                _gear.count,
                equipment.MergeMaterialValue));

            float gearCount = _gear.count;
            float mergeMaterialValue = equipment.MergeMaterialValue;

            countProgressBar.SetProgressAt(gearCount / mergeMaterialValue);

            if (maxRarity <= equipment.EquipRarity && equipment.EquipGrade <= maxGrade)
                countProgressTxt.SetText($"{_gear.count}");
        }
    }

    public void SetDataScrollDataModel(ScrollDataModel scrollDataModel)
    {
        levelUpperTxt.SetActive(false);
        levelLowerTxt.SetActive(false);
        countProgressBar.SetActive(false);
        countProgressTxt.SetActive(false);
        costumeGrade.SetActive(false);

        bonusIcon.SetActive(1 <= scrollDataModel.bonusIconType);
        
        _gearIcons.ForEach(d => d.SetActive(false));
        gradeList.ForEach(d => d.SetActive(false));
        
        rarityBg.sprite = scrollDataModel.BackgroundSprite;
        
        int iconIndex;

        if (scrollDataModel.RewardType == 6)
        {
            iconIndex = _gearIcons.Length - 1;
            
            countTxt.SetActive(false);
        }
        else
        {
            iconIndex = scrollDataModel.IconType;
            
            int gradeCount = TableDataManager.Instance.data.Equipment
                .Select(t => t.EquipGrade).Distinct().Max();
        
            if (1 <= scrollDataModel.IconType && scrollDataModel.IconType <= 10)
            {
                for (int i = 0; i <= gradeCount - scrollDataModel.Grade; ++i)
                    gradeList[i].SetActive(true);
            }
        
            countTxt.SetActive(true);
            
            if(scrollDataModel.GoodsType == Numerals.Comma)
                countTxt.SetText($"{scrollDataModel.Count.ToGoodsString()}");
            else if(scrollDataModel.GoodsType == Numerals.KMGT)
                countTxt.SetText($"{scrollDataModel.Count.ToGoldString()}"); 
        }

        _gearIcons[iconIndex].SetActive(true);
        _iconImgs[iconIndex].sprite = scrollDataModel.IconSprite;
    }

    public void SetDataCostume(Costume costume)
    {
        CostumeData costumeData = UserDataManager.Instance.gearInfo.Costumes
            .SingleOrDefault(u => u.item_id == costume.Index);

        int costumeIndex = _gearIcons.Length - 1;
        _gearIcons.ForEach(g => g.SetActive(false));
        _gearIcons[costumeIndex].SetActive(true);

        CostumeRarityType costumeRarityType = TableDataManager.Instance.data.CostumeRarityType
            .Single(t => t.Grade == costume.Grade);
        
        rarityBg.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Equipment_Frame).GetSprite(costumeRarityType.IconBackground);
        
        _iconImgs[costumeIndex].sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.Summon_Shop).GetSprite(costume.Icon);

        if (costumeData != null)
        {
            _iconImgs[costumeIndex].color = new Color(255 / 255f, 255 / 255f, 255 / 255f);
        }
        else
        {
            _iconImgs[costumeIndex].color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace ProjectM.Battle
{
    public class UIBattleCheat : View
    {
        public override ViewName ID { get; set; } = ViewName.Battle_Cheat;

        public List<GameObject> CheatView;
        public List<UIButton> mainButtons;
        public UIButton backBtn;
        public UIButton closeBtn;
        
        public Stat stat;
        private bool noDamage;

        public List<UIButton> EditSkillButtons;
        public List<UIButton> EditHeroButtons;
        public List<UIButton> EditItemButtons;
        public TMP_Dropdown skillDropdown;
        private List<string> skillList = new List<string>();
        protected override void Init()
        {

            var bk = BlackBoard.Instance;
            backBtn.AddEvent(Back);
            closeBtn.AddEvent(()=> Hide());
            mainButtons[0].AddEvent(ShowBattleInfo);
            mainButtons[1].AddEvent(PlayerLevelUp);
            mainButtons[2].AddEvent(PlayerNoDamage);
            mainButtons[3].AddEvent(EditSkill);
            mainButtons[4].AddEvent(EditHero);
            mainButtons[5].AddEvent(EditItem);
            mainButtons[6].AddEvent(EditBoss);
            mainButtons[7].AddEvent(EditTutorial);
            mainButtons[8].AddEvent(EditWaveMode);
            mainButtons[9].AddEvent(EditLevelUpUI);
            
            
            
            EditSkillButtons[0].AddEvent(() => EditSkillUp(1));
            EditSkillButtons[1].AddEvent(() => EditSkillUp(2));
            EditSkillButtons[2].AddEvent(() => EditSkillUp(3));
            EditSkillButtons[3].AddEvent(() => EditSkillUp(4));
            EditSkillButtons[4].AddEvent(() => EditSkillUp(5));
            // EditSkillButtons[5].AddEvent(() => EditSkillUp(BlackBoard.Instance.GetSkillSet().ChangeSkillID));
            // EditSkillButtons[6].AddEvent(() => EditSkillUp(101));
            // EditSkillButtons[7].AddEvent(() => EditSkillUp(104));
            // EditSkillButtons[8].AddEvent(() => EditSkillUp(105));
            // EditSkillButtons[9].AddEvent(() => EditSkillUp(106));
            // EditSkillButtons[10].AddEvent(() => EditSkillUp(206));
            // EditSkillButtons[11].AddEvent(() => EditSkillUp(207));
            // EditSkillButtons[12].AddEvent(() => EditSkillUp(209));
            // //EditSkillButtons[13].AddEvent(() => EditSkillUp(208));
            // //EditSkillButtons[14].AddEvent(() => EditSkillUp(210));
            // EditSkillButtons[15].AddEvent(() => EditSkillUp(107));
            // EditSkillButtons[16].AddEvent(() => EditSkillUp(108));
            // EditSkillButtons[17].AddEvent(() => EditSkillUp(102));
            // EditSkillButtons[18].AddEvent(() => EditSkillUp(103));
            // EditSkillButtons[19].AddEvent(() => EditSkillUp(110));
            // EditSkillButtons[20].AddEvent(() => EditSkillUp(111));
            
            
            
            EditHeroButtons[0].AddEvent(() => CreateHero(102));       // 오공
            EditHeroButtons[1].AddEvent(() => CreateHero(101));       // 팬더
            EditHeroButtons[2].AddEvent(() => CreateHero(103));       // 토끼
            EditHeroButtons[3].AddEvent(() => CreateHero(106));       // 
            EditHeroButtons[4].AddEvent(() => CreateHero(105));       // 
            EditHeroButtons[5].AddEvent(() => CreateHero(104));       // 
            EditHeroButtons[6].AddEvent(() => CreateHero(107));       // 
            
            EditItemButtons[0].AddEvent(() => CreateItem(3));       // 폭탄
            EditItemButtons[1].AddEvent(() => CreateItem(2));       // 자석
            EditItemButtons[2].AddEvent(() => CreateItem(1));       // 회복약
            
            skillDropdown.ClearOptions();
            
            foreach (var type in Enum.GetValues(typeof(SkillType)))
            {
                skillList.Add(type.ToString());
            }
            skillDropdown.AddOptions(skillList);
            skillDropdown.onValueChanged.AddListener(_t =>
            {
                Debug.Log(_t);
                Debug.Log(skillDropdown.value);
                Debug.Log(skillList[_t]);
            });
            
            
        }

        void Back()
        {
            CheatView.ForEach(t=> t.SetActive(t == CheatView[0]));
            backBtn.SetActive(false);
        }

        void ShowBattleInfo()
        {
            CheatView.ForEach(t=> t.SetActive(t == CheatView[1]));
            backBtn.SetActive(true);

            var p = PlayerManager.Instance.player;
            
            stat.Atk.SetText($"ATK : {p.attack.ToParseString()}");
            stat.Hp.SetText($"HP : {p.maxHealth.ToParseString()}");
            stat.Reduction.SetText($"재사용 감소 :{SkillSystem.Instance.calcCoolTime}%");
            stat.Speed.SetText($"이동속도 : {(p.speed / 10f)}");
            stat.CriticalRate.SetText($"치명타 확률 : {PlayerManager.Instance.player.criticalRatio}");
            stat.Critical.SetText($"치명타 데미지 : {(UserDataManager.Instance.gearInfo.GetEquipGearsPower().CriticalDmg*100f) + 100}%");
            stat.NormalDamage.SetText($"일반몹 피해량 : {(UserDataManager.Instance.gearInfo.GetEquipGearsPower().NormalMobDmg * 100f)}%");
            stat.BossDamage.SetText($"보스몹 피해량 : {(UserDataManager.Instance.gearInfo.GetEquipGearsPower().BossMobDmg * 100f) + SkillSystem.Instance.incValue.BossDamgage}%");
            stat.CommonActive.SetText($"공용 액티브 공격력 :{SkillSystem.Instance.common}%");
            stat.Reduce.SetText($"받는 피해 감소 :{SkillSystem.Instance.calcDmgReduce.ToString("0.00")}%");
            stat.Miss.SetText($"피해 무시 :{SkillSystem.Instance.calcMiss / 100f}%");

        }

        void PlayerLevelUp()
        {
            BattleManager.Instance.LevelUp();
        }

        
        void PlayerNoDamage()
        {
            noDamage = !noDamage;
            PlayerManager.Instance.player.IsNoDamage = noDamage;
        }
        
        void EditSkill()
        {
            CheatView.ForEach(t=> t.SetActive(t == CheatView[2]));
            backBtn.SetActive(true);
        }

        void EditHero()
        {
            CheatView.ForEach(t=> t.SetActive(t == CheatView[3]));
            backBtn.SetActive(true);
        }

        void EditItem()
        {
            CheatView.ForEach(t=> t.SetActive(t == CheatView[4]));
            backBtn.SetActive(true);
        }

        void EditBoss()
        {
            BattleManager.Instance.BossStart();
        }
        
        void EditTutorial()
        {
            UserDataManager.Instance.clientInfo.AllClear();
        }
        
        void EditWaveMode()
        {
            BattleManager.Instance.spawner.waveType = WaveType.Wave;
        }
        
        void EditLevelUpUI()
        {
            BattleManager.Instance.skipLevelUpUI = !BattleManager.Instance.skipLevelUpUI;
        }

        void EditSkillUp(int level)
        {
            var sk = SkillSystem.Instance;
            var skill = skillList[skillDropdown.value];
            sk.LevelUp((int)skill.ToEnum<SkillType>(), level);
        }
        
        void CreateHero(int idx)
        {
            var p = PlayerManager.Instance;
            var hero = p.AddHero(idx);
            if(hero)
                hero.SetStat();
        }

        void CreateItem(int idx)
        {
            var pos = PlayerManager.Instance.player.transform.position + MyMath.RandomCirclePoint(3);
            var item = BattleItemManager.Instance.DropItem(idx, pos);
            // item.transform.position = PlayerManager.Instance.player.transform.position + MyMath.RandomCirclePoint(3);
            // item.Init(idx);
        }
    }

    [Serializable]
    public class Stat
    {
        public TMP_Text Atk, Hp, Reduction, Speed, CriticalRate, Critical, NormalDamage, BossDamage,CommonActive,Reduce, Miss;
    }

}
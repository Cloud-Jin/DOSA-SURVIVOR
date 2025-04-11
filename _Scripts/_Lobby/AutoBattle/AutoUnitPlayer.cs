
using System.Collections.Generic;
using System.Linq;
using ProjectM.Battle;
using UnityEngine;

namespace ProjectM.AutoBattle
{
    public class AutoUnitPlayer : AutoUnit
    {
        private string[] weapon =
            { "None", "Weapon_Charm", "Weapon_Sword", "Weapon_Fan", "Weapon_Bow", "Weapon_Stick" };

        protected override void Awake()
        {
            
        }

        public void SetPlayer()
        {
            var costumeInfo = BlackBoard.Instance.GetCostume();
            for (int i = transform.childCount -1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            var _player = ResourcesManager.Instance.Instantiate(costumeInfo.Resource);
            _player.transform.SetParent(transform);
            _player.transform.localPosition = Vector3.zero;
           
            SetUnit();
        }

        public override void Init(Monster stat)
        {
            base.Init(stat);
            // 무기변경 or 코스튬 변경시 캐릭터 수정.
            // 캐릭터 생성
            // var costumeInfo = BlackBoard.Instance.GetCostume();
            // var _player = ResourcesManager.Instance.Instantiate(costumeInfo.Resource);
            // _player.transform.position = Vector3.zero;
            // _player.transform.SetParent(transform);

            var weaponInfo = UserDataManager.Instance.gearInfo.GetEquipWeaponInfo();
            for (int i = BodyParts["Weapon_Pivot"].childCount -1; i >= 0; i--)
            {
                DestroyImmediate(BodyParts["Weapon_Pivot"].GetChild(i).gameObject);
            }
            
            // 장착 무기생성
            var costumeInfo = BlackBoard.Instance.GetCostume();
            // var weaponInfo = BlackBoard.Instance.data.weaponInfo;
            if (costumeInfo.TargetEquipType == 0 || costumeInfo.TargetEquipType != weaponInfo.EquipType)
            {
                var weaponItem = ResourcesManager.Instance.Instantiate(weapon[weaponInfo.EquipType], BodyParts["Weapon_Pivot"]);
                weaponItem.GetComponent<SpriteRenderer>().sprite = ResourcesManager.Instance.GearIcons[weaponInfo.EquipIcon];
            }
            else
            {
                // if (costumeInfo.TargetEquipType == weaponInfo.EquipType)
                {
                    // 코스튬 장비로 변경
                    var weaponCostume = ResourcesManager.Instance.Instantiate(costumeInfo.ChangeEquipIcon, BodyParts["Weapon_Pivot"]);
                    // weaponCostume.GetComponent<SpriteRenderer>().sprite = ResourcesManager.Instance.GearIcons[weaponInfo.EquipIcon];
                }
            }
            
            // var weaponItem = ResourcesManager.Instance.Instantiate(weapon[weaponInfo.EquipType], BodyParts["Weapon_Pivot"]);
            // weaponItem.GetComponent<SpriteRenderer>().sprite = ResourcesManager.Instance.GearIcons[weaponInfo.EquipIcon];
            
            
            var tbSet = BlackBoard.Instance.GetSkillSet();
            var tbSkill = TableDataManager.Instance.data.AutoBattleSkillAI.First(t => t.TypeID == tbSet.ChangeSkillID);
            SetSkillData(tbSkill);

            unitId = stat.Index;
            attack = UserDataManager.Instance.gearInfo.GetEquipGearsPower().Attack;
            moveSpeed = TableDataManager.Instance.data.AutoBattleConfig.Single(t => t.Index == 18).Value;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

// 영웅 레벨업 스킬 아이콘 변경에 따른
// 영웅 아이콘 개별표시

namespace ProjectM.Battle
{
    public class HeroSkillUpViewer : MonoBehaviour//SerializedMonoBehaviour
    {
        public List<GameObject> heroCountUI;

        // [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        // public Dictionary<int, List<Image>> heroIcon = new();

        public List<Image> heroIcon1;
        public List<Image> heroIcon2;
        public List<Image> heroIcon3;
        public void SetData(List<Hero> data)
        {
            int count = data.Count;

            if (count == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            for (int i = 0; i < heroCountUI.Count; i++)
            {
                heroCountUI[i].SetActive( count - 1 == i);
            }

            var heroIconList = GetHeroIcon(count);

            for (int i = 0; i < heroIconList.Count; i++)
            {
                string icon = data[i].GetArtifact.HeroCharacterIcon;
                heroIconList[i].sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(icon);
            }
            
        }

        List<Image> GetHeroIcon(int count)
        {
            switch (count)
            {
                case 1:
                    return heroIcon1;
                case 2:
                    return heroIcon2;
                case 3:
                    return heroIcon3;
                default:
                    
                    break;
            }

            return null;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using AssetKits.ParticleImage;
using UnityEngine;

namespace ProjectM
{
    public class GoodsAttractionEffect : MonoBehaviour
    {
        public static GoodsAttractionEffect Instance;
        
        public List<Transform> rewardEffectList;

        public void Start()
        {
            Instance = this;
            
            rewardEffectList.ForEach(d =>
            {
                ParticleImage particleImage = d.GetChild(0).GetComponent<ParticleImage>();
                particleImage.onStop.AddListener(() => ParticleOnStop(d));
            });
        }
        
        public void ShowRewardEffect(List<ScrollDataModel> rewardDataList)
        {
            ScrollDataModel rewardExp = rewardDataList
                .SingleOrDefault(d => d.RewardType == 10 && d.Index == 1);
            
            if (rewardExp != null) rewardEffectList[2].SetActive(true);

            List<ScrollDataModel> rewards = rewardDataList
                .Where(d => d.IconType != 99).Distinct().ToList();
            
            rewards.ForEach(d =>
            {
                if (d.RewardType == 1 || d.RewardType == 5)
                {
                    switch (d.Index)
                    {
                        case 1:
                        case 2: // 다이아
                            rewardEffectList[1].SetActive(true);
                            break;
                        case 3: // 흑요석
                            rewardEffectList[5].SetActive(true);
                            break;
                        case 11: // 골드
                            rewardEffectList[0].SetActive(true);
                            break;
                        case 12: // 에너지
                            rewardEffectList[4].SetActive(true);
                            break;
                        case 13: // 무기 뽑기 티켓
                            rewardEffectList[6].SetActive(true);
                            break;
                        case 14: // 방어구 뽑기 티켓
                            rewardEffectList[7].SetActive(true);
                            break;
                        default:
                            rewardEffectList[3].SetActive(true);
                            break;
                    }
                }
                else if (d.RewardType == 2) // || d.RewardType == 6)
                {
                    rewardEffectList[3].SetActive(true);
                }
            });
        }
        
        private void ParticleOnStop(Transform rewardEffect)
        {
            rewardEffect.SetActive(false);
        }
    }
}
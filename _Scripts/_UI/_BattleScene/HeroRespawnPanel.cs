using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gbros.UniRx;
using ProjectM;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class HeroRespawnPanel : MonoBehaviour
{
    public Image HeroIcon, GrayOverlay;

    public void SetRespawn(int heroIndex, float respawnTime, Action OnComplete)
    {
        gameObject.SetActive(true);
        GrayOverlay.fillAmount = 1f;
        //HeroIcon
        var data = TableDataManager.Instance.GetCard(heroIndex);
        HeroIcon.sprite = ResourcesManager.Instance.GetAtlas(MyAtlas.BattleIcon).GetSprite(data.HeroCharacterIcon);
        
        var sequence = DOTween.Sequence();
        sequence.Append(GrayOverlay.DOFillAmount(0, respawnTime).SetEase(Ease.Linear))
            .SetAutoKill(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                OnComplete?.Invoke();
            });
    }

}

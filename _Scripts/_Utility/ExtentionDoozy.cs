using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager;
using Doozy.Runtime.UIManager.Components;
using ProjectM;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Unit = UniRx.Unit;

public static class ExtensionDoozy
{
    public static void AddEvent(this UIButton button, Action action)
    {
        button.onClickEvent.AddListener(()=>
        {
            if (!UIManager.Instance.EnableTouch) return;
            
            action?.Invoke();
            SoundManager.Instance.PlayFXUIButton();
        });
    }

    public static void AddBehavioursPointerClick(this UITab tab, Action action = null)
    {
        tab.behaviours.AddBehaviour(UIBehaviour.Name.PointerClick).Event.AddListener(() =>
        {
            action?.Invoke();
            SoundManager.Instance.PlayFXUIButton();
        });
    }
    
    
    public static IDisposable AddPointerEvent(this UIButton button, Action action)
    {
        return button.OnPointerClickAsObservable().Subscribe(t =>
        {
            if (!UIManager.Instance.EnableTouch) return;

            action?.Invoke();
            SoundManager.Instance.PlayFXUIButton();
        });
    }
    
    public static Canvas Highlight(this UIButton button)
    { 
        if (button.GetComponent<Canvas>() == null)
        {
            button.AddComponent<Canvas>();
        }
        var canvas = button.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "Guide";
        canvas.sortingOrder = 20001;
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                                          AdditionalCanvasShaderChannels.Normal |
                                          AdditionalCanvasShaderChannels.Tangent;
        
        return canvas;
    }
   
    public static void AddTutorialEvent(this UIButton button, Action action)
    {
        if (button.GetComponent<Canvas>() == null)
        {
            button.AddComponent<Canvas>();
        }
        var canvas = button.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "Guide";
        canvas.sortingOrder = 20001;
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                                          AdditionalCanvasShaderChannels.Normal |
                                          AdditionalCanvasShaderChannels.Tangent;
        var graphicRaycaster = button.AddComponent<GraphicRaycaster>();
        
        UnityAction add = null;
        
        add = () =>
        {
            Object.Destroy(graphicRaycaster);
            Object.Destroy(canvas);
            action?.Invoke();
            button.onClickEvent.RemoveListener(add);
        };

        button.onClickEvent.AddListener(add);
        
    }
    
    public static void AddTutorialEvent(this UIToggle button, Action action)
    {
        if (button.GetComponent<Canvas>() == null)
        {
            button.AddComponent<Canvas>();
        }
        var canvas = button.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "Guide";
        canvas.sortingOrder = 20001;
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                                          AdditionalCanvasShaderChannels.Normal |
                                          AdditionalCanvasShaderChannels.Tangent;
        
        var graphicRaycaster = button.AddComponent<GraphicRaycaster>();
        
        UnityAction add = null;
        add = () =>
        {
            Object.Destroy(graphicRaycaster);
            Object.Destroy(canvas);
            action?.Invoke();
            button.onClickEvent.RemoveListener(add);
        };

        button.onClickEvent.AddListener(add);
    }
    
    public static void ClearEvent(this UIButton button)
    {
        button.onClickEvent.RemoveAllListeners();
    }

    public static IObservable<Unit> OnLongPressAsObservable(this UIButton button, float pressSeconds = 0.2f)
    {
        // pressSeconds 당 Button 이벤트 발생
        return button
            .OnPointerDownAsObservable()
            .SelectMany(_ => button.UpdateAsObservable())
            .ThrottleFirst(TimeSpan.FromSeconds(pressSeconds))
            .TakeUntil(button.OnPointerExitAsObservable()) // 누른 상태에서 손가락이 버튼 영역에서 벗어나면 종료
            .TakeUntil(button.OnPointerUpAsObservable())
            .RepeatUntilDisable(button)
            .RepeatUntilDestroy(button)
            .AsUnitObservable();
    }
    
    public static IObservable<Unit> OnLongTapAsObservable(this UIButton button, float pressSeconds = 1f)
    {
        // pressSeconds 이후 버튼 이벤트 1회 발생
        return button
            .OnPointerDownAsObservable()
            .Throttle(TimeSpan.FromSeconds(pressSeconds))
            .TakeUntil(button.OnPointerExitAsObservable()) // 누른 상태에서 손가락이 버튼 영역에서 벗어나면 종료
            .TakeUntil(button.OnPointerUpAsObservable())
            .RepeatUntilDestroy(button)
            .AsUnitObservable();
    }

    // public static IObservable<Unit> OnClickAsObservableSafety(this UIButton button, float duplicateSafetySeconds = 1f, float pressSafetySeconds = 1f)
    // {
    //     return button
    //         // .OnClickAsObservable()
    //         .ThrottleFirst(TimeSpan.FromSeconds(duplicateSafetySeconds)) // 연타방지
    //         .SkipUntil(button.OnPointerDownAsObservable())
    //         .TakeUntil(button.OnLongTapAsObservable(pressSafetySeconds)) // 길게 누른 후 손가락을 떼도 탭 이벤트를 발행하지 않는다
    //         .RepeatUntilDestroy(button)
    //         .AsUnitObservable();
    // }
}
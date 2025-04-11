using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorPlayer : MonoBehaviour
{
    Animator _animator;
    Action _beginCallback = null;
    Action _midCallback = null;
    Action _endCallback = null;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public Animator Animator => _animator;

    public void Play(string stateName, Action beginCallback = null, Action midCallback = null, Action endCallback = null)
    {
        _animator.Play(stateName);
        _beginCallback = beginCallback;
        _midCallback = midCallback;
        _endCallback = endCallback;
    }

    public void Play(string stateName, Action endCallback)
    {
        _animator.Play(stateName);
        _endCallback = endCallback;
    }
    
    public void PlayMidEnd(string stateName, Action midCallback, Action endCallback)
    {
        _animator.Play(stateName);
        _midCallback = midCallback;
        _endCallback = endCallback;
    }

    //Animation Event
    public void OnBeginEvent()
    {
        _beginCallback?.Invoke();
        _beginCallback = null;
    }

    public void OnMidEvent()
    {
        _midCallback?.Invoke();
        _midCallback = null;
    }

    public void OnEndEvent()
    {
        // Debug.Log("Animaton End Event");
        _endCallback?.Invoke();
        _endCallback = null;
    }
}

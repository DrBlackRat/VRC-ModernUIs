using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class TweenAnimCurveTest : UdonSharpBehaviour
{
    [Header("Animation")]
    public AnimationCurve curve;
    public float duration;
    
    [Header("UI")]
    public Slider slider;
    public TextMeshProUGUI text;
    
    [Header("Manual Input")]
    public Slider inputSlider;

    [NonSerialized] public float tweenValue;
    private VRCTweenHandle tween;

    public void ManualInput()
    {
        tweenValue = inputSlider.value;
        AnimUpdate();
    }

    public void StartAnim()
    {
        VRCTween.Kill(tween);
        tween = VRCTween.TweenFloat(
            0f, 
            01f, 
            duration, 
            (IUdonEventReceiver)this, 
            nameof(tweenValue), 
            nameof(AnimUpdate), 
            VRCTweenEase.Linear
            );
        VRCTween.OnComplete(tween, (IUdonEventReceiver)this, nameof(AnimComplete));
    }

    public void AnimUpdate()
    {
        var smoothedTransitionValue = curve.Evaluate(tweenValue);
        slider.SetValueWithoutNotify(smoothedTransitionValue);

        text.text = $"Transition Value: {tweenValue.ToString("0.000")}\nSmooth Trans Value: {smoothedTransitionValue.ToString("0.000")}";
    }
    
    public void AnimComplete()
    {
        
    }
}

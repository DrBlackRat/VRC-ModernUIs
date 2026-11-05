using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;


namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Selector : UdonSharpBehaviour
    {
        [Tooltip("If enabled the Color and Animation Settings can be overriden, otherwise the defaults provided by the Selector UI will be used.")]
        public bool overrideDefaults;

        [Tooltip("If the size of the Selector should be fixed, otherwise the size of the button it's transitioning to will be used.")]
        [SerializeField] protected bool fixedSize;
        [Tooltip("If the rounding of the Selector should be fixed, otherwise the rounding of the button it's transitioning to will be used.")]
        [SerializeField] protected bool fixedRounding;
        
        [SerializeField] protected AnimationCurve smoothingCurve;
        [SerializeField] protected float movementDuration;
        
        protected Color enabledColor;
        protected Color disabledColor;
        
        // Grabbed at Start
        private RectTransform selectorTransform;
        private Image selectorImage;
        
        // Provided by Main Script
        private SelectorUI selectorUI;
        
        [NonSerialized] public float selectorPixelPerUnit; // Set by Tweening
        private VRCTweenHandle posTween;
        private VRCTweenHandle sizeTween;
        private VRCTweenHandle roundingTween;

        public void _Setup(SelectorUI newSelectorUI)
        {
            selectorUI = newSelectorUI;

            selectorTransform = GetComponent<RectTransform>();
            selectorImage = GetComponent<Image>();
            SetupTween();
        }

        private void SetupTween()
        {
            posTween = gameObject.TweenLocalPosition(new Vector3(), movementDuration, VRCTweenEase.None)
                .SetEase(smoothingCurve)
                .Pause();
            
            sizeTween = selectorTransform.TweenSizeDelta(new Vector2(), movementDuration, VRCTweenEase.None)
                .SetEase(smoothingCurve)
                .Pause();
            
            roundingTween = VRCTween.TweenFloat(0f, 1f, movementDuration,this , nameof(selectorPixelPerUnit), nameof(_TransitionRounding), VRCTweenEase.None)
                .SetEase(smoothingCurve)
                .Pause();
        }

        public void _SetDefaults(AnimationCurve newSmoothingCurve, float newMovementDuration)
        {
            if(overrideDefaults) return;
            smoothingCurve = newSmoothingCurve;
            movementDuration = newMovementDuration;
        }

        public void _SetDefaultColors(Color newEnabledColor, Color newDisabledColor)
        {
            enabledColor = newEnabledColor;
            disabledColor = newDisabledColor;
        }

        public void _SetEnabled(bool selectorEnabled)
        {
            selectorImage.color = selectorEnabled ? enabledColor : disabledColor;
        }
        
        public void _MoveTo(SelectorUIButton button)
        {
            posTween.ChangeEndValue(button.LocalPos(), true)
                    .Restart();

            if (!fixedSize)
            {
                sizeTween.ChangeEndValue(button.SelectedSize(), true)
                    .Restart(); 
            }
            
            if (!fixedRounding)
            {
                roundingTween.ChangeEndValue(button.SelectedCornerRadius(), true)
                    .Restart();
            }
                
        }
        
        public void _TransitionRounding()
        {
            selectorImage.pixelsPerUnitMultiplier = selectorPixelPerUnit;
        }
    }
}

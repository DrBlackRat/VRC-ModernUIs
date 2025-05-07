using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Selector : UdonSharpBehaviour
    {
        [Tooltip("If enabled the Color and Animation Settings can be overriden, otherwise the defaults provided by the Selector UI will be used.")]
        public bool overrideDefaults;
        
        [SerializeField] protected AnimationCurve smoothingCurve;
        [SerializeField] protected float movementDuration;
        
        protected Color enabledColor;
        protected Color disabledColor;
        
        // Grabbed at Start
        private RectTransform selectorTransform;
        private Image selectorImage;
        
        // Provided by Main Script
        private SelectorUI selectorUI;
        
        // Animation
        private bool animate;
        private float animationElapsedTime;

        private SelectorUIButton selectorUIButton;
        
        private Vector3 prevPos;
        private Vector3 newPos;

        private Vector2 prevSize;
        private Vector2 newSize;

        private float prevCorner;
        private float newCorner;

        public void _Setup(SelectorUI newSelectorUI)
        {
            selectorUI = newSelectorUI;

            selectorTransform = GetComponent<RectTransform>();
            selectorImage = GetComponent<Image>();
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
            selectorUIButton = button;
            animate = false;
            SendCustomEventDelayedFrames(nameof(_StartAnimation), 0);
        }
        
        // Is called one frame delayed to allow the update loop to stop.
        public void _StartAnimation()
        {
            animate = true;
            animationElapsedTime = 0f;
            
            prevPos = selectorTransform.localPosition;;
            newPos = selectorUIButton.LocalPos();

            prevSize = selectorTransform.sizeDelta;
            newSize = selectorUIButton.SelectedSize();

            prevCorner = selectorImage.pixelsPerUnitMultiplier;
            newCorner = selectorUIButton.SelectedCornerRadius();
            
            _CustomUpdate();
        }
        
        public void _CustomUpdate()
        {
            if (!animate) return;
            AnimateUI();
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }
        
        private void AnimateUI()
        {
            animationElapsedTime += Time.deltaTime;
            var percentageComplete = animationElapsedTime / movementDuration;
            var smoothPercentageComplete = smoothingCurve.Evaluate(percentageComplete);

            selectorTransform.localPosition = Vector3.LerpUnclamped(prevPos, newPos, smoothPercentageComplete);
            selectorTransform.sizeDelta = Vector2.LerpUnclamped(prevSize, newSize, smoothPercentageComplete);
            selectorImage.pixelsPerUnitMultiplier = Mathf.LerpUnclamped(prevCorner, newCorner, smoothPercentageComplete);
            
            if (percentageComplete >= 1f)
            {
                animationElapsedTime = 0f;
                animate = false;
            }
        }
    }
}

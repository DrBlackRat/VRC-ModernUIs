using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Selector : UdonSharpBehaviour
    {
        // Grabbed at Start
        private RectTransform selectorTransform;
        private Image selectorImage;
        
        // Provided by Main Script
        private SelectorUI selectorUI;
        private AnimationCurve smoothingCurve;
        private float movementDuration;
        
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

        public void Setup(SelectorUI newSelectorUI, AnimationCurve newSmoothingCurve, float newMovementDuration)
        {
            selectorUI = newSelectorUI;
            smoothingCurve = newSmoothingCurve;
            movementDuration = newMovementDuration;

            selectorTransform = GetComponent<RectTransform>();
            selectorImage = GetComponent<Image>();
            
            newPos = selectorTransform.localPosition;
            newSize = selectorTransform.sizeDelta;
            newCorner = selectorImage.pixelsPerUnitMultiplier;
        }

        public void _SetColor(Color color)
        {
            selectorImage.color = color;
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
            
            prevPos = newPos;
            newPos = selectorUIButton.LocalPos();

            prevSize = newSize;
            newSize = selectorUIButton.SelectedSize();

            prevCorner = newCorner;
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

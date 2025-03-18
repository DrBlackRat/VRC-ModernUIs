
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)]
    public class TabsUI : UdonSharpBehaviour
    {
        [Header("Settings:")] 
        [SerializeField] private int buttonId;
        [Space(10)]
        [SerializeField] private TabButton[] tabButtons;
        [SerializeField] private GameObject selector;
        [SerializeField] private TabSwitcher tabSwitcher;
        
        // UI Stuff
        [Header("Text Colors:")]
        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;
        
        [Header("Internals:")]

        
        [Header("UI Animation:")]
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;

        private RectTransform selectorTransform;
        
        private bool animateUI;
        private float movementElapsedTime;
        private Vector3 oldSelectorPos;

        private int prevButtonId;
        
        private void Start()
        {
            selectorTransform = selector.GetComponent<RectTransform>();

            for (int i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].Setup(normalColor, selectedColor, animationCurve, movementDuration, this, i);
            }
            
            _UpdateTab(buttonId, true);
        }
        public void _CustomUpdate()
        {
            if (!animateUI) return;
            AnimateUI(oldSelectorPos, tabButtons[buttonId].Position());
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }
        
        public void _UpdateTab(int newButtonId, bool skipCheck)
        {
            // Change State
            if (newButtonId == buttonId && !skipCheck) return;
            prevButtonId = buttonId;
            buttonId = newButtonId;

            tabSwitcher._SwitchTab(buttonId);
            
            // Button Transition
            tabButtons[prevButtonId]._UnSelect();
            tabButtons[buttonId]._Select();
            
            // UI Animation
            animateUI = true;
            movementElapsedTime = 0f;
            oldSelectorPos = selectorTransform.position;
            _CustomUpdate();
        }
        
        private void AnimateUI(Vector3 startPos, Vector3 endPos)
        {
            movementElapsedTime += Time.deltaTime;
            var percentageComplete = movementElapsedTime / movementDuration;
            var smoothPercentageComplete = animationCurve.Evaluate(percentageComplete);
            // Set Selector Position
            selectorTransform.position = Vector3.LerpUnclamped(startPos, endPos, smoothPercentageComplete);
            
            if (percentageComplete >= 1f)
            {
                movementElapsedTime = 0f;
                animateUI = false;
            }
        }
        
    }
}

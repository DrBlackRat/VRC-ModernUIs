using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIToggle : UdonSharpBehaviour
    {
        [Header("Settings:")]
        [Tooltip("This is the default state the toggle will be in.")]
        [SerializeField] private bool state = true;
        [Header("Toggle:")]
        [Tooltip("Object that should be toggled.")]
        [SerializeField] private GameObject toggleObj;
        [Space(10)]
        [Tooltip("Udon Behaviour that _TurnOn and _TurnOff will be send to.")]
        [SerializeField] private UdonBehaviour toggleBehaviour;
        [Tooltip("Turn on event that will be called on the Toggle Behaviour if the state changes to on.")]
        [SerializeField] private string onEventName = "_TurnOn";
        [Tooltip("Turn onf event that will be called on the Toggle Behaviour if the state changes to off.")]
        [SerializeField] private string offEventName = "_TurnOff";

        [Header("Persistence:")]
        [Tooltip("Turn on if this Toggle should be saved using Persistence.")]
        [SerializeField] private bool usePersistence = true;
        [Tooltip("Data Key that will be used to save / load this Setting, everything using Persistence should have a different Data Key.")]
        [SerializeField] private string dataKey = "CHANGE THIS";
        
        // UI Stuff
        [Header("Text Colors:")]
        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;
        
        [Header("Internals:")]
        [SerializeField] private GameObject selector;

        [SerializeField] private SelectorButton onButtom; 
        [SerializeField] private SelectorButton offButtom;
        
        [Header("UI Animation:")]
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;

        private RectTransform selectorTransform;
        
        private bool animateUI;
        private float movementElapsedTime;
        private Vector3 oldSelectorPos;
        
        private SelectorButton prevButton;
        private SelectorButton currentButton;
        private void Start()
        {
            selectorTransform = selector.GetComponent<RectTransform>();
            
            prevButton = onButtom;
            currentButton = onButtom;
            
            onButtom.Setup(normalColor, selectedColor, animationCurve, movementDuration);
            offButtom.Setup(normalColor, selectedColor, animationCurve, movementDuration);
            
            UpdateUI(state, true, true);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence) return;
            if (PlayerData.TryGetBool(player, dataKey, out bool value))
            {
                UpdateUI(value, true, false);
            }

        }
        
        public void _CustomUpdate()
        {
            if (!animateUI) return;
            if (state)
            {
                AnimateUI(oldSelectorPos, onButtom.Position());
            }
            else
            {
                AnimateUI(oldSelectorPos, offButtom.Position());
            }
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }

        public void _On()
        {
            UpdateUI(true, false, false);
        }

        public void _Off()
        {
            UpdateUI(false, false, false);
        }
        private void UpdateUI(bool newState, bool fromStorage, bool skipCheck)
        {
            // Change State
            if (newState == state && !skipCheck) return;
            state = newState;

            Toggle(state);
            
            if (usePersistence && !fromStorage) PlayerData.SetBool(dataKey, state);
            
            // Button Transition
            prevButton = currentButton;
            currentButton = state? onButtom : offButtom;
            
            prevButton._UnSelect();
            currentButton._Select();
            
            // UI Animation
            animateUI = true;
            movementElapsedTime = 0f; 
            oldSelectorPos = selectorTransform.position;
            _CustomUpdate();
        }

        private void Toggle(bool newState)
        {
            if (toggleObj) toggleObj.SetActive(newState);
            
            if (toggleBehaviour != null) toggleBehaviour.SendCustomEvent(newState ? onEventName : offEventName);
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


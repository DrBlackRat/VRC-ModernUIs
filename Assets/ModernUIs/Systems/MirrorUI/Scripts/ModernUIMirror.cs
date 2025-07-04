using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ModernUIMirror : UdonSharpBehaviour
    {
        [Header("Mirror Objects")]
        [SerializeField] private GameObject highMirror;
        [SerializeField] private GameObject lowMirror;
        [SerializeField] private GameObject playerMirror;
        
        [Header("Text Colors")]
        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;
        
        [Header("Internals")]
        [SerializeField] private GameObject selector;
        [SerializeField] private GameObject separator;

        [SerializeField] private SelectorButton highButton;
        [SerializeField] private SelectorButton lowButton;
        [SerializeField] private SelectorButton playerButton;
        [SerializeField] private SelectorButton offButton;
        
        [Header("UI Animation")]
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;

        private RectTransform selectorTransform;
        private ModernUIMirrorSeparator separatorScript;
        
        private bool animateUI;
        private float movementElapsedTime;
        private Vector3 oldSelectorPos;
        private MirrorUIState state = MirrorUIState.Off;
        
        private SelectorButton prevButton;
        private SelectorButton currentButton;

        private void Start()
        {
            selectorTransform = selector.GetComponent<RectTransform>();
            separatorScript = separator.GetComponent<ModernUIMirrorSeparator>();
            
            prevButton = offButton;
            currentButton = offButton;
            
            ToggleMirrors();
            
            highButton.Setup(null, normalColor, selectedColor, animationCurve, movementDuration);
            lowButton.Setup(null, normalColor, selectedColor, animationCurve, movementDuration);
            playerButton.Setup(null, normalColor, selectedColor, animationCurve, movementDuration);
            offButton.Setup(null, normalColor, selectedColor, animationCurve, movementDuration);
        }

        private void Update()
        {
            if (!animateUI) return;
            switch (state)
            {
                case MirrorUIState.High:
                    AnimateUI(oldSelectorPos, highButton.Position());
                    break;
                case MirrorUIState.Low:
                    AnimateUI(oldSelectorPos, lowButton.Position());
                    break;
                case MirrorUIState.Player:
                    AnimateUI(oldSelectorPos, playerButton.Position());
                    break;
                case MirrorUIState.Off:
                    AnimateUI(oldSelectorPos, offButton.Position());
                    break;
            }
        }
        // UI Input
        public void _High()
        {
            UpdateUI(MirrorUIState.High, highButton);
        }        
        public void _Low()
        {
            UpdateUI(MirrorUIState.Low, lowButton);
        }
        public void _Player()
        {
            UpdateUI(MirrorUIState.Player, playerButton);
        }
        public void _Off()
        {
            UpdateUI(MirrorUIState.Off, offButton);
        }
        
        private void UpdateUI(MirrorUIState newState, SelectorButton newButton)
        {
            // Change State
            if (newState == state && newState == MirrorUIState.Off) return;

            if (newState == state)
            {
                state = MirrorUIState.Off;
                newButton = offButton;
            }
            else
            {
                state = newState;
            }
            
            ToggleMirrors();
            
            // Button Transition
            prevButton = currentButton;
            currentButton = newButton;
            
            prevButton.UnSelect();
            currentButton.Select();
            
            // UI Animation
            animateUI = true;
            movementElapsedTime = 0f;
            oldSelectorPos = selectorTransform.position;
            separatorScript._UpdateSeparatorInfo(state);
        }

        private void ToggleMirrors()
        {
            switch (state)
            {
                case MirrorUIState.High:
                    highMirror.SetActive(true);
                    lowMirror.SetActive(false);
                    playerMirror.SetActive(false);
                    break;
                case MirrorUIState.Low:
                    highMirror.SetActive(false);
                    lowMirror.SetActive(true);
                    playerMirror.SetActive(false);
                    break;
                case MirrorUIState.Player:
                    highMirror.SetActive(false);
                    lowMirror.SetActive(false);
                    playerMirror.SetActive(true);
                    break;
                case MirrorUIState.Off:
                    highMirror.SetActive(false);
                    lowMirror.SetActive(false);
                    playerMirror.SetActive(false);
                    break;
            }
        }
        
        private void AnimateUI(Vector3 startPos, Vector3 endPos)
        {
            movementElapsedTime += Time.deltaTime;
            var percentageComplete = movementElapsedTime / movementDuration;
            var smoothPercentageComplete = animationCurve.Evaluate(percentageComplete);
            // Set Selector Position
            selectorTransform.position = Vector3.LerpUnclamped(startPos, endPos, smoothPercentageComplete);
            // Move Separator
            separatorScript._AnimateSeparator(smoothPercentageComplete);
            
            if (percentageComplete >= 1f)
            {
                movementElapsedTime = 0f;
                animateUI = false;
            }
        }
    }
    public enum MirrorUIState
    {
        High,
        Low,
        Player,
        Off
    }
}


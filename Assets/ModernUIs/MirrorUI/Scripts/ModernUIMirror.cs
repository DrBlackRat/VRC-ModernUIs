using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ModernUIMirror : UdonSharpBehaviour
    {
        [Header("Mirror Objects")]
        [SerializeField] private GameObject highMirror;
        [SerializeField] private GameObject lowMirror;
        [SerializeField] private GameObject playerMirror;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;
        
        [Header("Internals")]
        [SerializeField] private GameObject selector;
        [SerializeField] private GameObject highButtonObj;
        [SerializeField] private GameObject lowButtonObj;
        [SerializeField] private GameObject playerButtonObj;
        [SerializeField] private GameObject offButtonObj;
        
        [Header("Selector Movement")]
        [SerializeField] private AnimationCurve selectorCurve;
        [SerializeField] private float movementDuration;

        private RectTransform selectorTransform;
        
        private bool animateUI;
        private float movementElapsedTime;
        private Vector3 oldSelectorPos;
        private MirrorUIState state = MirrorUIState.Off;
        
        private Image prevButtonImage;
        private Image currentButtonImage;
        
        private Image highButtonImage;
        private Image lowButtonImage;
        private Image playerButtonImage;
        private Image offButtonImage;
        
        private Vector3 selectorHighButtonPos;
        private Vector3 selectorLowButtonPos;
        private Vector3 selectorPlayerButtonPos;
        private Vector3 selectorOffButtonPos;

        private void Start()
        {
            // Getting Transforms & Positions
            selectorTransform = selector.GetComponent<RectTransform>();
            
            selectorHighButtonPos = highButtonObj.GetComponent<RectTransform>().position;
            selectorLowButtonPos = lowButtonObj.GetComponent<RectTransform>().position;
            selectorPlayerButtonPos = playerButtonObj.GetComponent<RectTransform>().position;
            selectorOffButtonPos= offButtonObj.GetComponent<RectTransform>().position;
            
            // Getting Button Images
            highButtonImage = highButtonObj.GetComponent<Image>();
            lowButtonImage = lowButtonObj.GetComponent<Image>();
            playerButtonImage = playerButtonObj.GetComponent<Image>();
            offButtonImage = offButtonObj.GetComponent<Image>();
            
            prevButtonImage = offButtonImage;
            currentButtonImage = offButtonImage;
            
            ToggleMirrors();
        }

        private void Update()
        {
            if (!animateUI) return;
            switch (state)
            {
                case MirrorUIState.High:
                    AnimateUI(oldSelectorPos, selectorHighButtonPos);
                    break;
                case MirrorUIState.Low:
                    AnimateUI(oldSelectorPos, selectorLowButtonPos);
                    break;
                case MirrorUIState.Player:
                    AnimateUI(oldSelectorPos, selectorPlayerButtonPos);
                    break;
                case MirrorUIState.Off:
                    AnimateUI(oldSelectorPos, selectorOffButtonPos);
                    break;
            }
        }
        // UI Input
        public void _High()
        {
            UpdateUI(MirrorUIState.High, highButtonImage);
        }        
        public void _Low()
        {
            UpdateUI(MirrorUIState.Low, lowButtonImage);
        }
        public void _Player()
        {
            UpdateUI(MirrorUIState.Player, playerButtonImage);
        }
        public void _Off()
        {
            UpdateUI(MirrorUIState.Off, offButtonImage);
        }
        
        private void UpdateUI(MirrorUIState newState, Image newButtonImage)
        {
            // Change State
            if (newState == state && newState == MirrorUIState.Off) return;

            if (newState == state)
            {
                state = MirrorUIState.Off;
                newButtonImage = offButtonImage;
            }
            else
            {
                state = newState;
            }
            ToggleMirrors();
            
            // Color Transition
            prevButtonImage = currentButtonImage;
            currentButtonImage = newButtonImage;
            
            // Selector Movement
            animateUI = true;
            movementElapsedTime = 0f;
            oldSelectorPos = selectorTransform.position;
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
            var smoothPercentageComplete = selectorCurve.Evaluate(percentageComplete);
            // Set Selector Position
            selectorTransform.position = Vector3.Lerp(startPos, endPos, smoothPercentageComplete);
            // Set Button Colors
            prevButtonImage.color = Color.Lerp(selectedColor, normalColor, smoothPercentageComplete);
            currentButtonImage.color = Color.Lerp(normalColor, selectedColor, smoothPercentageComplete);
            
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


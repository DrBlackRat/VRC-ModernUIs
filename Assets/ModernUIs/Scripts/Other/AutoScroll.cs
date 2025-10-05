using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [RequireComponent(typeof(ScrollRect))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoScroll : UdonSharpBehaviour
    {
        [Tooltip("Units in the UI to move per second.")]
        [SerializeField] private float unitsPerSec = 25f;
        
        [Tooltip("Curve used for smoothing the start & end.")]
        [SerializeField] private AnimationCurve startEndCurve;
        [Range(0f, 0.5f)]
        [SerializeField] private float startPercentage = 0.25f;
        [Range(0f, 0.5f)]
        [SerializeField] private float endPercentage = 0.25f;

        [Tooltip("If the Scroll Info should be updated continuously. Needs to be enabled for anything that can change it's size later on.")]
        [SerializeField] private bool autoUpdateScrollInfo = true;
        [Tooltip("Scroll Info Update Interval in seconds."), Range(0.5f, 5f)]
        [SerializeField] private float scrollInfoUpdateInterval = 2.5f;

        private ScrollRect scrollRect;
        
        private bool autoScroll;
        private float normalizedSpeed;
        private ScrollState state = ScrollState.ScrollDown;


        private void Start()
        {
            scrollRect = GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 1f;
            _UpdateScrollInfo();
        }

        private void Update()
        {
            AnimateScroll(); 
        }

        public void _UpdateScrollInfo()
        {
            var scrollHeight= scrollRect.content.rect.height - scrollRect.viewport.rect.height;
            autoScroll = scrollHeight > 0;

            if (autoScroll)
                normalizedSpeed = scrollHeight / unitsPerSec;
            
            if (autoUpdateScrollInfo)
                SendCustomEventDelayedSeconds(nameof(_UpdateScrollInfo), scrollInfoUpdateInterval);
        }

        private void AnimateScroll()
        {
            if (!autoScroll) return;
            var normalizedPos= scrollRect.verticalNormalizedPosition;
            var invertedNormalizedPos = 1f - normalizedPos;
            
            

            if (state == ScrollState.ScrollDown)
            {
                if (invertedNormalizedPos < startPercentage)
                {
                    var percentageComplete = Mathf.InverseLerp(0f, startPercentage, invertedNormalizedPos);
                    var curveMod = startEndCurve.Evaluate(percentageComplete);
                    scrollRect.verticalNormalizedPosition -= normalizedSpeed * Time.deltaTime * curveMod;
                }
                else if (normalizedPos < endPercentage)
                {
                    var percentageComplete = Mathf.InverseLerp(0f, endPercentage, normalizedPos);
                    var curveMod = startEndCurve.Evaluate(percentageComplete);
                    scrollRect.verticalNormalizedPosition -= normalizedSpeed * Time.deltaTime * curveMod;
                }
                else
                {
                    scrollRect.verticalNormalizedPosition -= normalizedSpeed * Time.deltaTime;
                }
                
                if (normalizedPos <= 0f)
                {
                    state = ScrollState.ScrollUp;
                }
            }
            else
            {
                if (invertedNormalizedPos < startPercentage)
                {
                    var percentageComplete = Mathf.InverseLerp(0f, startPercentage, invertedNormalizedPos);
                    var curveMod = startEndCurve.Evaluate(percentageComplete);
                    scrollRect.verticalNormalizedPosition += normalizedSpeed * Time.deltaTime * curveMod;
                }
                else if (normalizedPos < endPercentage)
                {
                    var percentageComplete = Mathf.InverseLerp(0f, endPercentage, normalizedPos);
                    var curveMod = startEndCurve.Evaluate(percentageComplete);
                    scrollRect.verticalNormalizedPosition += normalizedSpeed * Time.deltaTime * curveMod;
                }
                else
                {
                    scrollRect.verticalNormalizedPosition += normalizedSpeed * Time.deltaTime;
                }
                
                if (normalizedPos >= 1f)
                {
                    state = ScrollState.ScrollDown;
                }
            }
        }
    }

    internal enum ScrollState
    {
        ScrollDown,
        ScrollUp,
    }
}


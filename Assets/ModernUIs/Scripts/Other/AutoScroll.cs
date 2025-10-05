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
        [SerializeField] private float unitsPerSec;
        
        [Tooltip("Curve used for something the start & end.")]
        [SerializeField] private AnimationCurve startEndCurve;
        [SerializeField] private float startTime = 1f;
        [SerializeField] private float endTime = 1f;

        [Tooltip("If the Scroll Info should be updated continuously. Needs to be enabled for anything that can change it's size later on.")]
        [SerializeField] private bool autoUpdateScrollInfo = true;
        [Tooltip("Scroll Info Update Interval in seconds."), Range(0.5f, 5f)]
        [SerializeField] private float scrollInfoUpdateInterval = 2.5f;

        private ScrollRect scrollRect;
        
        private bool autoScroll;
        private ScrollState state = ScrollState.ScrollDown;
        private float animationElapsedTime;
        private float movementDuration;

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
                movementDuration = scrollHeight / unitsPerSec;
            
            if (autoUpdateScrollInfo)
                SendCustomEventDelayedSeconds(nameof(_UpdateScrollInfo), scrollInfoUpdateInterval);
        }

        private void AnimateScroll()
        {
            if (!autoScroll) return;
            
            animationElapsedTime += Time.deltaTime;
            var percentageComplete = animationElapsedTime / movementDuration;
            var invertPercentageComplete = 1 - percentageComplete;
            

            if (state == ScrollState.ScrollDown)
            {
                if (animationElapsedTime < startTime)
                {
                    /*
                    var startPercentage = animationElapsedTime / startTime;
                    var curvePercentage = startEndCurve.Evaluate(startPercentage);
                    scrollRect.verticalNormalizedPosition =
                    */
                    scrollRect.verticalNormalizedPosition = invertPercentageComplete;
                }
                else
                {
                    scrollRect.verticalNormalizedPosition = invertPercentageComplete;
                }
                
                if (percentageComplete >= 1f)
                {
                    state = ScrollState.ScrollUp;
                    animationElapsedTime = 0f;
                    return;
                }
            }
            else
            {
                var invertedElapsedTime = movementDuration - animationElapsedTime;
                if (invertedElapsedTime < endTime)
                {
                    /*
                    var endPercentage = invertedElapsedTime / endTime;
                    var curvePercentage = startEndCurve.Evaluate(endPercentage);
                    */
                    scrollRect.verticalNormalizedPosition = percentageComplete;
                }
                else
                {
                    scrollRect.verticalNormalizedPosition = percentageComplete;
                }
                
                if (percentageComplete >= 1f)
                {
                    state = ScrollState.ScrollDown;
                    animationElapsedTime = 0f;
                    return;
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


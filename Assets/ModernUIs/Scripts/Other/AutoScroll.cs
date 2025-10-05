using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
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
        [SerializeField] private float unitsPerSec = 50f;
        
        [FormerlySerializedAs("startEndCurve")]
        [Tooltip("Curve used for smoothing the start & end.")]
        [SerializeField] private AnimationCurve smoothingCurve;
        [Range(0f, 10f)]
        [SerializeField] private float startTime = 5f;
        //[Range(0f, 5f)] [SerializeField] private float endTime = 2.5f;

        [Tooltip("If the Scroll Info should be updated continuously. Needs to be enabled for anything that can change it's size later on.")]
        [SerializeField] private bool autoUpdateScrollInfo = true;
        [Tooltip("Scroll Info Update Interval in seconds."), Range(0.5f, 5f)]
        [SerializeField] private float scrollInfoUpdateInterval = 2.5f;

        private ScrollRect scrollRect;
        
        private bool autoScroll;
        private float normalizedSpeed;
        private float elapsedTime;
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
                normalizedSpeed = unitsPerSec / scrollHeight;
            
            if (autoUpdateScrollInfo)
                SendCustomEventDelayedSeconds(nameof(_UpdateScrollInfo), scrollInfoUpdateInterval);
        }

        private void AnimateScroll()
        {
            if (!autoScroll) return;

            elapsedTime += Time.deltaTime;
            var normalizedPos= scrollRect.verticalNormalizedPosition;
            //var invertedNormalizedPos = 1f - normalizedPos;
            //var remainingTime = duration - elapsedTime;
            
            var moveSpeed = 0f;
            if (elapsedTime < startTime)
            {
                var percentageComplete = Mathf.InverseLerp(0f, startTime, elapsedTime);
                 moveSpeed = normalizedSpeed * Time.deltaTime * smoothingCurve.Evaluate(percentageComplete);
            }
            /*
            else if (remainingTime < endTime)
            {
                var percentageComplete = Mathf.InverseLerp(0f, endTime, remainingTime);
                moveSpeed = normalizedSpeed * Time.deltaTime * startEndCurve.Evaluate(percentageComplete);
            }
            */
            else
            {
                moveSpeed = normalizedSpeed * Time.deltaTime;
            }
            
            if (state == ScrollState.ScrollDown)
            {
                scrollRect.verticalNormalizedPosition -= moveSpeed;
                
                if (normalizedPos <= 0f)
                {
                    state = ScrollState.ScrollUp;
                    elapsedTime = 0f;
                }
            }
            else
            {
                scrollRect.verticalNormalizedPosition += moveSpeed;
                
                if (normalizedPos >= 1f)
                {
                    state = ScrollState.ScrollDown;
                    elapsedTime = 0f;
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


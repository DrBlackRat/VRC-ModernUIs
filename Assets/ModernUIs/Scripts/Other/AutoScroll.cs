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
        [SerializeField] private float unitsPerSec = 25f;
        [Tooltip("Whether to start scrolling from the top or bottom.")]
        [SerializeField] private StartAt startAt = StartAt.Top;

        [Tooltip("Curve used for smoothing the start & end.")] [SerializeField]
        private AnimationCurve startCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Range(0f, 10f)] [Tooltip("Time spent inside the curve when starting.")]
        [SerializeField] private float startCurveDuration = 2.5f;
        [Range(0f, 10f)] [Tooltip("Time spent waiting at the end or when a user is manually scrolling.")]
        [SerializeField] private float waitDuration = 2.5f;

        [Tooltip("If the Scroll Info should be updated continuously. Needs to be enabled for anything that can change it's size later on.")]
        [SerializeField] private bool autoUpdateScrollInfo = true;
        [Tooltip("Scroll Info Update Interval in seconds."), Range(0.5f, 5f)]
        [SerializeField] private float scrollInfoUpdateInterval = 2.5f;

        private bool scroll = true;
        /// <summary>
        /// Enabled / Disables the Auto Scrolling
        /// </summary>
        public bool Scroll
        {
            get => scroll;
            set
            {
                if (value)
                {
                    _UpdateScrollInfo();
                }
                else
                {
                    elapsedTime = 0f;
                }
                scroll = value;
            }
        }

        private ScrollRect scrollRect;
        
        private bool enoughHeight;
        private float normalizedSpeed;
        
        private float elapsedTime;
        private bool wait;
        
        private ScrollState state = ScrollState.ScrollDown;
        
        private void Start()
        {
            scrollRect = GetComponent<ScrollRect>();

            switch (startAt)
            {
                case StartAt.Top:
                    scrollRect.verticalNormalizedPosition = 1f;
                    state = ScrollState.ScrollDown;
                    break;
                case StartAt.Bottom:
                    scrollRect.verticalNormalizedPosition = 0f;
                    state = ScrollState.ScrollUp;
                    break;
            }

            _UpdateScrollInfo();
        }

        private void Update()
        {
            AnimateScroll(); 
        }

        /// <summary>
        /// Enabled Auto Scrolling
        /// </summary>
        public void _EnableAutoScroll()
        {
            Scroll = true;
        }

        /// <summary>
        /// Disables Auto Scrolling
        /// </summary>
        public void _DisableAutoScroll()
        {
            Scroll = false;
        }

        /// <summary>
        /// Updates info required for auto scrolling, such as scroll height, scroll speed etc.
        /// </summary>
        public void _UpdateScrollInfo()
        {
            var scrollHeight= scrollRect.content.rect.height - scrollRect.viewport.rect.height;
            enoughHeight = scrollHeight > 0;

            if (enoughHeight)
                normalizedSpeed = unitsPerSec / scrollHeight;
            
            if (autoUpdateScrollInfo && scroll)
                SendCustomEventDelayedSeconds(nameof(_UpdateScrollInfo), scrollInfoUpdateInterval);
        }

        /// <summary>
        /// Forces the auto scroll to wait.
        /// Uses the wait time set in the inspector.
        /// </summary>
        public void _Wait()
        {
            elapsedTime = 0f;
            wait = true;
        }

        private void AnimateScroll()
        {
            if (!enoughHeight || !scroll) return;
            elapsedTime += Time.deltaTime;
            
            if (Mathf.Abs(scrollRect.velocity.y) > 0.00f) // Wait if user is manually scrolling, normally there are interfaces for this... but this is Udon....
                _Wait();
            
            // Waiting
            if (wait)
            {
                if (elapsedTime >= waitDuration)
                {
                    elapsedTime = 0f;
                    wait = false;
                }
                return;
            }
            
            // Speed
            var normalizedPos= scrollRect.verticalNormalizedPosition;
            var moveSpeed = 0f;
            if (elapsedTime < startCurveDuration)
            {
                var percentageComplete = Mathf.InverseLerp(0f, startCurveDuration, elapsedTime);
                moveSpeed = normalizedSpeed * Time.deltaTime * startCurve.Evaluate(percentageComplete);
            }
            else
            {
                moveSpeed = normalizedSpeed * Time.deltaTime;
            }
            
            // Direction
            if (state == ScrollState.ScrollDown)
            {
                scrollRect.verticalNormalizedPosition -= moveSpeed;
                
                if (normalizedPos <= 0f)
                {
                    state = ScrollState.ScrollUp;
                    _Wait();
                }
            }
            else
            {
                scrollRect.verticalNormalizedPosition += moveSpeed;
                
                if (normalizedPos >= 1f)
                {
                    state = ScrollState.ScrollDown;
                    _Wait();
                }
            }
        }
    }

    internal enum ScrollState
    {
        ScrollDown,
        ScrollUp,
    }

    internal enum StartAt
    {
        Top,
        Bottom
    }
}


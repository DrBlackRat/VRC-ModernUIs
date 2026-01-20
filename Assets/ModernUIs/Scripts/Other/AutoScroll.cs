using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using Random = UnityEngine.Random;

namespace DrBlackRat.VRC.ModernUIs
{
    [RequireComponent(typeof(ScrollRect))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoScroll : UdonSharpBehaviour
    {
        [Tooltip("Units in the UI to move per second.")]
        [SerializeField] private float unitsPerSec = 25f;
        [Tooltip("Whether to scroll horizontally or vertically.")]
        [SerializeField] private bool horizontal = false;
        [Tooltip("Whether to start scrolling from the \"Start\" (Top / Left), the \"End\" (Bottom / Right) or a random point.")]
        [SerializeField] private StartAt startAt = StartAt.Start;

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
        
        private bool enoughDistance;
        private float normalizedSpeed;
        
        private float elapsedTime;
        private bool wait;
        
        private ScrollState state = ScrollState.ScrollToEnd;
        
        private void Start()
        {
            scrollRect = GetComponent<ScrollRect>();

            switch (startAt)
            {
                case StartAt.Start:
                    SetNormalizedPosition(horizontal ? 0f : 1f);
                    state = ScrollState.ScrollToEnd;
                    break;
                case StartAt.End:
                    SetNormalizedPosition(horizontal ? 1f : 0f);
                    state = ScrollState.ScrollToStart;
                    break;
                case StartAt.Random:
                    Random.InitState(System.DateTime.Now.Millisecond);
                    var random = Random.Range(0f, 1f);
                    SetNormalizedPosition(random);
                    state = random < 0.5f ? ScrollState.ScrollToEnd : ScrollState.ScrollToStart;
                    break;
            }

            _UpdateScrollInfo();
        }
        
        private void SetNormalizedPosition(float value)
        {
            if (horizontal)
                scrollRect.horizontalNormalizedPosition = value;
            else
                scrollRect.verticalNormalizedPosition = value;
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
            var scrollDistance= horizontal ? scrollRect.content.rect.width - scrollRect.viewport.rect.width : 
                scrollRect.content.rect.height - scrollRect.viewport.rect.height;
            enoughDistance = scrollDistance > 0;

            if (enoughDistance)
                normalizedSpeed = unitsPerSec / scrollDistance;
            
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
            if (!enoughDistance || !scroll) return;
            elapsedTime += Time.deltaTime;

            if (Mathf.Abs(horizontal ? scrollRect.velocity.x : scrollRect.velocity.y) > 0.00f) // Wait if user is manually scrolling, normally there are interfaces for this... but this is Udon....
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
            var normalizedPos= horizontal ? 1 - scrollRect.horizontalNormalizedPosition : scrollRect.verticalNormalizedPosition;
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
            if (state == ScrollState.ScrollToEnd)
            {
                if (horizontal)
                {
                    scrollRect.horizontalNormalizedPosition += moveSpeed;
                }
                else
                {
                    scrollRect.verticalNormalizedPosition -= moveSpeed;
                }
                
                if (normalizedPos <= 0f)
                {
                    state = ScrollState.ScrollToStart;
                    _Wait();
                }
            }
            else
            {
                if (horizontal)
                {
                    scrollRect.horizontalNormalizedPosition -= moveSpeed;
                }
                else
                {
                    scrollRect.verticalNormalizedPosition += moveSpeed;
                }
                
                if (normalizedPos >= 1f)
                {
                    state = ScrollState.ScrollToEnd;
                    _Wait();
                }
            }
        }
    }

    internal enum ScrollState
    {
        ScrollToEnd, // Scrolling from top to bottom or left to right       
        ScrollToStart, // Scrolling from bottom to top or right to left
    }

    internal enum StartAt
    {
        Start, // Top or Left
        End, // Bottom or Right
        Random
    }
}


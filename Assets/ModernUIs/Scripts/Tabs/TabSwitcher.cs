
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class TabSwitcher : UdonSharpBehaviour
    {
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;
        
        private bool animate;
        private float animationElapsedTime;
        private Vector2 oldTabPos;
        private int prevTabID = 0;
        private int currentTabID = 0;
        
        private RectTransform tabsTransform;

        [HideInInspector] public int tabId;
        [HideInInspector] public Tab[] tabs;

        private void Start()
        {
            tabsTransform = GetComponent<RectTransform>();
            // Hide Tabs on Start
            foreach (var tab in tabs)
            {
                tab._Hide();
            }
        }

        public void _SwitchTab()
        {
            animate = false;
            SendCustomEventDelayedFrames(nameof(_StartAnimation), 0);
        }
        // Is called one frame delayed to allow the update loop to stop.
        public void _StartAnimation()
        {
            prevTabID = currentTabID;
            currentTabID = tabId;
            
            // Show tabs that are visible during transition
            for (int i = prevTabID;(prevTabID < currentTabID)? i <= currentTabID : i >= currentTabID ; i += (prevTabID < currentTabID)? 1 : -1)
            {
                tabs[i]._Show();
            }
            
            animate = true;
            animationElapsedTime = 0f;
            oldTabPos = tabs[prevTabID]._GetPos();
            _CustomUpdate();
        }
        public void _CustomUpdate()
        {
            if (!animate) return;
            AnimateUI(oldTabPos, currentTabID);
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }
        
        private void AnimateUI(Vector2 startPos, int endTabID)
        {
            animationElapsedTime += Time.deltaTime;
            var percentageComplete = animationElapsedTime / movementDuration;
            var smoothPercentageComplete = animationCurve.Evaluate(percentageComplete);
            // Set Selector Position
            var pos = tabsTransform.anchoredPosition;
            tabsTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, tabs[endTabID]._GetPos(), smoothPercentageComplete);
            
            if (percentageComplete >= 1f)
            {
                for (int i = 0; i < tabs.Length; i++)
                {
                    if (i == endTabID) continue;
                    tabs[i]._Hide();
                }
                
                animationElapsedTime = 0f;
                animate = false;
            }
        }
    }
}

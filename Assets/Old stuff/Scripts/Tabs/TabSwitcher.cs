﻿
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class TabSwitcher : UdonSharpBehaviour
    {
        [Header("Settings:")]
        [Tooltip("Transform that gets moved to show a Tab.")]
        [SerializeField] private RectTransform tabsTransform;
        [Tooltip("The Tabs that should be switched to.")]
        [SerializeField] private Tab[] tabs;
        
        [Header("UI Animation:")]
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;
        
        private bool animateUI;
        private float movementElapsedTime;
        private Vector2 oldTabPos;
        private int prevTabID = 0;
        private int currentTabID = 0;

        private void Start()
        {
            // Hide Tabs on Start
            foreach (var tab in tabs)
            {
                tab._Hide();
            }
        }

        public void _CustomUpdate()
        {
            if (!animateUI) return;
            AnimateUI(oldTabPos, currentTabID);
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }
        
        private void AnimateUI(Vector2 startPos, int endTabID)
        {
            movementElapsedTime += Time.deltaTime;
            var percentageComplete = movementElapsedTime / movementDuration;
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
                
                movementElapsedTime = 0f;
                animateUI = false;
            }
        }
        
        public void _SwitchTab(int newTab)
        {
            prevTabID = currentTabID;
            currentTabID = newTab;
            
            // Show all Tabs for transition (when switching from 1st to 4th only showing 1st and 4th would look weird)
            for (int i = prevTabID;(prevTabID < currentTabID)? i <= currentTabID : i >= currentTabID ; i += (prevTabID < currentTabID)? 1 : -1)
            {
                tabs[i]._Show();
            }
            
            animateUI = true;
            movementElapsedTime = 0f;
            oldTabPos = tabs[prevTabID]._GetPos();
            
            _CustomUpdate();
        }
    }
}

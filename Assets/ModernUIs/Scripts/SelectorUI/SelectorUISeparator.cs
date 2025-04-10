﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorUISeparator : UdonSharpBehaviour
    {
        [Header("Settings:")]
        [SerializeField] private RectTransform recTransform;
        [SerializeField] private Vector2[] positions;
        
        [Header("UI Animation:")]
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;
        
        private bool animate;
        private float animationElapsedTime;
        
        private Vector2 prevSeparatorPos;
        private Vector2 nextSeparatorPos;

        [HideInInspector] public int selectionId;

        public void _SelectionChanged()
        {
            prevSeparatorPos = recTransform.anchoredPosition;
            nextSeparatorPos = positions[selectionId];
            
            // UI Animation
            animate = false;
            SendCustomEventDelayedFrames(nameof(_StartAnimation), 0);
        }
        // Is called one frame delayed to allow the update loop to stop.
        public void _StartAnimation()
        {
            animate = true;
            animationElapsedTime = 0f;
            _CustomUpdate();
        }
        
        public void _CustomUpdate()
        {
            if (!animate) return;
            AnimateUI(prevSeparatorPos, nextSeparatorPos);
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }
        
        private void AnimateUI(Vector2 startPos, Vector2 endPos)
        {
            animationElapsedTime += Time.deltaTime;
            var percentageComplete = animationElapsedTime / movementDuration;
            var smoothPercentageComplete = animationCurve.Evaluate(percentageComplete);
            // Set Selector Position
            recTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, smoothPercentageComplete);
            
            if (percentageComplete >= 1f)
            {
                animationElapsedTime = 0f;
                animate = false;
            }
        }
    }
}

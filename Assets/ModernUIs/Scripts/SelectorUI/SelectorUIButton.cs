﻿using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(-10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorUIButton : UdonSharpBehaviour
    {
        [Header("Button:")]
        [SerializeField] protected GameObject buttonObj;
        [SerializeField] protected Vector2 buttonNormalScale;
        [SerializeField] protected Vector2 buttonSelectedScale;
        [Space(10)]
        [SerializeField] protected float buttonNormalPixelPerUnit;
        [SerializeField] protected float buttonSelectedPixelPerUnit;

        [Header("Icon")]
        [SerializeField] protected GameObject iconObj;
        [SerializeField] protected Vector2 iconNormalPos;
        [SerializeField] protected Vector2 iconSelectedPos;
        
        [Header("Text")]
        [SerializeField] protected GameObject textObj;
        [SerializeField] protected Vector2 textNormalPos;
        [SerializeField] protected Vector2 textSelectedPos;

        protected Button button;
        protected RectTransform buttonTransform;
        protected Image buttonImage;
        protected Image icon;
        protected RectTransform iconTransform;
        protected TextMeshProUGUI text;
        protected RectTransform textTransform;

        protected RectTransform parentTransform;
        
        protected bool animate;
        protected float animationElapsedTime;

        protected Vector2 prevButtonScale;
        protected float prevButtonPixelPerUnit;
        protected Vector2 prevIconPos;
        protected Color prevIconColor;
        protected Vector2 prevTextPos;
        protected Color prevTextColor;
        
        protected Vector2 newButtonScale;
        protected float newButtonPixelPerUnit;
        protected Vector2 newIconPos;
        protected Color newIconColor;
        protected Vector2 newTextPos;
        protected Color newTextColor;
        
        // Provided by main script
        protected Color normalColor;
        protected Color selectedColor;
        protected AnimationCurve smoothingCurve;
        protected float movementDuration;
        
        protected SelectorUI selectorUI;
        protected int buttonId;

        protected bool locked;

        #region SelectorUI Connection
        public virtual void _Setup(Color newNormalColor, Color newSelectedColor, AnimationCurve newSmoothingCurve, float newMovementDuration, SelectorUI newSelectorUI, int newButtonId)
        {
            // Inital Setup
            if (buttonObj != null)
            {
                button = buttonObj.GetComponent<Button>();
                buttonTransform = buttonObj.GetComponent<RectTransform>();
                buttonImage = buttonObj.GetComponent<Image>();
            }
            if (iconObj != null)
            {
                icon = iconObj.GetComponent<Image>();
                iconTransform = iconObj.GetComponent<RectTransform>();
            }
            if (textObj != null)
            {
                text = textObj.GetComponent<TextMeshProUGUI>();
                textTransform = textObj.GetComponent<RectTransform>();
            }

            parentTransform = GetComponent<RectTransform>();
            
            // Animation
            normalColor = newNormalColor;
            selectedColor = newSelectedColor;
            smoothingCurve = newSmoothingCurve;
            movementDuration = newMovementDuration;
            
            // Tab UI
            selectorUI = newSelectorUI;
            buttonId = newButtonId;
            
            // Reset
            button.interactable = !locked;
            UpdateUIState(false);
            UpdateUI(1f);
        }

        public virtual void _ButtonPressed()
        {
            if (locked) return;
            selectorUI._ButtonSelected(buttonId);
        }
        
        // API
        /// <summary>
        /// Returns the local center position of the Selector Button. Used for selector UI.
        /// </summary>
        public Vector3 LocalPos()
        {
            return parentTransform.localPosition;
        }

        /// <summary>
        /// Returns the selected size of the Selector Button. Used for selector UI.
        /// </summary>
        public Vector2 SelectedSize()
        {
            return parentTransform.sizeDelta;
        }
        
        /// <summary>
        /// Returns the selected "Pixel Per Unit Multiplier" of the Selector Button. Used for selector UI.
        /// </summary>
        public float SelectedCornerRadius()
        {
            return buttonSelectedPixelPerUnit;
        }

        public void _UpdateSelected(bool selected)
        {
            UpdateUIState(selected);
            
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

        public virtual void _UpdateLocked(bool newLocked)
        {
            locked = newLocked;
            button.interactable = !locked;
        }
        #endregion
        #region UI Animation
        protected void UpdateUIState(bool selected)
        {
            if (buttonTransform) prevButtonScale = buttonTransform.sizeDelta;
            if (buttonImage) prevButtonPixelPerUnit = buttonImage.pixelsPerUnitMultiplier;
            if (iconTransform) prevIconPos = iconTransform.anchoredPosition;
            if (icon) prevIconColor = icon.color;
            if (textTransform) prevTextPos = textTransform.anchoredPosition;
            if (text) prevTextColor = text.color;

            if (selected)
            {
                newButtonScale = buttonSelectedScale;
                newButtonPixelPerUnit = buttonSelectedPixelPerUnit;
                newIconPos = iconSelectedPos;
                newIconColor = selectedColor;
                newTextPos = textSelectedPos;
                newTextColor = selectedColor;
            }
            else
            {
                newButtonScale = buttonNormalScale;
                newButtonPixelPerUnit = buttonNormalPixelPerUnit;
                newIconPos = iconNormalPos;
                newIconColor = normalColor;
                newTextPos = textNormalPos;
                newTextColor = normalColor;
            }
        }
        
        public void _CustomUpdate()
        {
            if (!animate) return;
            AnimateUI();
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }
        
        protected void AnimateUI()
        {
            animationElapsedTime += Time.deltaTime;
            var percentageComplete = animationElapsedTime / movementDuration;
            UpdateUI(smoothingCurve.Evaluate(percentageComplete));
            if (percentageComplete >= 1f)
            {
                animationElapsedTime = 0f;
                animate = false;
            }
        }
        protected void UpdateUI(float transition)
        {
            if (icon) icon.color = Color.LerpUnclamped(prevIconColor, newIconColor, transition);
            if (text) text.color = Color.LerpUnclamped(prevTextColor, newTextColor, transition);
            
            if (buttonTransform) buttonTransform.sizeDelta = Vector2.LerpUnclamped(prevButtonScale, newButtonScale, transition);
            if (buttonImage) buttonImage.pixelsPerUnitMultiplier = Mathf.LerpUnclamped(prevButtonPixelPerUnit, newButtonPixelPerUnit, transition);
            if (iconTransform) iconTransform.anchoredPosition = Vector2.LerpUnclamped(prevIconPos, newIconPos, transition);
            if (textTransform) textTransform.anchoredPosition = Vector2.LerpUnclamped(prevTextPos, newTextPos, transition);
        }
        #endregion
    }
}

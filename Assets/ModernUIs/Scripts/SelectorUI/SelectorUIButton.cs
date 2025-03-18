using System;
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
        [SerializeField] private GameObject buttonObj;
        [SerializeField] private Vector2 buttonNormalScale;
        [SerializeField] private Vector2 buttonSelectedScale;
        [Space(10)]
        [SerializeField] private float buttonNormalPixelPerUnit;
        [SerializeField] private float buttonSelectedPixelPerUnit;

        [Header("Icon")]
        [SerializeField] private GameObject iconObj;
        [SerializeField] private Vector2 iconNormalPos;
        [SerializeField] private Vector2 iconSelectedPos;
        
        [Header("Text")]
        [SerializeField] private GameObject textObj;
        [SerializeField] private Vector2 textNormalPos;
        [SerializeField] private Vector2 textSelectedPos;
        
        private RectTransform buttonTransform;
        private Image buttonImage;
        private Image icon;
        private RectTransform iconTransform;
        private TextMeshProUGUI text;
        private RectTransform textTransform;
        
        private bool animate;
        private float elapsedTime;

        private Vector2 prevButtonScale;
        private float prevButtonPixelPerUnit;
        private Vector2 prevIconPos;
        private Color prevIconColor;
        private Vector2 prevTextPos;
        private Color prevTextColor;
        
        private Vector2 newButtonScale;
        private float newButtonPixelPerUnit;
        private Vector2 newIconPos;
        private Color newIconColor;
        private Vector2 newTextPos;
        private Color newTextColor;
        
        // Provided by main script
        private Color normalColor;
        private Color selectedColor;
        private AnimationCurve smoothingCurve;
        private float movementDuration;
        
        private SelectorUI selectorUI;
        private int buttonId;

        #region SelectorUI Connection
        public void Setup(Color newNormalColor, Color newSelectedColor, AnimationCurve newSmoothingCurve, float newMovementDuration, SelectorUI newSelectorUI, int newButtonId)
        {
            // Inital Setup
            if (buttonObj != null)
            {
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
            
            // Animation
            normalColor = newNormalColor;
            selectedColor = newSelectedColor;
            smoothingCurve = newSmoothingCurve;
            movementDuration = newMovementDuration;
            
            // Tab UI
            selectorUI = newSelectorUI;
            buttonId = newButtonId;
            
            // Reset 
            UpdateUIState(false);
            UpdateUI(1f);
        }

        public void _ButtonPressed()
        {
            selectorUI._UpdateSelection(buttonId, false, false);
        }
        
        public Vector3 Position()
        {
            return buttonTransform.position;
        }

        public void _Select()
        {
            UpdateUIState(true);
            
            animate = true;
            elapsedTime = 0f;
            _CustomUpdate();
        }
        public void _UnSelect()
        {
            UpdateUIState(false);
            
            animate = true;
            elapsedTime = 0f;
            _CustomUpdate();
        }
        #endregion
        #region UI Animation
        private void UpdateUIState(bool selected)
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
        
        private void AnimateUI()
        {
            elapsedTime += Time.deltaTime;
            var percentageComplete = elapsedTime / movementDuration;
            UpdateUI(smoothingCurve.Evaluate(percentageComplete));
            if (percentageComplete >= 1f)
            {
                elapsedTime = 0f;
                animate = false;
            }
        }
        private void UpdateUI(float transition)
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

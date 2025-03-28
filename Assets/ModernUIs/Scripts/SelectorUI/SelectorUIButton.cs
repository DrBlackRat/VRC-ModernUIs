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
        
        protected bool animate;
        protected float elapsedTime;

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
            selectorUI._ButtonSelected(buttonId);
        }
        
        public Vector3 Position()
        {
            return buttonTransform.position;
        }

        public void _UpdateSelected(bool selected)
        {
            UpdateUIState(selected);
            
            animate = true;
            elapsedTime = 0f;
            _CustomUpdate();
        }

        public void _UpdateInteractable(bool interactable)
        {
            button.interactable = interactable;
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
            elapsedTime += Time.deltaTime;
            var percentageComplete = elapsedTime / movementDuration;
            UpdateUI(smoothingCurve.Evaluate(percentageComplete));
            if (percentageComplete >= 1f)
            {
                elapsedTime = 0f;
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

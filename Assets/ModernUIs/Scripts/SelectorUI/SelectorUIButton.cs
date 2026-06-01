using System;
using DG.Tweening;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [DefaultExecutionOrder(-10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorUIButton : UdonSharpBehaviour
    {
        [SerializeField] protected GameObject buttonObj;
        [SerializeField] protected GameObject iconObj;
        [FormerlySerializedAs("textObj")] [SerializeField] protected GameObject graphicObj;
        
        [Tooltip("If enabled the Color, Icon, Text and Animation Settings can be overriden, otherwise the defaults provided by the Selector UI will be used.")]
        public bool overrideDefaults;
        
        [SerializeField] protected Color normalColor;
        [SerializeField] protected Color selectedColor;

        public virtual Color NormalColor
        {
            get => normalColor;
            set => normalColor = value;
        }
        
        public virtual Color SelectedColor
        {
            get => selectedColor;
            set => selectedColor = value;
        }
        
        [SerializeField] protected AnimationCurve smoothingCurve;
        [SerializeField] protected float movementDuration;
        
        [SerializeField] protected Vector2 buttonNormalScale;
        [SerializeField] protected Vector2 buttonSelectedScale;

        [SerializeField] protected float buttonNormalPixelPerUnit;
        [SerializeField] protected float buttonSelectedPixelPerUnit;
        
        [SerializeField] protected Vector2 iconNormalPos;
        [SerializeField] protected Vector2 iconSelectedPos;
        
        [SerializeField] protected Vector2 textNormalPos;
        [SerializeField] protected Vector2 textSelectedPos;

        protected Button button;
        protected RectTransform buttonTransform;
        protected Image buttonImage;
        protected Image icon;
        protected RectTransform iconTransform;
        protected Graphic graphic;
        protected RectTransform textTransform;

        protected RectTransform parentTransform;
        
        // Provided by main script
        protected SelectorUI selectorUI;
        protected int buttonId;
        
        protected bool locked;
        
        // Only used by versions that implement it
        protected bool selectable = true;
        protected bool selected;

        [NonSerialized] public float buttonPixelPerUnit; // Set by Tweening
        private VRCTweenHandle buttonSizeTween;
        private VRCTweenHandle iconPosTween;
        private VRCTweenHandle iconColorTween;
        private VRCTweenHandle textPosTween;
        private VRCTweenHandle graphicColorTween;
        private VRCTweenHandle buttonImagePixelsPerUnitTween;
        

        /// <summary>
        /// If this selector UI button can be selected. Can be false due to CE or Whitelists.
        /// </summary>
        public bool Selectable => selectable;

        #region SelectorUI Connection
        public virtual void _Setup(SelectorUI newSelectorUI, int newButtonId)
        {
            // Inital Setup
            if (buttonObj != null)
            {
                button = buttonObj.GetComponent<Button>();
                buttonTransform = buttonObj.GetComponent<RectTransform>();
                buttonImage = button.image;
            }
            if (iconObj != null)
            {
                icon = iconObj.GetComponent<Image>();
                iconTransform = iconObj.GetComponent<RectTransform>();
            }
            if (graphicObj != null)
            {
                graphic = graphicObj.GetComponent<Graphic>();
                textTransform = graphicObj.GetComponent<RectTransform>();
            }

            parentTransform = GetComponent<RectTransform>();
            
            // Tab UI
            selectorUI = newSelectorUI;
            buttonId = newButtonId;
            
            // Reset
            SetupTween();
            UpdateInteractable();
            ResetUI();
        }

        private void SetupTween()
        {
            if (buttonTransform)
            {
                buttonSizeTween = buttonTransform.TweenSizeDelta(new Vector2(), movementDuration, VRCTweenEase.None)
                    .SetEase(smoothingCurve)
                    .Pause();
            }
            if (iconTransform)
            {
                iconPosTween = iconTransform.TweenAnchorPos(new Vector2(), movementDuration, VRCTweenEase.None)
                    .SetEase(smoothingCurve)
                    .Pause();
            }
            if (icon)
            {
                iconColorTween = icon.TweenColor(new Color(), movementDuration, VRCTweenEase.None)
                    .SetEase(smoothingCurve)
                    .Pause();
            }
            if (textTransform)
            {
                textPosTween = textTransform.TweenAnchorPos(new Vector2(), movementDuration, VRCTweenEase.None)
                    .SetEase(smoothingCurve)
                    .Pause();
            }
            if (graphic)
            {
                graphicColorTween = graphic.TweenColor(new Color(), movementDuration, VRCTweenEase.None)
                    .SetEase(smoothingCurve)
                    .Pause();
            }
            if (buttonImage)
            {
                buttonImagePixelsPerUnitTween = VRCTween.TweenFloat(buttonNormalPixelPerUnit, 0f, movementDuration,this , nameof(buttonPixelPerUnit), nameof(_TransitionRounding), VRCTweenEase.None)
                    .SetEase(smoothingCurve)
                    .Pause();
            }
        }

        public virtual void _SetDefaults(
            Vector2 newButtonNormalScale,
            Vector2 newButtonSelectedScale,
            float newButtonNormalPixelPerUnit,
            float newButtonSelectedPixelPerUnit,
            Color newNormalColor,
            Color newSelectedColor,
            Vector2 newIconNormalPos,
            Vector2 newIconSelectedPos,
            Vector2 newTextNormalPos,
            Vector2 newTextSelectedPos,
            AnimationCurve newSmoothingCurve,
            float newMovementDuration)
        {
            if (overrideDefaults) return;
            buttonNormalScale = newButtonNormalScale;
            buttonSelectedScale = newButtonSelectedScale;

            buttonNormalPixelPerUnit = newButtonNormalPixelPerUnit;
            buttonSelectedPixelPerUnit = newButtonSelectedPixelPerUnit;

            normalColor = newNormalColor;
            selectedColor = newSelectedColor;
            
            iconNormalPos = newIconNormalPos;
            iconSelectedPos = newIconSelectedPos;

            textNormalPos = newTextNormalPos;
            textSelectedPos = newTextSelectedPos;

            smoothingCurve = newSmoothingCurve;
            movementDuration = newMovementDuration;
        }

        public virtual void _ButtonPressed()
        {
            if (locked && !selectable)
            {
                MUIDebug.LogError($"Selector UI Button {buttonId} | Locked: {locked}, Selectable: {selectable} | You should not be ale to press this button!");
                return;
            }
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

        public void _UpdateSelected(bool newSelected)
        {
            selected = newSelected;
            
            if (buttonTransform) buttonSizeTween.ChangeEndValue(newSelected ? buttonSelectedScale : buttonNormalScale, true)
                    .Restart();
            if (iconTransform) iconPosTween.ChangeEndValue(newSelected ? iconSelectedPos : iconNormalPos, true)
                    .Restart();
            if (icon) iconColorTween.ChangeEndValue(newSelected ? selectedColor : normalColor, true)
                    .Restart();
            if (textTransform) textPosTween.ChangeEndValue(newSelected ? textSelectedPos : textNormalPos, true)
                    .Restart();
            if (graphic) graphicColorTween.ChangeEndValue(newSelected ? selectedColor : normalColor, true)
                    .Restart();
            if (buttonImage) buttonImagePixelsPerUnitTween.ChangeEndValue(newSelected ? buttonSelectedPixelPerUnit : buttonNormalPixelPerUnit, true)
                    .Restart();
        }

        public void _TransitionRounding()
        {
            if (buttonImage == null) return;
            buttonImage.pixelsPerUnitMultiplier = buttonPixelPerUnit;
        }

        public virtual void _UpdateLocked(bool newLocked)
        {
            locked = newLocked;
            UpdateInteractable();
        }

        protected virtual void UpdateInteractable()
        {
            button.interactable = !locked && selectable;
        }

        protected virtual void ResetUI()
        {
            if (buttonImage) buttonTransform.sizeDelta = buttonNormalScale;
            if (iconTransform) iconTransform.anchoredPosition = iconNormalPos;
            if (icon) icon.color = normalColor;
            if (textTransform) textTransform.anchoredPosition = textNormalPos;
            if (graphic) graphic.color = normalColor;
            if (buttonImage) buttonImage.pixelsPerUnitMultiplier = buttonNormalPixelPerUnit;
        }
        #endregion

    }
}

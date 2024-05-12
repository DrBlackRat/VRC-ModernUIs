using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [DefaultExecutionOrder(-10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorButton : UdonSharpBehaviour
    {
        [Header("Button")]
        [SerializeField] private bool selected;
        [SerializeField] private GameObject buttonObj;
        [SerializeField] private Vector2 buttonNormalScale;
        [SerializeField] private Vector2 buttonSelectedScale;

        [Header("Icon")]
        [SerializeField] private GameObject iconObj;
        [SerializeField] private Vector2 iconNormalPos;
        [SerializeField] private Vector2 iconSelectedPos;
        
        [Header("Text")]
        [SerializeField] private GameObject textObj;
        [SerializeField] private Vector2 textNormalPos;
        [SerializeField] private Vector2 textSelectedPos;
        
        private Button button;
        private RectTransform buttonTransform;
        private Image icon;
        private RectTransform iconTransform;
        private TextMeshProUGUI text;
        private RectTransform textTransform;
        
        private bool animate;
        private float elapsedTime;

        private Vector2 prevButtonScale;
        private Vector2 prevIconPos;
        private Color prevIconColor;
        private Vector2 prevTextPos;
        private Color prevTextColor;
        
        private Vector2 newButtonScale;
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

        private void Start()
        {
            button = buttonObj.GetComponent<Button>();
            buttonTransform = buttonObj.GetComponent<RectTransform>();
            icon = iconObj.GetComponent<Image>();
            iconTransform = iconObj.GetComponent<RectTransform>();
            text = textObj.GetComponent<TextMeshProUGUI>();
            textTransform = textObj.GetComponent<RectTransform>();
        }
        private void Update()
        {
            if (animate) AnimateUI();
        }
        #region Selector UI Connection
        public Vector3 Position()
        {
            return buttonTransform.position;
        }
        public void Setup(SelectorUI newSelectorUI, Color newNormalColor, Color newSelectedColor, AnimationCurve newSmoothingCurve, float newMovementDuration)
        {
            selectorUI = newSelectorUI;
            normalColor = newNormalColor;
            selectedColor = newSelectedColor;
            smoothingCurve = newSmoothingCurve;
            movementDuration = newMovementDuration;
            // Reset 
            UpdateUIState();
            UpdateUI(1f);
        }
        public void Select()
        {
            selected = true;
            
            animate = true;
            elapsedTime = 0f;
            UpdateUIState();
        }
        public void UnSelect()
        {
            selected = false;
            
            animate = true;
            elapsedTime = 0f;
            UpdateUIState();
        }
        #endregion

        #region UI
        private void UpdateUIState()
        {
            prevButtonScale = buttonTransform.sizeDelta;
            prevIconPos = iconTransform.anchoredPosition;
            prevIconColor = icon.color;
            prevTextPos = textTransform.anchoredPosition;
            prevTextColor = text.color;

            if (selected)
            {
                newButtonScale = buttonSelectedScale;
                newIconPos = iconSelectedPos;
                newIconColor = selectedColor;
                newTextPos = textSelectedPos;
                newTextColor = selectedColor;
            }
            else
            {
                newButtonScale = buttonNormalScale;
                newIconPos = iconNormalPos;
                newIconColor = normalColor;
                newTextPos = textNormalPos;
                newTextColor = normalColor;
            }
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
            icon.color = Color.Lerp(prevIconColor, newIconColor, transition);
            text.color = Color.Lerp(prevTextColor, newTextColor, transition);
            
            buttonTransform.sizeDelta = Vector2.Lerp(prevButtonScale, newButtonScale, transition);
            iconTransform.anchoredPosition = Vector2.Lerp(prevIconPos, newIconPos, transition);
            textTransform.anchoredPosition = Vector2.Lerp(prevTextPos, newTextPos, transition);
        }
        #endregion

    }
}


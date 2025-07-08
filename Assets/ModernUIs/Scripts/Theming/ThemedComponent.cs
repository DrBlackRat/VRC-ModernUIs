using System;
using DrBlackRat.VRC.ModernUIs.SelectorUI;
using DrBlackRat.VRC.ModernUIs.SliderUI;
using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using UnityEngine.Serialization;
using VRC.Udon;

namespace ModernUIs.Scripts.Theming
{
    public class ThemedComponent : MonoBehaviour
    {
        [Tooltip("Theme Color the that should be used for the Graphic Component.")]
        [SerializeField] private ThemeColor themeColor;
        [FormerlySerializedAs("graphicComponent")]
        [Tooltip("Unity Image, Text, Text Mesh Pro, etc. the Theme Color should be applied to.")]
        [SerializeField] private Graphic graphic;

        [Tooltip("Selector UI of which the Selected, Not Selected, Whitelisted & Not Whitelisted Colors will be set.")]
        [SerializeField] private SelectorUI selectorUI;

        private void Reset()
        {
            if (selectorUI == null) selectorUI = GetComponent<SelectorUI>();
            if (graphic == null) graphic = GetComponent<Graphic>();
        }

        public void ApplyTheme(Theme theme)
        {
            Debug.LogError("APPLYYYYYYYYYYYYYYYYYYYYYYYYYYYY");
            if (graphic != null)
            {
                graphic.color = theme.GetColor(themeColor);
            }
            
            if (selectorUI != null)
            {
                selectorUI.SelectedColor = theme.GetColor(ThemeColor.ButtonTextIcon);
                selectorUI.NormalColor = theme.GetColor(ThemeColor.ButtonInactiveTextIcon);

                if (selectorUI is WhitelistSelectorUI whitelistSelectorUI)
                {
                    whitelistSelectorUI.WhitelistedColor = theme.GetColor(ThemeColor.ButtonPrimary);
                    whitelistSelectorUI.NotWhitelistedColor = theme.GetColor(ThemeColor.ButtonInactivePrimary);
                }
            }
        }
    }
}

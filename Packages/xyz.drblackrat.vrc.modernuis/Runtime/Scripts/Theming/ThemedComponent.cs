using System;
using DrBlackRat.VRC.ModernUIs.SelectorUI;
using DrBlackRat.VRC.ModernUIs.SliderUI;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using UnityEngine;
using UnityEngine.UI;
using UdonSharp;
using UnityEngine.Serialization;
using VRC.Udon;

namespace ModernUIs.Theming
{
    public class ThemedComponent : MonoBehaviour
    {
        [Tooltip("Theme Color the that should be used for the Graphic Component.")]
        [SerializeField] private ThemeColor themeColor;
        [FormerlySerializedAs("graphicComponent")]
        [Tooltip("Unity Image, Text, Text Mesh Pro, etc. the Theme Color should be applied to.")]
        [SerializeField] private Graphic graphic;

        [Tooltip("If enabled, alpha will be overriden with that's set here.")]
        [SerializeField] private bool overrideAlpha;
        [Range(0, 1)]
        [Tooltip("Alpha that is used instead of the theme provided one, requires \"Override Alpha\" to be enabled.")]
        [SerializeField] private float alpha;

        [Tooltip("Selector UI of which the Selected, Not Selected, Whitelisted & Not Whitelisted Colors will be set.")]
        [SerializeField] private SelectorUI selectorUI;
        [Tooltip("Selector UI of which the Selected & Not Selected Colors will be set.")]
        [SerializeField] private SelectorUIButton selectorUIButton;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            if (graphic == null) graphic = GetComponent<Graphic>();
            if (selectorUI == null) selectorUI = GetComponent<SelectorUI>();
            if (selectorUIButton == null) selectorUIButton = GetComponent<SelectorUIButton>();
        }

        public void ApplyTheme(Theme theme)
        {
            if (graphic != null)
            {
                UnityEditor.EditorUtility.SetDirty(graphic);
                var color = theme.GetColor(themeColor);

                if (overrideAlpha)
                    color.a = alpha;
                
                graphic.color = color;
            }
            
            if (selectorUI != null)
            {
                UnityEditor.EditorUtility.SetDirty(selectorUI);
                selectorUI.SelectedColor = theme.GetColor(ThemeColor.ButtonTextIcon);
                selectorUI.NormalColor = theme.GetColor(ThemeColor.ButtonInactiveTextIcon);

                if (selectorUI is WhitelistSelectorUI whitelistSelectorUI)
                {
                    whitelistSelectorUI.WhitelistedColor = theme.GetColor(ThemeColor.ButtonPrimary);
                    whitelistSelectorUI.NotWhitelistedColor = theme.GetColor(ThemeColor.ButtonInactivePrimary);
                }
            }

            if (selectorUIButton != null)
            {
                UnityEditor.EditorUtility.SetDirty(selectorUIButton);
                selectorUIButton.SelectedColor = theme.GetColor(ThemeColor.ButtonTextIcon);
                selectorUIButton.NormalColor = theme.GetColor(ThemeColor.ButtonInactiveTextIcon);
            }
        }
#endif
    }
}

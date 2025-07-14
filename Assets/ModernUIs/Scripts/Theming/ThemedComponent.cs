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

        [Tooltip("Selector UI of which the Selected, Not Selected, Whitelisted & Not Whitelisted Colors will be set.")]
        [SerializeField] private SelectorUI selectorUI;
        [Tooltip("Selector UI of which the Selected & Not Selected Colors will be set.")]
        [SerializeField] private SelectorUIButton selectorUIButton;
        [Tooltip("Whitelist Editor / Requestor of which the Prefab Colors will be adjusted.")]
        [SerializeField] private WhitelistEditorBase whitelistEditorBase;
        [Tooltip("Fancy Whitelist Name Display of which the Prefab Colors will be adjusted.")]
        [SerializeField] private FancyWhitelistNameDisplay fancyWhitelistNameDisplay;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            if (graphic == null) graphic = GetComponent<Graphic>();
            if (selectorUI == null) selectorUI = GetComponent<SelectorUI>();
            if (selectorUIButton == null) selectorUIButton = GetComponent<SelectorUIButton>();
            if (whitelistEditorBase == null) whitelistEditorBase = GetComponent<WhitelistEditorBase>();
            if (fancyWhitelistNameDisplay == null) fancyWhitelistNameDisplay = GetComponent<FancyWhitelistNameDisplay>();
        }

        public void ApplyTheme(Theme theme)
        {
            if (graphic != null)
            {
                UnityEditor.EditorUtility.SetDirty(graphic);
                graphic.color = theme.GetColor(themeColor);
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

            if (whitelistEditorBase != null)
            {
                UnityEditor.EditorUtility.SetDirty(whitelistEditorBase);
                whitelistEditorBase.PrefabTextColor = theme.GetColor(ThemeColor.Text);
                whitelistEditorBase.PrefabBackgroundColor = theme.GetColor(ThemeColor.FieldBackground);
            }

            if (fancyWhitelistNameDisplay != null)
            {
                UnityEditor.EditorUtility.SetDirty(fancyWhitelistNameDisplay);
                fancyWhitelistNameDisplay.PrefabTextColor = theme.GetColor(ThemeColor.Text);
                fancyWhitelistNameDisplay.PrefabBackgroundColor = theme.GetColor(ThemeColor.FieldBackground);
            }
        }
#endif
    }
}

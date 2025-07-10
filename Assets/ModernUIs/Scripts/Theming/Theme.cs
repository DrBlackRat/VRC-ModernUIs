using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ModernUIs.Theming
{
    [CreateAssetMenu(menuName = "ModernUIs / Theme")]
    public class Theme : ScriptableObject
    {
        [Tooltip("Color of the Main UI Background.")]
        [SerializeField] private Color background;
        public Color Background => background;

        [Tooltip("Color of Secondary UI Backgrounds, Sections etc.")]
        [SerializeField] private Color secondaryBackground;
        public Color SecondaryBackground => secondaryBackground;

        [Tooltip("Color of Field Backgrounds, e.g. Slider or Selector UI Backgrounds.")]
        [SerializeField] private Color fieldBackground;
        public Color FieldBackground => fieldBackground;

        [FormerlySerializedAs("primary")]
        [Tooltip("Color of the primary interactable UI Elements, e.g. Sliders, Selectors.")]
        [SerializeField] private Color buttonPrimary;
        public Color ButtonPrimary => buttonPrimary;

        [FormerlySerializedAs("inactivePrimary")]
        [Tooltip("Color of the primary interactable UI Elements, e.g. Sliders, Selectors, when they are disabled by things like Whitelists.")]
        [SerializeField] private Color buttonInactivePrimary;
        public Color ButtonInactivePrimary => buttonInactivePrimary;

        [FormerlySerializedAs("selectedTextIcon")]
        [Tooltip("Color of Text & Icons on Selector UIs if they are selected.")]
        [SerializeField] private Color buttonTextIcon;
        public Color ButtonTextIcon => buttonTextIcon;

        [FormerlySerializedAs("unselectedTextIcon")]
        [Tooltip("Color of Text & Icons on Selector UIs if they are not selected.")]
        [SerializeField] private Color buttonInactiveTextIcon;
        public Color ButtonInactiveTextIcon => buttonInactiveTextIcon;

        [Tooltip("Color of primary Icons on things like Headers etc.")]
        [SerializeField] private Color icon;
        public Color Icon => icon;

        [Tooltip("Color of secondary Icons.")]
        [SerializeField] private Color secondaryIcon;
        public Color SecondaryIcon => secondaryIcon;
        
        [Tooltip("Color of primary Text on things like Headers, Selector / Slider Titles etc.")]
        [SerializeField] private Color text;
        public Color Text => text;

        [Tooltip("Color of secondary Text on things like info boxes, lock buttons etc.")]
        [SerializeField] private Color secondaryText;
        public Color SecondaryText => secondaryText;
        

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        /// <summary>
        /// Applies the Theme to everything using it.
        /// </summary>
        [ContextMenu("Apply")]
        public void ApplyToAllUsing()
        {
            if (Application.isPlaying) return;
            var themeSelectors = FindObjectsOfType<ThemeSelector>(true);
            foreach (var themeSelector in themeSelectors)
            {
                if (themeSelector.Theme == this)
                {
                    themeSelector.ApplyThemeToChildren();
                }
            }
        }

        [ContextMenu("Apply To Everything")]
        public void ApplyToEverything()
        {
            if (Application.isPlaying) return;
            var themeSelectors = FindObjectsOfType<ThemeSelector>(true);
            foreach (var themeSelector in themeSelectors)
            {
                themeSelector.Theme = this;
                themeSelector.ApplyThemeToChildren();
            }
        }
        
        /// <summary>
        /// Retrieves a color from the current <see cref="Theme"/> based on the specified <see cref="ThemeColor"/> role.
        /// </summary>
        /// <param name="themeColor">The color role to retrieve from the theme (e.g., Background, Primary, Text).</param>
        /// <returns>The <see cref="Color"/> value associated with the specified <paramref name="themeColor"/> role.</returns>
        public Color GetColor(ThemeColor themeColor)
        {
            return themeColor switch
            {
                ThemeColor.Background => Background,
                ThemeColor.SecondaryBackground => SecondaryBackground,
                ThemeColor.FieldBackground => FieldBackground,
                ThemeColor.ButtonPrimary => ButtonPrimary,
                ThemeColor.ButtonInactivePrimary => ButtonInactivePrimary,
                ThemeColor.ButtonTextIcon => ButtonTextIcon,
                ThemeColor.ButtonInactiveTextIcon => ButtonInactiveTextIcon,
                ThemeColor.Icon => Icon,
                ThemeColor.SecondaryIcon => SecondaryIcon,
                ThemeColor.Text => Text,
                ThemeColor.SecondaryText => SecondaryText,
                _ => Color.magenta
            };
        }
#endif
    }

    public enum ThemeColor
    {
        // Backgrounds
        Background,
        SecondaryBackground,
        FieldBackground,
        // Selector UI, Sliders, Buttons
        ButtonPrimary,
        ButtonInactivePrimary,
        ButtonTextIcon,
        ButtonInactiveTextIcon,
        // General
        Icon,
        SecondaryIcon,
        Text,
        SecondaryText,
    }
}

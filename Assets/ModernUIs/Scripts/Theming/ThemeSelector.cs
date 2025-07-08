using System;
using System.Collections.Generic;
using DrBlackRat.VRC.ModernUIs;
using UnityEngine;

namespace ModernUIs.Scripts.Theming
{
    [ExecuteAlways]
    public class ThemeSelector : MonoBehaviour
    {
        [SerializeField] private Theme theme;
        public Theme Theme
        {
            get => theme;
            set => theme = value;
        }

        private void OnValidate() 
        {
            ApplyThemeToChildren();
        }
        
        [ContextMenu("Apply Theme")]
        public void ApplyThemeToChildren()
        {
            if (theme == null)
            {
                MUIDebug.LogWarning("Could not apply Theme: No Theme Provided!");
                return;
            }
            
            var themeComponents = GetComponentsInChildren<ThemedComponent>(true);
            foreach (var themeComponent in themeComponents)
            {
               themeComponent.ApplyTheme(theme); 
            }
        }
    }
}

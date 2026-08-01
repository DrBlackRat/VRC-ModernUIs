using System;
using System.Collections.Generic;
using DrBlackRat.VRC.ModernUIs;
using UnityEngine;

namespace ModernUIs.Theming
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

        private Theme prevTheme;
        #if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate() 
        {
            if (theme == prevTheme) return; // To prevent trying to apply the theme to often, as without it every single change in the scene causes it to be applied
            prevTheme = theme;
            
            ApplyThemeToChildren();
        }
        
        [ContextMenu("Apply")]
        public void ApplyThemeToChildren()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorUtility.SetDirty(this);
            if (theme == null)
            {
                MUIDebug.LogWarning($"Could not apply Theme to {gameObject.name}: No Theme Provided!");
                return;
            }
            
            MUIDebug.Log($"Applying {theme.name} Theme to {gameObject.name}.");
            var themeComponents = GetComponentsInChildren<ThemedComponent>(true);
            foreach (var themeComponent in themeComponents)
            {
               themeComponent.ApplyTheme(theme); 
            }
        }
#endif
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using ModernUIs.Theming;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace DrBlackRat.VRC.ModernUIs
{
    [CustomEditor(typeof(ThemeSelector))]
    public class ThemeSelectorEditor : UnityEditor.Editor
    {
        public VisualTreeAsset visualTree;
        private UnityEditor.Editor themeEditor;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            visualTree.CloneTree(root);
            
            var selector = (ThemeSelector)target;
            
            var applyButton = root.Q<Button>("applyButton");

            if (applyButton != null)
                applyButton.clicked += () => selector.ApplyThemeToChildren();
            
            return root;
        }
        
    }
}

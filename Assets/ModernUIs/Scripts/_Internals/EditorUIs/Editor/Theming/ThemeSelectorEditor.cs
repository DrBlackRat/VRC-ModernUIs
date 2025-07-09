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

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            visualTree.CloneTree(root);
            
            var theme = (ThemeSelector)target;
            
            var applyButton = root.Q<Button>("applyButton");

            if (applyButton != null)
                applyButton.clicked += () => theme.ApplyThemeToChildren();
            
            return root;
        }
    }
}

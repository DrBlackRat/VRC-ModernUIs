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
    [CustomEditor(typeof(Theme))]
    public class ThemeEditor : UnityEditor.Editor
    {
        public VisualTreeAsset visualTree;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            visualTree.CloneTree(root);
            var theme = (Theme)target;
            
            var applyButton = root.Q<Button>("applyButton");
            var applyAllButton = root.Q<Button>("applyEverythingButton");
            
            if (applyButton != null)
                applyButton.clicked += () => theme.ApplyToAllUsing();
            if (applyAllButton != null)
                applyAllButton.clicked += () => theme.ApplyToEverything();
            
            return root;
        }
    }
}

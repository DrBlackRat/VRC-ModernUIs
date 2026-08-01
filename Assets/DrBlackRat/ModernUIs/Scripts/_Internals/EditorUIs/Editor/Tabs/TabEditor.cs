using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace DrBlackRat.VRC.ModernUIs
{
    [CustomEditor(typeof(Tab))]
    public class TabEditor : UnityEditor.Editor
    {
        public VisualTreeAsset visualTree;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            visualTree.CloneTree(root);
            
            var theme = (Tab)target;
            var button = root.Q<Button>("moveToButton");
            if (button != null)
                button.clicked += () => theme.MoveToTab();
            
            return root;
        }
    }
}

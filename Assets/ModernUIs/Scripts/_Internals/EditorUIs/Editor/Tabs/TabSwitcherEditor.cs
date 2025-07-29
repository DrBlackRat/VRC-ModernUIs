using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    [CustomEditor(typeof(TabSwitcher))]
    public class TabSwitcherEditor : UnityEditor.Editor
    {
        public VisualTreeAsset visualTree;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement(); 
            visualTree.CloneTree(root);
            
            var tabSwitcher = (TabSwitcher)target;
            
            var addTabButton = root.Q<Button>("AddTabButton");

            if (addTabButton != null)
                addTabButton.clicked += () => tabSwitcher.AddTab();

            
            return root;
        }
    }
}

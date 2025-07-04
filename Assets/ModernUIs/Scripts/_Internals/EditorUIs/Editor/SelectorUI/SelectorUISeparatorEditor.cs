using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using DrBlackRat.VRC.ModernUIs.SelectorUI;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    [CustomEditor(typeof(SelectorUISeparator))]

    public class SelectorUISeparatorEditor : UnityEditor.Editor
    {
        public VisualTreeAsset visualTree;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            // UXML
            visualTree.CloneTree(root);
            
            return root;
        }
    }
}

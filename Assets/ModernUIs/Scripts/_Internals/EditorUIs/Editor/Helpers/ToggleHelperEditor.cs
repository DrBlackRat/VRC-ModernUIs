using System;
using System.Collections;
using System.Collections.Generic;
using DrBlackRat.VRC.ModernUIs.Helpers;
using DrBlackRat.VRC.ModernUIs.MirrorUI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    [CustomEditor(typeof(ToggleHelper))]
    public class ToggleHelperEditor : UnityEditor.Editor
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

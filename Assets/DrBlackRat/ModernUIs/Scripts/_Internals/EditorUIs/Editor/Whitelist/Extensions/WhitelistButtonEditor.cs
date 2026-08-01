using System;
using System.Collections;
using System.Collections.Generic;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using DrBlackRat.VRC.Whitelist;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    [CustomEditor(typeof(WhitelistButton))]
    public class WhitelistButtonEditor : UnityEditor.Editor
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

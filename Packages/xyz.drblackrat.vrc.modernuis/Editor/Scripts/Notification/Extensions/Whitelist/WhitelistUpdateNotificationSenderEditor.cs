using System;
using System.Collections;
using System.Collections.Generic;
using DrBlackRat.VRC.ModernUIs.Notifications;
using ModernUIs.Theming;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace DrBlackRat.VRC.ModernUIs
{
    [CustomEditor(typeof(WhitelistUpdateNotificationSender))]
    public class WhitelistUpdateNotificationSenderEditor : UnityEditor.Editor
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

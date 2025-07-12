using System;
using System.Collections;
using System.Collections.Generic;
using DrBlackRat.VRC.Hideout;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using DrBlackRat.VRC.ModernUIs.SelectorUI;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    [CustomEditor(typeof(SelectorUIEventProxy))]
    public class SelectorUIEventProxyEditor : UnityEditor.Editor

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

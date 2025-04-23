using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace DrBlackRat.VRC.ModernUIs
{
    [CustomEditor(typeof(Selector))]
    public class SelectorEditor : Editor
    {
        public VisualTreeAsset visualTree;
        
        private Toggle overrideProperty;
        private VisualElement overrideElements;
        private SerializedProperty overrideBoolProperty;

        public void OnEnable()
        {
            overrideBoolProperty = serializedObject.FindProperty("overrideDefaults");
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            
            // UXML
            visualTree.CloneTree(root);
            
            // Callbacks
            overrideElements = root.Q<VisualElement>("Overrides");
            overrideProperty = root.Q<Toggle>("overrideDefaults");
            overrideProperty.RegisterCallback<ChangeEvent<bool>>(OnOverrideChanged);

            CheckForDisplayType(overrideBoolProperty.boolValue);
            
            return root;
        }

        private void OnOverrideChanged(ChangeEvent<bool> evt)
        {
            CheckForDisplayType(evt.newValue);
        }

        private void CheckForDisplayType(bool value)
        {
            overrideElements.SetEnabled(value);
        }
    }
}

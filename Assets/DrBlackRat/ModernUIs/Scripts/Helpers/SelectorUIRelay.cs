using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.Udon;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UdonSharpEditor;
#endif

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    /// <summary>
    /// Sets up a Selector UI Relay Instance (Editor Only).
    /// </summary>
    [RequireComponent(typeof(SelectorUIRelayInstance))]
    public class SelectorUIRelay : MonoBehaviour
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        [SerializeField]
        [Tooltip("The Udon Behaviours that will be updated when the selection changes.")]
        private SelectorUIRelayOption[] selectorUIRelayOptions;
        
        private const string SelectionIdVariableName = "selectionId";
        private const string SelectionChangedEventName = "_SelectionChanged";
        
        [Serializable]
        private class SelectorUIRelayOption
        {
            [Tooltip("The Udon Behaviours that will be updated when the selection changes.")]
            public UdonBehaviour[] udonBehaviours;
            [Tooltip("The name of the Integer Variable that will be updated to the corresponding id when the selection changes.")]
            public string selectionIdVariableName = SelectionIdVariableName;
            [Tooltip("The name of the Event that will be fired when the selection changes.")]
            public string selectionChangedEventName = SelectionChangedEventName;
        }
        
        private void OnValidate()
        {
            foreach (var option in selectorUIRelayOptions)
            {
                if (string.IsNullOrEmpty(option.selectionIdVariableName))
                    option.selectionIdVariableName = SelectionIdVariableName;

                if (string.IsNullOrEmpty(option.selectionChangedEventName))
                    option.selectionChangedEventName = SelectionChangedEventName;
            }
        }

        /// <summary>
        /// Finds the <see cref="SelectorUIRelayInstance"/> and transfers the values to it.
        /// </summary>
        public void TransferValues()
        {
            var instance = GetComponent<SelectorUIRelayInstance>();
            if (instance == null)
            {
                MUIDebug.LogError("Selector UI Relay | No Selector UI Relay Instance found!", gameObject);
                return;
            }

            UdonBehaviour[][] udonBehaviours = new UdonBehaviour[selectorUIRelayOptions.Length][];
            string[] selectionIdVariableNames = new string[selectorUIRelayOptions.Length];
            string[] selectionChangedEventNames = new string[selectorUIRelayOptions.Length];

            for (int i = 0; i < selectorUIRelayOptions.Length; i++)
            {
                udonBehaviours[i] = selectorUIRelayOptions[i].udonBehaviours;
                selectionIdVariableNames[i] = selectorUIRelayOptions[i].selectionIdVariableName;
                selectionChangedEventNames[i] = selectorUIRelayOptions[i].selectionChangedEventName;
            }
            
            instance.udonBehaviours = udonBehaviours;
            instance.selectionIdVariableNames = selectionIdVariableNames;
            instance.selectionChangedEventNames = selectionChangedEventNames;
            
            UdonSharpEditorUtility.CopyProxyToUdon(instance);
        }
        
#endif
    }
}


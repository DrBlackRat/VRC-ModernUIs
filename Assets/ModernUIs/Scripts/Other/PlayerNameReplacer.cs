using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(-100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerNameReplacer : UdonSharpBehaviour
    {
        [SerializeField] [Tooltip("Key that will be replaced on the provided TextMeshProUGUI objects with the local player's name.")]
        private string playerNameKey = "{localPlayer}";
        public bool ReplaceOnLocalizationChange => replaceOnLocalizationChange;

        [SerializeField] [Tooltip("If enabled, the text on the TextMeshProUGUI objects will be replaced when the language is changed by the Language Selector UI.")]
        private bool replaceOnLocalizationChange = true;
        
        [SerializeField] [Tooltip("TextMeshProUGUI objects that will have the Player Name Key replaced with the local player's name.")]
        private TextMeshProUGUI[] textMeshProUGUI;

        private string localPlayerName;
        
        private void Start()
        {
            localPlayerName = Networking.LocalPlayer.displayName;
            ReplacePlayerName();
        }

        /// <summary>
        /// Replaces the player name in all provided TextMeshProUGUI objects with the local player's name.
        /// </summary>
        public void ReplacePlayerName()
        {
            foreach (var textMesh in textMeshProUGUI)
            {
                if (!Utilities.IsValid(textMesh)) continue;
                textMesh.text = textMesh.text.Replace(playerNameKey, localPlayerName);
            }
        }

        #if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            textMeshProUGUI = GetComponents<TextMeshProUGUI>();
        }
        #endif
    }
}


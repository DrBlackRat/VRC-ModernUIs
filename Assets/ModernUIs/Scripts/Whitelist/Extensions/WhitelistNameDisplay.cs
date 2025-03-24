using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistNameDisplay : UdonSharpBehaviour
    {
        [Header("Settings:")] 
        [Tooltip("Text Mesh Pro UGUI component that should get the names added to it.")]
        [SerializeField] private TextMeshProUGUI textMesh;
        [Tooltip("Whitelist Manager to grab the names from.")]
        [SerializeField] private WhitelistManager whitelistManager;

        private void Start()
        {
            whitelistManager._SetUpConnection(GetComponent<UdonBehaviour>());
        }

        public void _WhitelistUpdated()
        {
            textMesh.text = whitelistManager._GetNamesFormatted();
        }
    }
}

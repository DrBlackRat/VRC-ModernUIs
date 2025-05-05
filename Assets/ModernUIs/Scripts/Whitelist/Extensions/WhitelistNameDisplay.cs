using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistNameDisplay : UdonSharpBehaviour
    {
        [Tooltip("Whitelist Manager to grab the names from.")]
        [SerializeField] private WhitelistManager whitelistManager;
        
        [Tooltip("Text Mesh Pro UGUI component that should get the names added to it.")]
        [SerializeField] private TextMeshProUGUI textMesh;

        private void Start()
        {
            whitelistManager._SetUpConnection((IUdonEventReceiver)this);
        }

        public void _WhitelistUpdated()
        {
            textMesh.text = whitelistManager._GetNamesFormatted();
        }
    }
}

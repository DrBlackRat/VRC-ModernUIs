using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistNameDisplay : UdonSharpBehaviour
    {
        [FormerlySerializedAs("whitelistManager")]
        [Tooltip("Whitelist Manager to grab the names from.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        
        [Tooltip("Text Mesh Pro UGUI component that should get the names added to it.")]
        [SerializeField] private TextMeshProUGUI textMesh;

        private void Start()
        {
            whitelist._SetUpConnection((IUdonEventReceiver)this);
        }

        public void _WhitelistUpdated()
        {
            textMesh.text = whitelist._GetNamesFormatted();
        }
    }
}

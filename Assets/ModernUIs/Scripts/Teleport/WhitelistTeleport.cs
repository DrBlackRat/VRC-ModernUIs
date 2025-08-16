
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistTeleport : UdonSharpBehaviour
    {
        [SerializeField] private Transform teleportTransform;
        [FormerlySerializedAs("whitelistManager")] 
        [SerializeField] private WhitelistGetterBase whitelist;
        
        [Tooltip("Button that will have it's intractability enabled / disabled depending on if you are on the whitelist or not.")]
        [SerializeField] private Button teleportButton;
        [Tooltip("Game Object that is enabled if you are on the whitelist.")]
        [SerializeField] private GameObject whitelistedText;
        [Tooltip("Game Object that is enabled if you are NOT on the whitelist.")]
        [SerializeField] private GameObject notWhitelistedText;

        private bool hasAccess;

        private void Start()
        {
            whitelist._SetUpConnection(GetComponent<UdonBehaviour>());
        }

        public void _WhitelistUpdated()
        {
            hasAccess = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);
            
            if (teleportButton != null) teleportButton.interactable = hasAccess;
            
            if (whitelistedText != null) whitelistedText.SetActive(hasAccess);
            if (notWhitelistedText != null) notWhitelistedText.SetActive(!hasAccess);
        }
        
        public void _Teleport()
        {
            if (!hasAccess)
            {
                MUIDebug.LogError("You are not whitelisted!");
                return;
            }
            Networking.LocalPlayer.TeleportTo(teleportTransform.position, teleportTransform.rotation);
        }
    }
}

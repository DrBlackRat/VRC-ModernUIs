
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistTeleport : UdonSharpBehaviour
    {
        [SerializeField] private Transform teleportTransform;
        [SerializeField] private WhitelistManager whitelistManager;
        
        [Tooltip("Button that will have it's intractability enabled / disabled depending on if you are on the whitelist or not.")]
        [SerializeField] private Button teleportButton;
        [Tooltip("Text Mesh Pro UGUI component that will have it's text changed depending on if you are on the whitelist or not.")]
        [SerializeField] private TextMeshProUGUI teleportButtonText;
        [Tooltip("Text that is displayed if you are on the whitelist.")]
        [SerializeField] private string whitelistedText = "Teleport";
        [Tooltip("Text that is displayed if you are NOT on the whitelist.")]
        [SerializeField] private string notWhitelistedText = "Locked";

        private bool hasAccess;

        private void Start()
        {
            whitelistManager._SetUpConnection(GetComponent<UdonBehaviour>());
        }

        public void _WhitelistUpdated()
        {
            hasAccess = whitelistManager._IsPlayerWhitelisted(Networking.LocalPlayer);
            
            if (teleportButton != null) teleportButton.interactable = hasAccess;
            if (teleportButtonText != null) teleportButtonText.text = hasAccess ? whitelistedText : notWhitelistedText;
            
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

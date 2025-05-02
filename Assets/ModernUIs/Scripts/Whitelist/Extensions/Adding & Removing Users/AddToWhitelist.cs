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
    public class AddToWhitelist : UdonSharpBehaviour
    {
        [Tooltip("Whitelist Manager that will be adjusted")]
        [SerializeField] protected WhitelistManager whitelistManager;
        [Tooltip("Input Field of which to get the username from.")]
        [SerializeField] protected TMP_InputField inputField;
        
        [Tooltip("Requires user to be whitelisted to be able to add / remove a username. This will be enabled automatically when using the Synced Whitelist Manager.")]
        [SerializeField] protected bool requireWhitelisted;
        [Tooltip("Whitelist a user needs to be on to add / remove a username. If left empty it will be the same as the whitelistManager")]
        [SerializeField] protected WhitelistManager inputWhitelistManager;

        [Tooltip("Text Mesh Pro UGUI component that will have it's text changed depending on if you are on the whitelist or not.")]
        [SerializeField] protected TextMeshProUGUI placeholderText;
        [Tooltip("Text that is displayed if you are on the whitelist.")]
        [SerializeField] protected string whitelistedText = "Enter Username...";
        [Tooltip("Text that is displayed if you are NOT on the whitelist.")]
        [SerializeField] protected string notWhitelistedText = "Not Whitelisted!";
        
        protected bool hasAccess;

        protected void Start()
        {
            if (whitelistManager.GetUdonTypeID() == GetUdonTypeID<SyncedWhitelistManager>())
            {
                requireWhitelisted = true;
            }
            if (inputWhitelistManager == null) inputWhitelistManager = whitelistManager;
            inputWhitelistManager._SetUpConnection(GetComponent<UdonBehaviour>());
        }

        public void _WhitelistUpdated()
        {
            if (!requireWhitelisted) return;
            hasAccess = inputWhitelistManager._IsPlayerWhitelisted(Networking.LocalPlayer);

            inputField.interactable = hasAccess;
            placeholderText.text = hasAccess ? whitelistedText : notWhitelistedText;
        }

        public virtual void _InputFinished()
        {
            if (requireWhitelisted && !hasAccess)
            {
                MUIDebug.LogError("You are not whitelisted!");
                return;
            }
            whitelistManager._AddUser(inputField.text);
            inputField.SetTextWithoutNotify(String.Empty);
        }
    }
}


using System;
using System.Runtime.Remoting.Messaging;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using VRC.Economy;
using VRC.Udon.Common.Interfaces;


namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CustomGroupPageOpener : UdonSharpBehaviour
    {

        [Tooltip("Default GroupID")]
        [UdonSynced] [SerializeField] private string groupID;
        
        [Tooltip("Whitelist Manager to lock access.")]
        [SerializeField] private WhitelistSetterBase whitelist;
        [Tooltip("Input Field of which to get the group id from.")]
        [SerializeField] private TMP_InputField inputField;
        
        [Tooltip("Text Mesh Pro UGUI component that will have it's text changed depending on if you are on the whitelist or not.")]
        [SerializeField] private TextMeshProUGUI placeholderText;
        [Tooltip("Text that is displayed if you are on the whitelist and haven't entered an ID yet.")]
        [SerializeField] private string whitelistedText = "Enter GroupID...";
        [Tooltip("Text that is displayed if you are NOT on the whitelist.")]
        [SerializeField] private string notWhitelistedText = "Not Whitelisted!";

        private bool hasAccess;
        private const string LOG_PREFIX = "Custom Group Page Opener: ";
        
        
        void Start()
        {
            if (whitelist != null)
            {
                whitelist._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                hasAccess = true;
                UpdateUI();
            }
        }
        
        public override void OnDeserialization()
        {
            inputField.SetTextWithoutNotify(groupID);
        }
        
        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (CheckAccess(newOwner))
            {
                MUIDebug.Log($"{LOG_PREFIX}{requester.displayName} has made {newOwner.displayName} the new Network Owner.");
                return true; 
            }
            else
            {
                MUIDebug.LogError($"{LOG_PREFIX}{requester.displayName} has tried to make {newOwner.displayName} the Network Owner, but isn't allowed to!");
                return false;
            }
        }
        
        public void _WhitelistUpdated()
        {
            hasAccess = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);
            UpdateUI();
        }

        private bool CheckAccess(VRCPlayerApi player)
        {
            if (whitelist == null) 
                return true;
            return whitelist._IsPlayerWhitelisted(player);
        }

        private void UpdateUI()
        {
            inputField.interactable = hasAccess;
            placeholderText.text = hasAccess ? whitelistedText : notWhitelistedText;
        }
        
        public void _InputFinished()
        {
            if (!hasAccess)
            {
                MUIDebug.LogError($"{LOG_PREFIX}You are not whitelisted!");
                return;
            }

            groupID = inputField.text;
            //inputField.SetTextWithoutNotify(String.Empty);
            
            RequestSerialization();
        }

        public void _OpenGroupPage()
        {
            Store.OpenGroupPage(groupID);
        }
    }
}


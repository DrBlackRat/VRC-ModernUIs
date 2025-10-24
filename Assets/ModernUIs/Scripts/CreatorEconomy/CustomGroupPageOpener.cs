using System;
using System.Runtime.Remoting.Messaging;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UnityEngine.Serialization;
using VRC.Economy;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.Persistence;


namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CustomGroupPageOpener : UdonSharpBehaviour
    {

        [FormerlySerializedAs("groupID")]
        [Tooltip("Default GroupID")]
        [SerializeField] private string groupId = "grp_ae026340-d6a8-453f-9582-b4f2ee5ef4ca";
        
        [Tooltip("Whitelist Manager to lock access.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("Input Field of which to get the group id from.")]
        [SerializeField] private TMP_InputField inputField;
        
        [Tooltip("Turn on if the Group ID should be saved using Persistence. \nSaves for the player who entered it, loads for the first player who joins.")]
        [SerializeField] protected bool usePersistence;
        [Tooltip("Data Key that will be used to save / load this Setting, everything using Persistence should have a different Data Key.")]
        [SerializeField] protected string dataKey = "CHANGE THIS";
        [Range(0f, 30f)] [Tooltip("Persistence Load delay, useful if you need to wait for the Admin Whitelist to finish updating etc.")]
        [SerializeField] private float loadDelay = 5f;
        
        [Tooltip("Text Mesh Pro UGUI component that will have it's text changed depending on if you are on the whitelist or not.")]
        [SerializeField] private TextMeshProUGUI placeholderText;
        [Tooltip("Text that is displayed if you are on the whitelist and haven't entered an ID yet.")]
        public string whitelistedText = "Enter GroupID...";
        [Tooltip("Text that is displayed if you are NOT on the whitelist.")]
        public string notWhitelistedText = "Not Whitelisted!";

        [UdonSynced] private string netGroupID;
        
        private bool hasAccess;
        private VRCPlayerApi localPlayer;
        
        private const string LOG_PREFIX = "Custom Group Page Opener: ";
        
        void Start()
        {
            localPlayer = Networking.LocalPlayer;
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
        
        public void _WhitelistUpdated()
        {
            hasAccess = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);
            UpdateUI();
        }
        
        public void _LocalizationChanged() // Used for applying the changed strings
        {
            UpdateUI();
        }

        #region Persistence & Networking
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            // Needs to be local, have access, be the first who joined and requires an empty whitelist.
            if (player != localPlayer) return; 
            SendCustomEventDelayedSeconds(nameof(_DelayedLoad), loadDelay);
        }

        public void _DelayedLoad()
        {
            if (!hasAccess || !localPlayer.IsOwner(gameObject)) return;
            if (PlayerData.TryGetString(localPlayer, dataKey, out string result))
            {
                UpdateGroupID(result, false, true);
            }
        }
        
        public override void OnDeserialization()
        {
            UpdateGroupID(netGroupID, false, false);
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
        #endregion

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

        private void UpdateGroupID(string newId, bool saveData, bool updateNet)
        {
            if (newId == groupId) return;
            
            groupId = newId;
            inputField.SetTextWithoutNotify(groupId);
            
            if (saveData)
                PlayerData.SetString(dataKey, groupId);

            if (updateNet)
            {
                if (!localPlayer.IsOwner(gameObject))
                    Networking.SetOwner(localPlayer, gameObject);
                netGroupID = groupId;
                RequestSerialization();
            }
        }

        #region UI Input
        public void _InputFinished()
        {
            if (!hasAccess)
            {
                MUIDebug.LogError($"{LOG_PREFIX}You are not whitelisted!");
                return;
            }
            UpdateGroupID(inputField.text, true, true);
        }
        
        public void _OpenGroupPage()
        {
            Store.OpenGroupPage(groupId);
        }
        #endregion

    }
}


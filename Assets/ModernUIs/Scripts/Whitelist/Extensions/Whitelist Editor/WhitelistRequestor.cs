using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class WhitelistRequestor : WhitelistEditorBase
    {
        [Tooltip("Button for Requesting Access. Will have it's interactable set.")]
        [SerializeField] protected Button requestButton;
        [Tooltip("Request Button TMP. Will be changed between \"Already Whitelisted\", \"Request Access\", \"Remove Request\" and \"You're an Admin\".")]
        [SerializeField] protected TextMeshProUGUI requestButtonText;
        
        [Tooltip("Text that is displayed on the Request Button if you are Whitelisted.")]
        [SerializeField] protected string whitelistedText = "Already Whitelisted";
        [Tooltip("Text that is displayed on the Request Button if you are an Admin and \"Allow Admins To Request\" is disabled.")]
        [SerializeField] protected string adminText = "You're an Admin";
        [Tooltip("Text that is displayed on the Request Button if you aren't whitelisted and can request.")]
        [SerializeField] protected string requestText = "Request Access";
        [Tooltip("Text that is displayed on the Request Button if you aren't whitelisted and are currently requesting")]
        [SerializeField] protected string removeRequestText = "Remove Request";

        [UdonSynced]
        protected string serializedRequestingUsers;

        #region Unity, VRChat & ModernUIs Events
        protected override void Start()
        {
            base.Start();
            UpdateRequestButton();
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            var displayName = player.displayName;
            if (Networking.LocalPlayer.isMaster)
            {
                RemoveOtherUser(displayName, true); 
                UpdateCountDisplays();
            }
        }
        
        public override void _WhitelistUpdated()
        {
            base._WhitelistUpdated();
            UpdateRequestButton();
        }

        public override void _AdminWhitelistUpdated()
        {
            base._AdminWhitelistUpdated();
            UpdateRequestButton();
        }

        #endregion

        #region Networking
        public override void OnDeserialization()
        {
            if (VRCJson.TryDeserializeFromJson(serializedRequestingUsers, out DataToken result))
            {
                // Add if not on whitelistedUsers
                var requestors = result.DataList;
                for (var i = 0; i < requestors.Count; i++)
                {
                    if (otherUsers.ContainsKey(requestors[i])) continue;
                    AddOtherUser(requestors[i].String, true);
                }
            
                // Remove if on whitelistedUsers but not on whitelist
                var keys = otherUsers.GetKeys();
                for (var i = 0; i < keys.Count; i++)
                {
                    if (requestors.Contains(keys[i])) continue;
                    RemoveOtherUser(keys[i].String, true);
                }
            }
            UpdateRequestButton();
            UpdateCountDisplays();
        }

        public override void OnPreSerialization()
        {
            if (VRCJson.TrySerializeToJson(otherUsers.GetKeys(), JsonExportType.Minify, out DataToken result))
            {
                serializedRequestingUsers = result.String;
            }
        }
        #endregion
        
        #region Accces Checks & Related UI Updates
        protected void UpdateRequestButton()
        {
            var isWhitelisted = whitelistedUsers.ContainsKey(localName);
            var isRequesting = otherUsers.ContainsKey(localName);
            var isBlockingAdmin = hasAdminWhitelist && !showOrAllowAdmins && adminWhitelist._IsPlayerWhitelisted(localName);
            
            requestButton.interactable = !(isWhitelisted || isBlockingAdmin);
            if (isWhitelisted)
            {
                requestButtonText.text = whitelistedText;
            } else if (isRequesting)
            {
                requestButtonText.text = removeRequestText;
            }
            else if (isBlockingAdmin)
            {
                requestButtonText.text = adminText;
            }
            else
            {
                requestButtonText.text = requestText;
            }
        }
        #endregion
        
        #region Adding & Removing Users
        protected override bool AddOtherUser(string username, bool sendNet)
        {
            if (!base.AddOtherUser(username, sendNet)) 
                return false;
            
            if (sendNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }

            return true;
        }
        
        protected override bool RemoveOtherUser(string username, bool sendNet)
        {
            if (!base.RemoveOtherUser(username, sendNet)) 
                return false;
            
            if (sendNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }

            return true;
        }
        #endregion

        #region Button API
        public override void _Add(string username)
        {
            base._Add(username);
            UpdateCountDisplays(); 
        }
        
        public override void _Remove(string username)
        {
            base._Remove(username);
            UpdateRequestButton();
        }

        public void _Decline(string username)
        {
            if (!hasAccess) return;
            
            RemoveOtherUser(username, true);
            
            UpdateRequestButton();
            UpdateCountDisplays(); 
        }

        public void _RequestAccess()
        {
            if (!otherUsers.ContainsKey(localName))
            {
                AddOtherUser(localName, true);
            }
            else
            {
                RemoveOtherUser(localName, true);
            }
            UpdateRequestButton();
            UpdateCountDisplays(); 
        }
        #endregion

    }
}

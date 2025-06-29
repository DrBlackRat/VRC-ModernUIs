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
    public class WhitelistRequestor : IWhitelistEditor
    {
        [Tooltip("Prefab that will be instantiated for each user on the whitelist.")]
        [SerializeField] protected GameObject whitelistedPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform whitelistedTransform;
        
        [Tooltip("Prefab that will be instantiated for each user that is requesting Access.")]
        [SerializeField] protected GameObject requestingPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform requestingTransform;

        [FormerlySerializedAs("whitelistManager")]
        [Tooltip("Whitelist that will be adjusted.")]
        [SerializeField] protected WhitelistSetterBase whitelist;
        [Tooltip("If being on the Admin Whitelist is required to edit.")]
        [SerializeField] protected bool requireWhitelisted;
        [FormerlySerializedAs("adminWhitelistManager")]
        [Tooltip("Admin Whitelist, if left empty the normal Whitelist Manger will be used.")]
        [SerializeField] protected WhitelistGetterBase adminWhitelist;
        [Tooltip("If Admins should be allowed to Request Access. It is recommended to keep this off.")]
        [SerializeField] protected bool allowAdminsToRequest;

        [Tooltip("Button for Requesting Access. Will have it's interacable set.")]
        [SerializeField] protected Button requestButton;
        [Tooltip("Request Button TMP. Will be changed between \"Already Whitelisted\", \"Request Access\", \"Remove Request\" and \"You're an Admin\".")]
        [SerializeField] protected TextMeshProUGUI requestButtonText;
        
        protected DataDictionary whitelistedUsers = new DataDictionary();
        protected DataDictionary requestingUsers = new DataDictionary();

        [UdonSynced]
        protected string serializedRequestingUsers;

        protected bool hasAccess;
        protected bool hasAdminWhitelist;

        private void Start()
        {
            whitelist._SetUpConnection((IUdonEventReceiver)this);
            
            if (whitelist.GetUdonTypeID() == GetUdonTypeID<SyncedWhitelistManager>()) requireWhitelisted = true;
            if (requireWhitelisted)
            {
                if (adminWhitelist == null)
                {
                    adminWhitelist = whitelist;
                    hasAdminWhitelist = false;
                }
                else
                {
                    hasAdminWhitelist = true;
                }
                adminWhitelist._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                hasAccess = true;
                hasAdminWhitelist = false;
            }
            UpdateRequestButton();
        }
        
        public override void OnDeserialization()
        {
            if (VRCJson.TryDeserializeFromJson(serializedRequestingUsers, out DataToken result))
            {
                // Add if not on whitelistedUsers
                var requestors = result.DataList;
                for (var i = 0; i < requestors.Count; i++)
                {
                    if (requestingUsers.ContainsKey(requestors[i])) continue;
                    AddRequestingUser(requestors[i].String, true);
                }
            
                // Remove if on whitelistedUsers but not on whitelist
                var keys = requestingUsers.GetKeys();
                for (var i = 0; i < keys.Count; i++)
                {
                    if (requestors.Contains(keys[i])) continue;
                    RemoveRequestingUser(keys[i].String, true);
                }
            }
            UpdateRequestButton();
        }

        public override void OnPreSerialization()
        {
            if (VRCJson.TrySerializeToJson(requestingUsers.GetKeys(), JsonExportType.Minify, out DataToken result))
            {
                serializedRequestingUsers = result.String;
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            RemoveRequestingUser(player.displayName, true);
        }
        
        public void _WhitelistUpdated()
        {
            if (requireWhitelisted) UpdateAccess(adminWhitelist._IsPlayerWhitelisted(Networking.LocalPlayer));
            
            // Add if not on whitelistedUsers
            var whitelist = this.whitelist._GetUsersAsList();
            for (var i = 0; i < whitelist.Count; i++)
            {
                if (whitelistedUsers.ContainsKey(whitelist[i])) continue;
                AddWhitelistUser(whitelist[i].String);
            }
            
            // Remove if on whitelistedUsers but not on whitelist
            var keys = whitelistedUsers.GetKeys();
            for (var i = 0; i < keys.Count; i++)
            {
                if (whitelist.Contains(keys[i])) continue;
                RemoveWhitelistUser(keys[i].String);
            }

            if (hasAdminWhitelist && !allowAdminsToRequest)
            {
                var admins = adminWhitelist._GetUsersAsList();
                for (var i = 0; i < admins.Count; i++)
                {
                    RemoveRequestingUser(admins[i].String, true); 
                }
            }
            UpdateRequestButton();
        }
        
        protected void UpdateAccess(bool newHasAccess)
        {
            if (hasAccess != newHasAccess)
            {
                hasAccess = newHasAccess;
                var whitelistKeys = whitelistedUsers.GetKeys();
                for (var i = 0; i < whitelistKeys.Count; i++)
                {
                    var user = (WhitelistUser)whitelistedUsers[whitelistKeys[i]].Reference;
                    user.HasAccess = hasAccess;
                }
                var requestingUsersKeys = requestingUsers.GetKeys();
                for (var i = 0; i < requestingUsersKeys.Count; i++)
                {
                    var user = (WhitelistRequestingUser)requestingUsers[requestingUsersKeys[i]].Reference;
                    user.HasAccess = hasAccess;
                }
            }
        }

        protected void UpdateRequestButton()
        {
            var localName = Networking.LocalPlayer.displayName;
            var isWhitelisted = whitelistedUsers.ContainsKey(localName);
            var isRequesting = requestingUsers.ContainsKey(localName);
            var isBlockingAdmin = hasAdminWhitelist && !allowAdminsToRequest && adminWhitelist._IsPlayerWhitelisted(localName);
            
            requestButton.interactable = !(isWhitelisted || isBlockingAdmin);
            if (isWhitelisted)
            {
                requestButtonText.text = "Already Whitelisted";
            } else if (isRequesting)
            {
                requestButtonText.text = "Remove Request";
            }
            else if (isBlockingAdmin)
            {
                requestButtonText.text = "You're an Admin";
            }
            else
            {
                requestButtonText.text = "Request Access";
            }
        }
        
        protected virtual void AddWhitelistUser(string username)
        {
            if (whitelistedUsers.ContainsKey(username)) return;

            var newUserObj = Instantiate(whitelistedPrefab, whitelistedTransform.position, whitelistedTransform.rotation, whitelistedTransform);
            newUserObj.transform.localScale = Vector3.one;
            var whitelistUser = newUserObj.GetComponent<WhitelistUser>();
            whitelistUser._Setup(this, username, true, hasAccess);
            whitelistedUsers.Add(username, whitelistUser);
            
            RemoveRequestingUser(username, true);
        }
        
        protected virtual void RemoveWhitelistUser(string username)
        {
            if (!whitelistedUsers.ContainsKey(username)) return;
            
            if (whitelistedUsers.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var whitelistUser = (WhitelistUser)value.Reference;
                whitelistUser._Destroy();
                whitelistedUsers.Remove(username);
            }
        }

        protected virtual void AddRequestingUser(string username, bool skipNet)
        {
            if (requestingUsers.ContainsKey(username) || whitelistedUsers.ContainsKey(username)) return;
            if (hasAdminWhitelist && !allowAdminsToRequest && adminWhitelist._IsPlayerWhitelisted(username)) return;
            
            var newUserObj = Instantiate(requestingPrefab, requestingTransform.position, requestingTransform.rotation, requestingTransform);
            newUserObj.transform.localScale = Vector3.one;
            var requestingUser = newUserObj.GetComponent<WhitelistRequestingUser>();
            requestingUser._Setup(this, username, hasAccess);
            requestingUsers.Add(username, requestingUser);

            if (!skipNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }
        }
        
        protected virtual void RemoveRequestingUser(string username, bool skipNet)
        {
            if (!requestingUsers.ContainsKey(username)) return;
            
            if (requestingUsers.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var requestingUser = (WhitelistRequestingUser)value.Reference;
                requestingUser._Destroy();
                requestingUsers.Remove(username);
            }
            if (!skipNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }
        }
        
        public override void _Add(string username)
        {
            if (!hasAccess) return;
            AddWhitelistUser(username);
            whitelist._AddUser(username, (IUdonEventReceiver)this);
            UpdateRequestButton();
        }
        
        public override void _Remove(string username)
        {
            if (!hasAccess) return;
            RemoveWhitelistUser(username);
            whitelist._RemoveUser(username, (IUdonEventReceiver)this);
            UpdateRequestButton();
        }

        public void _Decline(string username)
        {
            if (!hasAccess) return;
            RemoveRequestingUser(username, false);
            UpdateRequestButton();
        }

        public void _RequestAccess()
        {
            var localName = Networking.LocalPlayer.displayName;
            if (!requestingUsers.ContainsKey(localName))
            {
                AddRequestingUser(localName, false);
            }
            else
            {
                RemoveRequestingUser(localName, false);
            }
            UpdateRequestButton();
        }
    }
}

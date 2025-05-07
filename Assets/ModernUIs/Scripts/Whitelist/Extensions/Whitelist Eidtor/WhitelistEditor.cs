using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    public class WhitelistEditor : UdonSharpBehaviour
    {
        [SerializeField] private GameObject whitelistedPrefab;
        [SerializeField] private Transform whitelistedTransform;
        
        [SerializeField] private GameObject notWhitelistedPrefab;
        [SerializeField] private Transform notWhitelistedTransform;

        [SerializeField] private WhitelistManager whitelistManager;
        [SerializeField] private bool requireWhitelisted;
        [SerializeField] private WhitelistManager adminWhitelistManager;
        [SerializeField] private bool showAdmins;

        private DataDictionary whitelistedUsers = new DataDictionary();
        private DataDictionary notWhitelistedUsers = new DataDictionary();

        private DataList allUsers = new DataList();

        private bool hasAccess;
        private bool blockWhitelistUpdate;
        private bool hasAdminWhitelist;
        
        private void Start()
        {
            whitelistManager._SetUpConnection((IUdonEventReceiver)this);
            
            if (whitelistManager.GetUdonTypeID() == GetUdonTypeID<SyncedWhitelistManager>()) requireWhitelisted = true;
            if (requireWhitelisted)
            {
                if (adminWhitelistManager == null)
                {
                    adminWhitelistManager = whitelistManager;
                    hasAdminWhitelist = false;
                }
                else
                {
                    hasAdminWhitelist = true;
                }
                adminWhitelistManager._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                hasAccess = true;
                hasAdminWhitelist = false;
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var name = player.displayName;
            allUsers.Add(name);
            AddNotWhitelistUser(name);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            allUsers.Remove(player.displayName);
            RemoveNotWhitelistUser(player.displayName);
        }

        public void _WhitelistUpdated()
        {
            // blocks it being called by updating the whitelist manager
            if (blockWhitelistUpdate)
            {
                blockWhitelistUpdate = false;
                return;
            }
            
            if (requireWhitelisted) UpdateAccess(adminWhitelistManager._IsPlayerWhitelisted(Networking.LocalPlayer));
            
            // Add if not on whitelistedUsers
            var whitelist = whitelistManager._GetNames();
            foreach (var username in whitelist)
            {
                if (whitelistedUsers.ContainsKey(username)) continue;
                AddWhitelistUser(username);
            }
            
            // Remove if on whitelistedUsers but not on whitelist
            var keys = whitelistedUsers.GetKeys();
            for (var i = 0; i < keys.Count; i++)
            {
                if (whitelist.Contains((string)keys[i])) continue;
                RemoveWhitelistUser((string)keys[i]);
            }

            if (hasAdminWhitelist && !showAdmins)
            {
                var admins = adminWhitelistManager._GetNames();
                foreach (var admin in admins)
                {
                    RemoveNotWhitelistUser(admin);
                }
            }
        }

        private void UpdateAccess(bool newHasAccess)
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
                var notWhitelistKeys = notWhitelistedUsers.GetKeys();
                for (var i = 0; i < notWhitelistKeys.Count; i++)
                {
                    var user = (WhitelistUser)notWhitelistedUsers[notWhitelistKeys[i]].Reference;
                    user.HasAccess = hasAccess;
                }
            }
        }

        private void AddWhitelistUser(string username)
        {
            if (whitelistedUsers.ContainsKey(username)) return;

            var newUserObj = Instantiate(whitelistedPrefab, whitelistedTransform.position, whitelistedTransform.rotation, whitelistedTransform);
            newUserObj.transform.localScale = Vector3.one;
            var whitelistUser = newUserObj.GetComponent<WhitelistUser>();
            whitelistUser._Setup(this, username, true, hasAccess);
            whitelistedUsers.Add(username, whitelistUser);
            
            RemoveNotWhitelistUser(username);
        }
        
        private void RemoveWhitelistUser(string username)
        {
            if (!whitelistedUsers.ContainsKey(username)) return;
            
            if (whitelistedUsers.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var whitelistUser = (WhitelistUser)value.Reference;
                whitelistUser._Destroy();
                whitelistedUsers.Remove(username);
            }    
            
            AddNotWhitelistUser(username);
        }

        private void AddNotWhitelistUser(string username)
        {
            if (notWhitelistedUsers.ContainsKey(username) || whitelistedUsers.ContainsKey(username) || !allUsers.Contains(username)) return;
            if (hasAdminWhitelist && !showAdmins && adminWhitelistManager._IsPlayerWhitelisted(username)) return;
            
            var newUserObj = Instantiate(notWhitelistedPrefab, notWhitelistedTransform.position, notWhitelistedTransform.rotation, notWhitelistedTransform);
            newUserObj.transform.localScale = Vector3.one;
            var notWhitelistUser = newUserObj.GetComponent<WhitelistUser>();
            notWhitelistUser._Setup(this, username,  false, hasAccess);
            notWhitelistedUsers.Add(username, notWhitelistUser);
        }

        private void RemoveNotWhitelistUser(string username)
        {
            if (!notWhitelistedUsers.ContainsKey(username)) return;
            
            if (notWhitelistedUsers.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var notWhitelistUser = (WhitelistUser)value.Reference;
                notWhitelistUser._Destroy();
                notWhitelistedUsers.Remove(username);
            }   
        }

        public void _Add(string username)
        {
            if (!hasAccess) return;
            AddWhitelistUser(username);
            
            blockWhitelistUpdate = true;
            whitelistManager._AddUser(username);
        }

        public void _Remove(string username)
        {
            if (!hasAccess) return;
            RemoveWhitelistUser(username);
            
            blockWhitelistUpdate = true;
            whitelistManager._RemoveUser(username);
        }
    }
}


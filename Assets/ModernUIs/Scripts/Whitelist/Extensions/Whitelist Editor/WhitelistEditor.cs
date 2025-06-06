﻿using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistEditor : IWhitelistEditor
    {
        [Tooltip("Prefab that will be instantiated for each user on the whitelist.")]
        [SerializeField] protected GameObject whitelistedPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform whitelistedTransform;
        
        [Tooltip("Prefab that will be instantiated for each user that is NOT on the whitelist.")]
        [SerializeField] protected GameObject notWhitelistedPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform notWhitelistedTransform;

        [Tooltip("Whitelist Manager that will be adjusted.")]
        [SerializeField] protected WhitelistManager whitelistManager;
        [Tooltip("If being on the Admin Whitelist is required to edit.")]
        [SerializeField] protected bool requireWhitelisted;
        [Tooltip("Admin Whitelist Manager, if left empty the normal Whitelist Manger will be used.")]
        [SerializeField] protected WhitelistManager adminWhitelistManager;
        [Tooltip("If Admins should be shown in the \"Not Whitelisted\" section. It is recommended to keep this off.")]
        [SerializeField] protected bool showAdmins;

        protected DataDictionary whitelistedUsers = new DataDictionary();
        protected DataDictionary notWhitelistedUsers = new DataDictionary();

        protected DataList allUsers = new DataList();

        protected bool hasAccess;
        protected bool hasAdminWhitelist;
        
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
            var username = player.displayName;
            allUsers.Add(username);
            AddNotWhitelistUser(username);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            allUsers.Remove(player.displayName);
            RemoveNotWhitelistUser(player.displayName);
        }

        public void _WhitelistUpdated()
        {
            if (requireWhitelisted) UpdateAccess(adminWhitelistManager._IsPlayerWhitelisted(Networking.LocalPlayer));
            
            // Add if not on whitelistedUsers
            var whitelist = whitelistManager._GetUsersAsList();
            for (var i = 0; i < whitelist.Count; i++)
            {
                if (whitelistedUsers.ContainsKey(whitelist[i].String)) continue;
                AddWhitelistUser(whitelist[i].String);
            }
            
            // Remove if on whitelistedUsers but not on whitelist
            var keys = whitelistedUsers.GetKeys();
            for (var i = 0; i < keys.Count; i++)
            {
                if (whitelist.Contains(keys[i])) continue;
                RemoveWhitelistUser(keys[i].String);
            }

            if (hasAdminWhitelist && !showAdmins)
            {
                var admins = adminWhitelistManager._GetUsersAsList();
                for (var i = 0; i < admins.Count; i++)
                {
                    RemoveNotWhitelistUser(admins[i].String); 
                }
            }
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
                var notWhitelistKeys = notWhitelistedUsers.GetKeys();
                for (var i = 0; i < notWhitelistKeys.Count; i++)
                {
                    var user = (WhitelistUser)notWhitelistedUsers[notWhitelistKeys[i]].Reference;
                    user.HasAccess = hasAccess;
                }
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
            
            RemoveNotWhitelistUser(username);
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
            
            AddNotWhitelistUser(username);
        }

        protected virtual void AddNotWhitelistUser(string username)
        {
            if (notWhitelistedUsers.ContainsKey(username) || whitelistedUsers.ContainsKey(username) || !allUsers.Contains(username)) return;
            if (hasAdminWhitelist && !showAdmins && adminWhitelistManager._IsPlayerWhitelisted(username)) return;
            
            var newUserObj = Instantiate(notWhitelistedPrefab, notWhitelistedTransform.position, notWhitelistedTransform.rotation, notWhitelistedTransform);
            newUserObj.transform.localScale = Vector3.one;
            var notWhitelistUser = newUserObj.GetComponent<WhitelistUser>();
            notWhitelistUser._Setup(this, username,  false, hasAccess);
            notWhitelistedUsers.Add(username, notWhitelistUser);
        }

        protected virtual void RemoveNotWhitelistUser(string username)
        {
            if (!notWhitelistedUsers.ContainsKey(username)) return;
            
            if (notWhitelistedUsers.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var notWhitelistUser = (WhitelistUser)value.Reference;
                notWhitelistUser._Destroy();
                notWhitelistedUsers.Remove(username);
            }   
        }

        public override void _Add(string username)
        {
            if (!hasAccess) return;
            AddWhitelistUser(username);
            whitelistManager._AddUser(username, (IUdonEventReceiver)this);
        }

        public override void _Remove(string username)
        {
            if (!hasAccess) return;
            RemoveWhitelistUser(username);
            whitelistManager._RemoveUser(username, (IUdonEventReceiver)this);
        }
    }
}


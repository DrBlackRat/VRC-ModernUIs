using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using TMPro;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistEditor : WhitelistEditorBase
    {
        [Tooltip("Prefab that will be instantiated for each user on the whitelist.")]
        [SerializeField] protected GameObject whitelistedPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform whitelistedTransform;
        [Tooltip("Text Mesh Pro UGUI component that will have the amount of people on the whitelist displayed.")]
        [SerializeField] private TextMeshProUGUI whitelistedCountDisplay;
        
        [Tooltip("Prefab that will be instantiated for each user that is NOT on the whitelist.")]
        [SerializeField] protected GameObject notWhitelistedPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform notWhitelistedTransform;
        [Tooltip("Text Mesh Pro UGUI component that will have the amount of people NOT on whitelist displayed.")]
        [SerializeField] private TextMeshProUGUI notWhitelistedCountDisplay;

        [FormerlySerializedAs("whitelistManager")]
        [Tooltip("Whitelist that will be adjusted.")]
        [SerializeField] protected WhitelistSetterBase whitelist;
        [Tooltip("If being on the Admin Whitelist is required to edit.")]
        [SerializeField] protected bool requireWhitelisted;
        [FormerlySerializedAs("adminWhitelistManager")]
        [Tooltip("Admin Whitelist, if left empty the normal Whitelist Manger will be used.")]
        [SerializeField] protected WhitelistGetterBase adminWhitelist;
        [Tooltip("If Admins should be shown in the \"Not Whitelisted\" section. It is recommended to keep this off.")]
        [SerializeField] protected bool showAdmins;
        
        [Tooltip("If enabled admins can only add a limited amount of users. They will also only be able to remove the ones they added.")]
        [SerializeField] protected bool limitAdding;
        [Range(0, 40)] [Tooltip("The amount of users a single admin can add.")]
        [SerializeField] protected int maxAddAmount = 2;

        protected DataDictionary whitelistedUsers = new DataDictionary();
        protected DataDictionary notWhitelistedUsers = new DataDictionary();

        protected DataList allUsers = new DataList();

        protected bool hasAccess;
        protected bool hasAdminWhitelist;
        
        protected DataList usersAdded = new DataList();
        protected bool canAdd = true;
        
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
            
            CheckMaxUsersAdded(true);
            UpdateCountDisplays(); 
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var username = player.displayName;
            allUsers.Add(username);
            AddNotWhitelistUser(username);
            UpdateCountDisplays();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            allUsers.Remove(player.displayName);
            RemoveNotWhitelistUser(player.displayName);
            UpdateCountDisplays();
        }

        private void UpdateCountDisplays()
        {
            if (notWhitelistedCountDisplay != null)
                notWhitelistedCountDisplay.text = notWhitelistedUsers.Count.ToString();
            if (whitelistedCountDisplay != null)
                whitelistedCountDisplay.text = whitelistedUsers.Count.ToString();
        }

        public void _WhitelistUpdated()
        {
            if (requireWhitelisted) UpdateAccess(adminWhitelist._IsPlayerWhitelisted(Networking.LocalPlayer));
            
            // Add if not on whitelistedUsers
            var whitelist = this.whitelist._GetUsersAsList();
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
                var admins = adminWhitelist._GetUsersAsList();
                for (var i = 0; i < admins.Count; i++)
                {
                    RemoveNotWhitelistUser(admins[i].String); 
                }
            }
            UpdateCountDisplays();
        }

        protected void UpdateAccess(bool newHasAccess)
        {
            if (hasAccess != newHasAccess)
            {
                hasAccess = newHasAccess;
                UpdateWhitelistedAccess();
                UpdateNotWhitelistedAccess();
            }
        }

        private void UpdateWhitelistedAccess()
        {
            var whitelistKeys = whitelistedUsers.GetKeys();
            for (var i = 0; i < whitelistKeys.Count; i++)
            {
                var user = (WhitelistUser)whitelistedUsers[whitelistKeys[i]].Reference;
                if (limitAdding)
                {
                    user.HasAccess = hasAccess && usersAdded.Contains(user.DisplayName);
                }
                else
                {
                    user.HasAccess = hasAccess;
                }
                
            } 
        }
        
        private void UpdateNotWhitelistedAccess()
        {
            var notWhitelistKeys = notWhitelistedUsers.GetKeys();
            for (var i = 0; i < notWhitelistKeys.Count; i++)
            {
                var user = (WhitelistUser)notWhitelistedUsers[notWhitelistKeys[i]].Reference;
                user.HasAccess = hasAccess && canAdd;;
            }
        }

        private void CheckMaxUsersAdded(bool skipSameCheck = false)
        {
            if (!limitAdding) return;
            
            var newCanAdd = usersAdded.Count < maxAddAmount;
            if (newCanAdd == canAdd && !skipSameCheck) return;

            canAdd = newCanAdd;
            UpdateWhitelistedAccess();
            UpdateNotWhitelistedAccess();
        }

        protected virtual void AddWhitelistUser(string username)
        {
            if (whitelistedUsers.ContainsKey(username)) return;

            var newUserObj = Instantiate(whitelistedPrefab, whitelistedTransform.position, whitelistedTransform.rotation, whitelistedTransform);
            newUserObj.transform.localScale = Vector3.one;
            var whitelistUser = newUserObj.GetComponent<WhitelistUser>();
            if (limitAdding)
            {
                whitelistUser._Setup(this, username, true, hasAccess && usersAdded.Contains(username));
            }
            else
            {
                whitelistUser._Setup(this, username, true, hasAccess);
            }
            whitelistUser._ApplyThemeColors(prefabTextColor, prefabBackgroundColor);
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
            
            if (limitAdding && usersAdded.Contains(username))
            {
                usersAdded.Remove(username);
                CheckMaxUsersAdded();
            }
            
            AddNotWhitelistUser(username);
        }

        protected virtual void AddNotWhitelistUser(string username)
        {
            if (notWhitelistedUsers.ContainsKey(username) || whitelistedUsers.ContainsKey(username) || !allUsers.Contains(username)) return;
            if (hasAdminWhitelist && !showAdmins && adminWhitelist._IsPlayerWhitelisted(username)) return;
            
            var newUserObj = Instantiate(notWhitelistedPrefab, notWhitelistedTransform.position, notWhitelistedTransform.rotation, notWhitelistedTransform);
            newUserObj.transform.localScale = Vector3.one;
            var notWhitelistUser = newUserObj.GetComponent<WhitelistUser>();
            notWhitelistUser._Setup(this, username,  false, hasAccess && canAdd);
            notWhitelistUser._ApplyThemeColors(prefabTextColor, prefabBackgroundColor);
            notWhitelistedUsers.Add(username, notWhitelistUser);
            
            UpdateCountDisplays();
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
            
            if (limitAdding)
            {
                usersAdded.Add(username);
                CheckMaxUsersAdded();  
            }
            
            AddWhitelistUser(username);
            whitelist._AddUser(username, (IUdonEventReceiver)this);
            UpdateCountDisplays();
        }

        public override void _Remove(string username)
        {
            if (!hasAccess) return;
            RemoveWhitelistUser(username);
            whitelist._RemoveUser(username, (IUdonEventReceiver)this);
            UpdateCountDisplays();
        }
    }
}


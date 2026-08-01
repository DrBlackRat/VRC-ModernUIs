using System;
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
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WhitelistEditorBase : UdonSharpBehaviour
    {
        // Whitelist Prefabs
        [Tooltip("Prefab that will be instantiated for each user on the whitelist.")]
        [SerializeField] protected GameObject whitelistedPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform whitelistedTransform;
        [Tooltip("Text Mesh Pro UGUI component that will have the amount of people on the whitelist displayed.")]
        [SerializeField] protected TextMeshProUGUI whitelistedCountDisplay;
        
        // Not Whitelist / Requesting Prefabs
        [Tooltip("Prefab that will be instantiated for each user that is not whitelisted / requesting Access.")]
        [SerializeField] protected GameObject otherUsersPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform otherUsersTransform;
        [Tooltip("Text Mesh Pro UGUI component that will have the amount of people not whitelisted / requesting displayed.")]
        [SerializeField] protected TextMeshProUGUI otherUsersCountDisplay;

        // Whitelist Connection
        [Tooltip("Synced Whitelist Manager that will be adjusted.")]
        [SerializeField] protected WhitelistSetterBase whitelist;
        [Tooltip("Admin Whitelist to control who can use this, if left empty everyone will be able to.")]
        [SerializeField] protected WhitelistGetterBase adminWhitelist;
        [Tooltip("If Admins should be shown as not Whitelisted / allowed to Request Access. It is recommended to keep this off.")]
        [SerializeField] protected bool showOrAllowAdmins;
        
        // Limits
        [Tooltip("If enabled admins can only add a limited amount of users. They will also only be able to remove the ones they added.")]
        [SerializeField] protected bool limitAdding;
        [Range(0, 40)] [Tooltip("The amount of users a single admin can add.")]
        [SerializeField] protected int maxAddAmount = 2;
        
        // Sorting
        [Tooltip("If enabled the the users shown will automatically be sorted alphabetically.\n This does come with a slight performance impact, otherwise you can call \"_SortWhitelistedUsers\" and \"_SortOtherUsers\" manually.")]
        [SerializeField] protected bool autoSort = true;
        
        protected DataDictionary whitelistedUsers = new DataDictionary();
        protected DataDictionary otherUsers = new DataDictionary();

        protected string localName;
        protected bool hasAccess;
        protected bool hasAdminWhitelist;

        protected DataList addedList = new DataList();
        protected bool canAdd = true;

        protected bool whitelistCollapsed;
        protected bool otherUsersCollapsed;
        protected bool sortCooldownEnabled = true;

        #region Unity, VRChat & ModernUIs Events
        protected virtual void Start()
        {
            localName = Networking.LocalPlayer.displayName;
            
            whitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_WhitelistUpdated));
            hasAdminWhitelist = adminWhitelist != null;
            if (hasAdminWhitelist)
            {
                adminWhitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_AdminWhitelistUpdated));
            }
            else
            {
                UpdateAccess(true);
            }
            
            CheckMaxUsersAdded(true);
            UpdateCountDisplays(); 
            
            SendCustomEventDelayedSeconds(nameof(_EndWhitelistSortCooldown), 12f);
            SendCustomEventDelayedSeconds(nameof(_EndOtherSortCooldown), 13f);
        }
        
        public void _EndWhitelistSortCooldown()
        {
            sortCooldownEnabled = false;
            if (autoSort) _SortWhitelistedUsers();
        }
        
        public void _EndOtherSortCooldown()
        {
            sortCooldownEnabled = false;
            if (autoSort) _SortOtherUsers();
        }
        
        public virtual void _WhitelistUpdated()
        {
            addedList = this.whitelist._GetUsersAddedBy(localName);
            CheckMaxUsersAdded();
            
            var whitelist = this.whitelist._GetUsersAsList();
            
            // Remove if on whitelistedUsers but not on whitelist
            var keys = whitelistedUsers.GetKeys();
            for (var i = 0; i < keys.Count; i++)
            {
                if (whitelist.Contains(keys[i])) continue;
                RemoveWhitelistUser(keys[i].String);
            }
            
            // Add if not on whitelistedUsers
            for (var i = 0; i < whitelist.Count; i++)
            {
                if (whitelistedUsers.ContainsKey(whitelist[i])) continue;
                AddWhitelistUser(whitelist[i].String);
            }
            
            UpdateCountDisplays();
        }

        public virtual void _AdminWhitelistUpdated()
        {
            UpdateAccess(adminWhitelist._IsPlayerWhitelisted(localName));
            
            if (hasAdminWhitelist && !showOrAllowAdmins)
            {
                var admins = adminWhitelist._GetUsersAsList();
                for (var i = 0; i < admins.Count; i++)
                {
                    RemoveOtherUser(admins[i].String, false); 
                }
            }
            
            UpdateCountDisplays();
            
            addedList = this.whitelist._GetUsersAddedBy(localName);
            CheckMaxUsersAdded();
        }
        #endregion

        #region Accces Checks & Related UI Updates
        protected void UpdateCountDisplays()
        {
            if (otherUsersCountDisplay != null)
                otherUsersCountDisplay.text = otherUsers.Count.ToString();
            if (whitelistedCountDisplay != null)
                whitelistedCountDisplay.text = whitelistedUsers.Count.ToString();
        }

        protected void CheckMaxUsersAdded(bool skipSameCheck = false)
        {
            if (!limitAdding) return;
            
            var newCanAdd = addedList.Count < maxAddAmount;
            if (newCanAdd == canAdd && !skipSameCheck) return;

            canAdd = newCanAdd;
            UpdateWhitelistedAccess();
            UpdateOtherUsersAccess();
        }

        protected void UpdateAccess(bool newHasAccess)
        {
            if (hasAccess == newHasAccess) return;
            hasAccess = newHasAccess;
            UpdateWhitelistedAccess();
            UpdateOtherUsersAccess();
        }

        private void UpdateWhitelistedAccess()
        {
            var whitelistKeys = whitelistedUsers.GetKeys();
            for (var i = 0; i < whitelistKeys.Count; i++)
            {
                var user = (WhitelistUser)whitelistedUsers[whitelistKeys[i]].Reference;
                if (limitAdding)
                {
                    user.HasAccess = hasAccess && addedList.Contains(user.DisplayName);
                }
                else
                {
                    user.HasAccess = hasAccess;
                }
            } 
        }

        private void UpdateOtherUsersAccess()
        {
            var otherUsersKeys = otherUsers.GetKeys();
            for (var i = 0; i < otherUsersKeys.Count; i++)
            {
                var user = (WhitelistUser)otherUsers[otherUsersKeys[i]].Reference;
                user.HasAccess = hasAccess && canAdd;;
            }
        }
        #endregion

        #region Collapsing

        public virtual void _UpdateWhitelistCollapsed(bool newCollapsed)
        {
            whitelistCollapsed = newCollapsed;

            var values = whitelistedUsers.GetValues();
            for (int i = 0; i < values.Count; i++)
            {
                ((WhitelistUser)values[i].Reference).gameObject.SetActive(!whitelistCollapsed);
            }
        }

        public virtual void _UpdateOtherUsersCollapsed(bool newCollapsed)
        {
            otherUsersCollapsed = newCollapsed;

            var values = otherUsers.GetValues();
            for (int i = 0; i < values.Count; i++)
            {
                ((WhitelistUser)values[i].Reference).gameObject.SetActive(!otherUsersCollapsed);
            }
        }

        #endregion
        
        #region Adding & Removing Users
        protected virtual bool AddWhitelistUser(string username)
        {
            if (whitelistedUsers.ContainsKey(username)) return false;

            var newUserObj = Instantiate(whitelistedPrefab, whitelistedTransform.position,
                whitelistedTransform.rotation, whitelistedTransform);
            newUserObj.transform.localScale = Vector3.one;
            var whitelistUser = newUserObj.GetComponent<WhitelistUser>();
            if (limitAdding)
            {
                whitelistUser._Setup(this, username, true, hasAccess && addedList.Contains(username));
            }
            else
            {
                whitelistUser._Setup(this, username, true, hasAccess);
            }
            newUserObj.SetActive(!whitelistCollapsed);
            whitelistedUsers.Add(username, whitelistUser);

            RemoveOtherUser(username, false);
            
            if (autoSort && !sortCooldownEnabled) _SortWhitelistedUsers();
            return true;
        }

        protected virtual bool RemoveWhitelistUser(string username)
        {
            if (!whitelistedUsers.ContainsKey(username)) return false;
            
            if (whitelistedUsers.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var whitelistUser = (WhitelistUser)value.Reference;
                whitelistUser._Destroy();
                whitelistedUsers.Remove(username);
            }
            
            if (addedList.Contains(username))
            {
                addedList.Remove(username);
                CheckMaxUsersAdded();
            }

            return true;
        }
        
        protected virtual bool AddOtherUser(string username,  bool sendNet)
        {
            if (otherUsers.ContainsKey(username) || whitelistedUsers.ContainsKey(username)) return false;
            if (hasAdminWhitelist && !showOrAllowAdmins && adminWhitelist._IsPlayerWhitelisted(username)) return false;
            
            var newUserObj = Instantiate(otherUsersPrefab, otherUsersTransform.position, otherUsersTransform.rotation, otherUsersTransform);
            newUserObj.transform.localScale = Vector3.one;
            var otherUser = newUserObj.GetComponent<WhitelistUser>();
            otherUser._Setup(this, username,  false, hasAccess && canAdd);
            newUserObj.SetActive(!otherUsersCollapsed);
            otherUsers.Add(username, otherUser);
            
            UpdateCountDisplays();

            if (autoSort && !sortCooldownEnabled) _SortOtherUsers();
            return true;
        }
        
        protected virtual bool RemoveOtherUser(string username, bool sendNet)
        {
            if (!otherUsers.ContainsKey(username)) return false;
            
            if (otherUsers.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var otherUser = (WhitelistUser)value.Reference;
                otherUser._Destroy();
                otherUsers.Remove(username);
            }

            return true;
        }
        #endregion
        
        #region Sorting
        /// <summary>
        /// Sorts the whitelisted users alphabetically.
        /// </summary>
        public void _SortWhitelistedUsers()
        {
            var userKeys = whitelistedUsers.GetKeys();
            userKeys.Sort();

            for (int i = 0; i < userKeys.Count; i++)
                ((WhitelistUser)whitelistedUsers[userKeys[i]].Reference).transform.SetSiblingIndex(i + 2); // +2 as the first two elements are placeholder objects
        }
        
        /// <summary>
        /// Sorts the other users alphabetically.
        /// </summary>
        public void _SortOtherUsers()
        {
            var userKeys = otherUsers.GetKeys();
            userKeys.Sort();

            for (int i = 0; i < userKeys.Count; i++)
                ((WhitelistUser)otherUsers[userKeys[i]].Reference).transform.SetSiblingIndex(i + 2); // +2 as the first two elements are placeholder objects
        }
        #endregion
        
        #region Button API
        public virtual void _Add(string username)
        {
            if (!hasAccess || !canAdd) return;
            
            addedList.Add(username);
            CheckMaxUsersAdded(); 
            
            AddWhitelistUser(username);
            whitelist._AddUser(username, (IUdonEventReceiver)this, nameof(_WhitelistUpdated));
            
            UpdateCountDisplays();
        }
        
        public virtual void _Remove(string username)
        {
            if (!hasAccess) return;
            
            RemoveWhitelistUser(username);
            whitelist._RemoveUser(username, (IUdonEventReceiver)this, nameof(_WhitelistUpdated));
            
            UpdateCountDisplays();
        }
        #endregion

    }
}

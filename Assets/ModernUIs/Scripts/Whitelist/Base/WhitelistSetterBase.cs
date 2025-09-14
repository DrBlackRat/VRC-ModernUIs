using System.Text;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist.Base
{
    /// <summary>
    /// Extends <see cref="WhitelistGetterBase"/> to add functionality for modifying the whitelist.
    /// Provides methods to add and remove users.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WhitelistSetterBase : WhitelistGetterBase
    {
        /// <summary>
        /// If adding / removing users should log which users were added / removed.
        /// </summary>
        [Tooltip("If adding / removing users should log which users were added / removed.")]
        [SerializeField] protected bool debugLogging;

        public override bool _IsReadOnly() => false;

        #region Adding
        /// <summary>
        /// Adds a specific user to the white list. Skips duplicate username.
        /// </summary>
        /// <param name="username">The username to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was added. Defaults to true.</param>
        public virtual void _AddUser(string username, IUdonEventReceiver senderBehaviour = null, bool notify = true)
        {
            if (whitelist.Contains(username))
            {
                if (debugLogging)
                    MUIDebug.LogWarning($"Whitelist Manager: Could not add {username} to the whitelist as they are already on it!");
                return;
            }
            
            if (usersAddedBy.TryGetValue(localName, out DataToken added))
            {
                added.DataList.Add(username);
                usersAddedBy.SetValue(localName, added);
            }
            else
            {
                DataList newAdded = new DataList();
                newAdded.Add(username);
                usersAddedBy.Add(localName, newAdded);
            }
            
            whitelist.Add(username);
            
            if (debugLogging)
                MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
            if (notify) 
                WhitelistUpdated(senderBehaviour);
        }
        
        /// <summary>
        /// Adds a DataList of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        /// <param name="newUsernames">DataList of usernames to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (newUsernames == null || newUsernames.Count == 0) return;

            for (int i = 0; i < newUsernames.Count; i++)
            {
                var token = newUsernames[i];
                if (token.IsNull) continue;
                
                var username = token.String;
                _AddUser(username, null, false);
            }
            WhitelistUpdated(senderBehaviour);
        }
        
        /// <summary>
        /// Adds an Array of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        /// <param name="newUsernames">Array of usernames to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (newUsernames == null || newUsernames.Length == 0) return;

            foreach (var username in newUsernames)
            {
                _AddUser(username, null, false);
            }
            WhitelistUpdated(senderBehaviour);
        }
        #endregion

        #region Removing
        /// <summary>
        /// Removes a specific user from the white list.
        /// </summary>
        /// <param name="username">The username to remove from the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was removed. Defaults to true.</param>
        public virtual void _RemoveUser(string username, IUdonEventReceiver senderBehaviour = null, bool notify = true)
        {
            if (!whitelist.Contains(username))
            {
                if (debugLogging)
                    MUIDebug.LogWarning($"Whitelist Manager: Could not remove {username} from the whitelist as they are not on it!");
                return;
            }
            
            if (usersAddedBy.TryGetValue(localName, out DataToken added))
            {
                DataList addedList = added.DataList;
                if (addedList.Contains(username))
                {
                    addedList.Remove(username);
                    usersAddedBy.SetValue(localName, added);
                }
                else
                {
                    TryRemoveOther(username);
                }
            }
            else 
            {
                TryRemoveOther(username);
            }

            whitelist.Remove(username);
            
            if (debugLogging)
                MUIDebug.Log($"Whitelist Manager: Removed {username} from the whitelist.");
            if (notify) 
                WhitelistUpdated(senderBehaviour);
        }

        // I do not like doing this as this is slow but it *should* only be an edge case, and it only ever runs on one client
        private void TryRemoveOther(string username)
        {
            string adder = _GetWhoAdded(username);
            if (adder != null)
            {
                if (usersAddedBy.TryGetValue(adder, out DataToken added))
                {
                    DataList addedList = added.DataList;
                    if (addedList.Contains(username))
                    {
                        addedList.Remove(username);
                        usersAddedBy.SetValue(adder, added);
                    }
                    else
                    {
                        MUIDebug.LogError($"Whitelist Manager: Unexpected Error when trying to remove user! Please message DrBlackRat about this.");
                    }
                }
            }
        }

        /// <summary>
        /// Removes a DataList of usernames from the whitelist. Skips duplicate names.
        /// </summary>
        /// <param name="usernamesToRemove">DataList of usernames to remove from the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _RemoveUsers(DataList usernamesToRemove, IUdonEventReceiver senderBehaviour = null)
        {
            if (usernamesToRemove == null || usernamesToRemove.Count == 0) return;

            for (int i = 0; i < usernamesToRemove.Count; i++)
            {
                var token = usernamesToRemove[i];
                if (token.IsNull) continue;
                
                var username = token.String;
                _RemoveUser(username, null, false);
            }
            WhitelistUpdated(senderBehaviour);
        }
        
        /// <summary>
        /// Removes an Array of usernames from the whitelist. Skips duplicate names.
        /// </summary>
        /// <param name="usernamesToRemove">Array of usernames to remove from the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _RemoveUsers(string[] usernamesToRemove, IUdonEventReceiver senderBehaviour = null)
        {
            if (usernamesToRemove == null || usernamesToRemove.Length == 0) return;

            foreach (var username in usernamesToRemove)
            {
                _RemoveUser(username, null, false);
            }
            WhitelistUpdated(senderBehaviour);
        }
        #endregion

        #region Replacing
        /// <summary>
        /// Fully replaces the current whitelist with a new one. This is usually not recommended, try _AddUsers or _RemoveUser first.
        /// </summary>
        /// <param name="newUsernames">DataList of usernames to replace the whitelist with.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (newUsernames == null) return;
            whitelist = new DataList();
            usersAddedBy = new DataDictionary();
            _AddUsers(newUsernames, senderBehaviour);
        }

        /// <summary>
        /// Fully replaces the current whitelist with a new one. This is usually not recommended, try _AddUsers or _RemoveUser first.
        /// </summary>
        /// <param name="newUsernames">Array of usernames to replace the whitelist with</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _ReplaceWhitelist(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (newUsernames == null) return;
            whitelist = new DataList();
            usersAddedBy = new DataDictionary();
            _AddUsers(newUsernames, senderBehaviour);
        }
        #endregion
        
        #region Added By
        /// <summary>
        /// Removed everyone that was added by the provided user.
        /// </summary>
        /// <param name="username">Display Name of the user who you want to remove ever added user of.</param>
        public virtual void _RemoveAddedBy(string username)
        {
            if (usersAddedBy.TryGetValue(username, out DataToken added))
            {
                _RemoveUsers(added.DataList.DeepClone());
            }
            else
            {
                if (debugLogging)
                    MUIDebug.LogWarning($"Whitelist Manager: Could not remove users, as {username} has not added any yet!");
                    
            }
        }
        #endregion
    }
}

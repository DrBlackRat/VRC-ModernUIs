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

        public override bool _IsReadOnly() => false;

        #region Adding
        /// <summary>
        /// Adds a specific user to the whitelist. Skips duplicate username.
        /// </summary>
        /// <param name="username">The username to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was added. Defaults to true.</param>
        /// <returns>True if the user was added, false if they were already on the whitelist.</returns>
        public virtual bool _AddUser(string username, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (whitelist.Contains(username))
            {
                if (debugLogging)
                    MUIDebug.LogWarning($"Could not add {username} to {gameObject.name} as they are already on it!", gameObject);
                return false;
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
                MUIDebug.Log($"Added {username} to {gameObject.name}.", gameObject);
            if (notify) 
                WhitelistUpdated(senderBehaviour, senderEvent);
            return true;
        }
        
        /// <summary>
        /// Adds a DataList of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        /// <param name="newUsernames">DataList of usernames to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was added. Defaults to true.</param>
        /// <returns>True if all users were added, false if any were already on the whitelist or the provided list was invalid / empty.</returns>
        public virtual bool _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (newUsernames == null || newUsernames.Count == 0) return false;

            bool allAdded = true;
            for (int i = 0; i < newUsernames.Count; i++)
            {
                var token = newUsernames[i];
                if (token.IsNull) continue;
                
                var username = token.String;
                if (!_AddUser(username, null, null,false))
                    allAdded = false;
            }
            if (notify) 
                WhitelistUpdated(senderBehaviour, senderEvent);
            return allAdded;
        }
        
        /// <summary>
        /// Adds an Array of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        /// <param name="newUsernames">Array of usernames to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was added. Defaults to true.</param>
        /// <returns>True if all users were added, false if any were already on the whitelist or the provided array was invalid / empty.</returns>
        public virtual bool _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (newUsernames == null || newUsernames.Length == 0) return false;

            bool allAdded = true;
            foreach (var username in newUsernames)
            {
                if (!_AddUser(username, null, null, false))
                    allAdded = false;
            }
            if (notify) 
                WhitelistUpdated(senderBehaviour, senderEvent);
            return allAdded;
        }
        
        /// <summary>
        /// Adds an Array of <see cref="VRCPlayerApi"/>s to the whitelist. Skips duplicate usernames.
        /// </summary>
        /// <param name="newPlayers">Array of usernames to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was added. Defaults to true.</param>
        /// <returns>True if all users were added, false if any were already on the whitelist or the provided list was invalid / empty.</returns>
        public virtual bool _AddUsers(VRCPlayerApi[] newPlayers, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (newPlayers == null || newPlayers.Length == 0) return false;

            bool allAdded = true;
            foreach (var player in newPlayers)
            {
                if (!_AddUser(player.displayName, null, null, false))
                    allAdded = false;
            }
            if (notify) 
                WhitelistUpdated(senderBehaviour, senderEvent);
            return allAdded;
        }
        #endregion

        #region Removing

        /// <summary>
        /// Removes a specific user from the whitelist.
        /// </summary>
        /// <param name="username">The username to remove from the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was removed. Defaults to true.</param>
        /// <returns>True if the user was removed, false if they were not on the whitelist.</returns>
        public virtual bool _RemoveUser(string username, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (!whitelist.Contains(username))
            {
                if (debugLogging)
                    MUIDebug.LogWarning($"Could not remove {username} from the {gameObject.name} as they are not on it!", gameObject);
                return false;
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
                MUIDebug.Log($"Removed {username} from {gameObject.name}.", gameObject);
            if (notify) 
                WhitelistUpdated(senderBehaviour, senderEvent);
            return true;
        }

        // I do not like doing this as this is slow, but it *should* only be an edge case, and it only ever runs on one client
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
                        MUIDebug.LogError($"Unexpected Error when trying to remove {username} from {gameObject.name}!", gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a DataList of usernames from the whitelist. Skips duplicate names.
        /// </summary>
        /// <param name="usernamesToRemove">DataList of usernames to remove from the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">Optional | If other scripts should be notified that users were removed. Defaults to true.</param>
        /// <returns>True if all users were removed, false if any were not on the whitelist or the provided list was invalid / empty.</returns>
        public virtual bool _RemoveUsers(DataList usernamesToRemove, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (usernamesToRemove == null || usernamesToRemove.Count == 0) return false;

            bool allRemoved = true;
            for (int i = 0; i < usernamesToRemove.Count; i++)
            {
                var token = usernamesToRemove[i];
                if (token.IsNull) continue;
                
                var username = token.String;
                if (!_RemoveUser(username, null, null, false))
                    allRemoved = false;
            }
            if (notify)
                WhitelistUpdated(senderBehaviour, senderEvent);
            return allRemoved;
        }
        
        /// <summary>
        /// Removes an Array of usernames from the whitelist. Skips duplicate names.
        /// </summary>
        /// <param name="usernamesToRemove">Array of usernames to remove from the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">>Optional | If other scripts should be notified that users were removed. Defaults to true</param>
        /// <returns>True if all users were removed, false if any were not on the whitelist or the provided array was invalid / empty.</returns>
        public virtual bool _RemoveUsers(string[] usernamesToRemove, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (usernamesToRemove == null || usernamesToRemove.Length == 0) return false;

            bool allRemoved = true;
            foreach (var username in usernamesToRemove)
            {
                if (!_RemoveUser(username, null, null, false))
                    allRemoved = false;
            }
            if (notify)
                WhitelistUpdated(senderBehaviour, senderEvent);
            return allRemoved;
        }

        /// <summary>
        /// Resets / clears out the whitelist.
        /// </summary>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was removed. Defaults to true.</param>
        public virtual void _ClearWhitelist(IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            whitelist = new DataList();
            usersAddedBy = new DataDictionary();
            
            if (notify) 
                WhitelistUpdated(senderBehaviour, senderEvent);
        }
        #endregion

        #region Replacing
        /// <summary>
        /// Fully replaces the current whitelist with a new one. This is usually not recommended, try _AddUsers or _RemoveUser first.
        /// </summary>
        /// <param name="newUsernames">DataList of usernames to replace the whitelist with.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="directReplacement">Optional | If enabled, none of the normal duplication and safety checks will be done. Only enable if you know what you are doing!</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was added. Defaults to true.</param>
        public virtual void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool directReplacement = false, bool notify = true)
        {
            if (newUsernames == null) return;
            usersAddedBy = new DataDictionary();

            if (directReplacement)
            {
                whitelist = newUsernames.ShallowClone();
                usersAddedBy.Add(localName, newUsernames.ShallowClone());
            }
            else
            {
                whitelist = new DataList();
                _AddUsers(newUsernames, senderBehaviour, senderEvent, false);
            }
            if (notify)
                WhitelistUpdated(senderBehaviour, senderEvent);
        }

        /// <summary>
        /// Fully replaces the current whitelist with a new one. This is usually not recommended, try _AddUsers or _RemoveUser first.
        /// </summary>
        /// <param name="newUsernames">Array of usernames to replace the whitelist with</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// /// <param name="directReplacement">Optional | If enabled, none of the normal duplication and safety checks will be done. Only enable if you know what you are doing!</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was added. Defaults to true.</param>
        public virtual void _ReplaceWhitelist(string[] newUsernames, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool directReplacement = false, bool notify = true)
        {
            if (newUsernames == null) return;
            usersAddedBy = new DataDictionary();
            
            if (directReplacement)
            {
                whitelist = newUsernames.ToDataList();
                usersAddedBy.Add(localName, whitelist.ShallowClone());
            }
            else
            {
                whitelist = new DataList();
                _AddUsers(newUsernames, senderBehaviour, senderEvent, false);
            }
            
            if (notify)
                WhitelistUpdated(senderBehaviour, senderEvent);
        }
        #endregion
        
        #region Added By
        /// <summary>
        /// Removes everyone added by the provided user.
        /// </summary>
        /// <param name="username">Display Name of the user who you want to remove ever added user of.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event that called the method. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        /// <param name="notify">Optional | If other scripts should be notified that a user was removed. Defaults to true.</param>
        /// <returns> True if users added by the provided player were found and removed, false otherwise.</returns>
        public virtual bool _RemoveAddedBy(string username, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (usersAddedBy.TryGetValue(username, out DataToken added))
            {
                _RemoveUsers(added.DataList.DeepClone(), senderBehaviour, senderEvent, notify);
                return true;
            }
            return false;
        }
        #endregion
    }
}

using System;
using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist.Base
{
    /// <summary>
    /// Base class of the Modern UI Whitelist System.
    /// Handles <c>_WhitelistUpdated</c> connections and provides methods to check if a player is whitelisted, 
    /// as well as to retrieve the full whitelist.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WhitelistGetterBase : UdonSharpBehaviour
    {
        protected DataList whitelist = new DataList();
        protected DataDictionary usersAddedBy = new DataDictionary(); // Key: User Who added, Value: DataList of users they added
        
        protected DataDictionary connectedBehaviours = new DataDictionary(); // key: Udon Behaviour value: event name
        
        protected string localName;

        protected const string WHITELIST_UPDATED_EVENT = "_WhitelistUpdated";

        protected virtual void Start()
        {
            localName = Networking.LocalPlayer.displayName;
        }

        /// <summary>
        /// Sets up a connection with the Whitelist Manager.
        /// Used to receive an update event if the whitelist changes.
        /// </summary>
        /// <param name="behaviour">Event Receiver which should have the update event called on.</param>
        /// <param name="eventName">Optional | Event what should be called, if left empty "_WhitelistUpdated" will be used.</param>
        public virtual void _SetUpConnection(IUdonEventReceiver behaviour, string eventName = WHITELIST_UPDATED_EVENT)
        {
            connectedBehaviours.SetValue((UnityEngine.Object)behaviour, eventName);
            behaviour.SendCustomEventDelayedFrames(eventName, 1); // Send delayed by one frame to prevent too much happening inside one frame
        }

        #region Whitelist Checks
        /// <summary>
        /// Checks if a player is whitelisted.
        /// </summary>
        /// <param name="playerApi">Player that should be checked.</param>
        /// <returns>True if the player is whitelisted, false if not.</returns>
        public virtual bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            if (playerApi == null) return false;
            return whitelist.Contains(playerApi.displayName);
        }

        /// <summary>
        /// Checks if a player's display name is whitelisted.
        /// </summary>
        /// <param name="displayName">Display Name of the player that should be checked.</param>
        /// <returns>True if the player is whitelisted, false if not.</returns>
        public virtual bool _IsPlayerWhitelisted(string displayName)
        {
            if (displayName == null) return false;
            return whitelist.Contains(displayName);
        }
        /// <summary>
        /// Generates a string containing all whitelisted users.
        /// </summary>
        /// <returns>Formatted string with all whitelisted display names. Formatted to one line per display name.</returns>
        public virtual string _GetNamesFormatted()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < whitelist.Count; i++)
            {
                sb.Append(whitelist[i]);
                if (i != whitelist.Count - 1)
                {
                    sb.Append("\n");
                }
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// Generates an array containing all whitelisted users.
        /// This is slow! It's recommended to do <c>_GetUsersAsList()</c> instead if you can.
        /// </summary>
        /// <returns>An Array of usernames currently on the whitelist.</returns>
        public virtual string[] _GetUsersAsArray()
        {
            string[] usernames = new string[whitelist.Count];

            for (int i = 0; i < whitelist.Count; i++)
            {
                usernames[i] = whitelist[i].String;
            }
            
            return usernames;
        }
        
        /// <summary>
        /// Generates a copy of the current whitelist.
        /// Recommended over <c>_GetUsersAsArray()</c> for better performance.
        /// </summary>
        /// <returns>A copy of the whitelist.</returns>>
        public virtual DataList _GetUsersAsList()
        {
            return whitelist.DeepClone();
        }

        /// <summary>
        /// Returns the whitelist user count.
        /// </summary>
        /// <returns>Amount of users on the whitelist.</returns>
        public virtual int _GetCount()
        {
            return whitelist.Count;
        }
        #endregion

        #region Added By Checks
        /// <summary>
        /// Retrieves a list of all users that were added to the whitelist by the specified user.
        /// </summary>
        /// <param name="username">Display name of the user whose added entries should be returned.</param>
        /// <returns>
        /// A deep-cloned <see cref="DataList"/> containing the users added by the specified player. 
        /// Returns an empty list if none are found.
        /// </returns>
        public virtual DataList _GetUsersAddedBy(string username)
        {
            if (usersAddedBy.TryGetValue(username, out DataToken added))
            {
                return added.DataList.DeepClone();
            }
            else
            {
                return new DataList();
            }
        }

        /// <summary>
        /// Returns the number of users added to the whitelist by the specified user.
        /// </summary>
        /// <param name="username">Display name of the user whose additions should be counted.</param>
        /// <returns>
        /// The count of users added by the specified player. 
        /// Returns 0 if the player has not added any users.
        /// </returns>
        public virtual int _GetUsersAddedByCount(string username)
        {
            if (usersAddedBy.TryGetValue(username, out DataToken added))
            {
                return added.DataList.Count;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Retrieves the display name of the user who added the specified username to the whitelist.
        /// This is slow! Try to avoid this when possible.
        /// </summary>
        /// <param name="username">Display name of the user whose adder should be found.</param>
        /// <returns>
        /// The display name of the player who added the specified user, or <c>null</c> if not found.
        /// </returns>
        public virtual string _GetWhoAdded(string username)
        {
            DataList addedKeys = usersAddedBy.GetKeys();
            for (int i = 0; i < addedKeys.Count; i++)
            {
                if (usersAddedBy.TryGetValue(addedKeys[i], out DataToken value))
                {
                    DataList users = value.DataList;
                    for (int j = 0; j < users.Count; j++)
                    {
                        if (username == users[j].String)
                        {
                            return addedKeys[i].String;
                        }
                    }
                }
            }

            return null;
        }
        #endregion
        
        
        /// <summary>
        /// Should be called whenever the whitelist is changed or updated. 
        /// Notifies all connected behaviours, unless suppressed.
        /// </summary>
        /// <param name="senderBehaviour">
        /// Optional | The behaviour that triggered the update. If provided, it will be excluded from receiving the update event.
        /// </param>
        protected virtual void WhitelistUpdated(IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.Log($"Whitelist Manager: Whitelist Updated");
            
            if (connectedBehaviours == null) return;
            var keys = connectedBehaviours.GetKeys();
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].IsNull) continue;
                
                // Prevent Sending update event to sender
                var receiver = (IUdonEventReceiver)keys[i].Reference;
                if (receiver == senderBehaviour) continue;
                
                if (connectedBehaviours.TryGetValue(keys[i], out DataToken output))
                {
                    receiver.SendCustomEventDelayedFrames((string)output, 1); // Send delayed by one frame to prevent too much happening inside one frame
                }
            }
        }
    }
}

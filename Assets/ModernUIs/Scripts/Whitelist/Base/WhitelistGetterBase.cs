using System;
using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs;
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
        [Tooltip("If enabled you will see logs for things like whitelist updates, adding / removing users, etc.")]
        [SerializeField] protected bool debugLogging;
        
        protected DataList whitelist = new DataList(); // Note: In the futrue this could be replaced with a Dictionary for better performance.
        protected DataDictionary usersAddedBy = new DataDictionary(); // Key: User Who added, Value: DataList of users they added
        
        protected DataDictionary connectedBehaviours = new DataDictionary(); // Key: Udon Behaviour, Value: DataList of events (some scripts could try to connect with multiple events)

        // Cached values
        protected bool formattedNamesChanged = true;
        protected string formattedNames;
        
        protected VRCPlayerApi localPlayerApi;
        protected string localName;

        protected const string WHITELIST_UPDATED_EVENT = "_WhitelistUpdated";

        protected virtual void Start()
        {
            localPlayerApi = Networking.LocalPlayer;
            localName = Networking.LocalPlayer.displayName;
        }

        /// <summary>
        /// Allows checking if this whitelist can be written to. Useful to check before casting to <see cref="WhitelistSetterBase"/>.
        /// </summary>
        /// <returns>bool | True if Whitelist Getter Base, false if Whitelist Setter Base.</returns>
        public virtual bool _IsReadOnly() => true;
        
        /// <summary>
        /// Allows checking if this whitelist is synced. 
        /// </summary>
        /// <returns>bool | True if synced, false if not.</returns>
        public virtual bool _IsSynced() => false;

        /// <summary>
        /// Sets up a connection with the Whitelist Manager.
        /// Used to receive an update event if the whitelist changes.
        /// </summary>
        /// <param name="behaviour">Event Receiver which should have the update event called on.</param>
        /// <param name="eventName">Optional | Event what should be called, if left empty "_WhitelistUpdated" will be used.</param>
        public virtual void _SetUpConnection(IUdonEventReceiver behaviour, string eventName = WHITELIST_UPDATED_EVENT)
        {
            var behaviourObj = (UnityEngine.Object)behaviour;
            DataList events;
            
            if (connectedBehaviours.TryGetValue(behaviourObj, out DataToken value))
            {
                events = value.DataList;
            }
            else
            {
                events = new DataList();
            }
            
            events.Add(eventName);
            connectedBehaviours.SetValue(behaviourObj, events);
            
            behaviour.SendCustomEventDelayedFrames(eventName, 1); // Send delayed by one frame to prevent too much happening inside one frame
        }

        #region Whitelist Checks
        /// <summary>
        /// Checks if a whitelisted player is currently present in the instance.
        /// Needs to be checked in <see cref="UdonSharpBehaviour.OnPlayerJoined(VRCPlayerApi)"/> and <see cref="UdonSharpBehaviour.OnPlayerLeft(VRCPlayerApi)"/> to work properly.
        /// </summary>
        /// <param name="playerNameToExclude">Display Name of a player to exclude from checking.</param>
        /// <returns>bool | True if a whitelisted player is present, false otherwise.</returns>
        public virtual bool _IsWhitelistPlayerPresent(string playerNameToExclude)
        {
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            var hasNameToExclude = !string.IsNullOrEmpty(playerNameToExclude);
            
            foreach (var player in players)
            {
                if (!player.IsValid()) continue;
                if (!_IsPlayerWhitelisted(player)) continue;
                if (hasNameToExclude && player.displayName == playerNameToExclude) continue;
                return true;
            }

            return false;  
        }
        /// <summary>
        /// Checks if a whitelisted player is currently present in the instance.
        /// Needs to be checked in <see cref="UdonSharpBehaviour.OnPlayerJoined(VRCPlayerApi)"/> and <see cref="UdonSharpBehaviour.OnPlayerLeft(VRCPlayerApi)"/> to work properly.
        /// </summary>
        /// <param name="playerApiToExclude"><see cref="VRCPlayerApi"/> of a player to exclude from checking.</param>
        /// <returns>bool | True if a whitelisted player is present, false otherwise.</returns>
        public bool _IsWhitelistPlayerPresent(VRCPlayerApi playerApiToExclude)
        {
            return _IsWhitelistPlayerPresent(Utilities.IsValid(playerApiToExclude) ? playerApiToExclude.displayName : null);
        }
        
        /// <summary>
        /// Checks if a whitelisted player is currently present in the instance.
        /// Needs to be checked in <see cref="UdonSharpBehaviour.OnPlayerJoined(VRCPlayerApi)"/> and <see cref="UdonSharpBehaviour.OnPlayerLeft(VRCPlayerApi)"/> to work properly.
        /// </summary>
        /// <returns>bool | True if a whitelisted player is present, false otherwise.</returns>
        public bool _IsWhitelistPlayerPresent()
        {
            return _IsWhitelistPlayerPresent(string.Empty);
        }

        /// <summary>
        /// Checks if a player is whitelisted.
        /// </summary>
        /// <param name="playerApi">Player that should be checked.</param>
        /// <returns>True if the player is whitelisted, false if not.</returns>
        public virtual bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            if (playerApi == null) return false;
            return _IsPlayerWhitelisted(playerApi.displayName);
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
            if (!formattedNamesChanged)
                return formattedNames;

            formattedNamesChanged = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < whitelist.Count; i++)
            {
                sb.Append(whitelist[i]);
                if (i != whitelist.Count - 1)
                {
                    sb.Append("\n");
                }
            }
            formattedNames = sb.ToString();
            return formattedNames;
        }
        
        /// <summary>
        /// Generates an array containing all whitelisted users.
        /// This is slow! It's recommended to do <c>_GetUsersAsList()</c> instead if you can.
        /// </summary>
        /// <returns>An Array of usernames currently on the whitelist.</returns>
        public virtual string[] _GetUsersAsArray()
        {
            return DataListExtensions.ToArray(whitelist);
        }
        
        /// <summary>
        /// Generates a copy of the current whitelist.
        /// Recommended over <c>_GetUsersAsArray()</c> for better performance.
        /// </summary>
        /// <returns>A copy of the whitelist.</returns>>
        public virtual DataList _GetUsersAsList()
        {
            return whitelist.ShallowClone();
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
        
        #region Getting Players
        /// <summary>
        /// Returns the <see cref="VRCPlayerApi"/> of a whitelisted player in the instance.
        /// Useful for getting a player to move ownership of an object to.
        /// </summary>
        /// <param name="playerNameToExclude">Display Name of a player to exclude from checking.</param>
        /// <returns>A <see cref="VRCPlayerApi"/> of a whitelisted player in the instance, or <c>null</c> if none are present.</returns>
        public virtual VRCPlayerApi _GetPlayerInInstance(string playerNameToExclude)
        {
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            var hasNameToExclude = !string.IsNullOrEmpty(playerNameToExclude);
            
            foreach (var player in players)
            {
                if (!player.IsValid()) continue;
                if (!_IsPlayerWhitelisted(player)) continue;
                if (hasNameToExclude && player.displayName == playerNameToExclude) continue;
                return player;
            }
            
            return null;
        }
        
        /// <summary>
        /// Returns the <see cref="VRCPlayerApi"/> of a whitelisted player in the instance.
        /// Useful for getting a player to move ownership of an object to.
        /// </summary>
        /// <param name="playerApiToExclude"><see cref="VRCPlayerApi"/> of a player to exclude from checking.</param>
        /// <returns>A <see cref="VRCPlayerApi"/> of a whitelisted player in the instance, or <c>null</c> if none are present.</returns>
        public VRCPlayerApi _GetPlayerInInstance(VRCPlayerApi playerApiToExclude)
        {
            return _GetPlayerInInstance(Utilities.IsValid(playerApiToExclude) ? playerApiToExclude.displayName : null);
        }
        
        /// <summary>
        /// Returns the <see cref="VRCPlayerApi"/> of a whitelisted player in the instance.
        /// Useful for getting a player to move ownership of an object to.
        /// </summary>
        /// <returns>A <see cref="VRCPlayerApi"/> of a whitelisted player in the instance, or <c>null</c> if none are present.</returns>
        public VRCPlayerApi _GetPlayerInInstance()
        {
            return _GetPlayerInInstance(string.Empty);
        }
        #endregion


        /// <summary>
        /// Should be called whenever the whitelist is changed or updated. 
        /// Notifies all connected behaviours, unless suppressed.
        /// </summary>
        /// <param name="senderBehaviour">Optional | The behaviour that triggered the update. If provided, it will be excluded from receiving the update event.</param>
        /// <param name="senderEvent">Optional | The Whitelist Updated event hat triggered the update. Prevents it specifically from being fired. Treated as "all" when not provided.</param>
        protected virtual void WhitelistUpdated(IUdonEventReceiver senderBehaviour = null, string senderEvent = null)
        {
            if (debugLogging)
                MUIDebug.Log($"Whitelist on {gameObject.name} got updated.", gameObject);
            
            // Reset cached values
            formattedNamesChanged = true;
            
            if (connectedBehaviours == null || connectedBehaviours.Count == 0) return;
            var behaviours = connectedBehaviours.GetKeys();
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].IsNull) continue;
                
                var behaviour = (IUdonEventReceiver)behaviours[i].Reference;
                if (behaviour == senderBehaviour && string.IsNullOrEmpty(senderEvent)) continue;

                if (connectedBehaviours.TryGetValue(behaviours[i], out DataToken value))
                {
                    DataList eventNames = value.DataList;
                    for (int j = 0; j < eventNames.Count; j++)
                    {
                        var eventName = eventNames[j].String;
                        if (behaviour != senderBehaviour || eventName != senderEvent)
                        {
                            behaviour.SendCustomEventDelayedFrames(eventName, 1);
                        }
                    }
                }
            }
        }
    }
}

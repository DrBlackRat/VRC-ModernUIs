using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(-10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistManager : UdonSharpBehaviour
    {
        [Header("Whitelist:")] 
        [Tooltip("Display Name of each User you would want to be on the whitelist.")]
        [SerializeField] private string[] whitelistedUsers;
        
        private UdonBehaviour[] connectedBehaviours;

        #region UdonBehaviour connection & external API to get info
        /// <summary>
        /// Sets up a connection with the Whitelist Manager. Used to receive the "_WhitelistUpdated" event if the whitelist changes.
        /// </summary>
        public void _SetUpConnection(UdonBehaviour behaviour)
        {
            connectedBehaviours = connectedBehaviours.Add(behaviour);
        }
        
        /// <summary>
        /// Returns a bool for if a username is on the whitelist.
        /// </summary>
        public bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            foreach (var name in whitelistedUsers)
            {
                if (name == playerApi.displayName) return true;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a formatted string with all whitelisted usernames. Formatted to one line per username.
        /// </summary>
        public string _GetNamesFormatted()
        {
            return string.Join("\n", whitelistedUsers);
        }

        /// <summary>
        /// Returns the array of whitelisted usernames.
        /// </summary>
        public string[] _GetNames()
        {
            return whitelistedUsers;
        }
        #endregion

        #region External API to change the whitelist
        /// <summary>
        /// Adds a specific user to the white list.
        /// </summary>
        public void _AddUser(string username)
        {
            _ReplaceWhitelist(whitelistedUsers.Add(username));
        }

        /// <summary>
        /// Removes a specific user from the white list.
        /// </summary>
        public void _RemoveUser(string username)
        {
            _ReplaceWhitelist(whitelistedUsers.Remove(username));
        }

        /// <summary>
        /// Adds an array of user to the white list.
        /// </summary>
        public void _AddUsers(string[] additionalUsernames)
        {
            _ReplaceWhitelist(ArrayExtensions.Combine(whitelistedUsers, additionalUsernames));
        }
        
        /// <summary>
        /// Fully replaces the current whitelist with a new one.
        /// </summary>
        public void _ReplaceWhitelist(string[] usernames)
        {
            whitelistedUsers = usernames;
            MUIDebug.Log($"Whitelist Updated");
            
            if (connectedBehaviours == null) return;
            foreach (var behaviour in connectedBehaviours)
            {
                if (behaviour == null) continue;
                behaviour.SendCustomEvent("_WhitelistUpdated");
            }
        }
        #endregion
    }
}

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistManager : UdonSharpBehaviour
    {
        [Tooltip("Display Name of each User you would want to be on the whitelist.")]
        [SerializeField] protected string[] whitelistedUsers;
        
        protected UdonBehaviour[] connectedBehaviours;

        protected virtual void Start()
        {
            ChangeWhitelist(whitelistedUsers, true);
        }

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
        /// Adds a specific user to the white list. Skips duplicate username.
        /// </summary>
        public virtual void _AddUser(string username)
        {
            foreach (var whitelistedUser in whitelistedUsers)
            {
                if (whitelistedUser == username)
                {
                    MUIDebug.LogWarning($"Whitelist Manager: User: {username} is already on the whitelist!");
                    return;
                }
            }
            MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
            ChangeWhitelist(whitelistedUsers.Add(username), false);
        }

        /// <summary>
        /// Removes a specific user from the white list.
        /// </summary>
        public virtual void _RemoveUser(string username)
        {
            MUIDebug.Log($"Whitelist Manager: Removed {username} from the whitelist.");
            ChangeWhitelist(whitelistedUsers.Remove(username), false);
        }

        /// <summary>
        /// Adds an array of user to the white list. Skips duplicate usernames.
        /// </summary>
        public virtual void _AddUsers(string[] newUsernames)
        {
            var tempWhitelist = whitelistedUsers;
            var usernames = newUsernames.Distinct();
            
            if (tempWhitelist.Length == 0)
            {
                ChangeWhitelist(usernames,  false);
                return;
            }
            // Checks for duplicate usernames and doesn't add them again.
            foreach (var username in usernames)
            {
                if (whitelistedUsers.Length != 0) {}
                foreach (var whitelistedUser in tempWhitelist)
                {
                    if (whitelistedUser == username)
                    {
                        MUIDebug.LogWarning($"Whitelist Manager: User: {username} is already on the whitelist!");
                    }
                    else
                    {
                        MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
                        tempWhitelist = tempWhitelist.Add(username);
                    }
                }
            }
            ChangeWhitelist(tempWhitelist, false);
        }
        
        /// <summary>
        /// Fully replaces the current whitelist with a new one.
        /// </summary>
        public virtual void _ReplaceWhitelist(string[] usernames)
        {
            ChangeWhitelist(usernames, false);
        }

        protected virtual void ChangeWhitelist(string[] usernames, bool fromNet)
        {
            whitelistedUsers = usernames;
            MUIDebug.Log($"Whitelist Manager: Whitelist Updated");
            
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

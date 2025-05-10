using System;
using System.Linq;
using System.Text;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistManager : UdonSharpBehaviour
    {
        [Tooltip("Display Name of each User you would want to be on the whitelist. Only correct on start!")]
        [SerializeField] protected string[] startWhitelist;

        protected DataList whitelist = new DataList();
        protected DataList connectedBehaviours = new DataList();

        protected virtual void Start()
        {
            foreach (var username in startWhitelist)
            {
                if (whitelist.Contains(username)) continue;
                whitelist.Add(username);
            }
            WhitelistUpdated(true);
        }

        #region UdonBehaviour connection & external API to get info
        /// <summary>
        /// Sets up a connection with the Whitelist Manager. Used to receive the "_WhitelistUpdated" event if the whitelist changes.
        /// </summary>
        public void _SetUpConnection(IUdonEventReceiver behaviour)
        {
            connectedBehaviours.Add((UnityEngine.Object)behaviour);
        }
        
        /// <summary>
        /// Returns a bool for if a username is on the whitelist.
        /// </summary>
        public bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            return whitelist.Contains(playerApi.displayName);;
        }
        
        public bool _IsPlayerWhitelisted(string username)
        {
            return whitelist.Contains(username);
        }
        
        /// <summary>
        /// Returns a formatted string with all whitelisted usernames. Formatted to one line per username.
        /// </summary>
        public string _GetNamesFormatted()
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
        /// Returns the whitelist as an array. This is slow! it's recommended to do "_GetUsersAsList() instead"
        /// </summary>
        public string[] _GetUsersAsArray()
        {
            string[] usernames = new string[whitelist.Count];

            for (int i = 0; i < whitelist.Count; i++)
            {
                usernames[i] = whitelist[i].String;
            }
            
            return usernames;
        }

        /// <summary>
        /// Returns a copy of the whitelist DataList. Don't use this for editing!
        /// </summary>
        public DataList _GetUsersAsList()
        {
            return whitelist.DeepClone();
        }
        #endregion

        #region External API to change the whitelist
        /// <summary>
        /// Adds a specific user to the white list. Skips duplicate username.
        /// </summary>
        public virtual void _AddUser(string username)
        {
            if (whitelist.Contains(username))
            {
                MUIDebug.LogWarning($"Whitelist Manager: Could not add {username} to the whitelist as they are already on it!");
                return;
            }
            whitelist.Add(username);
            MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
            WhitelistUpdated(false);
        }

        /// <summary>
        /// Removes a specific user from the white list.
        /// </summary>
        public virtual void _RemoveUser(string username)
        {
            if (!whitelist.Contains(username))
            {
                MUIDebug.LogWarning($"Whitelist Manager: Could not remove {username} from the whitelist as they are not on it!");
                return;
            }

            whitelist.Remove(username);
            MUIDebug.Log($"Whitelist Manager: Removed {username} from the whitelist.");
            WhitelistUpdated(false);
        }

        /// <summary>
        /// Adds a DataList of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        public virtual void _AddUsers(DataList newUsernames)
        {
            if (newUsernames == null || newUsernames.Count == 0) return;

            for (int i = 0; i < newUsernames.Count; i++)
            {
                var token = newUsernames[i];
                if (token.IsNull) continue;
                
                var username = token.String;

                if (whitelist.Contains(username))
                {
                    MUIDebug.LogWarning($"Whitelist Manager: Skipped adding {username} to the whitelist as they are already on it!");
                    continue;
                }
                whitelist.Add(username);
                MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
            }
            WhitelistUpdated(false);
        }

        /// <summary>
        /// Adds an Array of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        public virtual void _AddUsers(string[] newUsernames)
        {
            if (newUsernames == null || newUsernames.Length == 0) return;

            foreach (var username in newUsernames)
            {
                if (whitelist.Contains(username))
                {
                    MUIDebug.LogWarning($"Whitelist Manager: Skipped adding {username} to the whitelist as they are already on it!");
                    continue;
                }
                whitelist.Add(username);
                MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
            }
            WhitelistUpdated(false);
        }
        
        /// <summary>
        /// Fully replaces the current whitelist with a new one. This is usually not recommended, try _AddUsers or _RemoveUser first.
        /// </summary>
        public virtual void _ReplaceWhitelist(DataList newUsernames)
        {
            whitelist = newUsernames;
            WhitelistUpdated(false);
        }

        protected virtual void WhitelistUpdated(bool fromNet)
        {
            MUIDebug.Log($"Whitelist Manager: Whitelist Updated");
            
            if (connectedBehaviours == null) return;
            for (int i = 0; i < connectedBehaviours.Count; i++)
            {
                if (connectedBehaviours[i].IsNull) continue;
                ((IUdonEventReceiver)connectedBehaviours[i].Reference).SendCustomEvent("_WhitelistUpdated");
            }
        }
        #endregion
    }
}

using System;
using System.Linq;
using System.Text;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.Economy;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Enums;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistManager : UdonSharpBehaviour
    {
        [Tooltip("Display Name of each User you would want to be on the whitelist. Only correct on start!")]
        [SerializeField] protected string[] startWhitelist;
        [Tooltip("Automatically adds the Instance Owner to the whitelist.\nAn Instance Owner only exists in Invite, Invite+, Friends and Friends+ Instances")]
        [SerializeField] protected bool allowInstanceOwner;
        [Tooltip("Automatically adds the Instance Master to the whitelist. \nIf the master changes the new master will be added, but the old one wont be removed. (This is due to technical limitations)")]
        [SerializeField] protected bool allowInstanceMaster;
        [Tooltip("Automatically adds everyone in the instance who owns this Udon Product to the Whitelist. \nIf the product expires while the user is in the instance they wont be removed unless they rejoin.")]
        [SerializeField] protected UdonProduct productAccess;

        protected DataList whitelist = new DataList();
        protected DataList connectedBehaviours = new DataList();

        protected virtual void Start()
        {
            foreach (var username in startWhitelist)
            {
                if (whitelist.Contains(username)) continue;
                whitelist.Add(username);
            }
            WhitelistUpdated(false);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var displayName = player.displayName;
            if (whitelist.Contains(displayName)) return;
            
            if (allowInstanceOwner && player.isInstanceOwner)
            {
                whitelist.Add(displayName);
                WhitelistUpdated(false);
            }

            if (allowInstanceMaster && player.isMaster)
            {
                whitelist.Add(displayName);
                WhitelistUpdated(false);
            }
        }
        
        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            if (result.ID != productAccess.ID) return;
            
            var displayName = player.displayName;
            if (whitelist.Contains(displayName)) return;
            
            whitelist.Add(displayName);
            WhitelistUpdated(false);
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
        public virtual bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            if (playerApi == null) return false;
            return whitelist.Contains(playerApi.displayName);;
        }
        
        /// <summary>
        /// Returns a bool for if a username is on the whitelist.
        /// </summary>
        public virtual bool _IsPlayerWhitelisted(string username)
        {
            if (username == null) return false;
            return whitelist.Contains(username);
        }
        
        /// <summary>
        /// Returns a formatted string with all whitelisted usernames. Formatted to one line per username.
        /// </summary>
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
        /// Returns the whitelist as an array. This is slow! it's recommended to do "_GetUsersAsList() instead"
        /// </summary>
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
        /// Returns a copy of the whitelist DataList. Don't use this for editing!
        /// </summary>
        public virtual DataList _GetUsersAsList()
        {
            return whitelist.DeepClone();
        }
        #endregion

        #region External API to change the whitelist

        /// <summary>
        /// Adds a specific user to the white list. Skips duplicate username.
        /// </summary>
        /// <param name="username">The username to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional: The behaviour that triggered the add event. Prevents "_WhitelistUpdated being called on it."</param>
        public virtual void _AddUser(string username, IUdonEventReceiver senderBehaviour = null)
        {
            if (whitelist.Contains(username))
            {
                MUIDebug.LogWarning($"Whitelist Manager: Could not add {username} to the whitelist as they are already on it!");
                return;
            }
            whitelist.Add(username);
            MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
            WhitelistUpdated(false, senderBehaviour);
        }

        /// <summary>
        /// Removes a specific user from the white list.
        /// </summary>
        /// <param name="username">The username to remove from the whitelist.</param>
        /// <param name="senderBehaviour">Optional: The behaviour that triggered the add event. Prevents "_WhitelistUpdated being called on it."</param>
        public virtual void _RemoveUser(string username , IUdonEventReceiver senderBehaviour = null)
        {
            if (!whitelist.Contains(username))
            {
                MUIDebug.LogWarning($"Whitelist Manager: Could not remove {username} from the whitelist as they are not on it!");
                return;
            }

            whitelist.Remove(username);
            MUIDebug.Log($"Whitelist Manager: Removed {username} from the whitelist.");
            WhitelistUpdated(false, senderBehaviour);
        }

        /// <summary>
        /// Adds a DataList of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        /// <param name="newUsernames">DataList usernames to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional: The behaviour that triggered the add event. Prevents "_WhitelistUpdated being called on it."</param>
        public virtual void _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
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
            WhitelistUpdated(false, senderBehaviour);
        }

        /// <summary>
        /// Adds an Array of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        /// <param name="newUsernames">Array of usernames to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional: The behaviour that triggered the add event. Prevents "_WhitelistUpdated being called on it."</param>
        public virtual void _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
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
            WhitelistUpdated(false, senderBehaviour);
        }
        
        /// <summary>
        /// Fully replaces the current whitelist with a new one. This is usually not recommended, try _AddUsers or _RemoveUser first.
        /// </summary>
        /// <param name="newUsernames">DataList of usernames to replace the whitelist with.</param>
        /// <param name="senderBehaviour">Optional: The behaviour that triggered the add event. Prevents "_WhitelistUpdated being called on it."</param>
        public virtual void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            whitelist = newUsernames;
            WhitelistUpdated(false, senderBehaviour);
        }

        protected virtual void WhitelistUpdated(bool fromNet, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.Log($"Whitelist Manager: Whitelist Updated");
            
            if (connectedBehaviours == null) return;
            for (int i = 0; i < connectedBehaviours.Count; i++)
            {
                if (connectedBehaviours[i].IsNull) continue;
                // Prevent Sending update event to sender
                var receiver = (IUdonEventReceiver)connectedBehaviours[i].Reference;
                if (receiver == senderBehaviour) continue;
                // Send delayed by one frame for performance reasons
                receiver.SendCustomEventDelayedFrames("_WhitelistUpdated", 0);
            }
        }
        #endregion
    }
}

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
    /// Extends <see cref="WhitelistGetterBase"/> to add functionality for modifying the whitelist.
    /// Provides methods to add and remove users.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WhitelistSetterBase : WhitelistGetterBase
    {
        /// <summary>
        /// Adds a specific user to the white list. Skips duplicate username.
        /// </summary>
        /// <param name="username">The username to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _AddUser(string username, IUdonEventReceiver senderBehaviour = null)
        {
            if (whitelist.Contains(username))
            {
                MUIDebug.LogWarning($"Whitelist Manager: Could not add {username} to the whitelist as they are already on it!");
                return;
            }
            whitelist.Add(username);
            MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
            WhitelistUpdated(senderBehaviour);
        }
        
        /// <summary>
        /// Removes a specific user from the white list.
        /// </summary>
        /// <param name="username">The username to remove from the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _RemoveUser(string username , IUdonEventReceiver senderBehaviour = null)
        {
            if (!whitelist.Contains(username))
            {
                MUIDebug.LogWarning($"Whitelist Manager: Could not remove {username} from the whitelist as they are not on it!");
                return;
            }

            whitelist.Remove(username);
            MUIDebug.Log($"Whitelist Manager: Removed {username} from the whitelist.");
            WhitelistUpdated(senderBehaviour);
        }
        
        /// <summary>
        /// Adds a DataList of usernames to the whitelist. Skips duplicate usernames.
        /// </summary>
        /// <param name="newUsernames">DataList usernames to add to the whitelist.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
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
                if (whitelist.Contains(username))
                {
                    MUIDebug.LogWarning($"Whitelist Manager: Skipped adding {username} to the whitelist as they are already on it!");
                    continue;
                }
                whitelist.Add(username);
                MUIDebug.Log($"Whitelist Manager: Added {username} to the whitelist.");
            }
            WhitelistUpdated(senderBehaviour);
        }
        
        /// <summary>
        /// Fully replaces the current whitelist with a new one. This is usually not recommended, try _AddUsers or _RemoveUser first.
        /// </summary>
        /// <param name="newUsernames">DataList of usernames to replace the whitelist with.</param>
        /// <param name="senderBehaviour">Optional | The behaviour that called the method. Prevents the update event being called on it.</param>
        public virtual void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            whitelist = newUsernames;
            WhitelistUpdated(senderBehaviour);
        }
    }
}

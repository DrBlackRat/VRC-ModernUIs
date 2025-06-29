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
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WhitelistGetterBase : UdonSharpBehaviour
    {
        protected DataList whitelist = new DataList();
        protected DataList connectedBehaviours = new DataList();
        
        /// <summary>
        /// Sets up a connection with the Whitelist Manager.
        /// Used to receive the <c>_WhitelistUpdated</c> event if the whitelist changes.
        /// </summary>
        /// <param name="behaviour">Event Receiver which should have the "_WhitelistUpdated called on." </param>
        public virtual void _SetUpConnection(IUdonEventReceiver behaviour)
        {
            connectedBehaviours.Add((UnityEngine.Object)behaviour);
        }
        
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
            for (int i = 0; i < connectedBehaviours.Count; i++)
            {
                if (connectedBehaviours[i].IsNull) continue;
                
                // Prevent Sending update event to sender
                var receiver = (IUdonEventReceiver)connectedBehaviours[i].Reference;
                if (receiver == senderBehaviour) continue;
                
                // Send delayed by one frame for performance reasons
                receiver.SendCustomEventDelayedFrames("_WhitelistUpdated", 1);
            }
        }
    }
}

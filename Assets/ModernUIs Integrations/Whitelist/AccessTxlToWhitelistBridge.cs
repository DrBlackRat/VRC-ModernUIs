using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using Texel;

namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessTxlToWhitelistBridge : WhitelistManager
    {
        [SerializeField] protected AccessControl accessControl;
        protected bool txlHasChanged = true;

        protected override void Start()
        {
            accessControl._Register(AccessControl.EVENT_VALIDATE, this, nameof(_UpdateAccessList));
            _UpdateAccessList();
        }

        public void _UpdateAccessList()
        {
            txlHasChanged = true;
            WhitelistUpdated(false);
        }

        private void updateTxlData()
        {
            if (!txlHasChanged) return;

            whitelist = new DataList();
            
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            foreach (var player in players)
            {
                if (!player.IsValid() || !accessControl._HasAccess(player)) continue;
                whitelist.Add(player.displayName);
            }
            
            txlHasChanged = false;
        }
        
        #region Overrides 
        /// <summary>
        /// Returns a bool for if a username is on the whitelist.
        /// </summary>
        public override bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            if (playerApi == null) return false;
            return accessControl._HasAccess(playerApi);
        }
        
        /// <summary>
        /// Returns a bool for if a username is on the whitelist.
        /// Quite a lot more expensive than doing it by playerAPI, if possible use it instead.
        /// </summary>
        public override bool _IsPlayerWhitelisted(string username)
        {
            if (username == null) return false;
            
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            
            foreach (var player in players)
            {
                if (player.displayName != username) continue;
                return accessControl._HasAccess(player);
            }
            
            return false;
        }
        
        /// <summary>
        /// Returns a formatted string with all whitelisted usernames. Formatted to one line per username.
        /// </summary>
        public override string _GetNamesFormatted()
        {
            updateTxlData();
            return base._GetNamesFormatted();
        }
        
        /// <summary>
        /// Returns the whitelist as an array. This is slow! it's recommended to do "_GetUsersAsList() instead"
        /// </summary>
        public override string[] _GetUsersAsArray()
        {
            updateTxlData();
            return base._GetUsersAsArray();
        }
        
        /// <summary>
        /// Returns a copy of the whitelist DataList. Don't use this for editing!
        /// </summary>
        public override DataList _GetUsersAsList()
        {
            updateTxlData();
            return base._GetUsersAsList();
        }
        
        
        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _AddUser(string username, IUdonEventReceiver senderBehaviour)
        {
            MUIDebug.LogError("_AddUser | NOT ENABLED ON AccessTXLWhitelistBridge");
        }

        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _RemoveUser(string username, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("_RemoveUser | NOT ENABLED ON AccessTXLWhitelistBridge");
        }

        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("_AddUsers | NOT ENABLED ON AccessTXLWhitelistBridge");
        }
        
        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("_AddUsers | NOT ENABLED ON AccessTXLWhitelistBridge");
        }

        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("_ReplaceWhitelist | NOT ENABLED ON AccessTXLWhitelistBridge");
        }
        #endregion
    }

}

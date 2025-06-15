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
    public class AccessTXLWhitelistBridge : WhitelistManager
    {
        public AccessControl accessControl;

        protected override void Start()
        {
            accessControl._Register(AccessControl.EVENT_VALIDATE, this, nameof(_UpdateAccessList));
            _UpdateAccessList();
        }

        public void _UpdateAccessList()
        {
            Debug.LogError("LIST UPDATEDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            WhitelistUpdated(false);
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
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override string _GetNamesFormatted()
        {
            return "_GetNamesFormatted | NOT ENABLED ON AccessTXLWhitelistBridge";
        }
        
        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override string[] _GetUsersAsArray()
        {
            string[] names = new[] { "_GetUsersAsArray | NOT ENABLED ON AccessTXLWhitelistBridge" };
            return names;
        }
        
        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override DataList _GetUsersAsList()
        {
            DataList names = new DataList();
            names.Add("_GetUsersAsList | NOT ENABLED ON AccessTXLWhitelistBridge");
            return names;
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

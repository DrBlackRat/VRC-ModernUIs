using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using Texel;

namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessTxlToWhitelistBridge : WhitelistGetterBase
    {
        [SerializeField] protected AccessControl accessControl;
        protected bool txlHasChanged = true;

        private void Start()
        {
            accessControl._Register(AccessControl.EVENT_VALIDATE, this, nameof(_UpdateAccessList));
            _UpdateAccessList();
        }

        public void _UpdateAccessList()
        {
            txlHasChanged = true;
            WhitelistUpdated();
        }

        private void UpdateTxlData()
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
        public override bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            if (playerApi == null) return false;
            return accessControl._HasAccess(playerApi);
        }
        
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
        
        public override string _GetNamesFormatted()
        {
            UpdateTxlData();
            return base._GetNamesFormatted();
        }
        
        public override string[] _GetUsersAsArray()
        {
            UpdateTxlData();
            return base._GetUsersAsArray();
        }
        
        public override DataList _GetUsersAsList()
        {
            UpdateTxlData();
            return base._GetUsersAsList();
        }
        #endregion
    }

}

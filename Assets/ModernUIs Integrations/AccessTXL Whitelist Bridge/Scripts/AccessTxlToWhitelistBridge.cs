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

        protected override void Start()
        {
            base.Start();
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
            usersAddedBy = new DataDictionary();
            
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            foreach (var player in players)
            {
                if (!player.IsValid() || !accessControl._HasAccess(player)) continue;
                whitelist.Add(player.displayName);
            }
            
            usersAddedBy.SetValue(localName, whitelist.DeepClone());
            
            txlHasChanged = false;
        }
        
        #region Overrides

        public override bool _IsWhitelistPlayerPresent(string playerNameToExclude = null)
        {
            UpdateTxlData();
            return base._IsWhitelistPlayerPresent(playerNameToExclude);
        }


        public override bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            if (playerApi == null) return false;
            return accessControl._HasAccess(playerApi);
        }
        
        public override bool _IsPlayerWhitelisted(string username)
        {
            var player = GeneralExtensions.GetPlayerByDisplayName(username);
            if (player == null) return false;
            
            return accessControl._HasAccess(player);
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

        public override int _GetCount()
        {
            UpdateTxlData();
            return base._GetCount();
        }
        #endregion

        #region Get Added By Overrides
        public override DataList _GetUsersAddedBy(string username)
        {
            if (username == localName) 
                return whitelist.DeepClone();
            return new DataList();
        }

        public override int _GetUsersAddedByCount(string username)
        {
            if (username == localName) 
                return whitelist.Count;
            return 0;
        }

        public override string _GetWhoAdded(string username)
        {
            if (whitelist.Contains(username))
                return localName;
            return null;
        }
        #endregion

        #region Getting Players

        public override VRCPlayerApi _GetPlayerInInstance(string playerNameToExclude = null)
        {
            UpdateTxlData();
            return base._GetPlayerInInstance(playerNameToExclude);
        }

        #endregion
        
    }

}

﻿using System;
using BestHTTP.JSON;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Economy;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [DefaultExecutionOrder(900)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedWhitelistManager : WhitelistManager
    {
        [Tooltip("Whitelist of Admins which can change the Synced Whitelist without being on it them self.")]
        [SerializeField] protected WhitelistManager adminWhitelistManager;
        [Tooltip("Allow none Admins, but users who are on the whitelist it self to change it. Will be set to true if no Admin Whitelist is provided.")]
        [SerializeField] protected bool noneAdminAccess;
        
        [UdonSynced] protected string serializedWhitelist;
        protected bool canEdit;

        protected override void Start()
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                base.Start();
            }

            if (adminWhitelistManager != null)
            {
                adminWhitelistManager._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                noneAdminAccess = true;
            }
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                base.OnPlayerJoined(player);
            }
        }

        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                base.OnPurchaseConfirmed(result, player, purchased);
            }
        }

        // Only for Admin Whitelist
        public void _WhitelistUpdated()
        {
            canEdit = CanEdit(Networking.LocalPlayer);
        }


        // Idk why I suddenly started writing documentation for this, but I should probably do more of this in general, at least for public methods
        /// <summary>
        /// Checks if the provided player can edit the whitelist or not.
        /// </summary>
        /// <param name="player">VRCPlayerApi of which the edit access should be checked.</param>
        /// <returns>A bool depending on a player can edit or not.</returns>
        private bool CanEdit(VRCPlayerApi player)
        {
            return (noneAdminAccess && _IsPlayerWhitelisted(player)) || 
                   (adminWhitelistManager != null && adminWhitelistManager._IsPlayerWhitelisted(player));
        }

        /// <summary>
        /// Checks if the local player can edit the whitelist and writes an error message if they don't.
        /// </summary>
        /// <returns>A bool depending on a player can edit or not.</returns>
        private bool EditCheck()
        {
            if (!canEdit)
            {
                MUIDebug.LogError("Whitelist Manager: You are not allowed to edit this whitelist!");
                return false;
            }

            return true;
        }
        
        public override void OnDeserialization()
        {
            if (VRCJson.TryDeserializeFromJson(serializedWhitelist, out DataToken result))
            {
                whitelist = result.DataList;
                WhitelistUpdated(true);
            }
        }
        
        public override void OnPreSerialization()
        {
            if (VRCJson.TrySerializeToJson(whitelist, JsonExportType.Minify, out DataToken result))
            {
                serializedWhitelist = result.String;
            }
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (CanEdit(newOwner) || newOwner.IsOwner(gameObject))
            {
                MUIDebug.Log($"Whitelist Manager: {requester.displayName} has made {newOwner.displayName} the new Network Owner.");
                return true; 
            }
            else
            {
                MUIDebug.LogError($"Whitelist Manager: {requester.displayName} has tried to make {newOwner.displayName} the Network Owner, but isn't allowed to!");
                return false;
            }
        }
        
        protected override void WhitelistUpdated(bool fromNet, IUdonEventReceiver senderBehaviour = null)
        {
            var localPlayer = Networking.LocalPlayer;
            canEdit = CanEdit(localPlayer);
            if (!canEdit && !fromNet && !localPlayer.IsOwner(gameObject))
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            
            if (!fromNet)
            {
                if (!localPlayer.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
                RequestSerialization();
            }
            base.WhitelistUpdated(fromNet, senderBehaviour);
        }

        #region Access Check Overrides
        public override void _AddUser(string username, IUdonEventReceiver senderBehaviour)
        {
            if (!canEdit)
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            base._AddUser(username, senderBehaviour);
        }

        public override void _RemoveUser(string username, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;
            base._RemoveUser(username, senderBehaviour);
        }

        public override void _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;
            base._AddUsers(newUsernames, senderBehaviour);
        }
        
        public override void _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;
            base._AddUsers(newUsernames, senderBehaviour);
        }

        public override void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;
            base._ReplaceWhitelist(newUsernames, senderBehaviour);
        }
        #endregion
    }
}
    
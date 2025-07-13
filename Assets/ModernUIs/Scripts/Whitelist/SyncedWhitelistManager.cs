using System;
using BestHTTP.JSON;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Economy;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedWhitelistManager : WhitelistManager
    {
        [FormerlySerializedAs("adminWhitelistManager")]
        [Tooltip("Whitelist of Admins which can change the Synced Whitelist without being on it them self.")]
        [SerializeField] protected WhitelistGetterBase adminWhitelist;
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

            if (adminWhitelist != null)
            {
                adminWhitelist._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                noneAdminAccess = true;
            }
            canEdit = CanEdit(Networking.LocalPlayer);
        }

        // Only for Admin Whitelist
        public void _WhitelistUpdated()
        {
            canEdit = CanEdit(Networking.LocalPlayer);
        }

        
        /// <summary>
        /// Checks if the provided player can edit the whitelist or not.
        /// </summary>
        /// <param name="player">VRCPlayerApi of which the edit access should be checked.</param>
        /// <returns>A bool depending on a player can edit or not.</returns>
        private bool CanEdit(VRCPlayerApi player)
        {
            return (noneAdminAccess && _IsPlayerWhitelisted(player)) || 
                   (adminWhitelist != null && adminWhitelist._IsPlayerWhitelisted(player));
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
                base.WhitelistUpdated(); // Calls base class to skip the added access checks
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
        
        //Networked Version of update Event, if local only is needed use base. instead
        protected override void WhitelistUpdated(IUdonEventReceiver senderBehaviour = null)
        {
            var localPlayer = Networking.LocalPlayer;
            
            if (!CanEdit(localPlayer) && !localPlayer.IsOwner(gameObject))
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            
            if (!localPlayer.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
            RequestSerialization();

            base.WhitelistUpdated(senderBehaviour);
        }

        #region Access Check Overrides
        public override void _AddUser(string username, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;
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
    
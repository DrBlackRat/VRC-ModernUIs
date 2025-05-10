using System;
using BestHTTP.JSON;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedWhitelistManager : WhitelistManager
    {
        [Tooltip("Whitelist of Admins which can change the Synced Whitelist without being on it them self.")]
        [SerializeField] protected WhitelistManager adminWhitelistManager;
        [Tooltip("Allow none Admins, but users who are on the whitelist it self to change it. Will be set to true if no Admin Whitelist is provided.")]
        [SerializeField] protected bool noneAdminAccess;
        
        [UdonSynced] protected string serializedWhitelist;
        protected bool hasAccess;

        protected override void Start()
        {
            base.Start();
            if (adminWhitelistManager != null)
            {
                adminWhitelistManager._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                noneAdminAccess = true;
            }
        }

        // Only for Admin Whitelist
        public void _WhitelistUpdated()
        {
            hasAccess = (noneAdminAccess && _IsPlayerWhitelisted(Networking.LocalPlayer)) || adminWhitelistManager._IsPlayerWhitelisted(Networking.LocalPlayer);
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
            if ((noneAdminAccess && _IsPlayerWhitelisted(newOwner)) || (adminWhitelistManager != null && adminWhitelistManager._IsPlayerWhitelisted(newOwner)))
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
        
        protected override void WhitelistUpdated(bool fromNet)
        {
            var localPlayer = Networking.LocalPlayer;
            hasAccess = (noneAdminAccess && _IsPlayerWhitelisted(localPlayer)) || adminWhitelistManager._IsPlayerWhitelisted(localPlayer);
            if (!hasAccess && !fromNet)
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            
            if (!fromNet)
            {
                if (!localPlayer.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
                RequestSerialization();
            }
            base.WhitelistUpdated(fromNet);
        }

        #region Access Check Overrides
        public override void _AddUser(string username)
        {
            if (!hasAccess)
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            base._AddUser(username);
        }

        public override void _RemoveUser(string username)
        {
            if (!hasAccess)
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            base._RemoveUser(username);
        }

        public override void _AddUsers(DataList newUsernames)
        {
            if (!hasAccess)
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            base._AddUsers(newUsernames);
        }
        
        public override void _AddUsers(string[] newUsernames)
        {
            if (!hasAccess)
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            base._AddUsers(newUsernames);
        }

        public override void _ReplaceWhitelist(DataList newUsernames)
        {
            if (!hasAccess)
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            base._ReplaceWhitelist(newUsernames);
        }
        #endregion
    }
}
    
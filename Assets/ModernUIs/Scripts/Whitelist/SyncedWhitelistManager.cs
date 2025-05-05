using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedWhitelistManager : WhitelistManager
    {
        [Tooltip("Whitelist of Admins which can change the Synced Whitelist without being on it them self.")]
        [SerializeField] protected WhitelistManager adminWhitelistManager;
        [Tooltip("Allow none Admins, but users who are on the whitelist it self to change it. Will be set to true if no Admin Whitelist is provided.")]
        [SerializeField] protected bool noneAdminAccess;
        
        [UdonSynced] protected string[] netWhitelistedUsers;
        protected bool hasAdminAccess;

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
            hasAdminAccess = adminWhitelistManager._IsPlayerWhitelisted(Networking.LocalPlayer);
        }

        public override void OnDeserialization()
        {
            ChangeWhitelist(netWhitelistedUsers, true);
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
        
        protected override void ChangeWhitelist(string[] usernames, bool fromNet)
        {
            var localPlayer = Networking.LocalPlayer;
            var hasAccess = (noneAdminAccess && _IsPlayerWhitelisted(localPlayer)) || hasAdminAccess;
            if (!hasAccess && !fromNet)
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }

            netWhitelistedUsers = usernames;
            if (!fromNet)
            {
                if (!localPlayer.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
                RequestSerialization();
            }
            base.ChangeWhitelist(usernames, fromNet);
        }
    }
}
    
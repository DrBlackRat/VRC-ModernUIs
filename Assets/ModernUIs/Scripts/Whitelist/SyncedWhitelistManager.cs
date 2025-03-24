using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedWhitelistManager : WhitelistManager
    {
        [UdonSynced] protected string[] netWhitelistedUsers;
        protected bool hasAccess;


        public override void OnDeserialization()
        {
            ChangeWhitelist(netWhitelistedUsers, true);
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (_IsPlayerWhitelisted(newOwner))
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
            hasAccess = _IsPlayerWhitelisted(localPlayer);
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
    
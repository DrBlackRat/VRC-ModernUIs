using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedSelectorUI : WhitelistSelectorUI
    {
        [UdonSynced] protected bool persistenceLoaded;
        [UdonSynced] protected int netSelectedState;


        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence || !hasAccess || persistenceLoaded) return;
            
            if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            persistenceLoaded = true;
            RequestSerialization();
            
            if (PlayerData.TryGetInt(player, dataKey, out int value))
            {
                UpdateSelection(value, true, false, false);
            }
        }

        public override void OnDeserialization()
        {
            UpdateSelection(netSelectedState, true, false, true);
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (whitelistManager != null)
            {
                if (whitelistManager._IsPlayerWhitelisted(newOwner))
                {
                    MUIDebug.Log($"{requester.displayName} has made {newOwner.displayName} the new Network Owner.");
                    return true; 
                }
                else
                {
                    MUIDebug.LogError($"{requester.displayName} has tried to make {newOwner.displayName} the Network Owner, but isn't allowed to!");
                    return false;
                }
            }
            MUIDebug.Log($"{requester.displayName} has made {newOwner.displayName} the new Network Owner.");
            return true;
        }

        protected override bool UpdateSelection(int newState, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!base.UpdateSelection(newState, skipPersistence, skipSameCheck, fromNet)) return false;

            if (!fromNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                netSelectedState = selectedState;
                RequestSerialization();
            }

            return true;
        }
    }
}

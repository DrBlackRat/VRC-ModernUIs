using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs.Helpers;

namespace DrBlackRat.VRC.ModernUIs.SliderUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedSliderUI : WhitelistSliderUI
    {
        [UdonSynced] protected bool persistenceLoaded;
        [UdonSynced] protected float netValue;
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence || !hasAccess || persistenceLoaded) return;
            
            if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            if (PlayerData.TryGetFloat(player, dataKey, out float value))
            {
                persistenceLoaded = true;
                RequestSerialization();
                
                UpdateValue(value, true, false, false);
            }
        }

        public override void OnDeserialization()
        {
            UpdateValue(netValue, true, false, true);
        }
        
        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (whitelist != null)
            {
                if (whitelist._IsPlayerWhitelisted(newOwner))
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

        protected override bool UpdateValue(float newValue, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!base.UpdateValue(newValue, skipPersistence, skipSameCheck, fromNet)) return false;
            
            if (!fromNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                netValue = value;
                RequestSerialization();
            }

            return true;
        }
    }
}

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
        [UdonSynced] protected float netValue;
        protected VRCPlayerApi lastOwner;
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.IsOwner(gameObject)) return;
            base.OnPlayerRestored(player);
        }

        public override void OnDeserialization()
        {
            if (whitelist != null)
            {
                lastOwner = Networking.GetOwner(gameObject);
                if (whitelist._IsPlayerWhitelisted(lastOwner))
                {
                    UpdateValue(netValue, true, false, true);
                    return;
                }

                MUIDebug.LogWarning($"{lastOwner.displayName} is not whitelisted but tried to deserialize a synced SliderUI. Delaying deserialization by {loadDelay}s.");
                SendCustomEventDelayedSeconds(nameof(_DelayedOnDeserialization), loadDelay);
                return;
            }
            
            UpdateValue(netValue, true, false, true);
        }

        public void _DelayedOnDeserialization()
        {
            var newOwner = Networking.GetOwner(gameObject);
            if (newOwner != lastOwner)
            {
                MUIDebug.Log($"Delayed On Deserialization | Owner changed since original deserialization, aborting delayed deserialization.  Current owner: {newOwner.displayName}.");
                return;
            }
            
            if (!whitelist._IsPlayerWhitelisted(lastOwner))
            {
                MUIDebug.LogError($"Delayed On Deserialization | {lastOwner.displayName} is still not whitelisted! Aborting deserialization.");
                return;
            }
            
            MUIDebug.Log($"Delayed On Deserialization | {lastOwner.displayName} is now whitelisted! Running Deserialization,");
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

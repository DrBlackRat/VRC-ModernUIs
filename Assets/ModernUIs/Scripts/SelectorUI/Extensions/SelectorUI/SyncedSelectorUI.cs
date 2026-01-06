using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedSelectorUI : WhitelistSelectorUI
    {
        [UdonSynced] protected int netSelectedState;
        protected int queuedDeserializations = 0;

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.IsOwner(gameObject)) return;
            base.OnPlayerRestored(player);
        }

        public override void OnDeserialization()
        {
            var owner = Networking.GetOwner(gameObject);
            if (whitelist != null && !whitelist._IsPlayerWhitelisted(owner))
            {
                MUIDebug.LogWarning($"{owner.displayName} is not whitelisted but tried to deserialize {gameObject.name}. Delaying deserialization by {loadDelay}s.", gameObject);
                queuedDeserializations++;
                SendCustomEventDelayedSeconds(nameof(_DelayedOnDeserialization), loadDelay);
                return;
            }

            UpdateSelection(netSelectedState, true, false, true);
        }

        public void _DelayedOnDeserialization()
        {
            if (queuedDeserializations > 1)
            {
                MUIDebug.Log(
                    $"Delayed On Deserialization | {queuedDeserializations} queued deserializations," +
                    $" aborting delayed deserialization on {gameObject.name}.", gameObject);
                queuedDeserializations--;
                return;
            }
            
            var owner = Networking.GetOwner(gameObject);
            if (!whitelist._IsPlayerWhitelisted(owner))
            {
                MUIDebug.LogError($"Delayed On Deserialization | {owner.displayName} is not whitelisted! Aborting deserialization on {gameObject.name}.", gameObject);
                queuedDeserializations--;
                return;
            }
            
            MUIDebug.Log($"Delayed On Deserialization | {owner.displayName} is whitelisted! Running Deserialization on {gameObject.name}.", gameObject);
            queuedDeserializations--;
            UpdateSelection(netSelectedState, true, false, true);
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (whitelist != null && !whitelist._IsPlayerWhitelisted(newOwner))
            {
                MUIDebug.LogError($"{requester.displayName} has tried to make {newOwner.displayName} the Network Owner of {gameObject.name}, but isn't allowed to!", gameObject);
                return false;
            }
            MUIDebug.Log($"{requester.displayName} has made {newOwner.displayName} the new Network Owner of {gameObject.name}.", gameObject);
            return true;
        }
        
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            CheckOwnership();
        }

        private void CheckOwnership()
        {
            if (!Networking.LocalPlayer.isMaster) return;
            var owner = Networking.GetOwner(gameObject);
            
            if (whitelist == null || whitelist._IsPlayerWhitelisted(owner)) return;
            MUIDebug.LogWarning($"{owner.displayName} is the Network Owner of {gameObject.name}, but isn't whitelisted. " +
                                $"Trying to move ownership to a whitelisted player.", gameObject);

            var newOwner = whitelist._GetPlayerInInstance();
            if (newOwner == null)
            {
                MUIDebug.LogWarning($"Could not find a whitelisted player to transfer ownership of {gameObject.name} to. " +
                                    $"Keeping current owner, but synced data may get lost." , gameObject);
                return;
            }
            
            MUIDebug.Log($"Found whitelisted player \"{newOwner.displayName}\" to transfer ownership of {gameObject.name} to.", gameObject);
            Networking.SetOwner(newOwner, gameObject);
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

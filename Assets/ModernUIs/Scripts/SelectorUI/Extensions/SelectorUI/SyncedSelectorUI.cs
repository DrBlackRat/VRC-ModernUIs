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
        [Tooltip("If enabled before accepting network data, it checks if the network owner is whitelisted.\nThis is more secure, but only works if a whitelisted player is in the instance, disable if you prefer reliability over security.")]
        [SerializeField] protected bool secureNetworking = true;
        [Range(1, 10)][Tooltip("Hoe long to wait before retrying deserialization if the network owner is not whitelisted yet. (Only used by Secure Networking)")]
        [SerializeField] protected float deserializationRetryDelay = 5f;
        [Range(1, 10)][Tooltip("How many times to attempt deserialization if the network owner is not whitelisted yet. (Only used by Secure Networking)")]
        [SerializeField] protected int maxDeserializationAttempts = 4;
        
        [UdonSynced] protected int netSelectedState;
        
        protected int deserializationAttempts = 0;
        protected int scheduledDeserializations = 0;
        protected VRCPlayerApi deserializationPlayer;
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.IsOwner(gameObject)) return;
            base.OnPlayerRestored(player);
        }

        #region Deserialization
        public override void OnDeserialization()
        {
            deserializationAttempts = 0;
            deserializationPlayer = Networking.GetOwner(gameObject);
            HandleDeserialization();
        }
        
        public void _DelayedDeserialization()
        {
            scheduledDeserializations--;
            if (scheduledDeserializations > 0) return;
            if (deserializationAttempts == 0) return; // 0th = finished / new start
            HandleDeserialization();
        }

        private void HandleDeserialization()
        {
            if (!secureNetworking)
            {
                UpdateSelection(netSelectedState, true, false, true);
                return;
            }
            
            deserializationAttempts++;
            var owner = Networking.GetOwner(gameObject);
            if (owner != deserializationPlayer)
            {
                MUIDebug.LogError($"Network Owner has changed from {deserializationPlayer.displayName} to {owner.displayName} on {gameObject.name}! Aborting deserialization on {gameObject.name} after {deserializationAttempts} / {maxDeserializationAttempts} attempts.", gameObject);
                deserializationAttempts = 0;
                return;
            }
            
            if (whitelist != null && !whitelist._IsPlayerWhitelisted(owner))
            {
                if (deserializationAttempts >= maxDeserializationAttempts)
                {
                    MUIDebug.LogError($"{owner.displayName} is still not whitelisted! Aborting deserialization on {gameObject.name} after {deserializationAttempts} / {maxDeserializationAttempts} attempts.", gameObject);
                    deserializationAttempts = 0;
                    return;
                }
                
                MUIDebug.LogWarning($"{owner.displayName} is not whitelisted but tried to send network data on {gameObject.name}. Retrying in {deserializationRetryDelay}s, Attempt {deserializationAttempts} / {maxDeserializationAttempts}.", gameObject);
                scheduledDeserializations++;
                SendCustomEventDelayedSeconds(nameof(_DelayedDeserialization), deserializationRetryDelay);
                return;
            }

            if (deserializationAttempts > 1)
            {
                MUIDebug.Log($"{owner.displayName} is now whitelisted! Running Deserialization on {gameObject.name} after {deserializationAttempts} attempts.", gameObject);
            }
            else
            {
                MUIDebug.Log($"Received network data on {gameObject.name} from {owner.displayName}.");
            }
                
            deserializationAttempts = 0;
            UpdateSelection(netSelectedState, true, false, true);
        }
        #endregion
        
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
